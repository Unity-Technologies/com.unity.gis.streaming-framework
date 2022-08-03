using System.Collections.Generic;

using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    internal class CurrentStateController : ICurrentStateController
    {
        /// <summary>
        /// The hierarchy that the current state controller will interact with.
        /// </summary>
        private readonly IEditCurrentState m_Hierarchy;

        /// <summary>
        /// The content manager that the current state controller will interact with.
        /// </summary>
        private readonly INodeContentManager m_ContentManager;

        /// <summary>
        /// Constructor for the current state controller
        /// </summary>
        /// <param name="hierarchy">The hierarchy that the controller will interact with</param>
        /// <param name="contentManager">The content manager that the controller will interact with</param>
        public CurrentStateController(IEditCurrentState hierarchy, INodeContentManager contentManager)
        {
            m_Hierarchy = hierarchy;
            m_ContentManager = contentManager;
        }

        /// <summary>
        /// Implementation of <see cref="ICurrentStateController.LoadingCount"/>
        /// </summary>
        public int LoadingCount { get { return m_ContentManager.LoadingCount; } }

        /// <summary>
        /// Implementation of <see cref="ICurrentStateController.Load(NodeId)"/>
        /// </summary>
        /// <param name="nodeId">The ID of the node to be loaded</param>
        public void Load(NodeId nodeId)
        {
            Assert.IsTrue(m_Hierarchy.HasNode(nodeId), "The node id that has been provided is invalid");

            CurrentState state = m_Hierarchy.GetCurrentState(nodeId);

            Assert.IsTrue(state.IsUnloaded, "Nodes to be loaded are expected to be unloaded");
            Assert.IsTrue(state.IsHidden, "Nodes to be loaded are not expected to be visible");

            NodeContent content = m_Hierarchy.GetNodeContent(nodeId);

            m_ContentManager.Load(nodeId, content);
            m_Hierarchy.SetCurrentState(nodeId, state.SetLoaded());
        }

        /// <summary>
        /// Implementation of <see cref="ICurrentStateController.Unload(NodeId)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        public void Unload(NodeId nodeId)
        {
            Assert.IsTrue(m_Hierarchy.HasNode(nodeId), "The node id that has been provided is invalid");

            CurrentState state = m_Hierarchy.GetCurrentState(nodeId);

            Assert.IsTrue(state.IsLoaded, "Node to be unloaded is expected to be loaded");
            Assert.IsTrue(state.IsHidden, "Node to be unloaded is expected to be hidden");

            m_ContentManager.Unload(nodeId);
            m_Hierarchy.SetCurrentState(nodeId, state.SetUnloaded());
        }

        /// <summary>
        /// Implementation of <see cref="ICurrentStateController.UpdateVisibility(IEnumerable{NodeId}, IEnumerable{NodeId})"/>
        /// </summary>
        /// <param name="visible"></param>
        /// <param name="hidden"></param>
        public void UpdateVisibility(IEnumerable<NodeId> visible, IEnumerable<NodeId> hidden)
        {
            foreach(NodeId nodeId in visible)
            {
                Assert.IsTrue(m_Hierarchy.HasNode(nodeId), "The node id that has been provided is invalid");

                CurrentState state = m_Hierarchy.GetCurrentState(nodeId);

                Assert.IsTrue(state.IsHidden, "Node is expected to be hidden when updated to be visible.");
                Assert.IsTrue(state.IsLoaded, "Node is expected to be loaded when changing visibility.");

                m_Hierarchy.SetCurrentState(nodeId, state.SetVisible());
            }

            foreach(NodeId nodeId in hidden)
            {
                Assert.IsTrue(m_Hierarchy.HasNode(nodeId), "The node id that has been provided is invalid");

                CurrentState state = m_Hierarchy.GetCurrentState(nodeId);

                Assert.IsTrue(state.IsVisible, "Node is expected to be visible when updated to be hidden.");
                Assert.IsTrue(state.IsLoaded, "Node is expected to be loaded when changing visibility.");

                m_Hierarchy.SetCurrentState(nodeId, state.SetHidden());
            }

            m_ContentManager.UpdateVisibility(visible, hidden);
        }
    }

}
