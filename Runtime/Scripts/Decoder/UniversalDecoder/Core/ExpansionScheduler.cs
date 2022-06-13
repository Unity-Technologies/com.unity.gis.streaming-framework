using System.Collections.Generic;

using UnityEngine.Assertions;


namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    internal class ExpansionScheduler
    {
        public readonly struct Cache
        {
            public double TimeUntilCollapse { get; }
            public bool Expanded { get; }

            public Cache(bool expanded, double timeUntilCollapse)
            {
                Expanded = expanded;
                TimeUntilCollapse = timeUntilCollapse;
            }

            /// <summary>
            /// Set the cache data to be expanded. Since this is a readonly struct,
            /// this method returns a struct with the corresponding change.
            /// </summary>
            /// <returns>The modified cache</returns>
            public Cache SetExpanded()
            {
                return new Cache(expanded: true, TimeUntilCollapse);
            }

            /// <summary>
            /// Set the cache data to be collapsed. Since this is a readonly struct,
            /// this method returns a struct with the corresponding change.
            /// </summary>
            /// <returns>The modified cache</returns>
            public Cache SetCollapsed()
            {
                return new Cache(expanded: false, TimeUntilCollapse);
            }

            /// <summary>
            /// Set the cache's time variable. Since this is a readonly struct,
            /// this method returns a struct with the corresponding change.
            /// </summary>
            /// <returns>The modified cache</returns>
            public Cache SetTimeUntilCollapse(double time)
            {
                return new Cache(Expanded, time);
            }

            /// <summary>
            /// Generate human friendly string to represent the state of the cache struct.
            /// </summary>
            /// <returns>Human friendly string to represent the state of the cache struct.</returns>
            public override string ToString()
            {
                return $"ExpansionSchedulerCache(expanded:{Expanded}, timeUntilCollapse:{TimeUntilCollapse})";
            }
        }

        public enum State
        {
            /// <summary>
            /// Still working through the hierarchy to determine which nodes should
            /// be expanded next.
            /// </summary>
            Processing,

            /// <summary>
            /// Waiting on the content manager because it is overly backlogged.
            /// </summary>
            WaitingOnLoad,

            /// <summary>
            /// Done exploring and scheduling with the content manager
            /// </summary>
            Done
        }


        /// <summary>
        /// An internal reference to the hierarchy allowing the scheduler to query the current state of the system.
        /// </summary>
        private readonly IScheduleNodeChanges<Cache> m_Hierarchy;

        /// <summary>
        /// An internal reference to the current state controller, allowing the scheduler to change the state of the
        /// system.
        /// </summary>
        private readonly ICurrentStateController m_CurrentStateController;

        /// <summary>
        /// Used to to recursive work without using recursive function calls. Must always 
        /// be empty between function calls.
        /// </summary>
        private readonly Queue<NodeId> m_WorkingQueue = new Queue<NodeId>();

        /// <summary>
        /// This priority queue is used to determine which is the next node to be loaded.
        /// </summary>
        private readonly PriorityQueue<NodeId> m_PriorityQueue = new PriorityQueue<NodeId>();

        /// <summary>
        /// Used to collapse nodes recursively. Get's loaded from the front, starting with
        /// parents and moving down to children and gets unloaded from the back, child
        /// first, moving back towards the parents and collapsing as we go.
        /// </summary>
        private readonly Stack<NodeId> m_CollapseStack = new Stack<NodeId>();

        /// <summary>
        /// List recycled to obtain children in the hierarchy.
        /// </summary>
        private readonly List<NodeId> m_Children = new List<NodeId>();

        /// <summary>
        /// List recycled to communicate to the current state controller which
        /// nodes should be hidden.
        /// </summary>
        private readonly List<NodeId> m_HideList = new List<NodeId>();

        /// <summary>
        /// List recycled to communicate to the current state controller which
        /// nodes should be visible.
        /// </summary>
        private readonly List<NodeId> m_ShowList = new List<NodeId>();
        

        public ExpansionScheduler(IScheduleNodeChanges<Cache> hierarchy, ICurrentStateController currentStateController)
        {
            m_Hierarchy = hierarchy;
            m_CurrentStateController = currentStateController;
        }

        /// <summary>
        /// Defines how many nodes can be simultaneously loaded by the content manager. Beyond
        /// this number, the expansion scheduler will stop queuing up actions on the content
        /// manager and signal a Waiting state.
        /// </summary>
        public int MaximumSimultaneousContentRequests { get; set; } = 10;


        /// <summary>
        /// Wait this amount of time before collapsing a node. Time is in seconds.
        /// </summary>
        public float UnloadDelay { get; set; } = 2.0f;

        /// <summary>
        /// Reset the scheduler so that it reevaluates the entire tree again. This method simply
        /// resets the internal state of the expansion scheduler. Work is only performed the next
        /// time <see cref="ProcessNext"/> will be called.
        /// </summary>
        public void Reset()
        {
            m_PriorityQueue.Clear();
            m_PriorityQueue.Enqueue(0.0, m_Hierarchy.RootNode);
        }


        /// <summary>
        /// Return the current state of the scheduler.
        /// </summary>
        /// <returns></returns>
        public State GetState()
        {
            if (m_CurrentStateController.LoadingCount >= MaximumSimultaneousContentRequests)
                return State.WaitingOnLoad;

            if (m_PriorityQueue.Count > 0)
                return State.Processing;

            return State.Done;
        }

        /// <summary>
        /// Perform an atomic amount of work. If <see cref="Reset"/> has not been called, this will
        /// process the next chunk of work that needs to be performed.
        /// </summary>
        public void ProcessNext(double time)
        {
            while(m_PriorityQueue.Count > 0 && m_CurrentStateController.LoadingCount < MaximumSimultaneousContentRequests)
            {
                NodeId nodeId = m_PriorityQueue.Dequeue();

                TargetState targetState = m_Hierarchy.GetTargetState(nodeId);

                if (targetState.IsExpanded)
                {
                    EnqueueChildren(nodeId);
                    ExpandAndResetTimer(nodeId);
                }
                else
                {
                    CollapseAfterTimeout(nodeId, time);
                }
            }
        }

        /// <summary>
        /// Helper method to enqueue children into the priority queue.
        /// </summary>
        /// <param name="nodeId">Enqueue this node's children into the priority queue.</param>
        private void EnqueueChildren(NodeId nodeId)
        {
            Assert.AreEqual(0, m_Children.Count, "This buffer is expected to be empty at this point. Another method did not clean up as expected.");

            m_Hierarchy.GetChildren(nodeId, m_Children);

            foreach (NodeId child in m_Children)
            {
                float geometricError = m_Hierarchy.GetGeometricError(child);
                float errorSpecification = m_Hierarchy.GetErrorSpecification(child);
                m_PriorityQueue.Enqueue(errorSpecification / geometricError, child);
            }

            m_Children.Clear();
        }


        /// <summary>
        /// Expand the node and reset the collapse timer so that it cannot
        /// immediately collapse.
        /// </summary>
        /// <param name="nodeId"></param>
        private void ExpandAndResetTimer(NodeId nodeId)
        {
            Expand(nodeId);

            Cache cache = m_Hierarchy.GetSchedulerCache(nodeId);
            m_Hierarchy.SetSchedulerCache(nodeId, cache.SetTimeUntilCollapse(-1.0));
        }

        /// <summary>
        /// Check whether the node has been in the collapsed state for more than
        /// <see cref="UnloadDelay"/> seconds. If so, it will collapse it and
        /// its children.
        /// </summary>
        /// <param name="nodeId">The node to be potentially collapsed</param>
        /// <param name="time">The current time, in seconds.</param>
        private void CollapseAfterTimeout(NodeId nodeId, double time)
        {
            Cache cache = m_Hierarchy.GetSchedulerCache(nodeId);

            if (!cache.Expanded)
                return;

            if (cache.TimeUntilCollapse < 0.0)
            {
                cache = cache.SetTimeUntilCollapse(time + UnloadDelay);
                m_Hierarchy.SetSchedulerCache(nodeId, cache);
            }

            if (time >= cache.TimeUntilCollapse)
            {
                CollapseRecursively(nodeId);
            }
        }


        /// <summary>
        /// Expand the given node
        /// </summary>
        /// <param name="nodeId">Expand the given node</param>
        private void Expand(NodeId nodeId)
        {
            Assert.AreEqual(0, m_HideList.Count, "Hide list should be empty");
            Assert.AreEqual(0, m_ShowList.Count, "Show list should be empty");

            Assert.IsTrue(m_Hierarchy.HasNode(nodeId), "Hierarchy does not contain the specified node");
            if (!m_Hierarchy.NodeHasChildren(nodeId))
                return;
            Assert.AreEqual(TargetState.Expanded, m_Hierarchy.GetTargetState(nodeId), "A node that is not set to expand should not be expanded");

            Cache cache = m_Hierarchy.GetSchedulerCache(nodeId);
            if (cache.Expanded)
                return;

            CurrentState currentState = m_Hierarchy.GetCurrentState(nodeId);

            Assert.IsTrue(currentState.IsVisible, "Node state is not as expected when expanding a given node.");
            Assert.IsTrue(currentState.IsLoaded, "Node state is not as expected when expanding a given node.");

            m_Hierarchy.GetChildren(nodeId, m_ShowList);

            for (int i = 0; i < m_ShowList.Count; i++)
            {
                m_CurrentStateController.Load(m_ShowList[i]);
            }

            if (m_Hierarchy.GetRefinementMode(nodeId) == RefinementMode.Replace)
            {
                m_HideList.Add(nodeId);
            }

            m_CurrentStateController.UpdateVisibility(m_ShowList, m_HideList);
            m_Hierarchy.SetSchedulerCache(nodeId, cache.SetExpanded());

            m_ShowList.Clear();
            m_HideList.Clear();
        }

        /// <summary>
        /// Collapse the given node.
        /// </summary>
        /// <param name="nodeId">Collapse the given node.</param>
        private void Collapse(NodeId nodeId)
        {
            Assert.AreEqual(0, m_HideList.Count, "Hide list should be empty");
            Assert.AreEqual(0, m_ShowList.Count, "Show list should be empty");

            Assert.IsTrue(m_Hierarchy.HasNode(nodeId), "Hierarchy does not contain the specified node");
            Assert.AreEqual(TargetState.Collapsed, m_Hierarchy.GetTargetState(nodeId), "A node that is not set to collapse should not be collapsed");
            Assert.IsTrue(m_Hierarchy.NodeHasChildren(nodeId), "A node without children should never be collapsed");
            
            Cache cache = m_Hierarchy.GetSchedulerCache(nodeId);
            if(!cache.Expanded)
                return;

            CurrentState currentState = m_Hierarchy.GetCurrentState(nodeId);

            Assert.IsTrue(currentState.IsLoaded, "Node is expected to be loaded if it is going to be collapsed.");
            // Unlike expand, there is no check for visibility since it depends on the refinement mode

            m_Hierarchy.GetChildren(nodeId, m_HideList);

            if (m_Hierarchy.GetRefinementMode(nodeId) == RefinementMode.Replace)
            {
                m_ShowList.Add(nodeId);
            }

            m_CurrentStateController.UpdateVisibility(m_ShowList, m_HideList);
            m_Hierarchy.SetSchedulerCache(nodeId, cache.SetCollapsed());

            for (int i = 0; i < m_HideList.Count; i++)
            {
                m_CurrentStateController.Unload(m_HideList[i]);
            }

            m_ShowList.Clear();
            m_HideList.Clear();

            
        }

        /// <summary>
        /// Collapse nodes recursively such that all children of the
        /// provided node are also collapsed.
        /// </summary>
        /// <param name="nodeId">The node to be collapsed</param>
        private void CollapseRecursively(NodeId nodeId)
        {
            Assert.AreEqual(0, m_Children.Count, "HideList is expected to be clear between functions");

            m_WorkingQueue.Enqueue(nodeId);

            while(m_WorkingQueue.Count > 0)
            {
                nodeId = m_WorkingQueue.Dequeue();

                if (m_Hierarchy.GetSchedulerCache(nodeId).Expanded)
                {
                    m_CollapseStack.Push(nodeId);

                    m_Hierarchy.GetChildren(nodeId, m_Children);
                    EnqueueAll(m_Children, m_WorkingQueue);
                    m_Children.Clear();
                }
            }

            while(m_CollapseStack.Count > 0)
            {
                Collapse(m_CollapseStack.Pop());
            }
        }

        /// <summary>
        /// Helper method to enqueue a list into a queue
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        private static void EnqueueAll(List<NodeId> src, Queue<NodeId> dst)
        {
            for (int i = 0; i < src.Count; i++)
            {
                dst.Enqueue(src[i]);
            }
        }


    }
}
