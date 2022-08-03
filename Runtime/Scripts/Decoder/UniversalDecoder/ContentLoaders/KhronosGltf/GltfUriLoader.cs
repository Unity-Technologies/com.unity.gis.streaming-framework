using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GLTFast;
using UnityEngine;
using Unity.Mathematics;

using GltfMaterial = GLTFast.Schema.Material;
using GltfSampler = GLTFast.Schema.Sampler;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Default glTF <see cref="UriLoader"/>.
    /// </summary>
    public class GltfUriLoader : UriLoader
    {

        /// <summary>
        /// Default glTF files are encoded in Z Minus and result in a rotated model when imported in Unity
        /// which is Z Plus.
        /// </summary>
        public static readonly double4x4 ZMinusForwardMatrix = new double4x4
        (
            -1,  0,  0,  0,
             0,  1,  0,  0,
             0,  0, -1,  0,
             0,  0,  0,  1
        );

        /// <summary>
        /// Applying this matrix as the adjustment matrix will leave imported glTF files as encoded without any modification.
        /// </summary>
        public static readonly double4x4 ZPlusForwardMatrix = double4x4.identity;

        /// <summary>
        /// Used to interrupting the glTF loading procedure at certain points. This decision is always a trade-off
        /// between minimum loading time and a stable frame rate.
        /// </summary>
        public class AlwaysDeferAgent : IDeferAgent
        {
            /// <summary>
            /// Conditional yield. May continue right away or yield once, based on time.
            /// </summary>
            /// <returns>If <see cref="ShouldDefer()"/> returns true, returns Task.Yield(). Otherwise returns sync</returns>
            public async Task BreakPoint()
            {
                await Task.Yield();
            }

            /// <summary>
            /// Conditional yield. May continue right away or yield once, based on time and duration.
            /// </summary>
            /// <param name="duration">Predicted duration of upcoming processing in seconds</param>
            /// <returns>If <see cref="ShouldDefer(float)"/> returns true, returns Task.Yield(). Otherwise returns sync</returns>
            public async Task BreakPoint(float duration)
            {
                await Task.Yield();
            }

            /// <summary>
            /// This will be called by GltfImport at various points in the loading procedure.
            /// </summary>
            /// <returns>True if the remaining work of the loading procedure should
            /// be deferred to the next frame/Update loop invocation. False if
            /// work can continue.</returns>
            public bool ShouldDefer()
            {
                return true;
            }

            /// <summary>
            /// Indicates if upcoming work should be deferred to the next frame.
            /// </summary>
            /// <param name="duration">Predicted duration of upcoming processing in seconds</param>
            /// <returns>True if the remaining work of the loading procedure should
            /// be deferred to the next frame/Update loop invocation. False if
            /// work can continue.</returns>
            public bool ShouldDefer(float duration)
            {
                return true;
            }
        }

        /// <summary>
        /// Material generator allowing to import glTF files via GLTFast.
        /// </summary>
        /// <remarks>Since we are managing the materials on our side, this class intentionally does nothing.</remarks>
        private sealed class MockMaterialGenerator : IMaterialGenerator
        {
            /// <summary>
            /// Is called prior to <seealso cref="GenerateMaterial"/>. The logger should be used
            /// to inform users about incidents of arbitrary severity (error,warning or info)
            /// during material generation.
            /// </summary>
            /// <param name="logger">Code logger to set.</param>
            public void SetLogger(ICodeLogger logger)
            {
                // Skip
            }

            /// <summary>
            /// Converts a glTF material into a Unity <see cref="Material"/>.
            /// <see cref="gltfMaterial"/> might reference textures, which can be queried from <see cref="gltf"/>
            /// </summary>
            /// <param name="gltfMaterial">Source glTF material</param>
            /// <param name="gltf">Interface to a loaded glTF's resources (e.g. textures)</param>
            /// <returns><see langword="null"/></returns>
            public Material GenerateMaterial(GltfMaterial gltfMaterial, IGltfReadable gltf)
            {
                return null;
            }

            /// <summary>
            /// Get fallback material that is assigned to nodes without a material.
            /// </summary>
            /// <returns><see langword="null"/></returns>
            public Material GetDefaultMaterial()
            {
                return null;
            }
        }

        /// <inheritdoc cref="UriLoader.SupportedFileTypes"/>
        private static readonly FileType[] k_SupportedFileTypes = { FileType.Gltf, FileType.Glb };

        /// <summary>
        /// Used to interrupting the glTF loading procedure at certain points. This decision is always a trade-off
        /// between minimum loading time and a stable frame rate.
        /// </summary>
        private readonly AlwaysDeferAgent m_DeferAgent = new AlwaysDeferAgent();

        /// <summary>
        /// Material generator allowing to import glTF files via GLTFast.
        /// </summary>
        /// <remarks>Since we are managing the materials on our side, this class intentionally does nothing.</remarks>
        private readonly MockMaterialGenerator m_MockMaterialGenerator = new MockMaterialGenerator();

        /// <summary>
        /// This dictionary contains information pertaining to instances which have been loaded
        /// in order to be able to unload them.
        /// </summary>
        private readonly Dictionary<InstanceID, UGGltFastInstantiator> m_LoadedInstances = new Dictionary<InstanceID, UGGltFastInstantiator>();

        /// <summary>
        /// Unique identifier allowing to do indirect assignment.
        /// </summary>
        private readonly UGDataSourceID m_DataSource;

        /// <summary>
        /// Lighting type to apply to the shading.
        /// </summary>
        private readonly UGLighting m_Lighting;

        /// <summary>
        /// Texture options to apply for each imported texture.
        /// </summary>
        private readonly UGTextureSettings m_TextureSettings;

        /// <summary>
        /// Each time a node is loaded, multiply its transform by this matrix allowing axis alignments when the format is not left-handed, Y-Up.
        /// </summary>
        private readonly double4x4 m_AdjustmentMatrix;

        /// <summary>
        /// Constructor with the adjustment matrix set to identity.
        /// </summary>
        /// <param name="loaderActions">
        /// Reference to the command manager the <see cref="UriLoader"/> should publish it's requests to.
        /// </param>
        /// <param name="dataSource">Unique identifier allowing to do indirect assignment.</param>
        /// <param name="lighting">Lighting type to apply to the shading.</param>
        /// <param name="textureSettings">Texture options to apply for each imported texture.</param>
        public GltfUriLoader(ILoaderActions loaderActions, UGDataSourceID dataSource, UGLighting lighting, UGTextureSettings textureSettings)
            : this(loaderActions, dataSource, lighting, textureSettings, double4x4.identity) { }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="loaderActions">
        /// Reference to the command manager the <see cref="UriLoader"/> should publish it's requests to.
        /// </param>
        /// <param name="dataSource">Unique identifier allowing to do indirect assignment.</param>
        /// <param name="lighting">Lighting type to apply to the shading.</param>
        /// <param name="textureSettings">Texture options to apply for each imported texture.</param>
        /// <param name="adjustmentMatrix">
        /// Each time a node is loaded, multiply its transform by this matrix allowing axis alignments when the format is not left-handed, Y-Up.
        /// </param>
        public GltfUriLoader(ILoaderActions loaderActions, UGDataSourceID dataSource, UGLighting lighting, UGTextureSettings textureSettings, double4x4 adjustmentMatrix)
            : base(loaderActions)
        {
            m_DataSource = dataSource;
            m_Lighting = lighting;
            m_TextureSettings = textureSettings;
            m_AdjustmentMatrix = adjustmentMatrix;
        }

        /// <inheritdoc cref="UriLoader.SupportedFileTypes"/>
        public override IEnumerable<FileType> SupportedFileTypes
        {
            get
            {
                return k_SupportedFileTypes;
            }
        }

        /// <inheritdoc cref="UriLoader.LoadAsync(NodeId, UriNodeContent, double4x4)"/>
        public override async Task<InstanceID> LoadAsync(NodeId nodeId, UriNodeContent content, double4x4 transform)
        {
            Uri gltfUri = content.Uri.MainUri;

            async Task<bool> Func(GltfImport gltFast, ImportSettings importSettings) => await gltFast.Load(gltfUri, importSettings);
            return await LoadAsync(nodeId, Func, gltfUri, transform);
        }

        /// <summary>
        /// Load the content specified as a <see langword="byte"/> array. Returns a task which, when completed,
        /// will return the InstanceId of the generated instance.
        /// </summary>
        /// <param name="nodeId"><see cref="BoundingVolumeHierarchy{T}"/> <see cref="NodeId"/> requested to be loaded.</param>
        /// <param name="bytes">Load the file from this <see cref="byte"/> array instead from a path.</param>
        /// <param name="uri">The uri to be loaded.</param>
        /// <param name="transform">The transform to be applied to the underlying geometry</param>
        /// <returns>The instance ID of the resulting geometry.</returns>
        public virtual async Task<InstanceID> LoadAsync(NodeId nodeId, byte[] bytes, string uri, double4x4 transform)
        {
            Uri uriRelAbs = new Uri(uri, UriKind.RelativeOrAbsolute);
            return await LoadAsync(nodeId, bytes, uriRelAbs, transform);
        }

        /// <summary>
        /// Load the content specified as a <see langword="byte"/> array. Returns a task which, when completed,
        /// will return the InstanceId of the generated instance.
        /// </summary>
        /// <param name="nodeId"><see cref="BoundingVolumeHierarchy{T}"/> <see cref="NodeId"/> requested to be loaded.</param>
        /// <param name="bytes">Load the file from this <see cref="byte"/> array instead from a path.</param>
        /// <param name="uri">The uri to be loaded.</param>
        /// <param name="transform">The transform to be applied to the underlying geometry</param>
        /// <returns>The instance ID of the resulting geometry.</returns>
        public virtual async Task<InstanceID> LoadAsync(NodeId nodeId, byte[] bytes, Uri uri, double4x4 transform)
        {
            async Task<bool> Func(GltfImport gltFast, ImportSettings importSettings) => await gltFast.LoadGltfBinary(bytes, uri, importSettings);
            return await LoadAsync(nodeId, Func, uri, transform);
        }

        /// <summary>
        /// Figure out the settings to use when importing the glTF file.
        /// </summary>
        /// <returns>A new ImportSettings for gltFast importer.</returns>
        private ImportSettings CreateImportSettings()
        {
            GltfSampler.MinFilterMode minFilter = GltfSampler.MinFilterMode.None;
            GltfSampler.MagFilterMode magFilter = GltfSampler.MagFilterMode.None;

            switch (m_TextureSettings.defaultFilterMode)
            {
                case FilterMode.Trilinear:
                    minFilter = GltfSampler.MinFilterMode.LinearMipmapLinear;
                    break;
                case FilterMode.Bilinear:
                    magFilter = GltfSampler.MagFilterMode.Linear;
                    break;
                // FilterMode.Point
                default:
                    magFilter = GltfSampler.MagFilterMode.Nearest;
                    break;
            }

            return new ImportSettings
            {
                generateMipMaps = m_TextureSettings.generateMipMaps,
                defaultMinFilterMode = minFilter,
                defaultMagFilterMode = magFilter,
                anisotropicFilterLevel = m_TextureSettings.anisotropicFilterLevel
            };
        }

        /// <summary>
        /// Load the glTF file part of the given <paramref name="uri"/>.
        /// </summary>
        /// <param name="nodeId"><see cref="BoundingVolumeHierarchy{T}"/> <see cref="NodeId"/> requested to be loaded.</param>
        /// <param name="func">Async function to execute to load the glTF file.</param>
        /// <param name="uri">Where to load the file from.</param>
        /// <param name="transform">Move the loaded node to this position.</param>
        /// <returns>The <see cref="InstanceID"/> associated with the newly imported node.</returns>
        /// <remarks>The given <paramref name="transform"/> will be multiplied by the <see cref="m_AdjustmentMatrix"/>.</remarks>
        protected async Task<InstanceID> LoadAsync(NodeId nodeId, Func<GltfImport, ImportSettings, Task<bool>> func, Uri uri, double4x4 transform)
        {
            transform = math.mul(transform, m_AdjustmentMatrix);

            GltfImport gltFast = new GltfImport(
                                downloadProvider: null,
                                deferAgent: m_DeferAgent,
                                materialGenerator: m_MockMaterialGenerator);

            ImportSettings importSettings = CreateImportSettings();

            bool success = await func(gltFast, importSettings);

            if (!success)
            {
                Debug.LogWarning("Could not open: " + uri);
                return InstanceID.Null;
            }

            UGMetadata metadata = LoaderActions.InitializeMetadata(nodeId);

            UGGltFastInstantiator instantiator = new UGGltFastInstantiator(
                                                                    gltFast,
                                                                    LoaderActions,
                                                                    uri.AbsoluteUri,
                                                                    transform,
                                                                    m_DataSource,
                                                                    m_Lighting,
                                                                    metadata);

            if (gltFast.InstantiateScene(instantiator))
            {
                InstanceID instance = instantiator.AllocateInstance();
                m_LoadedInstances.Add(instance, instantiator);
                return instance;

            }
            else
            {
                Debug.LogError("Failed to instantiate: " + uri);
                return InstanceID.Null;
            }
        }

        /// <inheritdoc cref="UriLoader.Unload"/>
        public override void Unload(InstanceID instanceId)
        {
            m_LoadedInstances[instanceId].DisposeInstance();
            m_LoadedInstances.Remove(instanceId);
        }
    }
}
