
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Specify how a <see cref="MaterialProperty">property</see> affects the
    /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see>.
    /// </summary>
    public enum MaterialPropertyType
    {
        /// <summary>
        /// Apply a texture to the
        /// <see href="https://docs.unity3d.com/Manual/StandardShaderMaterialParameterAlbedoColor.html">albedo</see> channel.
        /// </summary>
        AlbedoTexture,
        
        /// <summary>
        /// Set the <see href="https://docs.unity3d.com/Manual/StandardShaderMaterialParameterAlbedoColor.html">albedo</see> color.
        /// </summary>
        AlbedoColor,
        
        /// <summary>
        /// Specify the <see href="https://docs.unity3d.com/Manual/StandardShaderMaterialParameterSmoothness.html">smoothness</see>
        /// of the surface.
        /// </summary>
        Smoothness,
        
        /// <summary>
        /// Set the transparency depending on which <see cref="MaterialType"/> is set.
        /// </summary>
        AlphaCutoff,
    }
}
