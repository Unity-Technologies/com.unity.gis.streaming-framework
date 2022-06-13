using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace Unity.Geospatial.Streaming
{
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

        public VertexAttributeDescriptor[] VertexAttributeDescriptors { get; private set; }

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

        public void SetVertexBuffers(byte[][] vertexStreams, int vertexCount)
        {
            m_VertexStreams = vertexStreams;
        }

        public int StreamCount
        {
            get { return m_VertexDataByteSize.Length; }
        }
        
        public int GetVertexDataByteSize(int stream) => m_VertexDataByteSize[stream];

        private static void InterpolateFloat32(void* a, void* b, float t, void* dst)
        {
            (*(float*)dst) = Mathf.LerpUnclamped(*(float*)a, *(float*)b, t);
        }

        public Vector3 GetPosition(int index)
        {
            fixed (byte* vertexStream = &m_VertexStreams[m_PositionStream][index * m_VertexDataByteSize[m_PositionStream] + m_PositionOffset])
            {
                return *((Vector3*)vertexStream);
            }
        }

        public void SetPosition(int index, Vector3 pos)
        {
            int dstOffset = index * m_VertexDataByteSize[m_PositionStream] + m_PositionOffset;

            Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, m_VertexStreams[m_PositionStream], dstOffset, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, m_VertexStreams[m_PositionStream], dstOffset + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.z), 0, m_VertexStreams[m_PositionStream], dstOffset + 8, 4);
        }

        public void DuplicateVertexData(SortedSet<int> srcIndices, int dstStartIndex, Dictionary<int, int> srcToDstIndexMap)
        {
            int streamIndex = 0;
            foreach (List<InterpolationOperator> streamOperators in m_InterpolationOperation)
            {
                byte[] vertexStream = m_VertexStreams[streamIndex];
                int vertexDataByteSize = GetVertexDataByteSize(streamIndex);

                Assert.IsTrue(vertexStream.Length >= (dstStartIndex + srcIndices.Count) * vertexDataByteSize);

                int dstIndex = dstStartIndex;
                foreach (int srcIndex in srcIndices)
                {
                    Buffer.BlockCopy(vertexStream, srcIndex * vertexDataByteSize, vertexStream, dstIndex * vertexDataByteSize, vertexDataByteSize);

                    if (streamIndex == 0)
                    {
                        srcToDstIndexMap.Add(srcIndex, dstIndex);
                    }

                    dstIndex++;
                }
                streamIndex++;
            }
        }

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
