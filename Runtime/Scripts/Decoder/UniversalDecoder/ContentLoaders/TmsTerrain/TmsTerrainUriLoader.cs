
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.Geospatial.HighPrecision;
using Unity.Geospatial.Streaming.UniversalDecoder;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Geospatial.Streaming.TmsTerrain
{
    /// <summary>
    /// Default UTR <see cref="UriLoader"/>.
    /// </summary>
    public class TmsTerrainUriLoader :
        UriLoader
    {
        /// <inheritdoc cref="UriLoader.SupportedFileTypes"/>
        private static readonly FileType[] k_SupportedFileTypes = { FileType.TmsTerrain };

        /// <summary>
        /// Unique identifier allowing to do indirect assignment.
        /// </summary>
        private readonly UGDataSourceID m_DataSource;

        /// <summary>
        /// Multiply the <see cref="NodeData.GeometricError"/> with this value allowing resolution
        /// control per <see cref="UGDataSource"/>.
        /// </summary>
        private readonly float m_DetailMultiplier;

        /// <summary>
        /// Lighting type to apply to the shading.
        /// </summary>
        private readonly UGLighting m_Lighting;

        /// <summary>
        /// Dictionary will all the currently loaded instances allowing to remove all
        /// related objects (mesh, materials, textures) when unloading the instance.
        /// </summary>
        private readonly Dictionary<InstanceID, RevertingCommandStack> m_LoadedInstances = new Dictionary<InstanceID, RevertingCommandStack>();

        /// <summary>
        /// Streamer used to load geometry from utr binary data.
        /// </summary>
        private readonly TmsTerrainStreamer m_Streamer = new TmsTerrainStreamer();

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="loaderActions">
        /// Reference to the command manager the <see cref="UriLoader"/> should publish it's requests to.
        /// </param>
        /// <param name="dataSource">Unique identifier allowing to do indirect assignment.</param>
        /// <param name="lighting">Lighting type to apply to the shading.</param>
        /// <param name="detailMultiplier">Multiply the <see cref="NodeData.GeometricError"/>
        /// with this value allowing resolution control per <see cref="UGDataSource"/>.</param>
        public TmsTerrainUriLoader(ILoaderActions loaderActions, UGDataSourceID dataSource, UGLighting lighting, float detailMultiplier) :
            base(loaderActions)
        {
            m_DataSource = dataSource;
            m_DetailMultiplier = detailMultiplier;
            m_Lighting = lighting;
        }

        /// <summary>
        /// Register a new <see cref="InstanceID"/> and a command stack allowing easy unloading.
        /// </summary>
        /// <param name="name">Name of the instance allowing to differentiate its content from other instances.</param>
        /// <param name="transform">The transform of the child. If this is a child instance, this
        /// will be degraded to a single precision matrix.</param>
        /// <param name="meshId">The ID of the mesh as provided by the UGCommandBuffer.
        /// Once the mesh has been instantiated, this ID will be used to
        /// reunite the mesh with the corresponding instance.</param>
        /// <param name="materialIds">The materials, once they have been instantiated. Null otherwise.</param>
        /// <param name="metadata">The metadata associated to the instance.</param>
        /// <param name="commandStack">Stack allowing to remove all related data when <see cref="Unload"/> is called.</param>
        /// <returns></returns>
        private InstanceID CreateInstance(string name, double4x4 transform, UGMetadata metadata, MeshID meshId, MaterialID[] materialIds, RevertingCommandStack commandStack)
        {
            InstanceData instanceData = new InstanceData(m_DataSource, transform, meshId, materialIds)
            {
                Name = name
            };

            metadata.InstanceData = instanceData;
            instanceData.Metadata = metadata;

            InstanceID instanceId = commandStack.AllocateInstance(instanceData);

            m_LoadedInstances.Add(instanceId, commandStack);
            return instanceId;
        }

        /// <inheritdoc cref="UriLoader.LoadAsync(NodeId, UriNodeContent, double4x4)"/>
        public override async Task<InstanceID> LoadAsync(NodeId nodeId, UriNodeContent content, double4x4 transform)
        {
            UriCollection uri = (UriCollection)content.Uri;

            Uri meshUri = uri.MeshUri;

            if (meshUri is null)
                return InstanceID.Null;

            Uri albedoUri = uri.AlbedoUri;

            byte[] meshData = await PathUtility.DownloadFileData(meshUri);
            byte[] albedoData = await PathUtility.DownloadFileData(albedoUri);

            RevertingCommandStack commandStack = new RevertingCommandStack(LoaderActions);

            TmsTerrainStreamer.TerrainTile meshTile;
            MeshID meshId;

            try
            {
                Load(meshData, commandStack, out meshId, out meshTile);
            }
            catch (FormatException error)
            {
                throw new FormatException("Format error during loading: " + meshUri, error);
            }
            MaterialID[] materialIds = Load(albedoData, albedoUri, commandStack);

            transform = UpdateTransform(transform, meshTile);

            NodeData nodeData = new NodeData(
                meshTile.GetBoundingVolume(transform),
                meshTile.GeometricError * m_DetailMultiplier,
                RefinementMode.Replace);

            LoaderActions.UpdateNode(nodeId, nodeData);

            if (content is IExpandingNodeContent { Leaf: Tile tile })
                tile.Update(meshTile);

            UGMetadata metadata = LoaderActions.InitializeMetadata(nodeId);

            return CreateInstance(meshUri.ToString(), transform, metadata, meshId, materialIds, commandStack);
        }

        /// <summary>
        /// Create a new Unity Mesh based on the given data.
        /// </summary>
        /// <param name="terrainData">Data representing the mesh to load.</param>
        /// <param name="commandStack">Add the loaded mesh to this stack allowing a clean unload when requested.</param>
        /// <param name="meshId">Will return the newly created <see cref="MeshID"/>.</param>
        /// <param name="tile">Result of the streamed data.</param>
        /// <exception cref="FormatException">Raised if the given data cannot be converted to a Unity Mesh.</exception>
        private void Load(byte[] terrainData, RevertingCommandStack commandStack, out MeshID meshId, out TmsTerrainStreamer.TerrainTile tile)
        {
            tile = m_Streamer.Load(terrainData);

            Mesh mesh = new Mesh();
            Mesh.ApplyAndDisposeWritableMeshData(tile.Mesh, mesh);
            mesh.RecalculateBounds();
            meshId = commandStack.AllocateMesh(mesh);
        }

        /// <summary>
        /// Create a Unity Material based on the given data.
        /// </summary>
        /// <param name="albedoData">Data representing the albedo texture to load.</param>
        /// <param name="albedoUri">Path of the source of the data. Used to name the loaded image.</param>
        /// <param name="commandStack">Add the loaded texture and its associated material allowing a clean unload when requested.</param>
        /// <returns>The list of the newly created <see cref="MaterialID"/>.</returns>
        /// <exception cref="NotImplementedException">Raised if the requested lighting setting is not supported.</exception>
        private MaterialID[] Load(byte[] albedoData, Uri albedoUri, RevertingCommandStack commandStack)
        {
            //
            //  TODO - Need to explicitly release texture resources
            //
            Texture2D albedoTexture = new Texture2D(2, 2);
            albedoTexture.LoadImage(albedoData);
            albedoTexture.name = albedoUri.ToString();
            albedoTexture.wrapMode = TextureWrapMode.Clamp;
            albedoTexture.filterMode = FilterMode.Trilinear;
            albedoTexture.anisoLevel = 6;
            TextureID textureId = commandStack.AllocateTexture(albedoTexture);

            Vector4 textureST = new Vector4(
                1, 1,  // scale
                0, 0   // transform
            );

            bool isLit = m_Lighting switch
            {
                UGLighting.Default => true,
                UGLighting.Lit => true,
                UGLighting.Unlit => false,
                _ => throw new NotImplementedException()
            };

            MaterialLighting lighting = isLit ? MaterialLighting.Lit : MaterialLighting.Unlit;
            MaterialType type = new MaterialType(lighting, MaterialAlphaMode.Opaque);
            MaterialID materialId = commandStack.AllocateMaterial(type);
            MaterialID[] materialIds = new MaterialID[1];
            materialIds[0] = materialId;

            commandStack.AddMaterialProperty(materialId, MaterialProperty.AlbedoTexture(textureId, textureST));
            commandStack.AddMaterialProperty(materialId, MaterialProperty.Smoothness(0));

            return materialIds;
        }

        /// <inheritdoc cref="UriLoader.SupportedFileTypes"/>
        public override IEnumerable<FileType> SupportedFileTypes
        {
            get { return k_SupportedFileTypes; }
        }

        /// <summary>
        /// Alter the position of the loaded data based on its content.
        /// </summary>
        /// <param name="transform">The expected position of node.</param>
        /// <param name="tile">The decoded terrain data to take its value from.</param>
        /// <returns>Where the node should be moved to.</returns>
        private static double4x4 UpdateTransform(double4x4 transform, TmsTerrainStreamer.TerrainTile tile)
        {
            //
            // TODO - Why can't we use the same concept as OGC?
            //
            // return math.mul(transform, HPMath.Translate(tile.Position));
            return HPMath.Translate(tile.Position);
        }

        /// <inheritdoc cref="UriLoader.Unload(InstanceID)"/>
        public override void Unload(InstanceID instanceId)
        {
            m_LoadedInstances[instanceId].Revert();
            m_LoadedInstances.Remove(instanceId);
        }
    }
}
