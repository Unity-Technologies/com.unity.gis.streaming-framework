using System;
using System.Collections.Generic;

using UnityEngine;
using Unity.Geospatial.Streaming.UniversalDecoder;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.collections.stack">Stack</see> of
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.action">Actions</see> to be
    /// executed by a <see cref="NodeContentManager"/> and store the equivalent
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.action">Actions</see> allowing to revert
    /// the previously executed <see href="https://docs.microsoft.com/en-us/dotnet/api/system.action">Actions</see>.
    /// </summary>
    public class RevertingCommandStack
    {
        private readonly ILoaderActions m_LoaderActions;
        private readonly Stack<Action> m_RevertStack = new Stack<Action>();

        /// <summary>
        /// To be executed after <see cref="Revert"/> is called.
        /// </summary>
        public Action OnRevertComplete { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="loaderActions">Queue those actions to be executed.</param>
        public RevertingCommandStack(ILoaderActions loaderActions)
        {
            m_LoaderActions = loaderActions;
        }

        /// <summary>
        /// Cancel the executed the <see href="https://docs.microsoft.com/en-us/dotnet/api/system.action">Actions</see>
        /// by disposing the created objects.
        /// </summary>
        public void Revert()
        {
            while(m_RevertStack.Count > 0)
                m_RevertStack.Pop().Invoke();

            if (OnRevertComplete != null)
                m_LoaderActions.QueueAction(OnRevertComplete);
        }

        /// <summary>
        /// Register a texture to be assigned add to the revert stack the corresponding dispose action.
        /// </summary>
        /// <param name="texture">Unity texture to register.</param>
        /// <returns>A new <see cref="TextureID"/> instance linked to the given <paramref name="texture"/>.</returns>
        public TextureID AllocateTexture(Texture2D texture)
        {
            TextureID result = m_LoaderActions.AllocateTexture(texture);
            m_RevertStack.Push(() => m_LoaderActions.DisposeTexture(result));
            return result;
        }

        /// <summary>
        /// Register a texture to be assigned and to the revert stack the corresponding dispose action.
        /// </summary>
        /// <param name="textureData">Data to register and converted to Unity texture.</param>
        /// <returns>A new <see cref="TextureID"/> instance linked to the resulting texture.</returns>
        public TextureID AllocateTexture(TextureData textureData)
        {
            TextureID result = m_LoaderActions.AllocateTexture(textureData);
            m_RevertStack.Push(() => m_LoaderActions.DisposeTexture(result));
            return result;
        }

        /// <summary>
        /// Register a mesh to be instantiated and to the revert stack the corresponding dispose action.
        /// </summary>
        /// <param name="meshData">Data to register and converted to Unity mesh.</param>
        /// <returns>A new <see cref="MeshID"/> instance linked to the resulting mesh.</returns>
        public MeshID AllocateMesh(MeshData meshData)
        {
            MeshID result = m_LoaderActions.AllocateMesh(meshData);
            m_RevertStack.Push(() => m_LoaderActions.DisposeMesh(result));
            return result;
        }

        /// <summary>
        /// Register a mesh to be instantiated and to the revert stack the corresponding dispose action.
        /// </summary>
        /// <param name="mesh">Unity mesh to register.</param>
        /// <returns>A new <see cref="MeshID"/> instance linked to the given <paramref name="mesh"/>.</returns>
        public MeshID AllocateMesh(Mesh mesh)
        {
            MeshID result = m_LoaderActions.AllocateMesh(mesh);
            m_RevertStack.Push(() => m_LoaderActions.DisposeMesh(result));
            return result;
        }

        /// <summary>
        /// Register a new instance with all its related data (mesh, material, textures...) and to the revert stack
        /// the corresponding dispose action.
        /// </summary>
        /// <param name="instanceData">Data specifying what is related to the new instance.</param>
        /// <returns>A new <see cref="InstanceID"/> linked to the newly created instance.</returns>
        public InstanceID AllocateInstance(InstanceData instanceData)
        {
            InstanceID result = m_LoaderActions.AllocateInstance(instanceData);
            m_RevertStack.Push(() => m_LoaderActions.DisposeInstance(result));
            return result;
        }

        /// <summary>
        /// Register a material to be instantiated and to the revert stack the corresponding dispose action.
        /// </summary>
        /// <param name="materialType">Type of material to create.</param>
        /// <returns>A new <see cref="MaterialID"/> instance linked to the newly created material.</returns>
        public MaterialID AllocateMaterial(MaterialType materialType)
        {
            MaterialID result = m_LoaderActions.AllocateMaterial(materialType);
            m_RevertStack.Push(() => m_LoaderActions.DisposeMaterial(result));
            return result;
        }

        /// <summary>
        /// Register a new material property for the given <paramref name="materialId"/> and to the revert stack
        /// the corresponding dispose action.
        /// </summary>
        /// <param name="materialId">Material the property will be linked with.</param>
        /// <param name="materialProperty">Property to register.</param>
        public void AddMaterialProperty(MaterialID materialId, MaterialProperty materialProperty)
        {
            m_LoaderActions.AddMaterialProperty(materialId, materialProperty);
            m_RevertStack.Push(() => m_LoaderActions.RemoveMaterialProperty(materialId, materialProperty));
        }
    }
}
