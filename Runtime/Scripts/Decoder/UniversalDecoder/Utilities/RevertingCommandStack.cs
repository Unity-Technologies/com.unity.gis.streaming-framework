using System;
using System.Collections.Generic;

using UnityEngine;
using Unity.Geospatial.Streaming.UniversalDecoder;

namespace Unity.Geospatial.Streaming
{
    public class RevertingCommandStack
    {
        private readonly ILoaderActions m_LoaderActions;
        private readonly Stack<Action> m_RevertStack = new Stack<Action>();

        public Action OnRevertComplete { get; set; }

        public RevertingCommandStack(ILoaderActions loaderActions)
        {
            m_LoaderActions = loaderActions;
        }

        public void Revert()
        {
            while(m_RevertStack.Count > 0)
                m_RevertStack.Pop().Invoke();

            if (OnRevertComplete != null)
                m_LoaderActions.QueueAction(OnRevertComplete);
        }

        public TextureID AllocateTexture(Texture2D texture)
        {
            TextureID result = m_LoaderActions.AllocateTexture(texture);
            m_RevertStack.Push(() => m_LoaderActions.DisposeTexture(result));
            return result;
        }

        public TextureID AllocateTexture(TextureData textureData)
        {
            TextureID result = m_LoaderActions.AllocateTexture(textureData);
            m_RevertStack.Push(() => m_LoaderActions.DisposeTexture(result));
            return result;
        }

        public MeshID AllocateMesh(MeshData meshData)
        {
            MeshID result = m_LoaderActions.AllocateMesh(meshData);
            m_RevertStack.Push(() => m_LoaderActions.DisposeMesh(result));
            return result;
        }

        public MeshID AllocateMesh(Mesh mesh)
        {
            MeshID result = m_LoaderActions.AllocateMesh(mesh);
            m_RevertStack.Push(() => m_LoaderActions.DisposeMesh(result));
            return result;
        }

        public InstanceID AllocateInstance(InstanceData instanceData)
        {
            InstanceID result = m_LoaderActions.AllocateInstance(instanceData);
            m_RevertStack.Push(() => m_LoaderActions.DisposeInstance(result));
            return result;
        }

        public MaterialID AllocateMaterial(MaterialType type)
        {
            MaterialID result = m_LoaderActions.AllocateMaterial(type);
            m_RevertStack.Push(() => m_LoaderActions.DisposeMaterial(result));
            return result;
        }

        public void AddMaterialProperty(MaterialID materialId, MaterialProperty materialProperty)
        {
            m_LoaderActions.AddMaterialProperty(materialId, materialProperty);
            m_RevertStack.Push(() => m_LoaderActions.RemoveMaterialProperty(materialId, materialProperty));
        }
    }
}
