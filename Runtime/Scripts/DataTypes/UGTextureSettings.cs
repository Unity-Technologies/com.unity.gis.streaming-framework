using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    public struct UGTextureSettings
    {
        /// <summary>
        /// Set this property to true to enable mip map generation.
        /// </summary>
        public bool generateMipMaps;

        /// <summary>
        /// This property defines the default filtering mode for textures that have no such specification in the dataset 
        /// </summary>
        public FilterMode defaultFilterMode;

        /// <summary>
        /// This property defines the anisotropic filtering level for textures
        /// </summary>
        public int anisotropicFilterLevel;

        public UGTextureSettings(bool generateMipMaps, FilterMode defaultFilterMode, int anisotropicFilterLevel)
        {
            this.generateMipMaps = generateMipMaps;
            this.defaultFilterMode = defaultFilterMode;
            this.anisotropicFilterLevel = anisotropicFilterLevel;
        }
    }
}
