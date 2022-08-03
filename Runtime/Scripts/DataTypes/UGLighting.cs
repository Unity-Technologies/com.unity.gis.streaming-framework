

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Specify the material type to use when loading a <see cref="UGDataSource"/>.
    /// </summary>
    public enum UGLighting
    {
        /// <summary>
        /// Use the default <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see> for
        /// the current <see cref="INodeContentLoader"/>.
        /// </summary>
        Default = 0,
        
        /// <summary>
        /// The <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see> will receive light.
        /// </summary>
        Lit,
        
        /// <summary>
        /// The <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see> will not receive light
        /// and will keep a constant look.
        /// </summary>
        Unlit,
    }
}
