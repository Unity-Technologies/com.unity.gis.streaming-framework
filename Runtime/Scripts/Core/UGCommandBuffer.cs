using System;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Queue of commands to be executed by a <see cref="IListener"/>.
    /// This allow to defer and batch the loading.
    /// </summary>
    public class UGCommandBuffer
    {
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="idGenerator">UUID Generator used to generate session unique identifiers per created instances.</param>
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

        /// <summary>
        /// Remove all pending commands.
        /// Executing this will cancel all upcoming commands.
        /// </summary>
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

        /// <summary>
        /// Request a <see href="https://docs.unity3d.com/ScriptReference/Texture2D.html">Texture2D</see> to be loaded.
        /// </summary>
        /// <param name="texture">Texture to be loaded.</param>
        /// <returns>Id of the texture allowing to refer to the command / loaded texture.</returns>
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
        
        /// <summary>
        /// Request a <see cref="TextureData"/> to be loaded as a <see href="https://docs.unity3d.com/ScriptReference/Texture2D.html">Texture2D</see>.
        /// </summary>
        /// <param name="textureData">Texture to be loaded.</param>
        /// <returns>Id of the texture allowing to refer to the command / loaded texture.</returns>
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

        /// <summary>
        /// Request to unload a <see href="https://docs.unity3d.com/ScriptReference/Texture2D.html">Texture2D</see>.
        /// </summary>
        /// <param name="textureId">Id of the texture to dispose of.</param>
        public void DisposeTexture(TextureID textureId)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.DisposeTexture);
                m_Textures.Enqueue(textureId);
            }
        }

        /// <summary>
        /// Request a <see cref="MeshData"/> to be loaded as a <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>.
        /// </summary>
        /// <param name="meshData">Mesh to be loaded.</param>
        /// <returns>Id of the meh allowing to refer to the command / loaded mesh.</returns>
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

        /// <summary>
        /// Request a <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see> to be loaded.
        /// </summary>
        /// <param name="mesh">Mesh to be loaded.</param>
        /// <returns>Id of the mesh allowing to refer to the command / loaded mesh.</returns>
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

        /// <summary>
        /// Request to unload a <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>.
        /// </summary>
        /// <param name="meshId">Id of the mesh to dispose of.</param>
        public void DisposeMesh(MeshID meshId)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.DisposeMesh);
                m_Meshes.Enqueue(meshId);
            }
        }

        /// <summary>
        /// Request to create an instance by linking a <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>,
        /// a <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see> and
        /// a <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see> together
        /// </summary>
        /// <param name="instanceData">Information of what to link together.</param>
        /// <returns>Id of the instance allowing to refer to the command / loaded instance.</returns>
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

        /// <summary>
        /// Request to unload an instance by unlinking its <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>,
        /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see> and
        /// <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see>.
        /// </summary>
        /// <param name="instanceId">Id of the instance to dispose of.</param>
        public void DisposeInstance(InstanceID instanceId)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.DisposeInstance);
                m_Instances.Enqueue(instanceId);
            }
        }

        /// <summary>
        /// Request to change the visibility state for the given <paramref name="instanceId">instance</paramref>.
        /// </summary>
        /// <param name="instanceId">Instance to change its visibility state.</param>
        /// <param name="visibility">
        /// <see langword="true"/> to set to display the instance;
        /// <see langword="false"/> to hide it.
        /// </param>
        public void UpdateInstanceVisibility(InstanceID instanceId, bool visibility)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.UpdateInstanceVisibility);
                m_Instances.Enqueue(instanceId);
                m_Visibility.Enqueue(visibility);
            }
        }

        /// <summary>
        /// Request a <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see> to be loaded.
        /// </summary>
        /// <param name="type">Type of the material be instantiate.</param>
        /// <returns>Id of the material allowing to refer to the command / loaded material.</returns>
        public MaterialID AllocateMaterial(MaterialType type)
        {
            lock (m_Lock)
            {
                MaterialID id = m_IDGenerator.GetMaterialID();

                m_Commands.Enqueue(Command.AllocateMaterial);
                m_MaterialType.Enqueue(type);
                m_Materials.Enqueue(id);

                return id;
            }
        }

        /// <summary>
        /// Request to unload a <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see>.
        /// </summary>
        /// <param name="materialId">Id of the material to dispose of.</param>
        public void DisposeMaterial(MaterialID materialId)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.DisposeMaterial);
                m_Materials.Enqueue(materialId);
            }
        }

        /// <summary>
        /// Request to create a <see cref="MaterialProperty"/> for the given <paramref name="materialId">material</paramref>.
        /// </summary>
        /// <param name="materialId">Material to modify</param>
        /// <param name="materialProperty">Property to add to the given <paramref name="materialId">material</paramref>.</param>
        public void AddMaterialProperty(MaterialID materialId, MaterialProperty materialProperty)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.AddMaterialProperty);
                m_Materials.Enqueue(materialId);
                m_MaterialProperty.Enqueue(materialProperty);
            }
        }

        /// <summary>
        /// Request to remove a <see cref="MaterialProperty"/> from the given <paramref name="materialId">material</paramref>.
        /// </summary>
        /// <param name="materialId">Material to modify</param>
        /// <param name="materialProperty">Property to remove from the given <paramref name="materialId">material</paramref>.</param>
        public void RemoveMaterialProperty(MaterialID materialId, MaterialProperty materialProperty)
        {
            lock (m_Lock)
            {
                m_Commands.Enqueue(Command.RemoveMaterialProperty);
                m_Materials.Enqueue(materialId);
                m_MaterialProperty.Enqueue(materialProperty);
            }
        }

        /// <summary>
        /// Request a custom <paramref name="action"/> to be executed.
        /// </summary>
        /// <param name="action">Action to append to the queue.</param>
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

        /// <summary>
        /// Amount of <see cref="Command"/> waiting to be executed.
        /// </summary>
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
        
        /// <summary>
        /// Move the commands from one <see cref="UGCommandBuffer"/> queue to this instance queue and flag them as a block.
        /// </summary>
        /// <param name="cb">Buffer to transfer and flag to be execute together.</param>
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

        /// <summary>
        /// Execute the next command which is at the top of the queue.
        /// </summary>
        /// <param name="listener">Use this listener to execute the command.</param>
        /// <returns>
        /// <see langword="true"/> if a command was executed;
        /// <see langword="false"/> if the command queue is empty.
        /// </returns>
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
        
        /// <summary>
        /// Expected interface when executing <see cref="UGCommandBuffer.ExecuteSingle"/>.
        /// </summary>
        public interface IListener
        {
            /// <summary>
            /// Load a <see href="https://docs.unity3d.com/ScriptReference/Texture2D.html">Texture2D</see>.
            /// </summary>
            /// <param name="textureId">Id of the texture allowing to refer to the command / loaded texture.</param>
            /// <param name="texture">Texture to be loaded.</param>
            void AllocateTexture(TextureID textureId, Texture2D texture);

            /// <summary>
            /// Load a <see cref="TextureData"/> as a <see href="https://docs.unity3d.com/ScriptReference/Texture2D.html">Texture2D</see>.
            /// </summary>
            /// <param name="textureId">Id of the texture allowing to refer to the command / loaded texture.</param>
            /// <param name="textureData">Texture to be loaded.</param>
            void AllocateTexture(TextureID textureId, TextureData textureData);
            
            /// <summary>
            /// Unload a <see href="https://docs.unity3d.com/ScriptReference/Texture2D.html">Texture2D</see>.
            /// </summary>
            /// <param name="textureId">Id of the texture to dispose of.</param>
            void DisposeTexture(TextureID textureId);

            /// <summary>
            /// Load a <see cref="MeshData"/> as a <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>.
            /// </summary>
            /// <param name="meshId">Id of the meh allowing to refer to the command / loaded mesh.</param>
            /// <param name="meshData">Mesh to be loaded.</param>
            void AllocateMesh(MeshID meshId, MeshData meshData);

            /// <summary>
            /// Load a <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>.
            /// </summary>
            /// <param name="meshId">Id of the mesh allowing to refer to the command / loaded mesh.</param>
            /// <param name="mesh">Mesh to be loaded.</param>
            void AllocateMesh(MeshID meshId, Mesh mesh);
            
            /// <summary>
            /// Unload a <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>.
            /// </summary>
            /// <param name="meshId">Id of the mesh to dispose of.</param>
            void DisposeMesh(MeshID meshId);

            /// <summary>
            /// Create an instance by linking a <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>,
            /// a <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see> and
            /// a <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see> together
            /// </summary>
            /// <param name="instanceId">Id of the instance allowing to refer to the command / loaded instance.</param>
            /// <param name="instanceData">Information of what to link together.</param>
            void AllocateInstance(InstanceID instanceId, InstanceData instanceData);
            
            /// <summary>
            /// Unload an instance by unlinking its <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>,
            /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see> and
            /// <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see>.
            /// </summary>
            /// <param name="instanceId">Id of the instance to dispose of.</param>
            void DisposeInstance(InstanceID instanceId);
            
            /// <summary>
            /// Change the visibility state for the given <paramref name="instanceId">instance</paramref>.
            /// </summary>
            /// <param name="instanceId">Instance to change its visibility state.</param>
            /// <param name="visibility">
            /// <see langword="true"/> to set to display the instance;
            /// <see langword="false"/> to hide it.
            /// </param>
            void UpdateInstanceVisibility(InstanceID instanceId, bool visibility);

            /// <summary>
            /// Create a <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see>.
            /// </summary>
            /// <param name="materialId">Id of the material allowing to refer to the command / loaded material.</param>
            /// <param name="materialType">Type of the material be instantiate.</param>
            void AllocateMaterial(MaterialID materialId, MaterialType materialType);
            
            /// <summary>
            /// Unload a <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see>.
            /// </summary>
            /// <param name="materialId">Id of the material to dispose of.</param>
            void DisposeMaterial(MaterialID materialId);
            
            /// <summary>
            /// Create a <see cref="MaterialProperty"/> for the given <paramref name="materialId">material</paramref>.
            /// </summary>
            /// <param name="materialId">Material to modify</param>
            /// <param name="materialProperty">Property to add to the given <paramref name="materialId">material</paramref>.</param>
            void AddMaterialProperty(MaterialID materialId, MaterialProperty materialProperty);
            
            /// <summary>
            /// Remove a <see cref="MaterialProperty"/> from the given <paramref name="materialId">material</paramref>.
            /// </summary>
            /// <param name="materialId">Material to modify</param>
            /// <param name="materialProperty">Property to remove from the given <paramref name="materialId">material</paramref>.</param>
            void RemoveMaterialProperty(MaterialID materialId, MaterialProperty materialProperty);

            /// <summary>
            /// Specify a new block of commands will be requested.
            /// </summary>
            void BeginAtomic();
            
            /// <summary>
            /// Specify the last block of commands have been completed.
            /// </summary>
            void EndAtomic();
        }
    }
}
