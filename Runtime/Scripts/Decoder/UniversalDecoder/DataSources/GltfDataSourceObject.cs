
using UnityEngine;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// <see href="https://docs.unity3d.com/ScriptReference/ScriptableObject.html">ScriptableObject</see> allowing
    /// to load gltf and glb files.
    /// </summary>
    [CreateAssetMenu(fileName = "GLTF File", menuName = "Geospatial/Data Sources/GLTF", order = AssetMenuOrder)]
    public class GltfDataSourceObject : UGDataSourceObject
    {
        /// <summary>
        /// The URI to the GLTF or GLB file. It can be either an absolute or relative path in the following form
        /// Note that relative path is relative to <see cref="Application.streamingAssetsPath"/>.
        /// <list>
        /// <item>file:///C:/path/to/file.glb</item>
        /// <item>http://www.example.com/path/to/file.glb</item>
        /// <item>relative/path/to/folder/tileset.json</item>
        /// </list>
        /// </summary>
        public string uri;

        /// <summary>
        /// The position of the GLB in universe space
        /// </summary>
        public double3 position = double3.zero;

        /// <summary>
        /// The rotation of the GLB, in universe space
        /// </summary>
        public quaternion rotation = quaternion.identity;

        /// <summary>
        /// The scale of the GLB, in universe space
        /// </summary>
        public float3 scale = new float3(1F);

        /// <summary>
        /// Allows you to override the shading of the given dataset. By default, many
        /// datasets which are derived from satelite imagery use unlit textures and call
        /// for unlit shading. This setting allows the user to override this and make
        /// use of lit shading even if the dataset calls for unlit shading.
        /// </summary>
        public UGLighting lighting = UGLighting.Default;

        /// <summary>
        /// Set this property to true to enable mip map generation.
        /// </summary>
        public bool generateMipMaps = true;

        /// <summary>
        /// This property defines the default filtering mode for textures that have no such specification in the dataset
        /// </summary>
        public FilterMode defaultFilterMode = FilterMode.Bilinear;

        /// <summary>
        /// This property defines the anisotropic filtering level for textures
        /// </summary>
        [Range(0, 16)]
        public int anisotropicFilterLevel = 1;

        /// <inheritdoc cref="UGDataSourceObject.DataSourceID"/>
        private UGDataSourceID m_DataSourceID = UGDataSourceID.Null;

        /// <inheritdoc cref="UGDataSourceObject.DataSourceID"/>
        public override UGDataSourceID DataSourceID
        {
            get { return m_DataSourceID; }
        }

        /// <inheritdoc cref="UGDataSourceObject.Instantiate(UGSystemBehaviour)"/>
        public override UGDataSource Instantiate(UGSystemBehaviour system)
        {
            m_DataSourceID = new UGDataSourceID(System.Convert.ToInt64(GetInstanceID()));
            UGTextureSettings textureSettings = new UGTextureSettings(generateMipMaps, defaultFilterMode, anisotropicFilterLevel);
            return new GltfDataSource(uri, position, rotation, scale, lighting, textureSettings, DataSourceID);
        }
    }

}
