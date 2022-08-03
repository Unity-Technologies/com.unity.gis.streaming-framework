
using UnityEngine;

namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    /// <summary>
    /// <see href="https://docs.unity3d.com/ScriptReference/ScriptableObject.html">ScriptableObject</see> allowing
    /// to load ogc3d tiles files.
    /// </summary>
    [CreateAssetMenu(fileName = "OGC-3DTiles", menuName = "Geospatial/Data Sources/OGC 3DTiles", order = AssetMenuOrder)]
    public class Ogc3dTilesDataSourceObject : UGDataSourceObject
    {
        /// <summary>
        /// The URI of the OGC 3DTiles dataset. It can be either an absolute or relative path in the following form
        /// Note that relative path is relative to <see cref="Application.streamingAssetsPath"/>.
        /// <list>
        /// <item>file:///C:/path/to/folder/tileset.json</item>
        /// <item>http://www.example.com/route/to/tileset.json</item>
        /// <item>relative/path/to/folder/tileset.json</item>
        /// </list>
        /// </summary>
        public string uri;

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

        /// <summary>
        /// This property adjusts the level of detail for the given dataset
        /// A lower value will lower the level of detail
        /// Min value must be > 0 to avoid division by 0 when this value is used later to calculate error ratio
        /// </summary>
        [Min(float.Epsilon)]
        public float detailMultiplier = 1;


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
            return new Ogc3dTilesDataSource(uri, lighting, textureSettings, detailMultiplier, DataSourceID);
        }
    }

}
