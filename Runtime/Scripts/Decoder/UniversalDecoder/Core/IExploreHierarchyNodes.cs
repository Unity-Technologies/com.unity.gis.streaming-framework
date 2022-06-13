using System.Collections.Generic;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    public interface IExploreHierarchyNodes
    {
        /// <summary>
        /// Get the root node of the hierarchy
        /// </summary>
        /// <returns>The <see cref="NodeId"/> which corresponds to the root node of the hierarchy</returns>
        NodeId RootNode { get; }

        /// <summary>
        /// Check whether a node has children.
        /// </summary>
        /// <param name="nodeId">The node to be tested</param>
        /// <returns>True if it has children, false otherwise.</returns>
        bool NodeHasChildren(NodeId nodeId);


        /// <summary>
        /// Get the children of the given node id. This method uses the provided list to return the <see cref="NodeId"/> of
        /// the children in order to prevent garbage collection.
        /// </summary>
        /// <param name="nodeId">The starting point of the node id</param>
        /// <param name="result">A list of children. This list is expected to be empty before the function call.</param>
        void GetChildren(NodeId nodeId, List<NodeId> result);

        /// <summary>
        /// Get the parent of the given node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the node of interest.</param>
        /// <returns>The <see cref="NodeId"/> of the parent.</returns>
        NodeId GetParent(NodeId nodeId);


        /// <summary>
        /// Determine whether a node is valid or not
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the node to be tested</param>
        /// <returns>True if the node is valid, false otherwise.</returns>
        bool HasNode(NodeId nodeId);
    }
}
