
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Specify how decode <see href="https://docs.unity3d.com/ScriptReference/Material.html">Materials</see>
    /// with transparency directives.
    /// </summary>
    public enum MaterialAlphaMode
    {
        /// <summary>
        /// Skip the transparency evaluation and consider the object to be opaque.
        /// </summary>
        Opaque,
        
        /// <summary>
        /// Use the <see href="https://docs.unity3d.com/Manual/StandardShaderMaterialParameterAlbedoColor.html">transparency</see>
        /// when evaluating the opacity.
        /// </summary>
        Transparent,
        
        /// <summary>
        /// Use <see href="https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Alpha-Clipping.html">alpha clipping</see>
        /// when evaluating the opacity.
        /// </summary>
        AlphaClip
    }
}
