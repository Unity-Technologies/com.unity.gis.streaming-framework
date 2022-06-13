using System.Threading.Tasks;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Interface for classes with <see cref="NodeId"/> <see cref="LoadNodeAsync">load</see> / <see cref="UnloadNode">unload</see> capabilities.
    /// </summary>
    public interface INodeContentLoader
    {
        /// <summary>
        /// Load the given NodeId given the provided NodeContent.
        /// </summary>
        /// <param name="nodeId">The NodeId of the node to be loaded. This should come directly from the bounding
        /// volume hierarchy.</param>
        /// <param name="nodeContent">The node content that should be loaded by the content loader. Given that this
        /// is an abstract class, various implementations can be passed here.</param>
        /// <returns>The newly created instance corresponding to the loaded content.</returns>
        Task<InstanceID> LoadNodeAsync(NodeId nodeId, NodeContent nodeContent);

        /// <summary>
        /// Unload the given node.
        /// </summary>
        /// <param name="nodeId">The NodeId of the node to be unloaded. This should come directly from the BoundingVolumeHierarchy.</param>
        void UnloadNode(NodeId nodeId);
    }
}
