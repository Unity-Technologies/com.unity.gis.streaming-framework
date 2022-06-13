using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    public class UGCommandBufferProcessor : UGCommandBuffer.IListener
    {
        public UGCommandBufferProcessor(UGMaterialFactory materialFactory, UGCommandBuffer commandBuffer, UGProcessingNode.NodeOutput<InstanceCommand> outputNode)
        {
            m_OutputNode = outputNode;

            //
            //  TODO - Material factory should probably not actually be in the command processor. Review this
            //
            m_MaterialFactory = materialFactory;
            m_CommandBuffer = commandBuffer;
        }

        private readonly UGMaterialFactory m_MaterialFactory;

        private readonly UGProcessingNode.NodeOutput<InstanceCommand> m_OutputNode;
        
        private readonly UGCommandBuffer m_CommandBuffer;

        private readonly Dictionary<TextureID, Texture2D> m_Textures = new Dictionary<TextureID, Texture2D>();

        private readonly Dictionary<MeshID, Mesh> m_Meshes = new Dictionary<MeshID, Mesh>();
        
        private readonly Dictionary<MaterialID, UGMaterial> m_Materials = new Dictionary<MaterialID, UGMaterial>();

        private readonly Queue<InstanceData> m_InstanceProcessingQueue = new Queue<InstanceData>();

        public bool TryExecuteSingle()
        {
            if (m_OutputNode.IsReadyForData)
                return m_CommandBuffer.ExecuteSingle(this);

            return false;
        }

        public bool IsComplete()
        {
            return m_CommandBuffer.Count == 0;
        }

        public void AllocateTexture(TextureID textureID, Texture2D texture)
        {
            m_Textures.Add(textureID, texture);
        }

        void UGCommandBuffer.IListener.AllocateTexture(TextureID textureId, TextureData textureData)
        {
            Texture2D result = new Texture2D(textureData.width, textureData.height, TextureFormat.RGBA32, false);
            result.LoadRawTextureData(textureData.data);
            result.Apply();

            result.name = textureId.ToString();

            m_Textures.Add(textureId, result);
        }

        void UGCommandBuffer.IListener.DisposeTexture(TextureID textureId)
        {
            //
            //  FIXME - Don't dispose of the texture if it was preallocated
            //
            if (m_Textures.TryGetValue(textureId, out Texture2D texture))
            {
                m_Textures.Remove(textureId);
                UnityEngine.Object.Destroy(texture);
            }
            else
            {
                Debug.LogError("Failed to dispose of texture");
            }

        }

        void UGCommandBuffer.IListener.AllocateMesh(MeshID meshId, MeshData meshData)
        {
            Mesh mesh = MeshDataUtil.ToUnityMesh(meshId.ToString(), meshData);

            m_Meshes.Add(meshId, mesh);
        }

        public void AllocateMesh(MeshID meshId, Mesh mesh)
        {
            m_Meshes.Add(meshId, mesh);
        }

        void UGCommandBuffer.IListener.DisposeMesh(MeshID meshId)
        {
            if (m_Meshes.TryGetValue(meshId, out Mesh mesh))
            {
                //
                //  FIXME - Don't dispose mesh if it was preallocated
                //
                m_Meshes.Remove(meshId);
                UnityEngine.Object.Destroy(mesh);
            }
            else
            {
                Debug.LogError("Failed to dispose of mesh");
            }

        }

        void UGCommandBuffer.IListener.AllocateInstance(InstanceID instanceId, InstanceData instanceData)
        {
            Assert.IsTrue(m_OutputNode.IsReadyForData);

            m_InstanceProcessingQueue.Enqueue(instanceData);
            while (m_InstanceProcessingQueue.Count > 0)
            {
                InstanceData current = m_InstanceProcessingQueue.Dequeue();
                ConvertToRenderable(current);

                List<InstanceData> children = current.Children;

                if (children == null)
                    continue;

                foreach (InstanceData child in children)
                    m_InstanceProcessingQueue.Enqueue(child);
            }

            InstanceCommand data = InstanceCommand.Allocate(instanceId, instanceData);

            m_OutputNode.ProcessData(ref data);
        }

        private void ConvertToRenderable(InstanceData instanceData)
        {
            if (instanceData.MeshID == MeshID.Null)
                return;

            if (instanceData.MaterialIDs == null)
                return;

            UGMaterial[] materials = new UGMaterial[instanceData.MaterialIDs.Length];
            for (int i = 0; i < instanceData.MaterialIDs.Length; i++)
                materials[i] = m_Materials[instanceData.MaterialIDs[i]];

            instanceData.ConvertToRenderable(m_Meshes[instanceData.MeshID], materials);
        }

        void UGCommandBuffer.IListener.DisposeInstance(InstanceID instanceId)
        {
            InstanceCommand data = InstanceCommand.Dispose(instanceId);
            m_OutputNode.ProcessData(ref data);
        }

        void UGCommandBuffer.IListener.UpdateInstanceVisibility(InstanceID instanceId, bool visibility)
        {
            var data = InstanceCommand.UpdateVisibility(instanceId, visibility);
            m_OutputNode.ProcessData(ref data);
        }
        
        public void BeginAtomic()
        {
            var data = InstanceCommand.BeginAtomic();
            m_OutputNode.ProcessData(ref data);
        }

        public void EndAtomic()
        {
            var data = InstanceCommand.EndAtomic();
            m_OutputNode.ProcessData(ref data);
        }

        void UGCommandBuffer.IListener.AllocateMaterial(MaterialID materialId, MaterialType materialType)
        {
            UGMaterial material = m_MaterialFactory.InstantiateMaterial(materialType);

            m_Materials.Add(materialId, material);
        }

        void UGCommandBuffer.IListener.DisposeMaterial(MaterialID materialId)
        {
            if (m_Materials.TryGetValue(materialId, out UGMaterial material))
            {
                m_Materials.Remove(materialId);
                material.Dispose();
            }

        }

        void UGCommandBuffer.IListener.AddMaterialProperty(MaterialID materialId, MaterialProperty materialProperty)
        {
            TryLoadComponentTexture(ref materialProperty);
            if (m_Materials.TryGetValue(materialId, out UGMaterial material))
                material.AddMaterialProperty(materialProperty);
        }

        void UGCommandBuffer.IListener.RemoveMaterialProperty(MaterialID materialId, MaterialProperty materialProperty)
        {
            TryLoadComponentTexture(ref materialProperty);
            if (m_Materials.TryGetValue(materialId, out UGMaterial material))
                material.RemoveMaterialProperty(materialProperty);
        }

        private void TryLoadComponentTexture(ref MaterialProperty property)
        {
            if (property.TextureId != TextureID.Null)
                property.Texture = m_Textures[property.TextureId];
        }
    }
}
