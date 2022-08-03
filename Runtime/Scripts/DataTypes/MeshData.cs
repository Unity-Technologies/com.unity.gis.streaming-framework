using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Use this <see langword="struct"/> when reading data that needs to be loaded
    /// as a <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>.
    /// This <see langword="struct"/> is used to pass a mesh information to the <see cref="UGCommandBuffer"/>
    /// to be converted to a Unity <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>.
    /// Unlike the <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">UnityEngine.Mesh</see> class,
    /// this struct can be populated off of the main thread.
    /// </summary>
    public struct MeshData
    {
        /// <summary>
        /// Gets the number of vertices in the <see cref="MeshData"/>.
        /// </summary>
        public int VertexCount;

        /// <summary>
        /// Information about each VertexAttribute of the <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see> vertices.
        /// </summary>
        public VertexAttributeDescriptor[] VertexAttributes;

        /// <summary>
        /// Gets raw data for a given vertex buffer stream format in the <see cref="MeshData"/>.
        /// </summary>
        public byte[] VertexData;

        /// <summary>
        /// Gets the number of index buffer in the <see cref="MeshData"/>.
        /// </summary>
        public int IndexCount;

        /// <summary>
        /// Gets the format of the index buffer data in the <see cref="MeshData"/>.
        /// </summary>
        public IndexFormat IndexFormat;

        /// <summary>
        /// Gets raw data from the index buffer of the <see cref="MeshData"/>.
        /// </summary>
        public byte[] IndexData;
    }

    /// <summary>
    /// Methods to help convert <see cref="MeshData"/> to a Unity <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>.
    /// </summary>
    public static class MeshDataUtil
    {
        /// <summary>
        /// Verify if the given <paramref name="meshData"/> has
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.Normal.html">normals</see> information.
        /// </summary>
        /// <param name="meshData">Check if this mesh data has
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.Normal.html">normals</see> information.</param>
        /// <returns>
        /// <see langword="true"/> if the given <paramref name="meshData"/> has normals data;
        /// <see langword="false"/> otherwise.
        /// </returns>
        private static bool MeshHasNormals(MeshData meshData)
        {
            for (int i = 0; i < meshData.VertexAttributes.Length; i++)
            {
                if (meshData.VertexAttributes[i].attribute == VertexAttribute.Normal)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Convert the given <paramref name="meshData"/> to a Unity <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>.
        /// </summary>
        /// <param name="name">Set the name of the object.</param>
        /// <param name="meshData">Data to be converted.</param>
        /// <returns>The newly created Unity <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>.</returns>
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
