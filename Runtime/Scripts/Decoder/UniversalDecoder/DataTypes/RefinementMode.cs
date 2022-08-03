
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Define how a node will be displayed when it's children gets loaded.
    /// </summary>
    public enum RefinementMode
    {
        /// <summary>
        /// Additive nodes do not become invisible when they are expanded.
        /// </summary>
        Add,

        /// <summary>
        /// Replace nodes are made invisible when they are expanded.
        /// </summary>
        Replace
    }
}
