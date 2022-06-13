
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using GLTFast;
using Newtonsoft.Json;
using Unity.Geospatial.HighPrecision;
using UnityEngine.Assertions;
using Unity.Geospatial.Streaming.UniversalDecoder;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    /// <summary>
    /// Default B3DM <see cref="UriLoader"/>.
    /// </summary>
    public class OgcB3dmUriLoader<TTableSchema, TBatchSchema> :
        GltfUriLoader
        where TTableSchema: B3dmTableSchema
    {
        /// <summary>
        /// Constructor with the adjustment matrix set to identity.
        /// </summary>
        /// <param name="loaderActions">
        /// Reference to the command buffer the <see cref="UriLoader"/> should publish it's output to.
        /// </param>
        /// <param name="dataSource">Unique identifier allowing to do indirect assignment.</param>
        /// <param name="lighting">Lighting type to apply to the shading.</param>
        /// <param name="textureSettings">Texture options to apply for each imported texture.</param>
        public OgcB3dmUriLoader(ILoaderActions loaderActions, UGDataSourceID dataSource, UGLighting lighting, UGTextureSettings textureSettings) :
            base(loaderActions, dataSource, lighting, textureSettings, ZMinusForwardMatrix) { }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="loaderActions">
        /// Reference to the command buffer the <see cref="UriLoader"/> should publish it's output to.
        /// </param>
        /// <param name="dataSource">Unique identifier allowing to do indirect assignment.</param>
        /// <param name="lighting">Lighting type to apply to the shading.</param>
        /// <param name="textureSettings">Texture options to apply for each imported texture.</param>
        /// <param name="adjustmentMatrix">
        /// Each time a node is loaded, multiply its transform by this matrix allowing axis alignments when the format is not left-handed, Y-Up.
        /// </param>
        public OgcB3dmUriLoader(ILoaderActions loaderActions, UGDataSourceID dataSource, UGLighting lighting, UGTextureSettings textureSettings, double4x4 adjustmentMatrix) :
            base(loaderActions, dataSource, lighting, textureSettings, adjustmentMatrix) { }

        /// <summary>
        /// B3DM file reader allowing to extract each part of its content.
        /// </summary>
        public class Reader
        {
            /// <summary>
            /// The header length before content.
            /// - 4 bits magic number [0]
            /// - 4 bits version of b3dm [4]
            /// - 4 bites length of the file [8]
            /// - 4 bites length of the Feature Table Json part [12]
            /// - 4 bites length of the Feature Table Binary part [16]
            /// - 4 bites length of the Batch Table Json part [20]
            /// - 4 bites length of the Batch Table Binary part [24]
            /// </summary>
            private const uint k_HeaderLength = 28;

            /// <summary>
            /// Content of the b3dm file as a <see langword="byte"/> <see langword="array"/>.
            /// </summary>
            private readonly byte[] m_Data;

            /// <summary>
            /// The first byte index of the glTF part of the data.
            /// </summary>
            private readonly uint m_GltfStart;

            /// <summary>
            /// Length of the Feature Table Json part.
            /// </summary>
            private readonly uint m_FeatureTableJsonLength;

            /// <summary>
            /// Length of the Feature Table Binary part.
            /// </summary>
            private readonly uint m_FeatureTableBinaryLength;

            /// <summary>
            /// Length of the Batch Table Json part.
            /// </summary>
            private readonly uint m_BatchTableJsonLength;

            /// <summary>
            /// Length of the Batch Table Binary part
            /// </summary>
            private readonly uint m_BatchTableBinaryLength;

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="data">Binary content of a b3dm file to parse.</param>
            /// <param name="name">Name of the file helping logs if the content is invalid.</param>
            public Reader(in byte[] data, string name)
            {
                m_Data = data;

                Assert.AreEqual(BitConverter.ToUInt32(data, 8), Length, $"The given b3dm length doesn't match the expected length: {name}");

                m_FeatureTableJsonLength = BitConverter.ToUInt32(data, 12);
                m_FeatureTableBinaryLength = BitConverter.ToUInt32(data, 16);
                m_BatchTableJsonLength = BitConverter.ToUInt32(data, 20);

                // Some versions of b3dm skip the batch table part if none. This results in a different header length.
                if (m_FeatureTableJsonLength == 0
                    && m_FeatureTableBinaryLength == 0
                    && m_BatchTableJsonLength == 1179937895u) // 1179937895 == glTF as a string and this is the beginning of the glTF header part.
                {
                    m_BatchTableJsonLength = 0;
                    m_BatchTableBinaryLength = 0;
                    m_GltfStart = 20;
                }
                else
                {
                    m_BatchTableBinaryLength = BitConverter.ToUInt32(data, 24);
                    m_GltfStart = k_HeaderLength + m_FeatureTableJsonLength + m_FeatureTableBinaryLength + m_BatchTableJsonLength + m_BatchTableBinaryLength;
                    Assert.IsTrue(m_GltfStart < Length, $"The following b3dm header have inconsistent values: {name}");
                }

            }

            /// <summary>
            /// Get the glTF part of the b3dm file.
            /// </summary>
            /// <returns>The glTF file as a <see langword="byte"/> <see langword="array"/>.</returns>
            public byte[] GetBinaryGltf()
            {
                byte[] result = new byte[Length - m_GltfStart];

                Array.Copy(m_Data, m_GltfStart, result, 0, result.Length);

                uint version = BitConverter.ToUInt32( result, 4 );

                Assert.IsTrue(version > 1, $"glTF version {version} is not supported.");

                return result;
            }

            /// <summary>
            /// Get the feature table json part of the b3dm file.
            /// </summary>
            /// <returns>The deserialized feature table json.</returns>
            public TTableSchema GetFeatureTableJson()
            {
                if (m_FeatureTableJsonLength == 0)
                    return null;

                string table = Encoding.UTF8.GetString(m_Data, (int)k_HeaderLength, (int)m_FeatureTableJsonLength);
                return JsonConvert.DeserializeObject<TTableSchema>(table);
            }

            /// <summary>
            /// Get the feature table binary part of the b3dm file.
            /// </summary>
            /// <returns>The feature table binary as a <see langword="byte"/> <see langword="array"/>.</returns>
            public byte[] GetFeatureTableBinary()
            {
                if (m_FeatureTableBinaryLength == 0)
                    return Array.Empty<byte>();

                uint offset = k_HeaderLength + m_FeatureTableJsonLength;
                byte[] result = new byte[m_FeatureTableBinaryLength];

                Array.Copy(m_Data, offset, result, 0, m_FeatureTableBinaryLength);

                return result;
            }

            /// <summary>
            /// Get the batch table json part of the b3dm file.
            /// </summary>
            /// <returns>The deserialized batch table json.</returns>
            public TBatchSchema GetBatchTableJson()
            {
                if (m_BatchTableJsonLength == 0)
                    return default;

                uint offset = k_HeaderLength + m_FeatureTableJsonLength + m_FeatureTableBinaryLength;

                string table = Encoding.UTF8.GetString(m_Data, (int)offset, (int)m_BatchTableJsonLength);
                return JsonConvert.DeserializeObject<TBatchSchema>(table);
            }

            /// <summary>
            /// Get the batch table binary part of the b3dm file.
            /// </summary>
            /// <returns>The batch table binary as a <see langword="byte"/> <see langword="array"/>.</returns>
            public byte[] GetBatchTableBinary()
            {
                if (m_BatchTableBinaryLength == 0)
                    return Array.Empty<byte>();

                uint offset = k_HeaderLength + m_FeatureTableJsonLength + m_FeatureTableBinaryLength + m_BatchTableJsonLength;
                byte[] result = new byte[m_BatchTableBinaryLength];

                Array.Copy(m_Data, offset, result, 0, m_BatchTableBinaryLength);

                return result;
            }

            /// <summary>
            /// Get the version of the Batched 3D Model format.
            /// </summary>
            /// <returns>The version number.</returns>
            public int GetVersion()
            {
                return BitConverter.ToInt32(m_Data, 4);
            }

            /// <summary>
            /// Get the length of the entire tile, including the header, in bytes.
            /// </summary>
            public int Length
            {
                get { return m_Data.Length; }
            }
        }

        /// <inheritdoc cref="UriLoader.SupportedFileTypes"/>
        private static readonly FileType[] k_SupportedFileTypes = { FileType.Ogc3dTilesB3dm };

        /// <inheritdoc cref="UriLoader.SupportedFileTypes"/>
        public override IEnumerable<FileType> SupportedFileTypes
        {
            get
            {
                return k_SupportedFileTypes;
            }
        }

        /// <inheritdoc cref="GltfUriLoader.LoadAsync(NodeId, UriNodeContent, double4x4)"/>
        public override async Task<InstanceID> LoadAsync(NodeId nodeId, UriNodeContent content, double4x4 transform)
        {
            Uri b3dmUri = content.Uri.MainUri;

            byte[] data = await PathUtility.DownloadFileData(b3dmUri);

            return await LoadAsync(nodeId, data, b3dmUri, transform);
        }

        /// <inheritdoc cref="GltfUriLoader.LoadAsync(NodeId, byte[], string, double4x4)"/>
        public override async Task<InstanceID> LoadAsync(NodeId nodeId, byte[] bytes, string uri, double4x4 transform)
        {
            return await LoadAsync(nodeId, bytes, new Uri(uri, UriKind.RelativeOrAbsolute), transform);
        }

        /// <inheritdoc cref="GltfUriLoader.LoadAsync(NodeId, byte[], Uri, double4x4)"/>
        public override async Task<InstanceID> LoadAsync(NodeId nodeId, byte[] bytes, Uri uri, double4x4 transform)
        {
            Reader reader = new Reader(bytes, uri.AbsolutePath);

            TTableSchema table = reader.GetFeatureTableJson();

            if (table?.Rtc_Center != null)
            {
                double[] center = table.Rtc_Center;
                double4x4 delta = HPMath.Translate(new double3(center[0], center[2], center[1]));
                transform = math.mul(transform, delta);
            }

            async Task<bool> Func(GltfImport gltFast, ImportSettings importSettings) => await gltFast.LoadGltfBinary(reader.GetBinaryGltf(), uri, importSettings);
            return await LoadAsync(nodeId, Func, uri, transform);
        }
    }
}
