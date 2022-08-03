using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Global options to apply when loading <see href="https://docs.unity3d.com/ScriptReference/Texture.html">textures</see>.
    /// </summary>
    public struct UGTextureSettings
    {
        /// <summary>
        /// Set this property to true to enable mip map generation.
        /// </summary>
        public bool generateMipMaps;

        /// <summary>
        /// This property defines the default filtering mode for textures that have no such specification in the dataset.
        /// </summary>
        public FilterMode defaultFilterMode;

        /// <summary>
        /// This property defines the anisotropic filtering level for textures.
        /// </summary>
        public int anisotropicFilterLevel;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="generateMipMaps">
        /// <see langword="true"/> to enable mip map generation.;
        /// <see langword="false"/> otherwise.
        /// </param>
        /// <param name="defaultFilterMode">
        /// Default filtering mode for textures that have no such specification in the dataset.
        /// </param>
        /// <param name="anisotropicFilterLevel">Specify the anisotropic filtering level for textures</param>
        public UGTextureSettings(bool generateMipMaps, FilterMode defaultFilterMode, int anisotropicFilterLevel)
        {
            this.generateMipMaps = generateMipMaps;
            this.defaultFilterMode = defaultFilterMode;
            this.anisotropicFilterLevel = anisotropicFilterLevel;
        }
    }
}
