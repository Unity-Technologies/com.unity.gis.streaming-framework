using System;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Unity.Geospatial.Streaming.UniversalDecoder;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming.TmsTerrain
{
    /// <summary>
    /// Main class used when loading TMS (Terrain Management System) Terrain file file format.
    /// </summary>
    public class TmsTerrainContentLoader :
        HierarchyContentLoader<Tile, UriCollection>
    {
        /// <summary>
        /// Constructor with double4x4.identity set as the adjustment matrix.
        /// </summary>
        /// <param name="contentManager"><see cref="NodeContentManager"/> to use when loading a tile.</param>
        /// <param name="contentType">Type identifier allowing to figure which <see cref="INodeContentLoader"/> to execute when loading this content.</param>
        /// <param name="createHierarchyOnDemand">
        /// <see langword="true"/> to create the <see cref="BoundingVolumeHierarchy{T}"/> nodes only when the parent node get loaded;
        /// <see langword="false"/> to create all the nodes when <see cref="HierarchyContentLoader{TLeaf,TUri}.LoadNodeAsync(NodeId, NodeContent)"/> is executed for a <see cref="HierarchyContentLoader{TLeaf,TUri}.RootContent"/> instance.
        /// </param>
        public TmsTerrainContentLoader(INodeContentManager contentManager, ContentType contentType, bool createHierarchyOnDemand = true) :
            base(contentManager, contentType, double4x4.identity, createHierarchyOnDemand) { }

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
        public TmsTerrainContentLoader(INodeContentManager contentManager, ContentType contentType, double4x4 adjustmentMatrix, bool createHierarchyOnDemand = true) :
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
        /// Deserialize the given <see cref="NodeContent"/> to a <see cref="Tile"/> instance.
        /// </summary>
        /// <param name="nodeContent">The node content that should be loaded by the content loader.</param>
        /// <returns>The newly created instance corresponding to the loaded content.</returns>
        public override async Task<Tile> LoadRootAsync(NodeContent nodeContent)
        {
            Uri uri = (nodeContent as RootContent)?.Uri;

            string tilesetJson = await PathUtility.DownloadFileText(uri);

            TaskCompletionSource<Tile> tcs = new TaskCompletionSource<Tile>();
            ScheduleTask(() => ParseJson(tilesetJson, tcs));

            return await tcs.Task;
        }

        /// <summary>
        /// Convert a serialized text to a <see cref="Tile"/> instance.
        /// </summary>
        /// <param name="tilesetJson">Text to deserialize.</param>
        /// <param name="result">The result of the deserialization will be outputted into this variable.</param>
        private static void ParseJson(string tilesetJson, TaskCompletionSource<Tile> result)
        {
            ConfigSchema tilesConfig = null;
            try
            {
                tilesConfig = JsonConvert.DeserializeObject<ConfigSchema>(tilesetJson);
            }
            catch (JsonReaderException)
            {
                // Skip
            }

            Assert.IsFalse(tilesConfig == null);

            Tile tile = tilesConfig.GetRoot();
            Assert.IsFalse(tile == null);

            result.SetResult(tile);
        }
    }
}
