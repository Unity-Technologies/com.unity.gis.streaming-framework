using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Class allowing to interpolate blocks of vertices from a source to a goal with a blending ratio.
    /// </summary>
    public unsafe class VertexInterpolator
    {
        private delegate void InterpolationOperatorMethod(void* a, void* b, float t, void* dst);
        
        private struct InterpolationOperator
        {
            public short ByteSize;
            public InterpolationOperatorMethod Method;
        }

        //
        //  TODO - Implement all of these
        //
        private static readonly Dictionary<VertexAttributeFormat, InterpolationOperator> k_InterpolationOperators = new Dictionary<VertexAttributeFormat, InterpolationOperator>()
        {
            { 
                VertexAttributeFormat.Float32, new InterpolationOperator()
                {
                    ByteSize = 4,
                    Method = InterpolateFloat32
                }
            }
        };

        private readonly int[] m_VertexDataByteSize;
        private byte[][] m_VertexStreams;

        private readonly int m_PositionStream;
        private readonly int m_PositionOffset;

        private readonly List<InterpolationOperator>[] m_InterpolationOperation;

        /// <summary>
        /// Information about the <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.html">VertexAttributes</see>
        /// of a <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see> <see href="https://docs.unity3d.com/ScriptReference/Mesh-vertices.html">vertices</see>.
        /// <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see> vertices vertex data comprised of
        /// different Vertex Attributes. For example, a vertex can include a Position, Normal, TexCoord0, and Color.
        /// <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Meshes</see> usually use a known format for
        /// data layout, for example, a position is most often a 3-component float
        /// vector (<see href="https://docs.unity3d.com/ScriptReference/Vector3.html">Vector3</see>), but you can also
        /// specify non-standard data formats and their layout for a <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>.
        /// You can use <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttributeDescriptor.html">VertexAttributeDescriptor</see>
        /// to specify custom mesh data layout in <see href="https://docs.unity3d.com/ScriptReference/Mesh.SetVertexBufferParams.html">Mesh.SetVertexBufferParams</see>.
        /// Vertex data is laid out in separate "streams" (each stream goes into a separate vertex buffer in the underlying graphics API).
        /// While Unity supports up to 4 vertex streams, most meshes use just one. Separate streams are most useful when
        /// some vertex attributes don't need to be processed, for example skinned meshes often use two vertex streams
        /// (one containing all the skinned data: positions, normals, tangents;
        /// while the other stream contains all the non-skinned data: colors and texture coordinates).
        /// Within each stream, attributes of a vertex are laid out one after another, in this order:
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.Position.html">VertexAttribute.Position</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.Normal.html">VertexAttribute.Normal</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.Tangent.html">VertexAttribute.Tangent</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.Color.html">VertexAttribute.Color</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.TexCoord0.html">VertexAttribute.TexCoord0</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.TexCoord1.html">VertexAttribute.TexCoord1</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.TexCoord2.html">VertexAttribute.TexCoord2</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.TexCoord3.html">VertexAttribute.TexCoord3</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.TexCoord4.html">VertexAttribute.TexCoord4</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.TexCoord5.html">VertexAttribute.TexCoord5</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.TexCoord6.html">VertexAttribute.TexCoord6</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.TexCoord7.html">VertexAttribute.TexCoord7</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.BlendWeight.html">VertexAttribute.BlendWeight</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.BlendIndices.html">VertexAttribute.BlendIndices</see>
        /// Not all <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttributeDescriptor-format.html">format</see>
        /// and <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttributeDescriptor-dimension.html">dimension</see>
        /// combinations are valid. Specifically, the data size of a vertex attribute must be a multiple of 4 bytes. For example,
        /// a <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttributeFormat.Float16.html">VertexAttributeFormat.Float16</see>
        /// format with dimension 3 is not valid.
        ///
        /// See Also: <see href="https://docs.unity3d.com/ScriptReference/SystemInfo.SupportsVertexAttributeFormat.html">SystemInfo.SupportsVertexAttributeFormat</see>.
        /// </summary>
        public VertexAttributeDescriptor[] VertexAttributeDescriptors { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="vertexDescriptors">Attributes defining the vertices the new instance will interact with.</param>
        public VertexInterpolator(VertexAttributeDescriptor[] vertexDescriptors)
        {
            Assert.IsNotNull(vertexDescriptors);

            int currentStream = 0;
            int streamCount = 1 + vertexDescriptors.Max(attribute => attribute.stream);

            m_VertexDataByteSize = new int[streamCount];

            m_InterpolationOperation = new List<InterpolationOperator>[streamCount];
            for (int i = 0; i < streamCount; i++)
            {
                m_InterpolationOperation[i] = new List<InterpolationOperator>();
            }

            VertexAttributeDescriptors = vertexDescriptors;
            

            foreach (VertexAttributeDescriptor descriptor in vertexDescriptors)
            {
                Assert.IsTrue(descriptor.stream == currentStream || descriptor.stream == currentStream + 1, "Streams are expected to be sequential");

                currentStream = descriptor.stream;

                if (descriptor.attribute == VertexAttribute.Position)
                {
                    Assert.AreEqual(VertexAttributeFormat.Float32, descriptor.format, "Expecting vertex position to be encoded in float32 format");
                    Assert.AreEqual(3, descriptor.dimension, "Expecting vertex position to be encoded in three dimensions");

                    m_PositionStream = descriptor.stream;
                    m_PositionOffset = m_VertexDataByteSize[descriptor.stream];
                }

                InterpolationOperator interpolationOperator = k_InterpolationOperators[descriptor.format];

                m_VertexDataByteSize[descriptor.stream] += interpolationOperator.ByteSize * descriptor.dimension;

                for (int i = 0; i < descriptor.dimension; i++)
                {
                    m_InterpolationOperation[currentStream].Add(interpolationOperator);
                }

                
            }
        }

        /// <summary>
        /// Vertices streams with the source / destination / result of the interpolation process.
        /// </summary>
        /// <param name="vertexStreams">Blocks of vertices data.</param>
        public void SetVertexBuffers(byte[][] vertexStreams)
        {
            m_VertexStreams = vertexStreams;
        }

        /// <summary>
        /// Get the amount vertex buffer streams part of this instance.
        /// </summary>
        public int StreamCount
        {
            get { return m_VertexDataByteSize.Length; }
        }
        
        /// <summary>
        /// Get the byte size for one given <paramref name="stream"/>
        /// </summary>
        /// <param name="stream">Index of the stream to get its size</param>
        /// <returns>The stream size.</returns>
        public int GetVertexDataByteSize(int stream)
        {
            return m_VertexDataByteSize[stream];
        }

        private static void InterpolateFloat32(void* a, void* b, float t, void* dst)
        {
            (*(float*)dst) = Mathf.LerpUnclamped(*(float*)a, *(float*)b, t);
        }

        /// <summary>
        /// Get the position of one vertex for the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of the array the <see cref="VertexInterpolator"/> was initialized with.</param>
        /// <returns>The position of the vertex.</returns>
        public Vector3 GetPosition(int index)
        {
            fixed (byte* vertexStream = &m_VertexStreams[m_PositionStream][index * m_VertexDataByteSize[m_PositionStream] + m_PositionOffset])
            {
                return *((Vector3*)vertexStream);
            }
        }

        /// <summary>
        /// Change the position of a single vertex.
        /// </summary>
        /// <param name="index">Index of the vertex to modify.</param>
        /// <param name="pos">The new position to set.</param>
        public void SetPosition(int index, Vector3 pos)
        {
            int dstOffset = index * m_VertexDataByteSize[m_PositionStream] + m_PositionOffset;

            Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, m_VertexStreams[m_PositionStream], dstOffset, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, m_VertexStreams[m_PositionStream], dstOffset + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.z), 0, m_VertexStreams[m_PositionStream], dstOffset + 8, 4);
        }

        /// <summary>
        /// Copy part of the vertex indices part of this instance to the given dictionary <paramref name="srcToDstIndexMap"/>.
        /// </summary>
        /// <param name="srcIndices">Keys to copy to the <paramref name="srcToDstIndexMap"/> dictionary.</param>
        /// <param name="dstStartIndex">Skip the vertices before this index.</param>
        /// <param name="srcToDstIndexMap">Copy the indices to this dictionary.</param>
        public void DuplicateVertexData(SortedSet<int> srcIndices, int dstStartIndex, Dictionary<int, int> srcToDstIndexMap)
        {
            int streamIndex = 0;
            for (int index = 0; index < m_InterpolationOperation.Length; index++)
            {
                byte[] vertexStream = m_VertexStreams[streamIndex];
                int vertexDataByteSize = GetVertexDataByteSize(streamIndex);

                Assert.IsTrue(vertexStream.Length >= (dstStartIndex + srcIndices.Count) * vertexDataByteSize);

                int dstIndex = dstStartIndex;
                foreach (int srcIndex in srcIndices)
                {
                    Buffer.BlockCopy(
                        vertexStream, 
                        srcIndex * vertexDataByteSize, 
                        vertexStream,
                        dstIndex * vertexDataByteSize, 
                        vertexDataByteSize);

                    if (streamIndex == 0)
                    {
                        srcToDstIndexMap.Add(srcIndex, dstIndex);
                    }

                    dstIndex++;
                }

                streamIndex++;
            }
        }

        /// <summary>
        /// Linearly interpolates between the source to the destination.
        /// </summary>
        /// <param name="indexA">Start of the interpolation index.</param>
        /// <param name="indexB">Goal of the interpolation index.</param>
        /// <param name="t">The blending amount to apply.</param>
        /// <param name="indexDst">Index where to save the result.</param>
        public void LerpUnclamped(int indexA, int indexB, float t, int indexDst)
        {
            int streamIndex = 0;
            foreach (List<InterpolationOperator> streamOperators in m_InterpolationOperation)
            {
                fixed (byte* vertexStream = m_VertexStreams[streamIndex])
                {
                    LerpSingleStream(vertexStream, m_VertexDataByteSize[streamIndex], indexA, indexB, t, indexDst, streamOperators);
                }
                streamIndex++;
            }
        }

        private static void LerpSingleStream(byte* vertexStream, int vertexSize, int indexA, int indexB, float t, int indexDst, List<InterpolationOperator> operators)
        {
            byte* a = vertexStream + indexA * vertexSize;
            byte* b = vertexStream + indexB * vertexSize;
            byte* dst = vertexStream + indexDst * vertexSize;

            foreach (InterpolationOperator op in operators)
            {
                op.Method.Invoke(a, b, t, dst);
                a += op.ByteSize;
                b += op.ByteSize;
                dst += op.ByteSize;
            }
        }
    }
}
