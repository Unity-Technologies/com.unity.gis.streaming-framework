using Unity.Geospatial.HighPrecision;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Interface to get information for a given <see cref="NodeId"/>.
    /// </summary>
    public interface IGetNodeData : IExploreHierarchyNodes
    {
        /// <summary>
        /// Get the bounds of the given node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the desired node.</param>
        /// <returns>The bounds of the given node.</returns>
        DoubleBounds GetBounds(NodeId nodeId);

        /// <summary>
        /// Get the geometric error of a given node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the node to be queried.</param>
        /// <returns>The geometric error of the given node.</returns>
        float GetGeometricError(NodeId nodeId);

        /// <summary>
        /// Get whether node is set to always expand. Some nodes, such as the root
        /// node of the hierarchy and other placeholder nodes are designed to always
        /// expand and their bounds and geometric error should never be evaluated.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the node to be evaluated.</param>
        /// <returns>True if the node is set to always expand, false otherwise.</returns>
        bool GetAlwaysExpandFlag(NodeId nodeId);

        /// <summary>
        /// Get the error specification of the specified node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the requested node.</param>
        /// <returns>The error specification of the specified node.</returns>
        float GetErrorSpecification(NodeId nodeId);
    }
}
