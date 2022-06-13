using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.Geospatial.Streaming
{
    public struct MeshData
    {
        public int VertexCount;

        public VertexAttributeDescriptor[] VertexAttributes;

        public byte[] VertexData;

        public int IndexCount;

        public IndexFormat IndexFormat;

        public byte[] IndexData;
    }

    public static class MeshDataUtil
    {
        private static bool MeshHasNormals(MeshData meshData)
        {
            for (int i = 0; i < meshData.VertexAttributes.Length; i++)
            {
                if (meshData.VertexAttributes[i].attribute == VertexAttribute.Normal)
                    return true;
            }

            return false;
        }

        public static unsafe Mesh ToUnityMesh(string name, MeshData meshData)
        {
            var dataArray = Mesh.AllocateWritableMeshData(1);
            var data = dataArray[0];

            bool hasNormals = MeshHasNormals(meshData);

            //
            //  TODO - We need way more safety checks here!
            //
            data.SetVertexBufferParams(meshData.VertexCount, meshData.VertexAttributes);
            var vertexBuffer = data.GetVertexData<byte>();
            fixed (byte* src = meshData.VertexData)
                Buffer.MemoryCopy(src, vertexBuffer.GetUnsafePtr(), meshData.VertexData.Length, meshData.VertexData.Length);

            data.SetIndexBufferParams(meshData.IndexCount, meshData.IndexFormat);
            var indexBuffer = data.GetIndexData<byte>();
            fixed (byte* src = meshData.IndexData)
                Buffer.MemoryCopy(src, indexBuffer.GetUnsafePtr(), meshData.IndexData.Length, meshData.IndexData.Length);

            data.subMeshCount = 1;
            data.SetSubMesh(0, new SubMeshDescriptor(0, meshData.IndexCount));

            Mesh result = new Mesh();

            result.name = name;
            Mesh.ApplyAndDisposeWritableMeshData(dataArray, result);
            result.RecalculateBounds();

            if (!hasNormals)
                result.RecalculateNormals();

            return result;
        }

    }
}
