using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Unity.Geospatial.HighPrecision;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// The BoundingVolumeHierarchy is one of the central classes which make up the UniversalDecoder.
    /// It is the memory representation of the bounding volume hierarchy structure that makes up HLODed
    /// streaming formats such as 3DTiles and many terrain formats. Each node has a bounding volume which
    /// is garanteed to contain all of its children, thus making it very efficient to tranverse and determine
    /// which nodes need to be visible as a function of any given set of observers.
    /// </summary>
    /// <typeparam name="T">The type passed here is the scheduler cache, which can be specified independantly of the rest of
    /// the BoundingVolumeHierarchy's implmementation.</typeparam>
    public class BoundingVolumeHierarchy<T> :
        IEditHierarchyNodes,
        IEditTargetState,
        IEditCurrentState,
        IScheduleNodeChanges<T>
        where T : struct
    {

        /// <summary>
        /// When creating a new <see cref="BoundingVolumeHierarchy{T}"/> instance, store in memory this amount of
        /// uninitialized <see cref="Node{T}"/> allowing less Array resizing.
        /// </summary>
        private const int k_InitialDetailNodeCapacity = 1000;

        /// <summary>
        /// When creating a new <see cref="BoundingVolumeHierarchy{T}"/> instance, create a List in memory with this amount
        /// of integer int it allowing less List resizing.
        /// </summary>
        private const int k_InitialChildrenIndirectionCapacity = 1000;

        /// <summary>
        /// <see cref="Node{T}"/> array allowing to indirectly access instances via <see cref="NodeId.Id"/>.
        /// </summary>
        private Node<T>[] m_DetailNodes;

        /// <summary>
        /// Number of <see cref="Node{T}"/> instances part of <see cref="m_DetailNodes"/> since <see cref="m_DetailNodes"/>
        /// is prefilled with uninitialized <see cref="Node{T}"/> for performances reasons.
        /// </summary>
        private int m_DetailNodesCount;

        /// <summary>
        /// Disposed <see cref="Node{T}"/> available to be replaced part of <see cref="m_DetailNodes"/>.
        /// </summary>
        private readonly Queue<int> m_AvailableDetailNodes;

        /// <summary>
        /// List of <see cref="IndirectionBlock"/> ids referring to the children <see cref="NodeId"/> of a given <see cref="Node{T}"/>.
        /// </summary>
        private readonly List<int> m_DetailNodeChildren;

        /// <summary>
        /// Indirection of <see cref="Node{T}"/> blocks used when linking a list of <see cref="Node{T}"/> as children of a given
        /// <see cref="Node{T}"/>.
        /// See <see cref="LinkChild"/>
        /// </summary>
        private readonly Dictionary<int, Queue<int>> m_AvailableDetailNodeChildren;

        /// <summary>
        /// Queue of <see cref="NodeId.Id"/> used by functions when recursive calls is needed.
        /// </summary>
        /// <example>
        /// m_WorkingQueue.Enqueue(0);
        ///
        /// while (m_WorkingQueue.Count > 0)
        /// {
        ///     int currentId = m_WorkingQueue.Dequeue();
        ///     ref Node current = ref m_DetailNodes[currentId];
        ///
        ///     EnqueueChildren(m_WorkingQueue, in current);
        /// }
        /// </example>
        private readonly Queue<int> m_WorkingQueue = new Queue<int>();

        /// <summary>
        /// Stack of <see cref="NodeId.Id"/> flagged to be unloaded.
        /// See <see cref="RemoveNode"/>
        /// </summary>
        /// <remarks>
        /// This is used after the unload was called to allow to flag the <see cref="Node{T}"/> memory space as available.
        /// </remarks>
        private readonly Stack<int> m_RemovalStack = new Stack<int>();



        /// <summary>
        /// Construct a new BoundingVolumeHierarchy
        /// </summary>
        public BoundingVolumeHierarchy()
        {
            m_DetailNodesCount = 0;
            m_DetailNodes = new Node<T>[k_InitialDetailNodeCapacity];
            m_AvailableDetailNodes = new Queue<int>(k_InitialDetailNodeCapacity);
            m_AvailableDetailNodeChildren = new Dictionary<int, Queue<int>>();

            m_DetailNodeChildren = new List<int>(k_InitialChildrenIndirectionCapacity);
            for (int i = 0; i < m_DetailNodeChildren.Count; i++)
                m_DetailNodeChildren.Add(NodeId.NullID);

            Node<T> root = Node<T>.Default;

            root.ChildrenBlockId = IndirectionBlock.NullBlockID;
            root.CurrentState = new CurrentState(CurrentState.Visible | CurrentState.Loaded);
            root.TargetState = default;
            root.ErrorSpecification = float.MaxValue;
            root.Parent = NodeId.NullID;
            root.Level = 0;
            root.Data = NodeData.Placeholder;
            root.SchedulerCache = default;

            AddDetailNode(in root);
        }

        /// <summary>
        /// Implementation of <see cref="IExploreHierarchyNodes.RootNode"/>
        /// </summary>
        public NodeId RootNode { get { return new NodeId(0); } }

        /// <summary>
        /// Implementation of <see cref="IExploreHierarchyNodes.NodeHasChildren(NodeId)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public bool NodeHasChildren(NodeId nodeId)
        {
            ref Node<T> node = ref m_DetailNodes[nodeId.Id];

            if (node.ChildrenBlockId == IndirectionBlock.NullBlockID)
                return false;

            IndirectionBlock children = new IndirectionBlock(m_DetailNodeChildren, node.ChildrenBlockId);

            return children.Count > 0;
        }

        /// <summary>
        /// Implementation of <see cref="IExploreHierarchyNodes.GetChildren(NodeId, List{NodeId})"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="result"></param>
        public void GetChildren(NodeId nodeId, List<NodeId> result)
        {
            Assert.AreEqual(0, result.Count, "Get children expects an empty list to contain the result. This is done to avoid allocation.");

            ref Node<T> node = ref m_DetailNodes[nodeId.Id];

            if (node.ChildrenBlockId == IndirectionBlock.NullBlockID)
                return;

            IndirectionBlock children = new IndirectionBlock(m_DetailNodeChildren, node.ChildrenBlockId);

            for (int i = 0; i < children.Count; i++)
                result.Add(new NodeId(children[i]));
        }

        /// <summary>
        /// Get children of a node while generating garbage. This method
        /// should be avoided. Instead, use <see cref="GetChildren(NodeId, List{NodeId})"/> 
        /// This way, the list can be recycled to avoid garbage collection. This method is useful
        /// for unit testing and debugging.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public List<NodeId> GetChildren(NodeId nodeId)
        {
            List<NodeId> result = new List<NodeId>();
            GetChildren(nodeId, result);
            return result;
        }

        /// <summary>
        /// Implementation of <see cref="IExploreHierarchyNodes.GetParent(NodeId)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public NodeId GetParent(NodeId nodeId)
        {
            return new NodeId(m_DetailNodes[nodeId.Id].Parent);
        }

        /// <summary>
        /// Implementation of <see cref="IExploreHierarchyNodes.HasNode(NodeId)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public bool HasNode(NodeId nodeId)
        {
            if (nodeId.Id < 0)
                return false;

            if (nodeId.Id >= m_DetailNodes.Length)
                return false;

            return m_DetailNodes[nodeId.Id].Level != -1;
        }

        /// <summary>
        /// Implementation of <see cref="IEditHierarchyNodes.AddNode(NodeId, NodeData, NodeContent)"/>
        /// </summary>
        /// <param name="parent">The node which should be the new node's parent.</param>
        /// <param name="data">The data that this node should contain.</param>
        /// <param name="content">Content of the data to load when requested.</param>
        /// <returns></returns>
        public NodeId AddNode(NodeId parent, NodeData data, NodeContent content)
        {
            int node = AddNodeData(in data, content);
            LinkChild(parent.Id, node);

            Assert.AreEqual(
                IndirectionBlock.NullBlockID,
                m_DetailNodes[node].ChildrenBlockId,
                "Cannot link to a parent with a Null child block.");

            NodeId result = new NodeId(node);

            if (content != null)
                content.NodeId = result;

            return result;
        }

        /// <summary>
        /// Implementation of <see cref="IEditHierarchyNodes.RemoveNode(NodeId)"/>
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the node to be removed.</param>
        public void RemoveNode(NodeId nodeId)
        {
            Assert.AreEqual(0, m_RemovalStack.Count, "Removal already in process. Cannot start an other one.");
            Assert.AreEqual(0, m_WorkingQueue.Count, "Working queue is not empty. Cannot start an other process.");

            ref Node<T> node = ref m_DetailNodes[nodeId.Id];

            Assert.AreNotEqual(-1, node.Level, "Attempting to remove non-existent node");

            int parentId = node.Parent;
            UnlinkChild(parentId, nodeId.Id);

            m_WorkingQueue.Enqueue(nodeId.Id);

            while (m_WorkingQueue.Count > 0)
            {
                int currentId = m_WorkingQueue.Dequeue();
                m_RemovalStack.Push(currentId);

                node = ref m_DetailNodes[currentId];

                Assert.IsTrue(!node.CurrentState.IsVisible, "Visible nodes cannot be removed.");
                EnqueueChildren(m_WorkingQueue, in node);
            }

            while (m_RemovalStack.Count > 0)
            {
                int removeId = m_RemovalStack.Pop();
                node = ref m_DetailNodes[removeId];

                if (node.ChildrenBlockId != IndirectionBlock.NullBlockID)
                {
                    IndirectionBlock children = new IndirectionBlock(m_DetailNodeChildren, node.ChildrenBlockId);
                    ReleaseChildrenBlock(ref children);
                }

                node = Node<T>.Default;
                m_AvailableDetailNodes.Enqueue(removeId);
            }
        }

        /// <summary>
        /// Implementation of <see cref="IEditHierarchyNodes.UpdateNode(NodeId, NodeData)"/>
        /// </summary>
        /// <param name="nodeId">The node to update.</param>
        /// <param name="data">The data that this node should contain.</param>
        public void UpdateNode(NodeId nodeId, NodeData data)
        {
            ValidateNodeDataCanBeUpdated(nodeId);

            m_DetailNodes[nodeId.Id].Data = data;
        }

        /// <summary>
        /// Validate if a <see cref="Node{T}.Data"/> can be updated.
        /// </summary>
        /// <param name="nodeId">Id of the node to validate.</param>
        private void ValidateNodeDataCanBeUpdated(NodeId nodeId)
        {
            foreach (NodeId child in GetChildren(nodeId))
                Assert.IsTrue(GetCurrentState(child).IsUnloaded, $"{nodeId} is loaded");
        }

        /// <summary>
        /// Implementation of <see cref="IGetNodeData.GetBounds(NodeId)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public DoubleBounds GetBounds(NodeId nodeId)
        {
            return m_DetailNodes[nodeId.Id].Data.Bounds;
        }

        /// <summary>
        /// Implementation of <see cref="IGetNodeData.GetGeometricError(NodeId)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public float GetGeometricError(NodeId nodeId)
        {
            return m_DetailNodes[nodeId.Id].Data.GeometricError;
        }

        /// <summary>
        /// Implementation of <see cref="IEditTargetState.GetTargetState(NodeId)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public TargetState GetTargetState(NodeId nodeId)
        {
            return m_DetailNodes[nodeId.Id].TargetState;
        }

        /// <summary>
        /// Implementation of <see cref="IEditTargetState.SetTargetState(NodeId, TargetState)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="targetState"></param>
        public void SetTargetState(NodeId nodeId, TargetState targetState)
        {
            m_DetailNodes[nodeId.Id].TargetState = targetState;
        }

        /// <summary>
        /// Implementation of <see cref="IEditCurrentState.GetCurrentState(NodeId)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public CurrentState GetCurrentState(NodeId nodeId)
        {
            return m_DetailNodes[nodeId.Id].CurrentState;
        }

        /// <summary>
        /// Implementation of <see cref="IEditCurrentState.SetCurrentState(NodeId, CurrentState)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="currentState"></param>
        public void SetCurrentState(NodeId nodeId, CurrentState currentState)
        {
            m_DetailNodes[nodeId.Id].CurrentState = currentState;
        }

        /// <summary>
        /// Implementation of <see cref="IScheduleNodeChanges{T}.GetRefinementMode(NodeId)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public RefinementMode GetRefinementMode(NodeId nodeId)
        {
            return m_DetailNodes[nodeId.Id].Data.RefinementMode;
        }

        /// <summary>
        /// Implementation of <see cref="IEditTargetState.GetAlwaysExpandFlag(NodeId)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public bool GetAlwaysExpandFlag(NodeId nodeId)
        {
            return m_DetailNodes[nodeId.Id].Data.AlwaysExpand;
        }

        /// <summary>
        /// Implementation of <see cref="IEditCurrentState.GetNodeContent(NodeId)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public NodeContent GetNodeContent(NodeId nodeId)
        {
            return m_DetailNodes[nodeId.Id].Content;
        }

        /// <summary>
        /// Implementation of <see cref="IGetNodeData.GetErrorSpecification(NodeId)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public float GetErrorSpecification(NodeId nodeId)
        {
            return m_DetailNodes[nodeId.Id].ErrorSpecification;
        }

        /// <summary>
        /// Implementation of <see cref="IEditTargetState.SetErrorSpecification(NodeId, float)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="errorSpecification"></param>
        public void SetErrorSpecification(NodeId nodeId, float errorSpecification)
        {
            m_DetailNodes[nodeId.Id].ErrorSpecification = errorSpecification;
        }

        /// <summary>
        /// Implementation of <see cref="IScheduleNodeChanges{T}.GetSchedulerCache(NodeId)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public T GetSchedulerCache(NodeId nodeId)
        {
            return m_DetailNodes[nodeId.Id].SchedulerCache;
        }

        /// <summary>
        /// Implementation of <see cref="IScheduleNodeChanges{T}.SetSchedulerCache(NodeId, T)"/>
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="cache"></param>
        public void SetSchedulerCache(NodeId nodeId, T cache)
        {
            m_DetailNodes[nodeId.Id].SchedulerCache = cache;
        }


        /// <summary>
        /// Register a new <see cref="Node{T}"/> at the end of the <see cref="m_DetailNodes"/> <see langword="Array"/>.
        /// If the <see langword="Array"/> has no more <see cref="Node{T}"/> memory space available, it will
        /// automatically augment the array size by doubling it.
        /// </summary>
        /// <param name="node"><see cref="Node{T}"/> details to register.</param>
        private void AddDetailNode(in Node<T> node)
        {
            if (m_DetailNodes.Length <= m_DetailNodesCount)
            {
                Node<T>[] tmp = new Node<T>[2 * m_DetailNodes.Length];
                Array.Copy(m_DetailNodes, tmp, m_DetailNodes.Length);
                m_DetailNodes = tmp;
            }
            m_DetailNodes[m_DetailNodesCount++] = node;
        }

        /// <summary>
        /// Create and register a new <see cref="Node{T}"/> based on the given <see cref="NodeData"/>.
        /// </summary>
        /// <param name="data">Data to register inside the <see cref="BoundingVolumeHierarchy{T}"/> tree.</param>
        /// <param name="content">Directives on how the <see cref="Node{T}"/> should be accessed and loaded.</param>
        /// <returns>The newly registered <see cref="Node{T}"/>.</returns>
        private int AddNodeData(in NodeData data, NodeContent content)
        {
            Node<T> node = default;

            node.Data = data;
            node.Content = content;
            node.ChildrenBlockId = IndirectionBlock.NullBlockID;
            node.TargetState = default;
            node.CurrentState = default;
            node.Level = -1;
            node.Parent = NodeId.NullID;
            node.ErrorSpecification = float.MaxValue;
            node.SchedulerCache = default;

            if (m_AvailableDetailNodes.Count == 0)
            {
                AddDetailNode(in node);
                return m_DetailNodesCount - 1;
            }
            else
            {
                int available = m_AvailableDetailNodes.Dequeue();
                m_DetailNodes[available] = node;
                return available;
            }
        }

        /// <summary>
        /// Set the parent of a <see cref="Node{T}"/> as child of an other <see cref="Node{T}"/>.
        /// </summary>
        /// <param name="parentId"><see cref="NodeId.Id"/> of the parent node.</param>
        /// <param name="childId"><see cref="NodeId.Id"/> of the child node.</param>
        private void LinkChild(int parentId, int childId)
        {
            Node<T> parentNode = m_DetailNodes[parentId];

            Node<T> childNode = m_DetailNodes[childId];
            childNode.Level = parentNode.Level + 1;
            childNode.Parent = parentId;
            m_DetailNodes[childId] = childNode;


            if (parentNode.ChildrenBlockId != IndirectionBlock.NullBlockID)
            {
                IndirectionBlock children = new IndirectionBlock(m_DetailNodeChildren, parentNode.ChildrenBlockId);

                if (children.Capacity > children.Count)
                {
                    children.Add(childId);
                }
                else
                {
                    IndirectionBlock newChildren = GetChildrenBlock(children.Count + 1);

                    for (int i = 0; i < children.Count; i++)
                        newChildren.Add(children[i]);

                    newChildren.Add(childId);

                    ReleaseChildrenBlock(ref children);
                    parentNode.ChildrenBlockId = newChildren.BlockId;
                    m_DetailNodes[parentId] = parentNode;
                }
            }
            else
            {
                IndirectionBlock children = GetChildrenBlock(1);

                parentNode.ChildrenBlockId = children.BlockId;
                children.Add(childId);

                m_DetailNodes[parentId] = parentNode;
            }
        }
        
        /// <summary>
        /// Remove the parenting link between two <see cref="Node{T}">Nodes</see>.
        /// </summary>
        /// <param name="parentId"><see cref="NodeId.Id"/> of the parent node to unlink with its child.</param>
        /// <param name="childId"><see cref="NodeId.Id"/> of the child to unlink.</param>
        private void UnlinkChild(int parentId, int childId)
        {
            ref Node<T> parentNode = ref m_DetailNodes[parentId];

            Node<T> childNode = m_DetailNodes[childId];
            childNode.Level = -1;
            childNode.Parent = NodeId.NullID;
            m_DetailNodes[childId] = childNode;

            Assert.AreNotEqual(
                IndirectionBlock.NullBlockID,
                parentNode.ChildrenBlockId,
                "Cannot unlink children of a node with a Null child block.");

            IndirectionBlock children = new IndirectionBlock(m_DetailNodeChildren, parentNode.ChildrenBlockId);

            children.Remove(childId);

            if (children.Count == 0)
            {
                IndirectionBlock childrenBlock = new IndirectionBlock(m_DetailNodeChildren, parentNode.ChildrenBlockId);
                ReleaseChildrenBlock(ref childrenBlock);
                parentNode.ChildrenBlockId = IndirectionBlock.NullBlockID;
            }
        }
        
        /// <summary>
        /// Tried to recycle an unused block of the indirection table that can fit the specified capacity.
        /// If none can be found, will create a new one at the end of the indirection table. If the table is
        /// full, then the table will be expanded and the block will be created.
        /// </summary>
        /// <param name="capacity">Number of children this block needs to point to.</param>
        /// <returns>The resulting indirection block</returns>
        private IndirectionBlock GetChildrenBlock(int capacity)
        {
            int blockSize = IndirectionBlock.BlockSizeFromCapacity(capacity);

            if (m_AvailableDetailNodeChildren.TryGetValue(blockSize, out Queue<int> available) && available.Count > 0)
                return new IndirectionBlock(m_DetailNodeChildren, available.Dequeue());

            else
                return IndirectionBlock.MakeNew(m_DetailNodeChildren, blockSize);
        }

        /// <summary>
        /// Mark this section of the indirection table as unused.
        /// </summary>
        /// <param name="childrenBlock"></param>
        private void ReleaseChildrenBlock(ref IndirectionBlock childrenBlock)
        {
            childrenBlock.Clear();
            int blockSize = childrenBlock.BlockSize;

            if (!m_AvailableDetailNodeChildren.ContainsKey(blockSize))
                m_AvailableDetailNodeChildren.Add(blockSize, new Queue<int>());

            m_AvailableDetailNodeChildren[blockSize].Enqueue(childrenBlock.BlockId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnqueueChildren(Queue<int> queue, in Node<T> node)
        {
            if (node.ChildrenBlockId == IndirectionBlock.NullBlockID)
                return;

            IndirectionBlock children = new IndirectionBlock(m_DetailNodeChildren, node.ChildrenBlockId);

            int length = children.Count;

            for (int i = 0; i < length; i++)
                queue.Enqueue(children[i]);
        }
    }
}
