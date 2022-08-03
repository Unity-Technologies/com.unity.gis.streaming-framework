using System.Collections.Generic;

using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;
using UnityEngine.Assertions;


namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    internal class TargetStateController
    {

        /// <summary>
        /// Work items are queued in order to traverse the relevant part of the BVH recusively
        /// </summary>
        private readonly struct WorkItem
        {
            public NodeId NodeId { get; }
            public bool CollapseBlindly { get; }

            public WorkItem(NodeId nodeId, bool collapseBlindly)
            {
                NodeId = nodeId;
                CollapseBlindly = collapseBlindly;
            }
        }

        /// <summary>
        /// Any node intersecting with this bound (center of the <see cref="UGSceneObserver"/>) will be requested to be
        /// loaded allowing to load all objects close too near.
        /// </summary>
        private static readonly DoubleBounds k_ClipSpace = new DoubleBounds(double3.zero, new double3(2));

        /// <summary>
        /// Reference to the hierarchy interface used to edit the target state.
        /// </summary>
        private readonly IEditTargetState m_Hierarchy;

        /// <summary>
        /// This is the queue of work items used to traverse the bounding volume hierarchy
        /// recursively.
        /// </summary>
        private readonly Queue<WorkItem> m_WorkingQueue = new Queue<WorkItem>();


        /// <summary>
        /// This is a buffer to facilitate obtaining children from the hierarchy without
        /// generating garbage.
        /// </summary>
        private readonly List<NodeId> m_ChildrenWorkingBuffer = new List<NodeId>();

        

        /// <summary>
        /// Constructor for the target state controller. Requires a bounding volume
        /// hierarchy to work on.
        /// </summary>
        /// <param name="hierarchy">The hierarchy that the target state controller will manipulate</param>
        public TargetStateController(IEditTargetState hierarchy)
        {
            m_Hierarchy = hierarchy;
        }

        /// <summary>
        /// Set the detail observer data that will be used on the next 
        /// </summary>
        /// <param name="detailObserverData">The detail observer data which will be used to determine the target state in the hierarchy.</param>
        public void UpdateTargetState(DetailObserverData[] detailObserverData)
        {
            Assert.AreEqual(0, m_WorkingQueue.Count, "Working queue should always be clear at this point.");

            m_WorkingQueue.Enqueue(new WorkItem(m_Hierarchy.RootNode, collapseBlindly: false));

            while(m_WorkingQueue.Count > 0)
            {
                ProcessQueueItem(detailObserverData);
            }

        }

        /// <summary>
        /// Process a single item from the working queue.
        /// </summary>
        /// <param name="detailObserverData">Propagation of the detail observer data required to process and item.</param>
        private void ProcessQueueItem(DetailObserverData[] detailObserverData)
        {
            WorkItem item = m_WorkingQueue.Dequeue();

            if (item.CollapseBlindly)
            {
                CollapseBlindly(item.NodeId);
            }
            else
            {
                EvaluateNode(detailObserverData, item.NodeId);
            }
        }

        /// <summary>
        /// Collapse all nodes blindly without doing any checks regarding their
        /// geometric error. If a parent is collapsed, all of its recursive
        /// children should also be collapsed.
        /// </summary>
        /// <param name="nodeId">The id of the node to be collapsed.</param>
        private void CollapseBlindly(NodeId nodeId)
        {
            if (m_Hierarchy.GetTargetState(nodeId).IsCollapsed)
                return;

            m_Hierarchy.SetTargetState(nodeId, TargetState.Collapsed);

            EnqueueChildren(nodeId, collapseBlindly: true);
        }

        /// <summary>
        /// Evaluate the given node and either collapse it or expand it based on
        /// its bounding box and its geometric error.
        /// </summary>
        /// <param name="detailObserverData">The detail observer data used to evaluate the node.</param>
        /// <param name="nodeId">The node to be evaluated.</param>
        private void EvaluateNode(DetailObserverData[] detailObserverData, NodeId nodeId)
        {
            float geometricError = m_Hierarchy.GetGeometricError(nodeId);
            float errorSpecification = float.MaxValue;

            for (int i = 0; i < detailObserverData.Length; i++)
            {
                errorSpecification = math.min(errorSpecification, ComputeErrorSpecification(ref detailObserverData[i], nodeId));
            }

            if (errorSpecification < geometricError)
            {
                m_Hierarchy.SetErrorSpecification(nodeId, errorSpecification);
                m_Hierarchy.SetTargetState(nodeId, TargetState.Expanded);
                EnqueueChildren(nodeId, collapseBlindly: false);
            }
            else
            {
                m_WorkingQueue.Enqueue(new WorkItem(nodeId, collapseBlindly: true));
            }
        }

        /// <summary>
        /// Compute the error specification for a given node, using the detail observer data.
        /// </summary>
        /// <param name="detailObserverData">The detail observer data used for the computation.</param>
        /// <param name="nodeId">The node id to be computed</param>
        /// <returns>The error specification</returns>
        private float ComputeErrorSpecification(ref DetailObserverData detailObserverData, NodeId nodeId)
        {
            if (m_Hierarchy.GetAlwaysExpandFlag(nodeId))
            {
                return float.MinValue;
            }
            else 
            {
                DoubleBounds nodeBounds = m_Hierarchy.GetBounds(nodeId);

                DoubleBounds clipBounds = detailObserverData.UseClipPlane
                    ? DoubleBounds.Transform(nodeBounds, detailObserverData.ClipFromUniverse, detailObserverData.ClipPlane)
                    : DoubleBounds.Transform3x4(nodeBounds, detailObserverData.ClipFromUniverse);

                return !clipBounds.Intersects(k_ClipSpace)
                    ? float.MaxValue
                    : detailObserverData.GetErrorSpecification(clipBounds);
            }
        }

        /// <summary>
        /// Helper method to enqueue children into the working queue.
        /// </summary>
        /// <param name="nodeId">The node id who's children are to be queued.</param>
        /// <param name="collapseBlindly">Determine whether these children should be collapsed recursively without testing the bounds. This is useful
        /// to recursively collpase children after a parent has been deemed collapsed.</param>
        private void EnqueueChildren(NodeId nodeId, bool collapseBlindly)
        {
            Assert.AreEqual(0, m_ChildrenWorkingBuffer.Count, "Working buffer should always be empty at this point.");
            m_Hierarchy.GetChildren(nodeId, m_ChildrenWorkingBuffer);

            for (int i = 0; i < m_ChildrenWorkingBuffer.Count; i++)
            {
                m_WorkingQueue.Enqueue(new WorkItem(m_ChildrenWorkingBuffer[i], collapseBlindly));
            }

            m_ChildrenWorkingBuffer.Clear();
        }
    }
}
