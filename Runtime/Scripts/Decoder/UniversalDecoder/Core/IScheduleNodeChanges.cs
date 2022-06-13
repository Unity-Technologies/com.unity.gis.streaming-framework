using Unity.Geospatial.HighPrecision;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    internal interface IScheduleNodeChanges<T> : 
        IExploreHierarchyNodes where T : struct
    {
        /// <summary>
        /// Get the <see cref="TargetState"/> of the specified node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the requested node.</param>
        /// <returns>The <see cref="TargetState"/> of the specified node.</returns>
        TargetState GetTargetState(NodeId nodeId);

        /// <summary>
        /// Get the <see cref="CurrentState"/> of the specified node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the requested node.</param>
        /// <returns>The <see cref="CurrentState"/> of the specified node.</returns>
        CurrentState GetCurrentState(NodeId nodeId);

        /// <summary>
        /// Get the geometric error of the specified node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the requested node.</param>
        /// <returns>The geometric error of the specified node.</returns>
        float GetGeometricError(NodeId nodeId);

        /// <summary>
        /// Get the error specification of the specified node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the requested node.</param>
        /// <returns>The error specification of the specified node.</returns>
        float GetErrorSpecification(NodeId nodeId);

        /// <summary>
        /// Get the <see cref="DoubleBounds"/> of the specified node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the requested node.</param>
        /// <returns>The <see cref="DoubleBounds"/> of the specified node.</returns>
        DoubleBounds GetBounds(NodeId nodeId);

        /// <summary>
        /// Get the <see cref="NodeData.RefinementMode"/> of the specified node.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the desired node.</param>
        /// <returns>The <see cref="NodeData.RefinementMode"/> of the specified node.</returns>
        RefinementMode GetRefinementMode(NodeId nodeId);

        /// <summary>
        /// Get a copy of the scheduler cache which can be used by the scheduler to 
        /// store node-specific values to better schedule node state changes.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the requested node.</param>
        /// <returns>A copy of the scheduler cache.</returns>
        T GetSchedulerCache(NodeId nodeId);

        /// <summary>
        /// Set the scheduler cache which can be used by the scheduler to store node-specific
        /// values to better schedule node state changes.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the requested node.</param>
        /// <param name="cache">The node-specific cache data.</param>
        void SetSchedulerCache(NodeId nodeId, T cache);
    }
}
