
namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    internal interface IEditTargetState : IGetNodeData
    {
        /// <summary>
        /// Get the <see cref="TargetState"/> of the given node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the node to be queried.</param>
        /// <returns>The <see cref="TargetState"/> of the given node.</returns>
        TargetState GetTargetState(NodeId nodeId);

        /// <summary>
        /// Set the target state of the given node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the node to be queried.</param>
        /// <param name="targetState">The <see cref="TargetState"/> of the given node.</param>
        void SetTargetState(NodeId nodeId, TargetState targetState);

        /// <summary>
        /// Set the error specification of the given node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the node to be queried.</param>
        /// <param name="errorSpecification">The error specification of the given node.</param>
        void SetErrorSpecification(NodeId nodeId, float errorSpecification);
    }
}
