using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    public class UGCommandBuffer
    {
        public UGCommandBuffer(UUIDGenerator idGenerator)
        {
            m_IDGenerator = idGenerator;
        }

        private readonly object m_Lock = new object();

        private readonly UUIDGenerator m_IDGenerator;

        private enum Command
        {
            AllocateTextureFromUnityTexture,
            AllocateTextureFromData,
            DisposeTexture,

            AllocateMeshFromUnityMesh,
            AllocateMeshFromData,
            DisposeMesh,

            AllocateInstance,
            DisposeInstance,
            UpdateInstanceVisibility,

            AllocateMaterial,
            DisposeMaterial,
            AddMaterialProperty,
            RemoveMaterialProperty,

            BeginAtomic,
            EndAtomic,

            CustomAction,
        }

        private readonly Queue<Command> m_Commands = new Queue<Command>();

        private readonly Queue<Action> m_CustomActions = new Queue<Action>();

        private readonly Queue<TextureData> m_TextureData = new Queue<TextureData>();
        private readonly Queue<Texture2D> m_UnityTexture = new Queue<Texture2D>();
        private readonly Queue<Mesh> m_UnityMesh = new Queue<Mesh>();
        private readonly Queue<MeshData> m_MeshData = new Queue<MeshData>();
        private readonly Queue<InstanceData> m_InstanceData = new Queue<InstanceData>();
        private readonly Queue<MaterialType> m_MaterialType = new Queue<MaterialType>();
        private readonly Queue<MaterialProperty> m_MaterialProperty = new Queue<MaterialProperty>();
        private readonly Queue<bool> m_Visibility = new Queue<bool>();

        private readonly Queue<TextureID> m_Textures = new Queue<TextureID>();
        private readonly Queue<MaterialID> m_Materials = new Queue<MaterialID>();
        private readonly Queue<MeshID> m_Meshes = new Queue<MeshID>();
        private readonly Queue<InstanceID> m_Instances = new Queue<InstanceID>();

        public void Clear()
        {
            m_Commands.Clear();

            m_UnityTexture.Clear();
            m_TextureData.Clear();
            m_Meshes.Clear();
            m_MeshData.Clear();
            m_InstanceData.Clear();
            m_MaterialType.Clear();
            m_MaterialProperty.Clear();
            m_Visibility.Clear();

            m_Textures.Clear();
            m_Materials.Clear();
            m_Meshes.Clear();
            m_Instances.Clear();
        }

        public TextureID AllocateTexture(Texture2D texture)
        {
            lock(m_Lock)
            {
                TextureID id = m_IDGenerator.GetTextureID();

                m_Commands.Enqueue(Command.AllocateTextureFromUnityTexture);
                m_Textures.Enqueue(id);
                m_UnityTexture.Enqueue(texture);

                return id;
            }
        }

        public TextureID AllocateTexture(TextureData textureData)
        {
            lock (m_Lock)
            {
                TextureID id = m_IDGenerator.GetTextureID();

                m_Commands.Enqueue(Command.AllocateTextureFromData);
                m_Textures.Enqueue(id);
                m_TextureData.Enqueue(textureData);

                return id;
            }
        }

        public void DisposeTexture(TextureID textureId)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.DisposeTexture);
                m_Textures.Enqueue(textureId);
            }
        }

        public MeshID AllocateMesh(MeshData meshData)
        {
            lock (m_Lock)
            {
                MeshID id = m_IDGenerator.GetMeshID();

                m_Commands.Enqueue(Command.AllocateMeshFromData);
                m_Meshes.Enqueue(id);
                m_MeshData.Enqueue(meshData);

                return id;
            }
        }

        public MeshID AllocateMesh(Mesh mesh)
        {
            lock (m_Lock)
            {
                MeshID id = m_IDGenerator.GetMeshID();

                m_Commands.Enqueue(Command.AllocateMeshFromUnityMesh);
                m_Meshes.Enqueue(id);
                m_UnityMesh.Enqueue(mesh);

                return id;
            }
        }

        public void DisposeMesh(MeshID meshId)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.DisposeMesh);
                m_Meshes.Enqueue(meshId);
            }
        }

        public InstanceID AllocateInstance(InstanceData instanceData)
        {
            lock (m_Lock)
            {
                InstanceID id = m_IDGenerator.GetInstanceID();

                m_Commands.Enqueue(Command.AllocateInstance);
                m_Instances.Enqueue(id);
                m_InstanceData.Enqueue(instanceData);

                return id;
            }
        }

        public void DisposeInstance(InstanceID instanceId)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.DisposeInstance);
                m_Instances.Enqueue(instanceId);
            }
        }

        public void UpdateInstanceVisibility(InstanceID instanceId, bool visibility)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.UpdateInstanceVisibility);
                m_Instances.Enqueue(instanceId);
                m_Visibility.Enqueue(visibility);
            }
        }

        public MaterialID AllocateMaterial(MaterialType materialType)
        {
            lock (m_Lock)
            {
                MaterialID id = m_IDGenerator.GetMaterialID();

                m_Commands.Enqueue(Command.AllocateMaterial);
                m_MaterialType.Enqueue(materialType);
                m_Materials.Enqueue(id);

                return id;
            }
        }

        public void DisposeMaterial(MaterialID materialId)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.DisposeMaterial);
                m_Materials.Enqueue(materialId);
            }
        }

        public void AddMaterialProperty(MaterialID materialId, MaterialProperty materialProperty)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.AddMaterialProperty);
                m_Materials.Enqueue(materialId);
                m_MaterialProperty.Enqueue(materialProperty);
            }
        }

        public void RemoveMaterialProperty(MaterialID materialId, MaterialProperty materialProperty)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.RemoveMaterialProperty);
                m_Materials.Enqueue(materialId);
                m_MaterialProperty.Enqueue(materialProperty);
            }
        }

        public void QueueAction(Action action)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.CustomAction);
                m_CustomActions.Enqueue(action);
            }
        }

        private static void EnqueueAll<T>(Queue<T> dst, Queue<T> src)
        {
            while (src.Count > 0)
                dst.Enqueue(src.Dequeue());
        }

        public int Count
        {
            get
            {
                lock (m_Lock)
                {
                    return m_Commands.Count;
                }
            }
        }
        
        public void QueueAtomic(UGCommandBuffer cb)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.BeginAtomic);
                EnqueueAll(m_Commands, cb.m_Commands);
                m_Commands.Enqueue(Command.EndAtomic);

                EnqueueAll(m_CustomActions, cb.m_CustomActions);

                EnqueueAll(m_UnityTexture, cb.m_UnityTexture);
                EnqueueAll(m_TextureData, cb.m_TextureData);
                EnqueueAll(m_MeshData, cb.m_MeshData);
                EnqueueAll(m_UnityMesh, cb.m_UnityMesh);
                EnqueueAll(m_InstanceData, cb.m_InstanceData);
                EnqueueAll(m_MaterialType, cb.m_MaterialType);
                EnqueueAll(m_MaterialProperty, cb.m_MaterialProperty);
                EnqueueAll(m_Visibility, cb.m_Visibility);

                EnqueueAll(m_Textures, cb.m_Textures);
                EnqueueAll(m_Materials, cb.m_Materials);
                EnqueueAll(m_Meshes, cb.m_Meshes);
                EnqueueAll(m_Instances, cb.m_Instances);
            }
        }

        public bool ExecuteSingle(IListener listener)
        {
            if (m_Commands.Count == 0)
                return false;

            Monitor.Enter(m_Lock);
            if(m_Commands.Count > 0)
            {
                Command command = m_Commands.Dequeue();
                switch (command)
                {
                    case Command.BeginAtomic:
                        listener.BeginAtomic();
                        break;

                    case Command.EndAtomic:
                        listener.EndAtomic();
                        break;

                    case Command.AllocateInstance:
                        {
                            InstanceID id = m_Instances.Dequeue();
                            InstanceData data = m_InstanceData.Dequeue();

                            Monitor.Exit(m_Lock);
                            listener.AllocateInstance(id, data);
                            Monitor.Enter(m_Lock);
                        }
                        break;

                    case Command.DisposeInstance:
                        {
                            InstanceID id = m_Instances.Dequeue();

                            Monitor.Exit(m_Lock);
                            listener.DisposeInstance(id);
                            Monitor.Enter(m_Lock);
                        }
                        break;

                    case Command.UpdateInstanceVisibility:
                        {
                            InstanceID id = m_Instances.Dequeue();
                            bool visible = m_Visibility.Dequeue();

                            Monitor.Exit(m_Lock);
                            listener.UpdateInstanceVisibility(id, visible);
                            Monitor.Enter(m_Lock);
                            
                        }
                        break;

                    case Command.AllocateMeshFromData:
                        {
                            MeshID id = m_Meshes.Dequeue();
                            MeshData data = m_MeshData.Dequeue();

                            Monitor.Exit(m_Lock);
                            listener.AllocateMesh(id, data);
                            Monitor.Enter(m_Lock);
                        }
                        break;

                    case Command.AllocateMeshFromUnityMesh:
                        {
                            MeshID id = m_Meshes.Dequeue();
                            Mesh mesh = m_UnityMesh.Dequeue();

                            Monitor.Exit(m_Lock);
                            listener.AllocateMesh(id, mesh);
                            Monitor.Enter(m_Lock);
                        }
                        break;

                    case Command.DisposeMesh:
                        {
                            MeshID id = m_Meshes.Dequeue();

                            Monitor.Exit(m_Lock);
                            listener.DisposeMesh(id);
                            Monitor.Enter(m_Lock);
                        }
                        break;

                    case Command.AllocateTextureFromData:
                        {
                            TextureID id = m_Textures.Dequeue();
                            TextureData data = m_TextureData.Dequeue();

                            Monitor.Exit(m_Lock);
                            listener.AllocateTexture(id, data);
                            Monitor.Enter(m_Lock);
                        }
                        break;

                    case Command.AllocateTextureFromUnityTexture:
                        {
                            TextureID id = m_Textures.Dequeue();
                            Texture2D texture = m_UnityTexture.Dequeue();

                            Monitor.Exit(m_Lock);
                            listener.AllocateTexture(id, texture);
                            Monitor.Enter(m_Lock);
                        }
                        break;

                    case Command.DisposeTexture:
                        {
                            TextureID id = m_Textures.Dequeue();

                            Monitor.Exit(m_Lock);
                            listener.DisposeTexture(id);
                            Monitor.Enter(m_Lock);
                        }
                        break;

                    case Command.AllocateMaterial:
                        {
                            MaterialID id = m_Materials.Dequeue();
                            MaterialType materialType = m_MaterialType.Dequeue();

                            Monitor.Exit(m_Lock);
                            listener.AllocateMaterial(id, materialType);
                            Monitor.Enter(m_Lock);
                        }
                        break;

                    case Command.DisposeMaterial:
                        {
                            MaterialID id = m_Materials.Dequeue();

                            Monitor.Exit(m_Lock);
                            listener.DisposeMaterial(id);
                            Monitor.Enter(m_Lock);
                        }
                        break;

                    case Command.AddMaterialProperty:
                        {
                            MaterialID id = m_Materials.Dequeue();
                            MaterialProperty property = m_MaterialProperty.Dequeue();

                            Monitor.Exit(m_Lock);
                            listener.AddMaterialProperty(id, property);
                            Monitor.Enter(m_Lock);
                        }
                        break;

                    case Command.RemoveMaterialProperty:
                        {
                            MaterialID id = m_Materials.Dequeue();
                            MaterialProperty property = m_MaterialProperty.Dequeue();

                            Monitor.Exit(m_Lock);
                            listener.RemoveMaterialProperty(id, property);
                            Monitor.Enter(m_Lock);
                        }
                        break;

                    case Command.CustomAction:
                        {
                            Action action = m_CustomActions.Dequeue();

                            Monitor.Exit(m_Lock);
                            action.Invoke();
                            Monitor.Enter(m_Lock);
                        }
                        break;

                    default:
                        Debug.LogErrorFormat("Missing Command {0} in switch case", command);
                        break;
                }
            }
            
            Monitor.Exit(m_Lock);

            return true;
        }
        
        public interface IListener
        {
            void AllocateTexture(TextureID textureID, Texture2D texture);
            void AllocateTexture(TextureID textureID, TextureData textureData);
            void DisposeTexture(TextureID textureId);

            void AllocateMesh(MeshID meshId, MeshData meshData);
            void AllocateMesh(MeshID meshId, Mesh mesh);
            void DisposeMesh(MeshID meshId);

            void AllocateInstance(InstanceID instanceId, InstanceData instanceData);
            void DisposeInstance(InstanceID instanceId);
            void UpdateInstanceVisibility(InstanceID instanceId, bool visibility);

            void AllocateMaterial(MaterialID materialId, MaterialType materialType);
            void DisposeMaterial(MaterialID materialId);
            void AddMaterialProperty(MaterialID materialId, MaterialProperty materialProperty);
            void RemoveMaterialProperty(MaterialID materialId, MaterialProperty materialProperty);

            void BeginAtomic();
            void EndAtomic();
        }
    }
}
