
using System;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Unity.Geospatial.Streaming.UniversalDecoder;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    /// <summary>
    /// Main OGC 3D Tiles dataset loader.
    /// This class is customizable and therefore cannot be used alone. For each <see cref="UniversalDecoder.FileType"/>
    /// present in a json <see cref="ITilesetSchema">tileset</see>, a corresponding <see cref="UriLoader"/> must be
    /// <see cref="HierarchyContentLoader{TLeaf, TUriCollection}.RegisterUriLoader">registered</see>.
    /// </summary>
    /// <remarks><typeparamref name="TTileset"/> and <typeparamref name="TTile"/> allow to use your own schema
    /// in case you want to support custom extensions.</remarks>
    /// <typeparam name="TTileset"><see cref="ITilesetSchema">Tileset</see> schema class allowing to deserialize the tileset json.</typeparam>
    /// <typeparam name="TTile"><see cref="ITileSchema{T}">Tile</see> schema class allowing to deserialize the tile son part.</typeparam>
    public class OgcTilesetContentLoader<TTileset, TTile> :
        HierarchyContentLoader<TTile, SingleUri>
        where TTileset: ITilesetSchema
        where TTile: ITileSchema<TTile>
    {
        /// <summary>
        /// Since Ogc is Z Up, multiplying transforms by this matrix will switch it to Y Up.
        /// </summary>
        public static readonly double4x4 SwapZYMatrix = new double4x4
        (
            1, 0, 0, 0,
            0, 0, 1, 0,
            0, 1, 0, 0,
            0, 0, 0, 1
        );

        /// <summary>
        /// Constructor with <see cref="SwapZYMatrix"/> set as the adjustment matrix.
        /// </summary>
        /// <param name="contentManager"><see cref="NodeContentManager"/> to use when loading a tile.</param>
        /// <param name="contentType">Type identifier allowing to figure which <see cref="INodeContentLoader"/> to execute when loading this content.</param>
        /// <param name="createHierarchyOnDemand">
        /// <see langword="true"/> to create the <see cref="BoundingVolumeHierarchy{T}"/> nodes only when the parent node get loaded;
        /// <see langword="false"/> to create all the nodes when <see cref="HierarchyContentLoader{TLeaf,TUri}.LoadNodeAsync(NodeId, NodeContent)"/> is executed for a <see cref="HierarchyContentLoader{TLeaf,TUri}.RootContent"/> instance.
        /// </param>
        public OgcTilesetContentLoader(INodeContentManager contentManager, ContentType contentType, bool createHierarchyOnDemand = false) :
            base(contentManager, contentType, SwapZYMatrix, createHierarchyOnDemand) { }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="contentManager"><see cref="NodeContentManager"/> to use when loading a tile.</param>
        /// <param name="contentType">Type identifier allowing to figure which <see cref="INodeContentLoader"/> to execute when loading this content.</param>
        /// <param name="adjustmentMatrix">Each time a node is loaded, multiply its transform by this matrix allowing axis alignments when the format is not left-handed, Y-Up.</param>
        /// <param name="createHierarchyOnDemand">
        /// <see langword="true"/> to create the <see cref="BoundingVolumeHierarchy{T}"/> nodes only when the parent node get loaded;
        /// <see langword="false"/> to create all the nodes when <see cref="HierarchyContentLoader{TLeaf,TUri}.LoadNodeAsync(NodeId, NodeContent)"/> is executed for a <see cref="HierarchyContentLoader{TLeaf,TUri}.RootContent"/> instance.
        /// </param>
        public OgcTilesetContentLoader(INodeContentManager contentManager, ContentType contentType, double4x4 adjustmentMatrix, bool createHierarchyOnDemand = false) :
            base(contentManager, contentType, adjustmentMatrix, createHierarchyOnDemand) { }

        /// <summary>
        /// <see cref="UniversalDecoder.FileType"/> associated with this loader allowing to call within
        /// <see cref="INodeContentLoader.LoadNodeAsync(NodeId, NodeContent)"/> when the <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see>
        /// is of the same type.
        /// </summary>
        public override FileType FileType
        {
            get { return FileType.Json; }
        }

        /// <summary>
        /// Deserialize the given <see cref="NodeContent"/> to a <typeparamref name="TTileset"/> instance.
        /// </summary>
        /// <param name="nodeContent">The node content that should be loaded by the content loader.</param>
        /// <returns>The newly created instance corresponding to the loaded content.</returns>
        public override async Task<TTile> LoadRootAsync(NodeContent nodeContent)
        {
            Uri uri = (nodeContent as RootContent)?.Uri;

            string tilesetJson = await PathUtility.DownloadFileText(uri);

            TaskCompletionSource<TTile> tcs = new TaskCompletionSource<TTile>();
            ScheduleTask(() => ParseJson(tilesetJson, tcs));

            return await tcs.Task;
        }

        /// <summary>
        /// Convert a serialized text to a <typeparamref name="TTileset"/> instance.
        /// </summary>
        /// <param name="tilesetJson">Text to deserialize.</param>
        /// <param name="result">The result of the deserialization will be outputted into this variable.</param>
        private static void ParseJson(string tilesetJson, TaskCompletionSource<TTile> result)
        {
            TTileset tilesetSchema = default;

            try
            {
                tilesetSchema = JsonConvert.DeserializeObject<TTileset>(tilesetJson);
            }
            catch (JsonReaderException)
            {
                // Skip
            }

            Assert.IsFalse(tilesetSchema == null);
            Assert.IsFalse(tilesetSchema.Asset == null);

            TTile root = tilesetSchema.GetRoot<TTile>();
            Assert.IsFalse(root == null);

            result.SetResult(root);
        }
    }
}
