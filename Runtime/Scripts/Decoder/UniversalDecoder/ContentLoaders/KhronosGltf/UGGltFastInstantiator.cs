using System.Collections.Generic;

using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Decoder class used to convert .gltf and .glb files to instances.
    /// </summary>
    public class UGGltFastInstantiator : GLTFast.IInstantiator
    {
        private readonly struct MappedTexture
        {
            public MappedTexture(Texture2D texture, Vector4 st)
            {
                Texture = texture;
                TextureSt = st;
            }

            public Texture2D Texture { get; }
            public Vector4 TextureSt { get; }
        }

        private const string k_MultipleUVsNotSupported = "Multiple UVs are not supported";
        private const int k_NullMaterialIndex = -1;

        //
        //  TODO - Make this constructor shorter
        //
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="gltf">Importer used to read gltf files and convert them to Unity objects.</param>
        /// <param name="loaderActions">Manager allowing to load / unload <see cref="NodeId"/> content via the <see cref="UGSystem"/> <see cref="ITaskManager"/>.</param>
        /// <param name="name">Name to set the Unity <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see>.</param>
        /// <param name="transform">Where to position the imported geometry.</param>
        /// <param name="dataSource">Unique identifier of the <see cref="UGDataSource"/> to import.</param>
        /// <param name="lighting">Specify the material type to use when loading the <paramref name="dataSource"/>.</param>
        /// <param name="metadata">Metadata information to attach to the imported <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see>.</param>
        public UGGltFastInstantiator(
            GLTFast.GltfImport gltf,
            ILoaderActions loaderActions,
            string name,
            double4x4 transform,
            UGDataSourceID dataSource,
            UGLighting lighting,
            UGMetadata metadata)
        {
            m_Gltf = gltf;
            m_CommandStack = new RevertingCommandStack(loaderActions);
            m_DataSource = dataSource;
            m_Transform = transform;
            m_Name = name;
            m_Lighting = lighting;
            m_Metadata = metadata;
        }

        private readonly GLTFast.GltfImport m_Gltf;
        private readonly double4x4 m_Transform;
        private readonly UGDataSourceID m_DataSource;
        private readonly RevertingCommandStack m_CommandStack;
        private readonly string m_Name;
        private readonly UGLighting m_Lighting;
        private readonly UGMetadata m_Metadata;

        private readonly Dictionary<uint, InstanceData> m_Nodes = new Dictionary<uint, InstanceData>();
        private readonly List<InstanceData> m_Scenes = new List<InstanceData>();

        /// <summary>
        /// Used to initialize Instantiators. Always called first.
        /// </summary>
        public void Init()
        {
            //
            //  Method intentionally left blank
            //
        }

        /// <summary>
        /// Register a new instance with all its related data (mesh and position).
        /// </summary>
        /// <returns>A new <see cref="InstanceID"/> linked to the newly created instance.</returns>
        public InstanceID AllocateInstance()
        {
            InstanceData root = new InstanceData(m_DataSource, m_Transform, MeshID.Null, null);
            root.Name = m_Name;
            m_Metadata.InstanceData = root;
            root.Metadata = m_Metadata;

            foreach (InstanceData scene in m_Scenes)
                root.AddChild(scene);

            return m_CommandStack.AllocateInstance(root);
        }

        /// <summary>
        /// Frees up memory by disposing all sub assets.
        /// There can be no instantiation or other element access afterwards.
        /// </summary>
        public void DisposeInstance()
        {
            m_CommandStack.OnRevertComplete += () => m_Gltf.Dispose();
            m_CommandStack.Revert();
        }

        /// <summary>
        /// Called for adding a glTF scene.
        /// </summary>
        /// <param name="name">Name of the scene</param>
        /// <param name="nodeIndices">Indices of root level nodes in scene</param>
        /// <param name="animationClips">Apply those animation on the instances.</param>
        public void AddScene(string name, uint[] nodeIndices, AnimationClip[] animationClips)
        {
            //
            //  TODO - Implement support for animation clips
            //
            InstanceData sceneInstance = new InstanceData(m_DataSource, double4x4.identity, MeshID.Null, null)
            {
                Name = name
            };

            foreach (uint index in nodeIndices)
                sceneInstance.AddChild(m_Nodes[index]);

            m_Scenes.Add(sceneInstance);
        }

        /// <summary>
        /// Called for every Node in the glTF file
        /// </summary>
        /// <param name="nodeIndex">Index of node. Serves as identifier.</param>
        /// <param name="position">Node's local position in hierarchy</param>
        /// <param name="rotation">Node's local rotation in hierarchy</param>
        /// <param name="scale">Node's local scale in hierarchy</param>
        public void CreateNode(uint nodeIndex, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            InstanceData node = new InstanceData(m_DataSource, HPMath.TRS(new double3(position.x, position.y, position.z), rotation, scale), MeshID.Null, null)
            {
                Name = $"{m_DataSource} - GLTF Node {nodeIndex}"
            };
            m_Nodes.Add(nodeIndex, node);
        }

        /// <summary>
        /// Sets the name of a node.
        /// If a node has no name it falls back to the first valid mesh name.
        /// Null otherwise.
        /// </summary>
        /// <param name="nodeIndex">Index of the node to be named.</param>
        /// <param name="name">Valid name or null</param>
        public void SetNodeName(uint nodeIndex, string name)
        {
            m_Nodes[nodeIndex].Name = name;
        }

        /// <summary>
        /// Is called to set up hierarchical parent-child relations between nodes.
        /// Always called after both nodes have been created via CreateNode before.
        /// </summary>
        /// <param name="nodeIndex">Index of child node.</param>
        /// <param name="parentIndex">Index of parent node.</param>
        public void SetParent(uint nodeIndex, uint parentIndex)
        {
            InstanceData parent = m_Nodes[parentIndex];
            InstanceData child = m_Nodes[nodeIndex];
            parent.AddChild(child);
        }

        /// <summary>
        /// Called for adding a Primitive/Mesh to a Node.
        /// </summary>
        /// <param name="nodeIndex">Index of the node</param>
        /// <param name="meshName">Mesh's name</param>
        /// <param name="mesh">The actual Mesh</param>
        /// <param name="materialIndices">Material indices. Should be used to query the material</param>
        /// <param name="joints">If a skin was attached, the joint indices. Null otherwise (This is not implemented).</param>
        /// <param name="morphTargetWeights">Array of weights applying the skin morph shapes (This is not implemented).</param>
        /// <param name="primitiveNumeration">Primitives are numerated per Node, starting with 0 (This is not implemented).</param>
        public void AddPrimitive(uint nodeIndex, string meshName, Mesh mesh, int[] materialIndices, uint[] joints = null, uint? rootJoint = null, float[] morphTargetWeights = null, int primitiveNumeration = 0)
        {
            MaterialID[] materialIds = new MaterialID[materialIndices.Length];
            for (int i = 0; i < materialIndices.Length; i++)
            {
                int materialIndex = materialIndices[i];
                MaterialID materialId = GenerateMaterial(materialIndex);
                materialIds[i] = materialId;
            }

            MeshID meshId = m_CommandStack.AllocateMesh(mesh);

            InstanceData primitive = new InstanceData(m_DataSource, double4x4.identity, meshId, materialIds);
            primitive.Name = "Primitive";
            m_Nodes[nodeIndex].AddChild(primitive);
        }

        /// <summary>
        /// Called for adding a Primitive/Mesh to a Node that uses
        /// GPU instancing (EXT_mesh_gpu_instancing) to render the same mesh/material combination many times.
        /// Similar to/called instead of <seealso cref="AddPrimitive"/>, without joints/skin support.
        /// </summary>
        /// <param name="nodeIndex">Index of the node</param>
        /// <param name="meshName">Mesh's name</param>
        /// <param name="mesh">The actual Mesh</param>
        /// <param name="materialIndices">Material indices. Should be used to query the material</param>
        /// <param name="instanceCount">Number of instances</param>
        /// <param name="positions">Instance positions</param>
        /// <param name="rotations">Instance rotations</param>
        /// <param name="scales">Instance scales</param>
        /// <param name="primitiveNumeration">Primitives are numerated per Node, starting with 0</param>
        public void AddPrimitiveInstanced(uint nodeIndex, string meshName, Mesh mesh, int[] materialIndices, uint instanceCount, NativeArray<Vector3>? positions, NativeArray<Quaternion>? rotations, NativeArray<Vector3>? scales, int primitiveNumeration = 0)
        {
            //
            //  TODO - Implement this
            //
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Create a new <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see>.
        /// </summary>
        /// <param name="materialIndex">Material index used to query the material by the GLTFast.GltfImport.</param>
        /// <returns>The Unique Identifier of the newly created material.</returns>
        public MaterialID GenerateMaterial(int materialIndex)
        {
            if (materialIndex == k_NullMaterialIndex)
            {
                return GenerateDefaultMaterial();
            }
            else
            {
                GLTFast.Schema.Material gltfMaterial = m_Gltf.GetSourceMaterial(materialIndex);

                Assert.IsNotNull(gltfMaterial);

                return GenerateMaterial(gltfMaterial);
            }
        }

        private MaterialID GenerateMaterial(GLTFast.Schema.Material gltfMaterial)
        {
            bool isLit = m_Lighting switch
            {
                UGLighting.Default => (gltfMaterial.extensions?.KHR_materials_unlit == null),
                UGLighting.Lit => true,
                UGLighting.Unlit => false,
                _ => throw new System.NotImplementedException()
            };

            MaterialLighting lighting = isLit ? MaterialLighting.Lit : MaterialLighting.Unlit;

            //
            //  Alpha Mode
            //
            MaterialAlphaMode alphaMode = MaterialAlphaMode.Opaque;
            if (gltfMaterial.alphaModeEnum == GLTFast.Schema.Material.AlphaMode.BLEND)
            {
                alphaMode = MaterialAlphaMode.Transparent;
            }
            else if (gltfMaterial.alphaModeEnum == GLTFast.Schema.Material.AlphaMode.MASK)
            {
                alphaMode = MaterialAlphaMode.AlphaClip;
            }

            MaterialType type = new MaterialType(lighting, alphaMode);
            MaterialID result = m_CommandStack.AllocateMaterial(type);

            if (gltfMaterial.pbrMetallicRoughness != null)
            {
                //
                //  Add Color Component
                //
                m_CommandStack.AddMaterialProperty(result, MaterialProperty.AlbedoColor(gltfMaterial.pbrMetallicRoughness.baseColor));

                //
                //  Add Albedo Texture
                //
                GLTFast.Schema.TextureInfo textureInfo = gltfMaterial.pbrMetallicRoughness.baseColorTexture;
                MappedTexture albedo = TryGetTexture(textureInfo, m_Gltf);
                if (albedo.Texture != null)
                {
                    TextureID texture = m_CommandStack.AllocateTexture(albedo.Texture);
                    m_CommandStack.AddMaterialProperty(result, MaterialProperty.AlbedoTexture(texture, albedo.TextureSt));
                }

                //
                //  Smoothness
                //
                m_CommandStack.AddMaterialProperty(result, MaterialProperty.Smoothness(1.0f - gltfMaterial.pbrMetallicRoughness.roughnessFactor));

                //
                //  Alpha Test
                //
                if (gltfMaterial.alphaModeEnum == GLTFast.Schema.Material.AlphaMode.MASK)
                {
                    m_CommandStack.AddMaterialProperty(result, MaterialProperty.AlphaCutoff(gltfMaterial.alphaCutoff));
                }
            }
            else
            {
                Debug.LogWarning("Non PBR GLTF is not yet supported, not reading material properties.");
            }

            return result;
        }

        private MaterialID GenerateDefaultMaterial()
        {
            bool isLit = m_Lighting switch
            {
                UGLighting.Default => true,
                UGLighting.Lit => true,
                UGLighting.Unlit => false,
                _ => throw new System.NotImplementedException()
            };

            MaterialLighting lighting = isLit ? MaterialLighting.Lit : MaterialLighting.Unlit;
            MaterialType type = new MaterialType(lighting, MaterialAlphaMode.Opaque);
            MaterialID result = m_CommandStack.AllocateMaterial(type);
            m_CommandStack.AddMaterialProperty(result, MaterialProperty.AlbedoColor(Color.grey));

            return result;
        }

        private static Vector4 GetTextureTransform(GLTFast.Schema.TextureInfo textureInfo, bool flipY = false)
        {
            // Scale (x,y) and Transform (z,w)
            Vector4 textureSt = new Vector4(
                1, 1,// scale
                0, 0 // transform
                );

            if (textureInfo?.extensions?.KHR_texture_transform != null)
            {
                var tt = textureInfo.extensions.KHR_texture_transform;
                if (tt.texCoord != 0)
                {
                    Debug.LogError(k_MultipleUVsNotSupported);
                }


                if (tt.offset != null)
                {
                    textureSt.z = tt.offset[0];
                    textureSt.w = 1 - tt.offset[1];
                }
                if (tt.scale != null)
                {
                    textureSt.x = tt.scale[0];
                    textureSt.y = tt.scale[1];
                }

                textureSt.w -= textureSt.y; // move offset to move flip axis point (vertically)
            }

            if (flipY)
            {
                textureSt.z = 1 - textureSt.z; // flip offset in Y
                textureSt.y = -textureSt.y; // flip scale in Y
            }

            return textureSt;

        }

        private static MappedTexture TryGetTexture(GLTFast.Schema.TextureInfo textureInfo, GLTFast.IGltfReadable gltf)
        {
            if (textureInfo != null && textureInfo.index >= 0)
            {
                int textureIndex = textureInfo.index;
                var srcTexture = gltf.GetSourceTexture(textureIndex);
                if (srcTexture != null)
                {
                    var texture = gltf.GetTexture(textureIndex);
                    if (texture != null)
                    {
                        if (textureInfo.texCoord != 0)
                        {
                            Debug.LogError("GLTF parser does not support multiple UVs");
                        }

                        return new MappedTexture(texture, GetTextureTransform(textureInfo));
                    }
                    Debug.LogErrorFormat("Failed to load texture {0}", textureIndex.ToString());
                }
                else
                {
                    Debug.LogErrorFormat("Failed to find texture {0}", textureIndex.ToString());
                }
            }
            return default;
        }

        /// <summary>
        /// Called when a node has a camera assigned
        /// </summary>
        /// <param name="nodeIndex">Index of the node</param>
        /// <param name="cameraIndex">Index of the assigned camera</param>
        public void AddCamera(uint nodeIndex, uint cameraIndex)
        {
            Debug.LogWarning("GLTF file contains camera. The streaming framework will ignore it.");
        }
    }
}
