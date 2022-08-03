
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Specify whether or not the <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see>
    /// will be affected by the scene lighting or not.
    /// </summary>
    public enum MaterialLighting
    {
        /// <summary>
        /// The <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see> will receive light.
        /// </summary>
        Lit,
        
        /// <summary>
        /// The <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see> will not receive light
        /// and will keep a constant look.
        /// </summary>
        Unlit
    }
}
