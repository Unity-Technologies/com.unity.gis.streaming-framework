
namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    public interface IEditCurrentState : IExploreHierarchyNodes
    {
        /// <summary>
        /// Get the <see cref="CurrentState"/> of the specified node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the desired node.</param>
        /// <returns>The <see cref="CurrentState"/> of the specified node.</returns>
        CurrentState GetCurrentState(NodeId nodeId);


        /// <summary>
        /// Get the <see cref="NodeContent"/> of the specified node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the desired node.</param>
        /// <returns>The <see cref="NodeContent"/> of the specified node.</returns>
        NodeContent GetNodeContent(NodeId nodeId);


        /// <summary>
        /// Set the <see cref="CurrentState"/> of the specified node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the desired node.</param>
        /// <param name="currentState">The <see cref="CurrentState"/> of the specified node.</param>
        void SetCurrentState(NodeId nodeId, CurrentState currentState);
    }
}
