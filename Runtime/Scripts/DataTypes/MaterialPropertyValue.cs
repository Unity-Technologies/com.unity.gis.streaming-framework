
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Specify which elements of the <see cref="MaterialProperty"/> to apply
    /// on the <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see>.
    /// </summary>
    public enum MaterialPropertyValue
    {
        /// <summary>
        /// The <see cref="MaterialProperty.FloatValue"/> should be applied.
        /// </summary>
        Float,
        
        /// <summary>
        /// The <see cref="MaterialProperty.Vector4Value"/> should be applied.
        /// </summary>
        Vector4,
        
        /// <summary>
        /// The <see cref="MaterialProperty.ColorValue"/> should be applied.
        /// </summary>
        Color,
        
        /// <summary>
        /// The <see cref="MaterialProperty.Texture"/> should be applied.
        /// </summary>
        Texture,
        
        /// <summary>
        /// Apply a multi-textured terrains.
        /// </summary>
        TerrainTexture
    }
}
