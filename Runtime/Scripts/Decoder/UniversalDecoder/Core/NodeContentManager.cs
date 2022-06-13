using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    public class NodeContentManager :
        INodeContentManager
    {
        private sealed class QueueItem
        {
            public enum ItemAction
            {
                FinishLoading, LoadLater, Unload, Update,
            }

            public ItemAction Action { get; private set; }

            public NodeId NodeId { get; private set; }

            public List<NodeId> Visible { get; private set; }
            public List<NodeId> Hidden { get; private set; }

            public NodeContent NodeContent { get; private set; }
            public Task<InstanceID> Promise { get; private set; }

            public static QueueItem Load(NodeId nodeId, NodeContent content, Task<InstanceID> promise)
            {
                return new QueueItem
                {
                    Action = ItemAction.FinishLoading,
                    NodeId = nodeId,
                    Promise = promise,
                    NodeContent = content
                };
            }

            public static QueueItem LoadWithoutContent(NodeId nodeId)
            {
                return new QueueItem
                {
                    Action = ItemAction.FinishLoading,
                    NodeId = nodeId,
                    Promise = null,
                    NodeContent = null
                };
            }

            public static QueueItem LoadLater(NodeId nodeId, NodeContent content)
            {
                return new QueueItem
                {
                    Action = ItemAction.LoadLater,
                    NodeId = nodeId,
                    NodeContent = content
                };
            }

            public static QueueItem Unload(NodeId nodeId)
            {
                return new QueueItem
                {
                    Action = ItemAction.Unload,
                    NodeId = nodeId
                };
            }

            public static QueueItem UpdateVisibility(IEnumerable<NodeId> visible, IEnumerable<NodeId> hidden)
            {
                return new QueueItem
                {
                    Action = ItemAction.Update,
                    Visible = new List<NodeId>(visible),
                    Hidden = new List<NodeId>(hidden)
                };
            }
        }

        public enum State
        {
            Done,
            Processing,
            Waiting
        }

        private readonly IEditHierarchyNodes m_Hierarchy;
        private readonly LinkedList<QueueItem> m_Queue = new LinkedList<QueueItem>();
        private readonly Dictionary<ContentType, INodeContentLoader> m_Loaders = new Dictionary<ContentType, INodeContentLoader>();

        //
        //  FIXME - Combine these dictionaries
        //
        private readonly Dictionary<NodeId, InstanceID> m_Instances = new Dictionary<NodeId, InstanceID>();
        private readonly Dictionary<NodeId, NodeContent> m_Content = new Dictionary<NodeId, NodeContent>();

        //
        //  Working buffers
        //
        private readonly UGCommandBuffer m_AtomicCommandBuffer = new UGCommandBuffer(UUIDGenerator.Instance);

        public UGCommandBuffer CommandBuffer { get; }

        public UniqueContentTypeGenerator ContentTypeGenerator { get; } = new UniqueContentTypeGenerator();

        /// <inheritdoc cref="INodeContentManager.GenerateContentType()"/>
        public ContentType GenerateContentType()
        {
            return ContentTypeGenerator.Generate();
        }

        public int LoadingCount { get; private set; }

        public ITaskManager TaskManager { get; }

        public int UnloadingCount { get; private set; }

        public NodeContentManager(IEditHierarchyNodes hierarchy, UGCommandBuffer commandBuffer, ITaskManager taskManager)
        {
            m_Hierarchy = hierarchy;
            CommandBuffer = commandBuffer;
            TaskManager = taskManager;
        }

        void ILoaderActions.AddMaterialProperty(MaterialID materialId, MaterialProperty materialProperty)
        {
            CommandBuffer.AddMaterialProperty(materialId, materialProperty);
        }

        /// <inheritdoc cref="IEditHierarchyNodes.AddNode(NodeId, NodeData, NodeContent)"/>
        public NodeId AddNode(NodeId parent, in NodeData data, in NodeContent content)
        {
            return m_Hierarchy.AddNode(parent, data, content);
        }

        InstanceID ILoaderActions.AllocateInstance(InstanceData instanceData)
        {
            return CommandBuffer.AllocateInstance(instanceData);
        }

        MaterialID ILoaderActions.AllocateMaterial(MaterialType type)
        {
            return CommandBuffer.AllocateMaterial(type);
        }

        MeshID ILoaderActions.AllocateMesh(Mesh mesh)
        {
            return CommandBuffer.AllocateMesh(mesh);
        }

        MeshID ILoaderActions.AllocateMesh(MeshData meshData)
        {
            return CommandBuffer.AllocateMesh(meshData);
        }

        TextureID ILoaderActions.AllocateTexture(Texture2D texture)
        {
            return CommandBuffer.AllocateTexture(texture);
        }

        TextureID ILoaderActions.AllocateTexture(TextureData textureData)
        {
            return CommandBuffer.AllocateTexture(textureData);
        }

        void ILoaderActions.DisposeInstance(InstanceID instanceId)
        {
            CommandBuffer.DisposeInstance(instanceId);
        }

        void ILoaderActions.DisposeMaterial(MaterialID materialId)
        {
            CommandBuffer.DisposeMaterial(materialId);
        }

        void ILoaderActions.DisposeMesh(MeshID meshId)
        {
            CommandBuffer.DisposeMesh(meshId);
        }

        void ILoaderActions.DisposeTexture(TextureID textureId)
        {
            CommandBuffer.DisposeTexture(textureId);
        }

        private async Task<InstanceID> DifferedLoad(INodeContentLoader loader, QueueItem queueItem)
        {
            await Task.Yield();
            Assert.IsTrue(m_Hierarchy.HasNode(queueItem.NodeId));
            InstanceID id = await loader.LoadNodeAsync(queueItem.NodeId, queueItem.NodeContent);
            return id;
        }

        bool ILoaderActions.ExecuteSingle(UGCommandBuffer.IListener listener)
        {
            return CommandBuffer.ExecuteSingle(listener);
        }

        public void GetChildren(NodeId parent, List<NodeId> children)
        {
            m_Hierarchy.GetChildren(parent, children);
        }

        public NodeId GetRootNode()
        {
            return m_Hierarchy.RootNode;
        }

        public State GetState()
        {
            if (m_Queue.Count == 0)
                return State.Done;

            LinkedListNode<QueueItem> first = m_Queue.First;

            if (first.Value.Action == QueueItem.ItemAction.FinishLoading && first.Value.Promise != null && !first.Value.Promise.IsCompleted)
                return State.Waiting;

            return State.Processing;
        }

        public UGMetadata InitializeMetadata(NodeId nodeId)
        {
            return new UGMetadata(nodeId, m_Hierarchy);
        }

        private static bool IsAllowedToStartLoading(LinkedListNode<QueueItem> node)
        {
            QueueItem item = node.Value;
            Assert.AreEqual(QueueItem.ItemAction.LoadLater, item.Action);

            node = node.Previous;
            while (node != null)
            {
                if (node.Value.NodeId.Id == item.NodeId.Id)
                    return false;

                node = node.Previous;
            }
            return true;
        }

        public void Load(NodeId nodeId, NodeContent content)
        {
            if (TryCancelUnload(nodeId))
                return;

            LinkedListNode<QueueItem> current = m_Queue.AddLast(QueueItem.LoadLater(nodeId, content));

            if (IsAllowedToStartLoading(current))
                StartLoadingItem(current);
        }

        public void ProcessNext()
        {
            if (m_Queue.Count <= 0)
                return;

            QueueItem current = m_Queue.First.Value;

            switch (current.Action)
            {
                case QueueItem.ItemAction.FinishLoading:
                    if (current.Promise is null)
                    {
                        m_Content.Add(current.NodeId, null);
                        m_Instances.Add(current.NodeId, InstanceID.Null);
                        m_Queue.RemoveFirst();
                    }
                    else if (current.Promise.IsCompleted)
                    {
                        m_Content.Add(current.NodeId, current.NodeContent);
                        m_Instances.Add(current.NodeId, current.Promise.Result);
                        m_Queue.RemoveFirst();
                        --LoadingCount;
                    }
                    break;

                case QueueItem.ItemAction.LoadLater:
                    StartLoading();
                    break;

                case QueueItem.ItemAction.Unload:
                    Assert.IsTrue(m_Hierarchy.HasNode(current.NodeId));
                    NodeContent content = m_Content[current.NodeId];
                    if (content != null)
                    {
                        INodeContentLoader loader = m_Loaders[content.Type];
                        loader.UnloadNode(current.NodeId);
                    }
                    m_Content.Remove(current.NodeId);
                    m_Instances.Remove(current.NodeId);
                    m_Queue.RemoveFirst();
                    UnloadingCount--;
                    break;

                case QueueItem.ItemAction.Update:
                    Assert.AreEqual(0, m_AtomicCommandBuffer.Count);
                    foreach (NodeId id in current.Visible)
                        UpdateInstanceVisibility(m_Instances[id], true);
                    foreach (NodeId id in current.Hidden)
                        UpdateInstanceVisibility(m_Instances[id], false);
                    CommandBuffer.QueueAtomic(m_AtomicCommandBuffer);
                    m_AtomicCommandBuffer.Clear();
                    m_Queue.RemoveFirst();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        void ILoaderActions.QueueAction(Action action)
        {
            CommandBuffer.QueueAction(action);
        }

        /// <inheritdoc cref="INodeContentManager.RegisterLoader(ContentType, INodeContentLoader)"/>
        public void RegisterLoader(ContentType contentType, INodeContentLoader contentLoader)
        {
            m_Loaders.Add(contentType, contentLoader);
        }

        /// <inheritdoc cref="INodeContentManager.ScheduleTask(Action)"/>
        public void ScheduleTask(Action task)
        {
            TaskManager.ScheduleTask(task);
        }

        void ILoaderActions.RemoveMaterialProperty(MaterialID materialId, MaterialProperty materialProperty)
        {
            CommandBuffer.RemoveMaterialProperty(materialId, materialProperty);
        }

        /// <inheritdoc cref="IEditHierarchyNodes.RemoveNode(NodeId)"/>
        public void RemoveNode(NodeId nodeId)
        {
            m_Hierarchy.RemoveNode(nodeId);
        }

        private void StartLoading()
        {
            LinkedListNode<QueueItem> current = m_Queue.First;

            while(current != null)
            {
                if (current.Value.Action == QueueItem.ItemAction.LoadLater && IsAllowedToStartLoading(current))
                    StartLoadingItem(current);

                current = current.Next;
            }
        }

        private void StartLoadingItem(LinkedListNode<QueueItem> node)
        {
            Assert.AreEqual(QueueItem.ItemAction.LoadLater, node.Value.Action);

            QueueItem current = node.Value;

            if (current.NodeContent != null)
            {
                ++LoadingCount;
                ContentType contentType = current.NodeContent.Type;
                INodeContentLoader loader = m_Loaders[contentType];

                Task<InstanceID> task = DifferedLoad(loader, current);
                node.Value = QueueItem.Load(current.NodeId, current.NodeContent, task);
            }
            else
            {
                node.Value = QueueItem.LoadWithoutContent(current.NodeId);
            }
        }

        private bool TryCancelUnload(NodeId nodeId)
        {
            LinkedListNode<QueueItem> current = m_Queue.Last;

            while(current != null)
            {
                if (current.Value.Action == QueueItem.ItemAction.Unload &&
                    current.Value.NodeId == nodeId)
                {
                    m_Queue.Remove(current);
                    return true;
                }


                current = current.Previous;
            }

            return false;
        }

        public void Unload(NodeId nodeId)
        {
            Assert.IsTrue(m_Hierarchy.HasNode(nodeId), "Cannot unload invalid node.");
            m_Queue.AddLast(QueueItem.Unload(nodeId));
            UnloadingCount++;
        }

        void ILoaderActions.UpdateInstanceVisibility(InstanceID instanceId, bool visibility)
        {
            m_AtomicCommandBuffer.UpdateInstanceVisibility(instanceId, visibility);
        }

        private void UpdateInstanceVisibility(InstanceID instanceId, bool visibility)
        {
            if (instanceId != InstanceID.Null)
                m_AtomicCommandBuffer.UpdateInstanceVisibility(instanceId, visibility);
        }

        /// <inheritdoc cref="IEditHierarchyNodes.UpdateNode(NodeId, NodeData)"/>
        public void UpdateNode(NodeId nodeId, NodeData data)
        {
            m_Hierarchy.UpdateNode(nodeId, data);
        }

        public void UpdateVisibility(IEnumerable<NodeId> visible, IEnumerable<NodeId> hidden)
        {
            m_Queue.AddLast(QueueItem.UpdateVisibility(visible, hidden));
        }
    }
}
