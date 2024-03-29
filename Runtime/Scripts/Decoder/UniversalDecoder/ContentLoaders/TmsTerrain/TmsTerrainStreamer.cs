
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Unity.Collections.LowLevel.Unsafe;
using Unity.Geospatial.HighPrecision;
using Unity.Geospatial.Streaming.UniversalDecoder;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace Unity.Geospatial.Streaming.TmsTerrain
{
    /// <summary>
    /// Decoder class used to convert serialized TMS (Terrain Management System) Terrain file to a
    /// <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see> instance and its respective
    /// <see href="https://docs.unity3d.com/ScriptReference/Texture2D.html">Texture</see>.
    /// </summary>
    public class TmsTerrainStreamer
    {
        /// <summary>
        /// The type for a given terrain data.
        /// </summary>
        public enum TileType
        {
            /// <summary>
            /// The tile has no mesh data and no child tiles.
            /// </summary>
            EmptyWithoutChildren = 0,

            /// <summary>
            /// The tile has no mesh data but have child tiles.
            /// </summary>
            EmptyWithChildren = 1,

            /// <summary>
            /// The tile has mesh data and no child tiles.
            /// </summary>
            DataWithoutChildren = 25,

            /// <summary>
            /// The tile has mesh data and have child tiles.
            /// </summary>
            DataWithChildren = 26
        }

        /// <summary>
        /// Represent a single tile part of a TMS terrain dataset. This contains all the information needed
        /// to load the data representing a single tile for a specific zoom level.
        /// </summary>
        public sealed class TerrainTile
        {
            /// <summary>
            /// Define the coordinate limits of the tile based on the WGS84 system.
            /// </summary>
            public ExtentSchema Extent { get; }

            /// <summary>
            /// The error, in meters, introduced if this tile is rendered and its children are not.
            /// At runtime, the geometric error is used to compute screen space error (SSE), i.e., the error measured in pixels.
            /// </summary>
            public float GeometricError { get; }

            /// <summary>
            /// Snapshot of mesh data of multiple meshes to read from.
            /// </summary>
            public Mesh.MeshDataArray Mesh { get; }

            /// <summary>
            /// The minimum elevation of the tile, relative to the terrain.json file.
            /// </summary>
            public double MinElevation { get; }

            /// <summary>
            /// The maximum elevation of the tile, relative to the terrain.json file.
            /// </summary>
            public double MaxElevation { get; }

            /// <summary>
            /// Where to position the geometry when loading the tile.
            /// </summary>
            public double3 Position { get; }

            /// <summary>
            /// Specify if the tile has geometry and if it has child tiles.
            /// </summary>
            public TileType Type { get; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="type">Specify if the tile has geometry and if it has child tiles.</param>
            /// <param name="mesh">Snapshot of mesh data of multiple meshes to read from.</param>
            /// <param name="position">Where to position the geometry when loading the tile.</param>
            /// <param name="extent">Define the coordinate limits of the tile based on the WGS84 system.</param>
            /// <param name="minElevation">The minimum elevation of the tile, relative to the terrain.json file.</param>
            /// <param name="maxElevation">The maximum elevation of the tile, relative to the terrain.json file.</param>
            /// <param name="geometricError">The error, in meters, introduced if this tile is rendered and its children are not.
            /// At runtime, the geometric error is used to compute screen space error (SSE), i.e., the error measured in pixels.</param>
            public TerrainTile(TileType type, Mesh.MeshDataArray mesh, double3 position, ExtentSchema extent, double minElevation, double maxElevation, float geometricError)
            {
                Assert.IsNotNull(extent, "Extent is null.");

                Type = type;
                Mesh = mesh;
                Position = position;
                Extent = extent;
                MinElevation = minElevation;
                MaxElevation = maxElevation;
                GeometricError = geometricError;
            }

            /// <summary>
            /// The bounding volume that encloses the tile geometry.
            /// </summary>
            /// <param name="transform">Offset the result by this position.</param>
            /// <returns>The bounding volume to use when calculating the screen space error of this <see cref="ILeaf"/> instance.</returns>
            public DoubleBounds GetBoundingVolume(double4x4 transform)
            {
                return Extent.ToBoundingVolume(transform, MinElevation, MaxElevation);
            }

            /// <summary>
            /// Get if this tile has child tiles that can be loaded.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if the tile has child tiles;
            /// <see langword="false"/> otherwise.
            /// </returns>
            public bool HasChildren()
            {
                return Type == TileType.DataWithChildren ||
                       Type == TileType.EmptyWithChildren;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct DstVertex
        {
            public DstVertex(Vector3 position, Vector3 normal, Vector2 uv)
            {
                Position = position;
                Normal = normal;
                UV = uv;
            }

            public Vector3 Position { get; }
            public Vector3 Normal { get; }
            public Vector2 UV { get; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private unsafe struct Header
        {
            public fixed byte magic[4];
            public readonly uint fileByteSize;
            public readonly int type;

            public readonly ushort majorVersion;
            public readonly ushort minorVersion;

            public TileType TileType
            {
                get { return (TileType)type; }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private readonly struct TileData
        {
            public readonly uint layout;
            public readonly double minLon;
            public readonly double maxLon;
            public readonly double minLat;
            public readonly double maxLat;
            public readonly double minElevation;
            public readonly double maxElevation;
            public readonly float geometricError;

            public readonly ushort vertexBufferOffset;
            public readonly ushort vertexBufferCount;
            public readonly ushort trianglesOffset;
            public readonly ushort trianglesCount;

            public readonly ushort northEdgeStart;
            public readonly ushort northEdgeCount;
            public readonly ushort northEdgeExtra;
            public readonly ushort eastEdgeStart;
            public readonly ushort eastEdgeCount;
            public readonly ushort eastEdgeExtra;
            public readonly ushort southEdgeStart;
            public readonly ushort southEdgeCount;
            public readonly ushort southEdgeExtra;
            public readonly ushort westEdgeStart;
            public readonly ushort westEdgeCount;
            public readonly ushort westEdgeExtra;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private readonly struct SrcVertex
        {
            /// <summary>
            /// The longitude of the vertex relative to minLon and maxLon.
            /// A value of 0 will make the longitude of this vertex correspond exactly to minLon and a value of 2^16-1
            /// would correspond exactly to maxLon. Vertices on the western-most edge of the tile are expected to have
            /// values of 0 and vertices on the eastern-most edge of the tile are expected to have values of 2^16-1.
            /// </summary>
            public readonly ushort u;

            /// <summary>
            /// The latitude of the vertex relative to minLat and maxLat.
            /// A value of 0 will make the latitude of this vertex correspond exactly to minLat and a value of 2^16-1
            /// would correspond exactly to maxLat. Vertices on the northern-most edge of the tile are expected to have
            /// values of 0 and vertices on the southern-most edge of the tile are expected to have values of 2^16-1.
            /// </summary>
            public readonly ushort v;

            /// <summary>
            /// The elevation of the vertex relative to maxElevation and minElevation.
            /// A value of 0 will make the elevation of this vertex correspond exactly to minElevation and a value
            /// of 2^16-1 would correspond exactly to maxElevation. It is normal and expected normal for values
            /// of 0 and 2^16-1 to appear in this field.
            /// </summary>
            public readonly ushort z;

            public readonly short nu;
            public readonly short nv;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        private struct Triangle
        {
            public ushort a;
            public ushort b;
            public ushort c;
        }

        private const ushort k_MajorVersion = 1;
        private const ushort k_MinorVersion = 0;
        private const double k_SkirtRatio = 0.1;

        private static readonly int k_SizeOfHeader = Marshal.SizeOf(typeof(Header));
        private static readonly int k_SizeOfTileData = Marshal.SizeOf(typeof(TileData));
        private static readonly int k_SizeOfSrcVertex = Marshal.SizeOf(typeof(SrcVertex));
        private static readonly int k_SizeOfSrcTriangle = Marshal.SizeOf(typeof(Triangle));
        private static readonly int k_SizeOfDstVertex = Marshal.SizeOf(typeof(DstVertex));

        private readonly VertexAttributeDescriptor[] m_VertexAttributeDescriptors =
        {
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        };

        private Mesh.MeshDataArray GenerateMesh(byte[] fileData, in TileData tileData, in double3 xzyecefCenter)
        {
            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            WriteVertexData(in tileData, xzyecefCenter, fileData, ref meshData);
            WriteIndexData(in tileData, fileData, ref meshData);

            return meshDataArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double3 GetCenter(in TileData tileData)
        {
            double latCenter = 0.5 * (tileData.minLat + tileData.maxLat);
            double lonCenter = 0.5 * (tileData.minLon + tileData.maxLon);
            double elevationCenter = 0.5 * (tileData.minElevation + tileData.maxElevation);

            GeodeticCoordinates tileCenter = new GeodeticCoordinates(latCenter, lonCenter, elevationCenter);

            return Wgs84.GeodeticToXzyEcef(tileCenter);
        }

        private static int GetEdgeTriangleCount(ushort count, ushort extra)
        {
            if (count == 0 && extra == ushort.MaxValue)
                return 0;

            if (count == 1 && extra == ushort.MaxValue || count == 0 && extra != ushort.MaxValue)
                throw new FormatException("Edge cannot be composed of only a single vertex.");

            return 2 * (count + (extra == ushort.MaxValue ? -1 : 0));
        }

        private static unsafe Header GetHeader(byte[] data)
        {
            ValidateHeader(data);

            fixed (void* ptr = data)
            {
                Header* header = (Header*)ptr;

                if (header->fileByteSize != data.Length)
                    throw new FormatException("File length does not match size specified in header.");

                Header result = *(Header*)ptr;
                ValidateHeader(result);
                return result;
            }
        }

        private static double GetSkirtHeight(in TileData tileData)
        {
            double dLat = tileData.maxLat - tileData.minLat;
            double dLon = tileData.maxLon - tileData.minLon;

            return math.radians(k_SkirtRatio * Wgs84.MajorRadius) * math.max(dLat, dLon);
        }

        private static unsafe TileData GetTileData(byte[] data)
        {
            if (data.Length < k_SizeOfHeader + k_SizeOfTileData)
                throw new FormatException("File is too small to contain tile data.");

            fixed (void* ptr = &data[k_SizeOfHeader])
            {
                TileData* tileData = (TileData*)ptr;

                return *tileData;
            }
        }

        private static ushort GetTotalTriangleCount(in TileData tileData)
        {
            return (ushort)(tileData.trianglesCount +
                            GetEdgeTriangleCount(tileData.northEdgeCount, tileData.northEdgeCount) +
                            GetEdgeTriangleCount(tileData.eastEdgeCount, tileData.eastEdgeCount) +
                            GetEdgeTriangleCount(tileData.southEdgeCount, tileData.southEdgeCount) +
                            GetEdgeTriangleCount(tileData.westEdgeCount, tileData.westEdgeCount));
        }

        private static ushort GetTotalVertexCount(in TileData tileData)
        {
            return (ushort)(tileData.vertexBufferCount +
                            tileData.northEdgeCount + (tileData.northEdgeExtra == ushort.MaxValue ? 0 : 1) +
                            tileData.eastEdgeCount + (tileData.eastEdgeExtra == ushort.MaxValue ? 0 : 1) +
                            tileData.southEdgeCount + (tileData.southEdgeExtra == ushort.MaxValue ? 0 : 1) +
                            tileData.westEdgeCount + (tileData.westEdgeExtra == ushort.MaxValue ? 0 : 1));
        }

        private static bool HasTileData(TileType type)
        {
            return type == TileType.DataWithoutChildren ||
                   type == TileType.DataWithChildren;
        }

        /// <summary>
        /// Convert a serialized TMS Terrain tile to a TerrainTile allowing to be converted to a
        /// <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see> instance with <see cref="TerrainTile.Mesh"/>.
        /// </summary>
        /// <param name="data">Data to be deserialized.</param>
        /// <returns>A new <see cref="TerrainTile"/> ready to be converted to a
        /// <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>.</returns>
        public TerrainTile Load(byte[] data)
        {
            Header header = GetHeader(data);

            TileType tileType = header.TileType;
            Mesh.MeshDataArray meshDataArray;
            ExtentSchema extent;
            double3 center;

            double minElevation = 0.0;
            double maxElevation = 0.0;
            float geometricError = 0;

            if (HasTileData(tileType))
            {
                TileData tileData = GetTileData(data);

                geometricError = tileData.geometricError;

                center = GetCenter(in tileData);
                minElevation = tileData.minElevation;
                maxElevation = tileData.maxElevation;

                extent = new ExtentSchema(tileData.minLat, tileData.maxLat, tileData.minLon, tileData.maxLon);

                meshDataArray = GenerateMesh(data, in tileData, in center);
            }
            else
            {
                center = double3.zero;
                extent = default;
                meshDataArray = default;
            }

            return new TerrainTile(tileType, meshDataArray, center, extent, minElevation, maxElevation, geometricError);
        }

        private static void ValidateHeader(byte[] data)
        {
            if (data.Length < k_SizeOfHeader)
                throw new FormatException("Header has an invalid size.");

            bool validMagic = true;

            foreach (byte[] magicNumber in FileType.TmsTerrain.MagicNumbers)
            {
                validMagic = true;
                for (int i = 0; i < magicNumber.Length; i++)
                {
                    if (data[i] != magicNumber[i])
                    {
                        validMagic = false;
                        break;
                    }
                }
                if (validMagic)
                    break;
            }

            if (!validMagic)
                throw new FormatException("Header has an invalid magic number.");
        }

        private static void ValidateHeader(in Header header)
        {
            if (header.majorVersion != k_MajorVersion)
                throw new FormatException("Version doesn't match");
        }

        private unsafe void WriteVertexData(in TileData tileData, double3 tileCenter, byte[] src, ref Mesh.MeshData dstMesh)
        {
            ushort vertexOffset = tileData.vertexBufferOffset;
            ushort vertexCount = tileData.vertexBufferCount;

            ushort totalVertexCount = GetTotalVertexCount(in tileData);

            double skirtHeight = GetSkirtHeight(in tileData);

            dstMesh.SetVertexBufferParams(totalVertexCount, m_VertexAttributeDescriptors);

            DstVertex* dstBuffer = (DstVertex*)dstMesh.GetVertexData<DstVertex>().GetUnsafePtr();

            if (vertexOffset < k_SizeOfHeader + k_SizeOfHeader)
                throw new IndexOutOfRangeException($"Vertex offset cannot be before the end of the {nameof(tileData)} section of the file.");

            if (vertexOffset + vertexCount * k_SizeOfSrcVertex > src.Length)
                throw new IndexOutOfRangeException("Vertex buffer exceeds the file size.");

            fixed (void* ptr = &src[vertexOffset])
            {
                SrcVertex* vertexBufferStart = (SrcVertex*)ptr;
                DstVertex* dstVertex = dstBuffer;

                for (int i = 0; i < vertexCount; i++)
                {
                    ref SrcVertex srcVertex = ref vertexBufferStart[i];
                    WriteSingleVertex(in tileData, in tileCenter, in srcVertex, out *dstVertex++, 0);
                }

                WriteSkirtVertices(in tileData, tileCenter, skirtHeight, tileData.northEdgeStart, tileData.northEdgeCount, tileData.northEdgeExtra, vertexBufferStart, ref dstVertex);
                WriteSkirtVertices(in tileData, tileCenter, skirtHeight, tileData.eastEdgeStart, tileData.eastEdgeCount, tileData.eastEdgeExtra, vertexBufferStart, ref dstVertex);
                WriteSkirtVertices(in tileData, tileCenter, skirtHeight, tileData.southEdgeStart, tileData.southEdgeCount, tileData.southEdgeExtra, vertexBufferStart, ref dstVertex);
                WriteSkirtVertices(in tileData, tileCenter, skirtHeight, tileData.westEdgeStart, tileData.westEdgeCount, tileData.westEdgeExtra, vertexBufferStart, ref dstVertex);
            }
        }

        private static unsafe void WriteSkirtVertices(in TileData tileData, in double3 center, double skirtHeight, ushort start, ushort count, ushort extra, SrcVertex* vertexBuffer, ref DstVertex* currentDst)
        {
            for (int i = 0; i < count; i++)
            {
                ref SrcVertex srcVertex = ref vertexBuffer[start + i];
                WriteSingleVertex(in tileData, in center, in srcVertex, out *currentDst++, -skirtHeight);
            }

            if (extra != ushort.MaxValue)
            {
                ref SrcVertex srcVertex = ref vertexBuffer[extra];
                WriteSingleVertex(in tileData, in center, in srcVertex, out *currentDst++, -skirtHeight);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteSingleVertex(in TileData tileData, in double3 centerPoint, in SrcVertex src, out DstVertex dst, double elevationOffset)
        {
            float u = src.u * (1.0f / 65535.0f);
            float v = src.v * (1.0f / 65535.0f);
            float z = src.z * (1.0f / 65535.0f);

            double lon = u * (tileData.maxLon - tileData.minLon) + tileData.minLon;
            double lat = v * (tileData.maxLat - tileData.minLat) + tileData.minLat;
            double ele = z * (tileData.maxElevation - tileData.minElevation) + tileData.minElevation;

            double4x4 xzyecefFromXzyenu = Wgs84.GetXzyEcefFromXzyEnuMatrix(new GeodeticCoordinates(lat, lon, ele + elevationOffset));

            double3 xzyecefPosition = new double3(xzyecefFromXzyenu.c3.x, xzyecefFromXzyenu.c3.y, xzyecefFromXzyenu.c3.z);

            Vector3 ru = new Vector3((float)xzyecefFromXzyenu.c0.x, (float)xzyecefFromXzyenu.c0.y, (float)xzyecefFromXzyenu.c0.z);
            Vector3 rv = new Vector3((float)xzyecefFromXzyenu.c2.x, (float)xzyecefFromXzyenu.c2.y, (float)xzyecefFromXzyenu.c2.z);
            Vector3 rw = new Vector3((float)xzyecefFromXzyenu.c1.x, (float)xzyecefFromXzyenu.c1.y, (float)xzyecefFromXzyenu.c1.z);

            float mu = 1.0f / 32768.0f * src.nu;
            float mv = 1.0f / 32768.0f * src.nv;
            float mw = math.sqrt(1 - mu * mu - mv * mv);

            dst = new DstVertex(
                (float3)(xzyecefPosition - centerPoint),
                mu * ru + mv * rv + mw * rw,
                new Vector2(u, v));
        }

        private static unsafe void WriteIndexData(in TileData tileData, byte[] src, ref Mesh.MeshData dstMesh)
        {
            ushort trianglesOffset = tileData.trianglesOffset;
            ushort trianglesCount = tileData.trianglesCount;

            ushort totalTrianglesCount = GetTotalTriangleCount(in tileData);

            dstMesh.SetIndexBufferParams(3 * totalTrianglesCount, IndexFormat.UInt16);

            Triangle* dstBuffer = (Triangle*)dstMesh.GetIndexData<Triangle>().GetUnsafePtr();

            if (trianglesOffset < k_SizeOfHeader + k_SizeOfHeader)
                throw new IndexOutOfRangeException($"Triangles offset cannot be before the end of the {nameof(tileData)} section of the file.");

            if (trianglesOffset + trianglesCount * k_SizeOfSrcTriangle > src.Length)
                throw new IndexOutOfRangeException("Triangles buffer exceeds the file size.");

            fixed (void* ptr = &src[trianglesOffset])
            {
                Triangle* srcBuffer = (Triangle*)ptr;
                Triangle* dstTriangle = dstBuffer;

                for (int i = 0; i < trianglesCount; i++)
                {
                    *(dstTriangle++) = srcBuffer[i];
                }

                ushort lowerSkirtStart = tileData.vertexBufferCount;

                WriteSkirtIndices(tileData.northEdgeStart, ref lowerSkirtStart, tileData.northEdgeCount, tileData.northEdgeExtra, ref dstTriangle);
                WriteSkirtIndices(tileData.eastEdgeStart, ref lowerSkirtStart, tileData.eastEdgeCount, tileData.eastEdgeExtra, ref dstTriangle);
                WriteSkirtIndices(tileData.southEdgeStart, ref lowerSkirtStart, tileData.southEdgeCount, tileData.southEdgeExtra, ref dstTriangle);
                WriteSkirtIndices(tileData.westEdgeStart, ref lowerSkirtStart, tileData.westEdgeCount, tileData.westEdgeExtra, ref dstTriangle);
            }

            dstMesh.subMeshCount = 1;
            dstMesh.SetSubMesh(0, new SubMeshDescriptor(0, 3 * totalTrianglesCount));
        }

        private static unsafe void WriteSkirtIndices(ushort upperStart, ref ushort lowerStart, ushort count, ushort extra, ref Triangle* currentTriangle)
        {
            if (count == 0 || (count == 1 && extra == ushort.MaxValue))
                return;

            for (int i = 0; i < count - 1; i++)
            {
                currentTriangle->a = (ushort)(upperStart + i + 1);
                currentTriangle->b = (ushort)(upperStart + i);
                currentTriangle->c = (ushort)(lowerStart + i);
                currentTriangle++;

                currentTriangle->a = (ushort)(upperStart + i + 1);
                currentTriangle->b = (ushort)(lowerStart + i);
                currentTriangle->c = (ushort)(lowerStart + i + 1);
                currentTriangle++;
            }

            lowerStart += count;

            if (extra != ushort.MaxValue)
            {
                currentTriangle->a = extra;
                currentTriangle->b = (ushort)(upperStart + count - 1);
                currentTriangle->c = (ushort)(lowerStart - 1);
                currentTriangle++;

                currentTriangle->a = extra;
                currentTriangle->b = (ushort)(lowerStart - 1);
                currentTriangle->c = lowerStart;
                currentTriangle++;

                lowerStart++;
            }
        }
    }
}
