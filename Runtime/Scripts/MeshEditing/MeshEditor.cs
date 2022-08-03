using System;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace Unity.Geospatial.Streaming
{
    //
    //  TODO - Actually make this work with burst... it's mostly burstable stuff and remove unsafe stuff
    //              and replace it with burst
    // 
    /// <summary>
    /// Set of tools allowing to modify <see href="https://docs.unity3d.com/ScriptReference/Mesh-vertices.html">vertices</see>
    /// on a <see cref="UnityEngine.Mesh">Mesh</see>.
    /// </summary>
    public struct MeshEditor : IDisposable
    {

        private struct VertexBuffer
        {
            public byte[][] data;
            public int vertexCount;
            public int capacity;
        }
        
        private struct TriangleCollection : IDisposable
        {
            public bool valid;
            public NativeArray<Triangle> data;
            public int size;

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (valid)
                {
                    data.Dispose();
                    valid = false;
                }
                else
                {
                    Assert.IsFalse(data.IsCreated);
                }
            }
        }

        private struct Triangle
        {
            public int a;
            public int b;
            public int c;
            public int subMesh;
        }

        private struct TriangleInteractions : IDisposable
        {
            public TriangleCollection positive;
            public TriangleCollection negative;

            public TriangleCollection middle;
            public NativeArray<bool> middleSide;

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                positive.Dispose();
                negative.Dispose();
                middle.Dispose();
            }
        }

        private struct ExtraVertex
        {
            public int vertexA;
            public int vertexB;
            public float blendFactor;
        }

        private struct NormalizedTriangle
        {
            public bool leftSide;
            public int left;
            public int right1;
            public int right2;
        }

        private readonly VertexInterpolator m_Interpolator;

        private VertexBuffer m_VertexBuffer;

        private readonly List<TriangleCollection> m_TriangleCollectionList;

        private readonly int m_SubMeshCount;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="meshData"></param>
        /// <param name="vertexDescriptor">
        /// Information about the <see href="https://docs.unity3d.com/ScriptReference/Rendering.VertexAttribute.html">VertexAttributes</see>
        /// of the <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see> <see href="https://docs.unity3d.com/ScriptReference/Mesh-vertices.html">vertices</see>.
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
        /// </param>
        public MeshEditor(ref Mesh.MeshData meshData, VertexAttributeDescriptor[] vertexDescriptor)
        {
            m_Interpolator = new VertexInterpolator(vertexDescriptor);

            //
            //  Copy vertex buffer
            //
            m_VertexBuffer.vertexCount = meshData.vertexCount;
            m_VertexBuffer.capacity = 2 * m_VertexBuffer.vertexCount;

            m_VertexBuffer.data = new byte[m_Interpolator.StreamCount][];
            for (int i = 0; i < m_Interpolator.StreamCount; i++)
            {
                int vertexDataByteSize = m_Interpolator.GetVertexDataByteSize(i);
                m_VertexBuffer.data[i] = new byte[2 * m_VertexBuffer.vertexCount * vertexDataByteSize];
                CopyVertexData(ref meshData, i, m_VertexBuffer.data[i], vertexDataByteSize);
            }

            m_Interpolator.SetVertexBuffers(m_VertexBuffer.data);

            //
            //  Copy index buffers
            //
            int triangleCount = 0;
            for (int i = 0; i < meshData.subMeshCount; i++)
                triangleCount += meshData.GetSubMesh(i).indexCount;
            Assert.IsTrue(triangleCount % 3 == 0);
            triangleCount /= 3;

            m_TriangleCollectionList = new List<TriangleCollection>(10);


            TriangleCollection rootTriangleCollection = new TriangleCollection
            {
                data = new NativeArray<Triangle>(triangleCount, Allocator.Temp),
                valid = true,
                size = triangleCount
            };
            m_TriangleCollectionList.Add(rootTriangleCollection);
            CopyIndexData(ref meshData, rootTriangleCollection.data);
            m_SubMeshCount = meshData.subMeshCount;
            for (int i = 0; i < m_SubMeshCount; i++)
            {
                SubMeshDescriptor descriptor = meshData.GetSubMesh(i);
                SetSubMeshIndex(rootTriangleCollection.data, descriptor.indexStart / 3, descriptor.indexCount / 3, i);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < m_TriangleCollectionList.Count; i++)
                m_TriangleCollectionList[i].Dispose();
        }

        private static unsafe void CopyVertexData(ref Mesh.MeshData src, int stream, byte[] dst, int vertexDataByteSize)
        {
            Assert.IsTrue(stream < src.vertexBufferCount);

            byte* srcPtr = (byte*)src.GetVertexData<byte>(stream).GetUnsafeReadOnlyPtr();
            fixed (byte* dstPtr = dst)
            {
                Buffer.MemoryCopy(srcPtr, dstPtr, dst.Length, src.vertexCount * vertexDataByteSize);
            }
        }

        private static unsafe void CopyIndexData(ref Mesh.MeshData src, NativeArray<Triangle> dst)
        {

            Triangle* dstPtr = (Triangle*)dst.GetUnsafePtr();

            if (src.indexFormat == UnityEngine.Rendering.IndexFormat.UInt16)
            {
                CopyIndexData_16(ref src, dstPtr);
            }
            else
            {
                Assert.AreEqual(UnityEngine.Rendering.IndexFormat.UInt32, src.indexFormat);
                CopyIndexData_32(ref src, dstPtr);
            }
        }

        private static unsafe void SetSubMeshIndex(NativeArray<Triangle> triangleCollection, int offset, int length, int subMeshIndex)
        {
            Triangle* ptr = (Triangle*)triangleCollection.GetUnsafePtr();

            for (int i = offset; i < offset + length; i++)
                ptr[i].subMesh = subMeshIndex;
        }

        private static unsafe void CopyIndexData_16(ref Mesh.MeshData src, Triangle* dst)
        {
            int dstTriangleOffset = 0;

            ushort* srcPtr = (ushort*)src.GetIndexData<ushort>().GetUnsafeReadOnlyPtr();
            for (int subMesh = 0; subMesh < src.subMeshCount; subMesh++)
            {
                SubMeshDescriptor descriptor = src.GetSubMesh(subMesh);
                int srcIndexOffset = descriptor.indexStart;
                int triangleCount = (descriptor.indexCount / 3);
                for (int i = 0; i < triangleCount; i++)
                {
                    Triangle* ptr = &dst[dstTriangleOffset];

                    ptr->a = srcPtr[srcIndexOffset];
                    ptr->b = srcPtr[srcIndexOffset + 1];
                    ptr->c = srcPtr[srcIndexOffset + 2];
                    ptr->subMesh = subMesh;

                    srcIndexOffset += 3;
                    dstTriangleOffset++;
                }
            }
        }

        private static unsafe void CopyIndexData_32(ref Mesh.MeshData src, Triangle* dst)
        {
            int dstTriangleOffset = 0;

            int* srcPtr = (int*)src.GetIndexData<int>().GetUnsafeReadOnlyPtr();
            for (int subMesh = 0; subMesh < src.subMeshCount; subMesh++)
            {
                SubMeshDescriptor descriptor = src.GetSubMesh(subMesh);
                int srcIndexOffset = descriptor.indexStart;
                int triangleCount = (descriptor.indexCount / 3);
                for (int i = 0; i < triangleCount; i++)
                {
                    Triangle* ptr = &dst[dstTriangleOffset];

                    ptr->a = srcPtr[srcIndexOffset];
                    ptr->b = srcPtr[srcIndexOffset + 1];
                    ptr->c = srcPtr[srcIndexOffset + 2];
                    ptr->subMesh = subMesh;

                    srcIndexOffset += 3;
                    dstTriangleOffset++;
                }
            }
        }

        /// <summary>
        /// Get the first valid collection of triangles that is not empty.
        /// </summary>
        /// <returns>The first valid triangle collection. If none are valid, <see cref="TriangleCollectionIndex.Null"/> will be returned.</returns>
        public TriangleCollectionIndex GetFirstCollection()
        {
            for (int i = 0; i < m_TriangleCollectionList.Count; i++)
            {
                if (m_TriangleCollectionList[i].valid)
                    return new TriangleCollectionIndex(i);
            }

            return TriangleCollectionIndex.Null;
        }

        /// <summary>
        /// Merge two triangle collections into a single one and dispose of them after.
        /// </summary>
        /// <param name="a">The first collection to merge with.</param>
        /// <param name="b">The second collection to merge with.</param>
        /// <returns>The new collection index of the merge result.</returns>
        public TriangleCollectionIndex CombineAndDispose(TriangleCollectionIndex a, TriangleCollectionIndex b)
        {
            if (!a.IsValid())
                return b;

            if (!b.IsValid())
                return a;

            Assert.AreNotEqual(a.Index, b.Index);

            TriangleCollection bufferA = m_TriangleCollectionList[a.Index];
            TriangleCollection bufferB = m_TriangleCollectionList[b.Index];

            NativeArray<Triangle> dst = bufferA.data;
            int requiredSize = bufferA.size + bufferB.size;
            if (bufferA.data.Length < requiredSize)
            {
                dst = new NativeArray<Triangle>(requiredSize, Allocator.Temp);
                NativeArray<Triangle>.Copy(bufferA.data, dst, bufferA.size);
                bufferA.data.Dispose();
            }

            NativeArray<Triangle>.Copy(bufferB.data, 0, dst, bufferA.size, bufferB.size);

            bufferA.data = dst;
            bufferA.size = requiredSize;
            bufferA.valid = true;
            m_TriangleCollectionList[a.Index] = bufferA;

            bufferB.data.Dispose();
            bufferB.size = 0;
            bufferB.valid = false;
            m_TriangleCollectionList[b.Index] = bufferB;

            return a;
        }

        /// <summary>
        /// Dispose of the given <paramref name="target"/> by emptying its buffer.
        /// </summary>
        /// <param name="target">Index of the collection to discard.</param>
        public void Discard(TriangleCollectionIndex target)
        {
            Assert.IsTrue(target.IsValid());
            Assert.IsTrue(target.Index < m_TriangleCollectionList.Count);

            TriangleCollection buffer = m_TriangleCollectionList[target.Index];

            Assert.IsTrue(buffer.valid);
            Assert.IsTrue(buffer.data.IsCreated);

            buffer.data.Dispose();
            buffer.size = 0;
            buffer.valid = false;

            m_TriangleCollectionList[target.Index] = buffer;
        }

        /// <summary>
        /// Split a triangle collection into two by a plane.
        /// </summary>
        /// <param name="target">The collection to cut.</param>
        /// <param name="plane">Cut the collection by this plane.</param>
        /// <param name="positive">The new collection index which is on the positive side of the plane.</param>
        /// <param name="negative">The new collection index which is on the negative side of the plane.</param>
        /// <param name="extraEdgeCollection">Output the newly created edges into this collection.</param>
        public unsafe void Cut(TriangleCollectionIndex target, Plane plane, out TriangleCollectionIndex positive, out TriangleCollectionIndex negative, List<Edge> extraEdgeCollection = null)
        {
            Assert.IsTrue(target.IsValid());
            Assert.IsTrue(target.Index < m_TriangleCollectionList.Count);

            TriangleCollection targetBuffer = m_TriangleCollectionList[target.Index];

            Assert.IsTrue(targetBuffer.valid);
            Assert.IsTrue(targetBuffer.data.IsCreated);

            NativeArray<bool> vertexSide = ComputeVertexSide(plane);
            TriangleInteractions triangleInteractions = ComputeTriangleInteraction(targetBuffer, vertexSide);
            NativeArray<ExtraVertex> extraVertices = new NativeArray<ExtraVertex>(2 * triangleInteractions.middle.size, Allocator.Temp);

            ExtraVertex* extraVertexBuffer = (ExtraVertex*)extraVertices.GetUnsafePtr();
            int extraVertexCount = 0;
            Triangle* srcTriangle = (Triangle*)triangleInteractions.middle.data.GetUnsafeReadOnlyPtr();
            bool* srcSide = (bool*)triangleInteractions.middleSide.GetUnsafeReadOnlyPtr();

            for (int i = 0; i < triangleInteractions.middle.size; i++, srcTriangle++)
            {
                bool aSide = *(srcSide++);
                bool bSide = *(srcSide++);
                bool cSide = *(srcSide++);

                NormalizeTriangle(ref *srcTriangle, aSide, bSide, cSide, out NormalizedTriangle normalizedTriangle);

                int extraVertex1 = TryGetExtraVertex(extraVertexBuffer, extraVertexCount, normalizedTriangle.left, normalizedTriangle.right1);
                int extraVertex2 = TryGetExtraVertex(extraVertexBuffer, extraVertexCount, normalizedTriangle.left, normalizedTriangle.right2);

                if (extraVertex1 == -1)
                {
                    extraVertex1 = extraVertexCount;
                    GenerateExtraVertex(normalizedTriangle.left, normalizedTriangle.right1, ref plane, &extraVertexBuffer[extraVertexCount++]);
                }

                if (extraVertex2 == -1)
                {
                    extraVertex2 = extraVertexCount;
                    GenerateExtraVertex(normalizedTriangle.left, normalizedTriangle.right2, ref plane, &extraVertexBuffer[extraVertexCount++]);
                }

                int e1 = m_VertexBuffer.vertexCount + extraVertex1;
                int e2 = m_VertexBuffer.vertexCount + extraVertex2;

                Triangle leftTriangle = new Triangle()
                {
                    subMesh = srcTriangle->subMesh,
                    a = normalizedTriangle.left,
                    b = e1,
                    c = e2
                };

                Triangle rightTriangle1 = new Triangle()
                {
                    subMesh = srcTriangle->subMesh,
                    a = normalizedTriangle.right1,
                    b = e2,
                    c = e1
                };

                Triangle rightTriangle2 = new Triangle()
                {
                    subMesh = srcTriangle->subMesh,
                    a = normalizedTriangle.right1,
                    b = normalizedTriangle.right2,
                    c = e2
                };


                if (normalizedTriangle.leftSide)
                {
                    Assert.IsTrue(triangleInteractions.positive.data.Length >= triangleInteractions.positive.size + 1);
                    Assert.IsTrue(triangleInteractions.negative.data.Length >= triangleInteractions.negative.size + 2);

                    triangleInteractions.positive.data[triangleInteractions.positive.size++] = leftTriangle;
                    triangleInteractions.negative.data[triangleInteractions.negative.size++] = rightTriangle1;
                    triangleInteractions.negative.data[triangleInteractions.negative.size++] = rightTriangle2;

                    if (extraEdgeCollection != null)
                    {
                        Edge newEdge = new Edge()
                        {
                            Index1 = e1,
                            Index2 = e2
                        };
                        extraEdgeCollection.Add(newEdge);
                    }
                }
                else
                {
                    Assert.IsTrue(triangleInteractions.negative.data.Length >= triangleInteractions.negative.size + 1);
                    Assert.IsTrue(triangleInteractions.positive.data.Length >= triangleInteractions.positive.size + 2);

                    triangleInteractions.negative.data[triangleInteractions.negative.size++] = leftTriangle;
                    triangleInteractions.positive.data[triangleInteractions.positive.size++] = rightTriangle1;
                    triangleInteractions.positive.data[triangleInteractions.positive.size++] = rightTriangle2;

                    if (extraEdgeCollection != null)
                    {
                        Edge newEdge = new Edge()
                        {
                            Index1 = e2,
                            Index2 = e1
                        };
                        extraEdgeCollection.Add(newEdge);
                    }
                }
            }

            ExtendVertexBufferSize(m_VertexBuffer.vertexCount + extraVertexCount);

            for (int i = 0; i < extraVertexCount; i++)
            {
                ref ExtraVertex extraVertex = ref extraVertexBuffer[i];
                m_Interpolator.LerpUnclamped(
                    extraVertex.vertexA,
                    extraVertex.vertexB,
                    extraVertex.blendFactor,
                    m_VertexBuffer.vertexCount++);
            }

            if (triangleInteractions.positive.size > 0)
            {
                positive = StoreTriangleCollection(triangleInteractions.positive);
                triangleInteractions.positive = default;
            }
            else
            {
                positive = TriangleCollectionIndex.Null;
            }

            if (triangleInteractions.negative.size > 0)
            {
                negative = StoreTriangleCollection(triangleInteractions.negative);
                triangleInteractions.negative = default;
            }
            else
            {
                negative = TriangleCollectionIndex.Null;
            }


            targetBuffer.Dispose();
            vertexSide.Dispose();
            triangleInteractions.Dispose();
            extraVertices.Dispose();
        }

        /// <summary>
        /// Pushes a new edge out from each selected edge in edgesToExtrude, connected by a new face for each edge.
        /// </summary>
        /// <param name="edgesToExtrude"> Edges that are extruded </param>
        /// <param name="reverseWindingOrder"> Winding order of the new faces created from extrusion </param>
        /// <param name="direction"> Edges are extruded in this given direction </param>
        /// <param name="extrudeResult"> List index for the triangle collection created from extrusion </param>
        public unsafe void EdgeExtrude(List<Edge> edgesToExtrude, bool reverseWindingOrder, in Vector3 direction, out TriangleCollectionIndex extrudeResult)
        {
            if (edgesToExtrude == null)
            {
                extrudeResult = TriangleCollectionIndex.Null;
                return;
            }

            SortedSet<int> uniqueVertices = edgesToExtrude.GetUniqueVertices();
            int vertexToExtrudeCount = uniqueVertices.Count;

            if (vertexToExtrudeCount == 0)
            {
                extrudeResult = TriangleCollectionIndex.Null;
                return;
            }

            ExtendVertexBufferSize(m_VertexBuffer.vertexCount + vertexToExtrudeCount);

            Dictionary<int, int> srcToDstIndexMap = new Dictionary<int, int>();
            int dstStartIndex = m_VertexBuffer.vertexCount;
            m_Interpolator.DuplicateVertexData(uniqueVertices, dstStartIndex, srcToDstIndexMap);
            m_VertexBuffer.vertexCount += vertexToExtrudeCount;

            Assert.AreEqual(vertexToExtrudeCount, srcToDstIndexMap.Count);

            for (int i = 0; i < vertexToExtrudeCount; i++)
            {
                Vector3 position = m_Interpolator.GetPosition(dstStartIndex + i) + direction;
                m_Interpolator.SetPosition(dstStartIndex + i, position);
            }

            TriangleCollection extrudeTriangleCollection = new TriangleCollection
            {
                data = new NativeArray<Triangle>(2 * edgesToExtrude.Count, Allocator.Temp),
                valid = true,
                size = 2 * edgesToExtrude.Count
            };
            Triangle* currentTriangle = (Triangle*)extrudeTriangleCollection.data.GetUnsafePtr();

            for (int i = 0; i < edgesToExtrude.Count; i++)
            {
                int newExtrudeEdgeIndex1 = srcToDstIndexMap[edgesToExtrude[i].Index1];
                int newExtrudeEdgeIndex2 = srcToDstIndexMap[edgesToExtrude[i].Index2];

                if (reverseWindingOrder)
                {
                    currentTriangle->a = newExtrudeEdgeIndex1;
                    currentTriangle->b = edgesToExtrude[i].Index2;
                    currentTriangle->c = edgesToExtrude[i].Index1;
                    currentTriangle->subMesh = m_SubMeshCount - 1;
                    currentTriangle++;

                    currentTriangle->a = newExtrudeEdgeIndex2;
                    currentTriangle->b = edgesToExtrude[i].Index2;
                    currentTriangle->c = newExtrudeEdgeIndex1;
                    currentTriangle->subMesh = m_SubMeshCount - 1;
                    currentTriangle++;
                }
                else
                {
                    currentTriangle->a = newExtrudeEdgeIndex1;
                    currentTriangle->b = edgesToExtrude[i].Index1;
                    currentTriangle->c = edgesToExtrude[i].Index2;
                    currentTriangle->subMesh = m_SubMeshCount - 1;
                    currentTriangle++;

                    currentTriangle->a = newExtrudeEdgeIndex2;
                    currentTriangle->b = newExtrudeEdgeIndex1;
                    currentTriangle->c = edgesToExtrude[i].Index2;
                    currentTriangle->subMesh = m_SubMeshCount - 1;
                    currentTriangle++;
                }
            }
            extrudeResult = StoreTriangleCollection(extrudeTriangleCollection);
        }

        private void ExtendVertexBufferSize(int requiredVertexCapacity)
        {
            if (m_VertexBuffer.capacity < requiredVertexCapacity)
            {
                m_VertexBuffer.capacity = 2 * requiredVertexCapacity;
                for (int i = 0; i < m_Interpolator.StreamCount; i++)
                {
                    int vertexDataByteSize = m_Interpolator.GetVertexDataByteSize(i);
                    byte[] oldData = m_VertexBuffer.data[i];
                    m_VertexBuffer.data[i] = new byte[m_VertexBuffer.capacity * vertexDataByteSize];
                    oldData.CopyTo(m_VertexBuffer.data[i], 0);
                }
            }
        }

        private unsafe void GenerateExtraVertex(int v1, int v2, ref Plane plane, ExtraVertex* result)
        {
            Vector3 pos1 = m_Interpolator.GetPosition(v1);
            Vector3 pos2 = m_Interpolator.GetPosition(v2);

            //
            //  TODO - Report bug on raycast vs side query?
            //
            /*
            bool success = plane.Raycast(new Ray(pos1, pos2 - pos1), out float distance);

            Assert.IsTrue(success);
            Assert.IsTrue(distance > 0);

            result->blendFactor = distance / Vector3.Distance(pos1, pos2);
            */

            Vector3 pos1Projection = plane.ClosestPointOnPlane(pos1);
            Vector3 pos2Projection = plane.ClosestPointOnPlane(pos2);

            float dist1 = Vector3.Distance(pos1, pos1Projection);
            float dist2 = Vector3.Distance(pos2, pos2Projection);

            result->blendFactor = (dist1 / (dist1 + dist2));
            result->vertexA = v1;
            result->vertexB = v2;
        }

        private unsafe NativeArray<bool> ComputeVertexSide(Plane plane)
        {
            int vertexCount = m_VertexBuffer.vertexCount;
            var result = new NativeArray<bool>(vertexCount, Allocator.Temp);

            bool * dst = (bool*)result.GetUnsafePtr();
            for (int i = 0; i < vertexCount; i++, dst++)
            {
                *dst = plane.GetSide(m_Interpolator.GetPosition(i));
            }

            return result;
        }

        private static unsafe TriangleInteractions ComputeTriangleInteraction(TriangleCollection triangles, NativeArray<bool> vertexSides)
        {
            Triangle* srcPtr = (Triangle*)triangles.data.GetUnsafePtr();
            bool* sidePtr = (bool*)vertexSides.GetUnsafeReadOnlyPtr();

            TriangleInteractions result;

            result.positive.data = new NativeArray<Triangle>(2 * triangles.size, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            Triangle* positive = (Triangle*)result.positive.data.GetUnsafePtr();
            result.positive.size = 0;
            result.positive.valid = true;

            result.negative.data = new NativeArray<Triangle>(2 * triangles.size, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            Triangle* negative = (Triangle*)result.negative.data.GetUnsafePtr();
            result.negative.size = 0;
            result.negative.valid = true;

            result.middle.data = new NativeArray<Triangle>(2 * triangles.size, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            result.middleSide = new NativeArray<bool>(2 * 3 * triangles.size, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            Triangle* middle = (Triangle*)result.middle.data.GetUnsafePtr();
            bool* middleSide = (bool*)result.middleSide.GetUnsafePtr();
            result.middle.size = 0;
            result.middle.valid = true;

            for(int i = 0; i < triangles.size; i++, srcPtr++)
            {
                bool a = sidePtr[srcPtr->a];
                bool b = sidePtr[srcPtr->b];
                bool c = sidePtr[srcPtr->c];

                if (a == b && a == c)
                {
                    if (a)
                    {
                        *positive++ = *srcPtr;
                        result.positive.size++;
                    }
                    else
                    {
                        *negative++ = *srcPtr;
                        result.negative.size++;
                    }
                }
                else
                {
                    *middle++ = *srcPtr;
                    *middleSide++ = a;
                    *middleSide++ = b;
                    *middleSide++ = c;
                    result.middle.size++;
                }
            }

            return result;
        }

        private static void NormalizeTriangle(ref Triangle srcTriangle, bool aSide, bool bSide, bool cSide, out NormalizedTriangle result)
        {
            if (aSide == bSide)
            {
                result.leftSide = cSide;
                result.left = srcTriangle.c;
                result.right1 = srcTriangle.a;
                result.right2 = srcTriangle.b;
            }
            else if (bSide == cSide)
            {
                result.leftSide = aSide;
                result.left = srcTriangle.a;
                result.right1 = srcTriangle.b;
                result.right2 = srcTriangle.c;
            }
            else
            {
                Assert.IsTrue(aSide == cSide);
                result.leftSide = bSide;
                result.left = srcTriangle.b;
                result.right1 = srcTriangle.c;
                result.right2 = srcTriangle.a;
            }
        }

        private static unsafe int TryGetExtraVertex(ExtraVertex* extraVertices, int count, int indexA, int indexB)
        {
            ExtraVertex* v = extraVertices;
            for(int i = 0; i < count; i++, v++)
            {
                if ((v->vertexA == indexA && v->vertexB == indexB) ||
                    (v->vertexA == indexB && v->vertexB == indexA))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Transfer the given <paramref name="selection">triangle collection</paramref> to the given <paramref name="meshData"/>.
        /// </summary>
        /// <param name="selection">Index of the triangle collection to apply.</param>
        /// <param name="meshData">Change the vertices of this instance.</param>
        public void AssignToMeshData(TriangleCollectionIndex selection, ref Mesh.MeshData meshData)
        {
            Assert.IsTrue(selection.IsValid());
            Assert.IsTrue(selection.Index < m_TriangleCollectionList.Count);

            meshData.SetVertexBufferParams(m_VertexBuffer.vertexCount, m_Interpolator.VertexAttributeDescriptors);
            for (int i = 0; i < m_Interpolator.StreamCount; i++)
            {
                NativeArray<byte> vertexData = meshData.GetVertexData<byte>(i);
                NativeArray<byte>.Copy(m_VertexBuffer.data[i], vertexData, m_VertexBuffer.vertexCount * m_Interpolator.GetVertexDataByteSize(i));
            }

            TriangleCollection triangleCollection = m_TriangleCollectionList[selection.Index];

            Assert.IsTrue(triangleCollection.valid);

            BuildIndexBuffer(ref triangleCollection, out NativeArray<int> indexBuffer, out NativeArray<SubMeshDescriptor> descriptors);

            //
            //  TODO - Implement 16 bit indices, which will actually be more common
            //
            meshData.SetIndexBufferParams(indexBuffer.Length, IndexFormat.UInt32);
            NativeArray<int> indexData = meshData.GetIndexData<int>();
            NativeArray<int>.Copy(indexBuffer, indexData);
            meshData.subMeshCount = descriptors.Length;
            for(int i = 0; i < descriptors.Length; i++)
                meshData.SetSubMesh(i, descriptors[i]);

            indexBuffer.Dispose();
            descriptors.Dispose();
        }

        private unsafe void BuildIndexBuffer(ref TriangleCollection triangleCollection, out NativeArray<int> indexBuffer, out NativeArray<SubMeshDescriptor> descriptors)
        {
            indexBuffer = new NativeArray<int>(3 * triangleCollection.size, Allocator.Temp);
            descriptors = new NativeArray<SubMeshDescriptor>(m_SubMeshCount, Allocator.Temp);

            Triangle* src = (Triangle*)triangleCollection.data.GetUnsafeReadOnlyPtr();
            int* dst = (int*)indexBuffer.GetUnsafePtr();
            int start = 0;

            for(int i = 0; i < m_SubMeshCount; i++)
            {
                BuildIndexBufferForSubMesh(src, triangleCollection.size, &dst[start], i, out int count);
                descriptors[i] = new SubMeshDescriptor(start, count);
            }
        }

        private static unsafe void BuildIndexBufferForSubMesh(Triangle* src, int srcLength, int* dst, int subMesh, out int count)
        {
            count = 0;

            for(int i = 0; i < srcLength; i++)
            {
                if (src->subMesh != subMesh)
                    continue;

                *(dst++) = src->a;
                *(dst++) = src->b;
                *(dst++) = src->c;

                src++;
                count++;
            }

            count *= 3;
        }

        private TriangleCollectionIndex StoreTriangleCollection(TriangleCollection triangleCollection)
        {
            Assert.IsTrue(triangleCollection.valid);

            for(int i = 0; i < m_TriangleCollectionList.Count; i++)
            {
                if(!m_TriangleCollectionList[i].valid)
                {
                    m_TriangleCollectionList[i] = triangleCollection;
                    return new TriangleCollectionIndex(i);
                }
            }


            m_TriangleCollectionList.Add(triangleCollection);
            return new TriangleCollectionIndex(m_TriangleCollectionList.Count - 1);
        }

    }
}
