
namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    public interface IEditHierarchyNodes : IGetNodeData
    {
        /// <summary>
        /// Add a node to the bounding volume hierarchy.
        /// </summary>
        /// <param name="parent">The node which should be the new node's parent.</param>
        /// <param name="data">The data that this node should contain.</param>
        /// <param name="content">Content of the data to load when requested.</param>
        /// <returns>The <see cref="NodeId"/> of the node that has been added to the hierarchy.</returns>
        NodeId AddNode(NodeId parent, NodeData data, NodeContent content);

        /// <summary>
        /// Remove a node from the hierarchy. If this node has children, its children will also be removed
        /// from the hierarchy.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the node to be removed.</param>
        void RemoveNode(NodeId nodeId);

        /// <summary>
        /// Change the <see cref="NodeData"/> to the bounding volume hierarchy.
        /// </summary>
        /// <param name="nodeId">The node to update.</param>
        /// <param name="data">The data that this node should contain.</param>
        void UpdateNode(NodeId nodeId, NodeData data);
    }
}
