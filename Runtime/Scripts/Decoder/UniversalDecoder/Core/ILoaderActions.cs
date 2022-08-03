
using System;

using UnityEngine;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Commands allowed to be executed when loading / unloading data.
    /// </summary>
    public interface ILoaderActions
    {
        /// <summary>
        /// Register a new material property for the given <paramref name="materialId"/>.
        /// </summary>
        /// <param name="materialId">Material the property will be linked with.</param>
        /// <param name="materialProperty">Property to register.</param>
        void AddMaterialProperty(MaterialID materialId, MaterialProperty materialProperty);

        /// <inheritdoc cref="IEditHierarchyNodes.AddNode(NodeId, NodeData, NodeContent)"/>
        NodeId AddNode(NodeId parent, in NodeData data, in NodeContent content);

        /// <summary>
        /// Register a new instance with all its related data (mesh, material, textures...).
        /// </summary>
        /// <param name="instanceData">Data specifying what is related to the new instance.</param>
        /// <returns>A new <see cref="InstanceID"/> linked to the newly created instance.</returns>
        InstanceID AllocateInstance(InstanceData instanceData);

        /// <summary>
        /// Register a material to be instantiated.
        /// </summary>
        /// <param name="type">Type of material to create.</param>
        /// <returns>A new <see cref="MaterialID"/> instance linked to the newly created material.</returns>
        MaterialID AllocateMaterial(MaterialType type);

        /// <summary>
        /// Register a mesh to be instantiated.
        /// </summary>
        /// <param name="mesh">Unity mesh to register.</param>
        /// <returns>A new <see cref="MeshID"/> instance linked to the given <paramref name="mesh"/>.</returns>
        MeshID AllocateMesh(Mesh mesh);

        /// <summary>
        /// Register a mesh to be instantiated.
        /// </summary>
        /// <param name="meshData">Data to register and converted to Unity mesh.</param>
        /// <returns>A new <see cref="MeshID"/> instance linked to the resulting mesh.</returns>
        MeshID AllocateMesh(MeshData meshData);

        /// <summary>
        /// Register a texture to be assigned.
        /// </summary>
        /// <param name="texture">Unity texture to register.</param>
        /// <returns>A new <see cref="TextureID"/> instance linked to the given <paramref name="texture"/>.</returns>
        TextureID AllocateTexture(Texture2D texture);

        /// <summary>
        /// Register a texture to be assigned.
        /// </summary>
        /// <param name="textureData">Data to register and converted to Unity texture.</param>
        /// <returns>A new <see cref="TextureID"/> instance linked to the resulting texture.</returns>
        TextureID AllocateTexture(TextureData textureData);

        /// <summary>
        /// Consider an instance to be disposed.
        /// </summary>
        /// <param name="instanceId">Remove the registration of this instance.</param>
        void DisposeInstance(InstanceID instanceId);

        /// <summary>
        /// Consider a material to be disposed.
        /// </summary>
        /// <param name="materialId">Remove the registration of this material.</param>
        void DisposeMaterial(MaterialID materialId);

        /// <summary>
        /// Consider a mesh to be disposed.
        /// </summary>
        /// <param name="meshId">Remove the registration of this mesh.</param>
        void DisposeMesh(MeshID meshId);

        /// <summary>
        /// Consider a texture to be disposed.
        /// </summary>
        /// <param name="textureId">Remove the registration of this texture.</param>
        void DisposeTexture(TextureID textureId);

        /// <summary>
        /// Execute the next item part of the queue.
        /// </summary>
        /// <param name="listener">Send the next queue item to this listener.</param>
        /// <returns>
        /// <see langword="true"/> if the command was successfully sent;
        /// <see langword="false"/> otherwise.
        /// </returns>
        bool ExecuteSingle(UGCommandBuffer.IListener listener);

        /// <summary>
        /// Create a new <see cref="UGMetadata"/> instance for the given <paramref name="nodeId"/>.
        /// </summary>
        /// <param name="nodeId">The ID of the node to create the metadata for.</param>
        /// <returns>A new <see cref="UGMetadata"/> instance.</returns>
        public UGMetadata InitializeMetadata(NodeId nodeId);

        /// <summary>
        /// Add a new item to the queue.
        /// </summary>
        /// <param name="action">Custom action to be executed by <see cref="ExecuteSingle"/>.</param>
        void QueueAction(Action action);

        /// <summary>
        /// Consider a material property to be disposed.
        /// </summary>
        /// <param name="materialId">Material the property is linked to.</param>
        /// <param name="materialProperty">Property to unregister.</param>
        void RemoveMaterialProperty(MaterialID materialId, MaterialProperty materialProperty);

        /// <inheritdoc cref="IEditHierarchyNodes.RemoveNode(NodeId)"/>
        void RemoveNode(NodeId nodeId);

        /// <summary>
        /// Change the visibility state of an instance.
        /// </summary>
        /// <param name="instanceId">Instance to change its visibility.</param>
        /// <param name="visibility">
        /// <see langword="true"/> to set visible the instance;
        /// <see langword="false"/> to set hidden.
        /// </param>
        void UpdateInstanceVisibility(InstanceID instanceId, bool visibility);

        /// <inheritdoc cref="IEditHierarchyNodes.UpdateNode(NodeId, NodeData)"/>
        void UpdateNode(NodeId nodeId, NodeData data);
    }
}
