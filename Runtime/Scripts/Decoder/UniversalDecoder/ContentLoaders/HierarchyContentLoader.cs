
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Main class used to load hierarchy structured dataset allowing to load linked sub-files via registered <see cref="UriLoader"/>.
    /// This class is able to load a schema based on your implementation (json, yaml, txt) and load it's referenced files
    /// has long you register a <see cref="UriLoader"/> for each <see cref="FileType"/> the
    /// <typeparamref name="TLeaf"/>.<see cref="ILeaf.GetUriCollection()">GetUriCollection</see> returns.
    /// </summary>
    /// <typeparam name="TLeaf">Each time a child representing a node to be potentially loaded, an instance of this type will be returned.
    /// See <see cref="ILeaf.GetChildren"/></typeparam>
    /// <typeparam name="TUriCollection">Type of <see cref="IUriCollection"/> used to load the content of each <typeparamref name="TLeaf"/></typeparam>
    public abstract class HierarchyContentLoader<TLeaf, TUriCollection> :
        INodeContentLoader
        where TLeaf: ILeaf
        where TUriCollection: IUriCollection
    {

        /// <summary>
        /// <see cref="NodeContent"/> used for holding tileset loading directives.
        /// </summary>
        internal class RootContent : NodeContent
        {
            /// <summary>
            /// Multiply the geometric error by this value allowing to compensate when other data sources gives too different results.
            /// </summary>
            internal float DetailMultiplier { get; }

            /// <summary>
            /// Get the transform of the given content where to position the content when loaded.
            /// </summary>
            internal double4x4 Transform { get; }

            /// <summary>
            /// Obtain the Uri of the given content allowing to point to the stream source paths.
            /// </summary>
            internal Uri Uri { get; }

            /// <summary>
            /// Constructor requesting no need to evaluate the screen space error since it will be considered always to be expanded.
            /// </summary>
            /// <param name="type">Type identifier allowing to figure which <see cref="INodeContentLoader"/> to execute when loading this content.</param>
            /// <param name="dataSource">Unique identifier allowing to do indirect assignment.</param>
            /// <param name="transform">Where to place the node once loaded.</param>
            /// <param name="detailMultiplier">Multiply the geometric error by this value allowing to compensate when other data sources gives too different results.</param>
            /// <param name="uri"><see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> providing where the resource is accessible.</param>
            internal RootContent(ContentType type, UGDataSourceID dataSource, double4x4 transform, float detailMultiplier, Uri uri) :
                base(type, dataSource)
            {
                Uri = uri;
                DetailMultiplier = detailMultiplier;
                Transform = transform;
            }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="type">Type identifier allowing to figure which <see cref="INodeContentLoader"/> to execute when loading this content.</param>
            /// <param name="dataSource">Unique identifier allowing to do indirect assignment.</param>
            /// <param name="transform">Where to place the node once loaded.</param>
            /// <param name="bounds">Limits of the node in space that will be evaluated within each <see cref="UGSceneObserver"/>.</param>
            /// <param name="geometricError">If the evaluated screen space error is higher than this value, the node will not be expanded.</param>
            /// <param name="detailMultiplier">Multiply the geometric error by this value allowing to compensate when other data sources gives too different results.</param>
            /// <param name="uri"><see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> providing where the resource is accessible.</param>
            internal RootContent(ContentType type, UGDataSourceID dataSource, double4x4 transform, in DoubleBounds bounds, float geometricError, float detailMultiplier, Uri uri) :
                base(type, dataSource, in bounds, geometricError)
            {
                Uri = uri;
                DetailMultiplier = detailMultiplier;
                Transform = transform;
            }

            /// <summary>
            /// Get the directory name of the first <see cref="System.Uri"/> part of this content.
            /// </summary>
            /// <returns>The base path of the Uri.</returns>
            internal Uri GetBaseUri()
            {
                return Uri is null
                    ? null
                    : GLTFast.UriHelper.GetBaseUri(Uri);
            }
        }

        /// <summary>
        /// <see cref="NodeContent"/> associated with a specific <see cref="UriLoader"/> and children information
        /// allowing to generate the hierarchy on demand.
        /// </summary>
        internal class ExpandingNodeContent :
            UriNodeContent,
            IExpandingNodeContent
        {
            /// <summary>
            /// Top parent <see cref="NodeContent"/> part of the same dataset.
            /// </summary>
            public RootContent Root { get; }

            /// <inheritdoc cref="IExpandingNodeContent.Leaf"/>
            public ILeaf Leaf { get; }

            /// <inheritdoc cref="IExpandingNodeContent.InheritedRefineMode"/>
            public RefinementMode InheritedRefineMode { get; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="type">Type identifier allowing to figure which <see cref="INodeContentLoader"/> to execute when loading this content.</param>
            /// <param name="dataSource">The <see cref="UGDataSource"/> instance id this <see cref="NodeContent"/> refers to.</param>
            /// <param name="bounds">Limits of the node in space that will be evaluated within each <see cref="UGSceneObserver"/>.</param>
            /// <param name="geometricError">If the evaluated screen space error is higher than this value, the node will not be expanded.</param>
            /// <param name="inheritedRefineMode">The refinement mode of the node defining inheritance to children if not defined.</param>
            /// <param name="transform">Where to place the node once loaded.</param>
            /// <param name="uris"><see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> providing where the resource is accessible.</param>
            /// <param name="rootContent"><see cref="NodeContent"/> holding the root information needed to create child content.</param>
            /// <param name="leaf">Item allowed to iterate through its children <see cref="ILeaf"/>.</param>
            public ExpandingNodeContent(
                ContentType type,
                UGDataSourceID dataSource,
                in DoubleBounds bounds,
                float geometricError,
                RefinementMode inheritedRefineMode,
                double4x4 transform,
                IUriCollection uris,
                RootContent rootContent,
                ILeaf leaf) :
                base(type, dataSource, bounds, geometricError, transform, uris)
            {
                Leaf = leaf;
                Root = rootContent;
                InheritedRefineMode = inheritedRefineMode;
            }
        }

        /// <summary>
        /// Link a <see cref="UriLoader"/> with a <see cref="ContentType"/> allowing to load
        /// linked <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> within the tileset via
        /// a <see cref="UniversalDecoder.UriLoader"/>.
        /// </summary>
        internal readonly struct SubContentLoader
        {
            /// <summary>
            /// Associated unique content id for this loader.
            /// </summary>
            public ContentType ContentType { get; }

            /// <summary>
            /// Loader to use to <see cref="UniversalDecoder.UriLoader.LoadAsync(NodeId, UriNodeContent, double4x4)"/> / <see cref="UniversalDecoder.UriLoader.Unload"/>
            /// </summary>
            public UriLoader UriLoader { get; }

            /// <summary>
            /// Loader to use to <see cref="INodeContentLoader.LoadNodeAsync"/> / <see cref="INodeContentLoader.UnloadNode"/>
            /// </summary>
            public INodeContentLoader NodeLoader { get; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="contentType">Associate a unique content id with the given <paramref name="uriLoader"/>.</param>
            /// <param name="uriLoader"></param>
            public SubContentLoader(ContentType contentType, UriLoader uriLoader)
            {
                ContentType = contentType;
                UriLoader = uriLoader;
                NodeLoader = new UriNodeContentLoader(uriLoader);
            }
        }

        /// <summary>
        /// Link an <see cref="InstanceId"/> with its corresponding <see cref="UriLoader"/> allowing to call its respective
        /// <see cref="UniversalDecoder.UriLoader.LoadAsync"/> / <see cref="UniversalDecoder.UriLoader.Unload"/>
        /// </summary>
        private readonly struct LoadedInstance
        {
            /// <summary>
            /// The instance ID of the loaded geometry.
            /// </summary>
            public InstanceID InstanceId { get; }

            /// <summary>
            /// Loader to use to <see cref="UniversalDecoder.UriLoader.LoadAsync"/> / <see cref="UniversalDecoder.UriLoader.Unload"/>
            /// </summary>
            public UriLoader UriLoader { get; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="instanceId">The instance ID of the loaded geometry.</param>
            /// <param name="uriLoader">Loader to use to <see cref="UniversalDecoder.UriLoader.LoadAsync"/> / <see cref="UniversalDecoder.UriLoader.Unload"/></param>
            public LoadedInstance(InstanceID instanceId, UriLoader uriLoader)
            {
                InstanceId = instanceId;
                UriLoader = uriLoader;
            }
        }

        /// <summary>
        /// Add to a queue the directives on how to load an <see cref="ILeaf"/>.
        /// </summary>
        private readonly struct QueueItem
        {
            /// <summary>
            /// Where to position the instance.
            /// </summary>
            public double4x4 Transform { get; }

            /// <summary>
            /// <see cref="NodeId"/> of the parent.
            /// </summary>
            public NodeId ParentNode { get; }

            /// <summary>
            /// <see cref="ILeaf">Item</see> requested to be loaded.
            /// </summary>
            public ILeaf Item { get; }

            /// <summary>
            /// <see cref="RefinementMode"/> of the parent tile.
            /// </summary>
            public RefinementMode InheritedRefineMode { get; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="parent"><see cref="NodeId"/> of the parent.</param>
            /// <param name="transform">Where to position the instance.</param>
            /// <param name="item"><see cref="ILeaf">tile</see> requested to be loaded.</param>
            /// <param name="inheritedRefineMode"><see cref="RefinementMode"/> of the parent tile.</param>
            public QueueItem(NodeId parent, double4x4 transform, ILeaf item, RefinementMode inheritedRefineMode)
            {
                ParentNode = parent;
                Transform = transform;
                Item = item;
                InheritedRefineMode = inheritedRefineMode;
            }
        }

        /// <summary>
        /// Each time a node is loaded, multiply its transform by this matrix allowing axis alignments when the format is not left-handed, Y-Up.
        /// </summary>
        private readonly double4x4 m_AdjustmentMatrix;

        /// <summary>
        /// <see cref="INodeContentManager"/> to use when loading a tile.
        /// </summary>
        private readonly INodeContentManager m_ContentManager;

        /// <summary>
        /// Type identifier allowing to figure which <see cref="INodeContentLoader"/> to execute when loading this content.
        /// </summary>
        public ContentType ContentType { get; }

        /// <summary>
        /// Associated <see cref="FileType"/> with its corresponding <see cref="SubContentLoader"/> allowing to customize
        /// the loader to use per <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> type.
        /// </summary>
        internal readonly Dictionary<FileType, SubContentLoader> LoaderByFileType = new Dictionary<FileType, SubContentLoader>();

        /// <summary>
        /// Associated <see cref="ContentType"/> with its corresponding <see cref="SubContentLoader"/> allowing to load
        /// <see cref="NodeContent"/> without having to evaluate a given <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see>
        /// if it was already figured out.
        /// </summary>
        private readonly Dictionary<ContentType, SubContentLoader> m_LoaderByContentType = new Dictionary<ContentType, SubContentLoader>();

        /// <summary>
        /// This dictionary contains information pertaining to instances which have been loaded
        /// in order to be able to unload them.
        /// </summary>
        private readonly Dictionary<NodeId, LoadedInstance> m_LoadedInstances = new Dictionary<NodeId, LoadedInstance>();

        /// <summary>
        /// List of <see cref="NodeId"/> used to <see cref="UnloadNode">unload</see>.
        /// This list is used as a queue list allowing recursive execution in a single loop.
        /// </summary>
        private readonly List<NodeId> m_WorkingNodeList = new List<NodeId>();

        private readonly bool m_CreateHierarchyOnDemand;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="contentManager"><see cref="INodeContentManager"/> to use when loading a tile.</param>
        /// <param name="contentType">Type identifier allowing to figure which <see cref="INodeContentLoader"/> to execute when loading this content.</param>
        /// <param name="adjustmentMatrix">Each time a node is loaded, multiply its transform by this matrix allowing axis alignments when the format is not left-handed, Y-Up.</param>
        /// <param name="createHierarchyOnDemand">
        /// <see langword="true"/> to create the <see cref="BoundingVolumeHierarchy{T}"/> nodes only when the parent node get loaded;
        /// <see langword="false"/> to create all the nodes when <see cref="LoadNodeAsync(NodeId, NodeContent)"/> is executed for a <see cref="RootContent"/> instance.
        /// </param>
        protected HierarchyContentLoader(INodeContentManager contentManager, ContentType contentType, double4x4 adjustmentMatrix, bool createHierarchyOnDemand = true)
        {
            m_ContentManager = contentManager;
            ContentType = contentType;
            m_AdjustmentMatrix = adjustmentMatrix;
            m_CreateHierarchyOnDemand = createHierarchyOnDemand;
        }

        /// <summary>
        /// Create a node child of the content manager root node.
        /// </summary>
        /// <param name="contentManager">Register the new node inside via this manager.</param>
        /// <param name="contentType">Type identifier allowing to figure which <see cref="INodeContentLoader"/> to execute when loading this content.</param>
        /// <param name="dataSource">Unique identifier allowing to do indirect assignment.</param>
        /// <param name="transform">Where to place the node once loaded.</param>
        /// <param name="detailMultiplier">Multiply the geometric error by this value allowing to compensate when other data sources gives too different results.</param>
        /// <param name="uri"><see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> providing where the resource is accessible.</param>
        /// <param name="refinementMode">Refinement mode to be applied when expanded.</param>
        public void AddTopLevelNode(NodeContentManager contentManager, ContentType contentType, UGDataSourceID dataSource, double4x4 transform, float detailMultiplier, Uri uri, RefinementMode refinementMode)
        {
            NodeContent content = new RootContent(
                contentType,
                dataSource,
                transform,
                detailMultiplier,
                uri);

            NodeData nodeData = new NodeData(content, refinementMode);
            contentManager.AddNode(contentManager.GetRootNode(), nodeData, content);
        }

        /// <summary>
        /// Create a node child of the content manager root node.
        /// </summary>
        /// <param name="contentManager">Register the new node inside via this manager.</param>
        /// <param name="contentType">Type identifier allowing to figure which <see cref="INodeContentLoader"/> to execute when loading this content.</param>
        /// <param name="dataSource">Unique identifier allowing to do indirect assignment.</param>
        /// <param name="transform">Where to place the node once loaded.</param>
        /// <param name="bounds">Limits of the node in space that will be evaluated within each <see cref="UGSceneObserver"/>.</param>
        /// <param name="geometricError">If the evaluated screen space error is higher than this value, the node will not be expanded.</param>
        /// <param name="detailMultiplier">Multiply the geometric error by this value allowing to compensate when other data sources gives too different results.</param>
        /// <param name="uri"><see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> providing where the resource is accessible.</param>
        /// <param name="refinementMode">Refinement mode to be applied when expanded.</param>
        internal void AddTopLevelNode(NodeContentManager contentManager, ContentType contentType, UGDataSourceID dataSource, double4x4 transform, in DoubleBounds bounds, float geometricError, float detailMultiplier, Uri uri, RefinementMode refinementMode)
        {
            NodeContent content = new RootContent(
                contentType,
                dataSource,
                transform,
                bounds,
                geometricError,
                detailMultiplier,
                uri);

            NodeData nodeData = new NodeData(content, refinementMode);
            contentManager.AddNode(contentManager.GetRootNode(), nodeData, content);
        }

        /// <summary>
        /// Register a new <see cref="InstanceID"/> and link it with its corresponding <see cref="NodeId"/>.
        /// </summary>
        /// <remarks>This is required whenever <see cref="UriLoader.LoadAsync"/> is called allowing <see cref="UnloadNode"/> to be executed.</remarks>
        /// <param name="nodeId">Link the given <paramref name="instanceId"/> with this node.</param>
        /// <param name="instanceId">Link the given <paramref name="nodeId"/> with this instance.</param>
        /// <param name="uriLoader">Loader that was used to create the instance.</param>
        private void RegisterInstance(NodeId nodeId, InstanceID instanceId, UriLoader uriLoader)
        {
            m_LoadedInstances.Add(nodeId, new LoadedInstance(instanceId, uriLoader));
        }

        /// <summary>
        /// <see cref="UniversalDecoder.FileType"/> associated with this loader allowing to call within
        /// <see cref="INodeContentLoader.LoadNodeAsync(NodeId, NodeContent)"/> when the
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> is of the same type.
        /// </summary>
        public abstract FileType FileType { get; }

        /// <summary>
        /// Get the loader to use when <see cref="UriLoader.LoadAsync"/> / <see cref="UriLoader.Unload"/> is requested.
        /// </summary>
        /// <remarks>This method require <see cref="UriLoader"/> instances to be registered via <see cref="RegisterUriLoader"/>.</remarks>
        /// <param name="uris">Search the <see cref="UriLoader"/> to use for loading these files.</param>
        /// <returns>The <see cref="UriLoader"/> registered to be used for the given <paramref name="uris"/>.</returns>
        /// <exception cref="NotSupportedException">If no <see cref="UriLoader"/> was registered for the given <paramref name="uris"/>.</exception>
        private SubContentLoader GetLoader(TUriCollection uris)
        {
            FileType type = uris.GetFileType();

            if (type is null || !LoaderByFileType.TryGetValue(type, out SubContentLoader loader))
                throw new NotSupportedException($"Unsupported file type for {uris}.");

            return loader;
        }

        /// <summary>
        /// Get the loader to use when <see cref="UriLoader.LoadAsync"/> / <see cref="UriLoader.Unload"/> is requested.
        /// </summary>
        /// <remarks>This method require <see cref="UriLoader"/> instances to be registered via <see cref="RegisterUriLoader"/>.</remarks>
        /// <param name="type">Search the <see cref="UriLoader"/> to use for loading this content type.</param>
        /// <returns>The <see cref="UriLoader"/> registered to be used for the given <paramref name="type"/>.</returns>
        /// <exception cref="NotSupportedException">If no <see cref="UriLoader"/> was registered for the given <paramref name="type"/>.</exception>
        private UriLoader GetLoader(ContentType type)
        {
            if (!m_LoaderByContentType.TryGetValue(type, out SubContentLoader loader))
                throw new NotSupportedException($"Unsupported file type for {type}.");

            return loader.UriLoader;
        }

        /// <summary>
        /// Based on the node content, get where the content should be loaded.
        /// </summary>
        /// <remarks>If you want a different behavior, override this method. It will be called before each load.</remarks>
        /// <param name="content">Content loaded from the <typeparamref name="TLeaf"/>.</param>
        /// <param name="transform">Value usually get from the <typeparamref name="TLeaf"/>.</param>
        /// <param name="adjustment">Adjustment matrix to apply to all transform.</param>
        /// <returns>Where to place the instance once loaded.</returns>
        protected virtual double4x4 GetTransform(NodeContent content, double4x4 transform, double4x4 adjustment)
        {
            return math.mul(transform, m_AdjustmentMatrix);
        }

        /// <inheritdoc cref="INodeContentLoader.LoadNodeAsync(NodeId, NodeContent)"/>
        public async Task<InstanceID> LoadNodeAsync(NodeId nodeId, NodeContent nodeContent)
        {
            return nodeContent switch
            {
                ExpandingNodeContent parentContent => await LoadNodeAsync(nodeId, parentContent),
                RootContent rootContent => await LoadNodeAsync(nodeId, rootContent),
                UriNodeContent _ => throw new NotSupportedException($"{nameof(UriNodeContent)} cannot be loaded via {GetType()}"),
                _ => throw new NotSupportedException($"{nodeContent} Type has no registered loader.")
            };
        }

        /// <summary>
        /// Load the given NodeId given the provided NodeContent.
        /// </summary>
        /// <param name="nodeId">The NodeId of the node to be loaded. This should come directly from the bounding
        /// volume hierarchy.</param>
        /// <param name="nodeContent">The node content that should be loaded by the content loader.</param>
        /// <returns>The newly created instance corresponding to the loaded content.</returns>
        private async Task<InstanceID> LoadNodeAsync(NodeId nodeId, ExpandingNodeContent nodeContent)
        {
            UriLoader loader = GetLoader(nodeContent.Type);

            InstanceID result = await loader.LoadAsync(nodeId, nodeContent, nodeContent.Transform);

            RegisterInstance(nodeId, result, loader);

            RefinementMode refine = nodeContent.InheritedRefineMode;

            Queue<QueueItem> tileQueue = new Queue<QueueItem>();

            double4x4 transform = math.mul(nodeContent.Transform, m_AdjustmentMatrix);

            foreach (ILeaf child in nodeContent.Leaf.GetChildren())
                tileQueue.Enqueue(new QueueItem(nodeId, transform, child, child.GetRefinement(refine)));

            while (tileQueue.Count > 0)
                ProcessQueueItem(tileQueue, nodeContent.Root.GetBaseUri(), nodeContent.Root);

            return result;
        }

        /// <summary>
        /// Load the given NodeId given the provided NodeContent.
        /// </summary>
        /// <param name="nodeId">The NodeId of the node to be loaded. This should come directly from the bounding
        /// volume hierarchy.</param>
        /// <param name="nodeContent">The node content that should be loaded by the content loader.</param>
        /// <returns>The newly created instance corresponding to the loaded content.</returns>
        private async Task<InstanceID> LoadNodeAsync(NodeId nodeId, RootContent nodeContent)
        {
            TLeaf root = await LoadRootAsync(nodeContent);

            Assert.IsFalse(root == null, $"Failed to read {nodeContent.Uri}");

            Uri baseUri = nodeContent.GetBaseUri();

            double4x4 transform = math.mul(nodeContent.Transform, m_AdjustmentMatrix);

            double4x4 rootTransform = math.mul(transform, root.GetTransform());
            RefinementMode rootRefine = root.GetRefinement(default);

            nodeContent.Bounds = root.GetBoundingVolume(rootTransform);
            nodeContent.GeometricError = root.GetGeometricError() * nodeContent.DetailMultiplier;

            NodeData rootData = new NodeData(
                nodeContent.Bounds,
                nodeContent.GeometricError,
                root.GetRefinement(default),
                nodeContent.AlwaysExpand);

            m_ContentManager.UpdateNode(nodeId, rootData);

            Queue<QueueItem> tileQueue = new Queue<QueueItem>();

            foreach (ILeaf child in root.GetChildren())
                tileQueue.Enqueue(new QueueItem(nodeId, rootTransform, child, rootRefine));

            while (tileQueue.Count > 0)
                ProcessQueueItem(tileQueue, baseUri, nodeContent);

            TUriCollection uris = (TUriCollection)root.GetUriCollection(baseUri);

            return uris.HasContent()
                ? await LoadRootInstanceAsync(nodeId, uris, rootTransform, nodeContent)
                : InstanceID.Null;
        }

        /// <summary>
        /// Load the <see cref="ILeaf"/> root <see cref="NodeContent"/> content.
        /// </summary>
        /// <param name="nodeId"><see cref="BoundingVolumeHierarchy{T}"/> <see cref="NodeId"/> requested to be loaded.</param>
        /// <param name="uri"><see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> collection of the content to load.</param>
        /// <param name="transform">Position the new instance to these coordinates.</param>
        /// <param name="rootContent">Load the content of this instance.</param>
        /// <returns>The newly created instance corresponding to the loaded content.</returns>
        private async Task<InstanceID> LoadRootInstanceAsync(NodeId nodeId, TUriCollection uri, double4x4 transform, NodeContent rootContent)
        {
            SubContentLoader loader = GetLoader(uri);

            transform = GetTransform(rootContent, transform, m_AdjustmentMatrix);

            UriNodeContent content = new UriNodeContent(
                loader.ContentType, new UGDataSourceID(), rootContent.Bounds, rootContent.GeometricError, transform, uri);

            InstanceID instanceId = await loader.UriLoader.LoadAsync(nodeId, content, transform);

            RegisterInstance(nodeId, instanceId, loader.UriLoader);

            return instanceId;
        }

        /// <summary>
        /// Deserialize the given <see cref="NodeContent"/> to a <typeparamref name="TLeaf"/> instance.
        /// </summary>
        /// <param name="nodeContent">The node content that should be loaded by the content loader.</param>
        /// <returns>The newly created instance corresponding to the loaded content.</returns>
        public abstract Task<TLeaf> LoadRootAsync(NodeContent nodeContent);

        /// <summary>
        /// Load a single <see cref="QueueItem"/> content.
        /// </summary>
        /// <param name="queue">Item to be loaded.</param>
        /// <param name="baseUri"><see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> pointing to the file to load.</param>
        /// <param name="content"><see cref="NodeContent"/> with the node information required to be loaded.</param>
        /// <exception cref="NotSupportedException">If no <see cref="UriLoader"/> is registered for a
        /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> type.</exception>
        private void ProcessQueueItem(Queue<QueueItem> queue, Uri baseUri, RootContent content)
        {


            QueueItem current = queue.Dequeue();
            ILeaf item = current.Item;

            double4x4 localTransform = item.GetTransform();
            double4x4 universeTransform = math.mul(current.Transform, localTransform);

            float geometricError = item.GetGeometricError();
            RefinementMode refinementMode = item.GetRefinement(current.InheritedRefineMode);
            DoubleBounds bounds = item.GetBoundingVolume(universeTransform);

            geometricError *= content.DetailMultiplier;

            NodeData nodeData;

            IUriCollection uriCollection = item.GetUriCollection(baseUri);

            NodeContent nodeContent = null;

            if (!uriCollection.HasContent())
            {
                nodeData = new NodeData(bounds, geometricError);
            }
            else
            {
                FileType type = uriCollection.GetFileType();

                if (type == FileType)
                {
                    nodeContent = new RootContent(ContentType, content.DataSource, math.mul(universeTransform, m_AdjustmentMatrix), content.DetailMultiplier, uriCollection.MainUri);

                    nodeData = new NodeData(nodeContent, refinementMode);
                }
                else if (type != null && LoaderByFileType.TryGetValue(type, out SubContentLoader loader))
                {
                    if (m_CreateHierarchyOnDemand)
                        nodeContent = new ExpandingNodeContent(loader.ContentType, content.DataSource, bounds, geometricError, refinementMode, math.mul(universeTransform, m_AdjustmentMatrix), uriCollection, content, item);
                    else
                        nodeContent = new UriNodeContent(loader.ContentType, content.DataSource, bounds, geometricError, math.mul(universeTransform, m_AdjustmentMatrix), uriCollection);

                    nodeData = new NodeData(nodeContent, refinementMode);
                }
                else
                {
                    throw new NotSupportedException($"Unsupported file type for {uriCollection}.");
                }
            }

            NodeId nodeId = m_ContentManager.AddNode(current.ParentNode, nodeData, nodeContent);

            if (!m_CreateHierarchyOnDemand)
            {
                foreach (ILeaf child in item.GetChildren())
                    queue.Enqueue(new QueueItem(nodeId, universeTransform, child, refinementMode));
            }
        }

        /// <summary>
        /// Register a sub-content loader to be used whenever a <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see>
        /// of the same <see cref="UniversalDecoder.FileType"/> is found.
        /// </summary>
        /// <param name="uriLoader">Loader to be registered.</param>
        public void RegisterUriLoader(UriLoader uriLoader)
        {
            foreach (FileType fileType in uriLoader.SupportedFileTypes)
            {
                ContentType contentType = m_ContentManager.GenerateContentType();

                SubContentLoader subContentLoader = new SubContentLoader(contentType, uriLoader);

                m_ContentManager.RegisterLoader(
                    contentType,
                    m_CreateHierarchyOnDemand ? this : subContentLoader.NodeLoader);

                LoaderByFileType.Add(fileType, subContentLoader);
                m_LoaderByContentType.Add(contentType, subContentLoader);
            }
        }

        /// <summary>
        /// Append a task to the list to be executed by the <see cref="INodeContentManager"/>.
        /// </summary>
        /// <param name="task">Task to be executed. Will be asynchronous if the <see cref="INodeContentManager"/> supports it.</param>
        protected void ScheduleTask(Action task)
        {
            m_ContentManager.ScheduleTask(task);
        }

        /// <summary>
        /// Unload a load and its linked loaded <see cref="InstanceID"/>.
        /// </summary>
        /// <remarks>When <see cref="UriLoader.LoadAsync"/> is called, <see cref="RegisterInstance"/> must have been called.
        /// Otherwise, <see cref="UnloadNode"/> won't work as expected.</remarks>
        /// <param name="nodeId">Node to unload.</param>
        public void UnloadNode(NodeId nodeId)
        {
            Assert.AreEqual(0, m_WorkingNodeList.Count);

            m_ContentManager.GetChildren(nodeId, m_WorkingNodeList);

            foreach (NodeId child in m_WorkingNodeList)
                m_ContentManager.RemoveNode(child);

            m_WorkingNodeList.Clear();

            if (m_LoadedInstances.TryGetValue(nodeId, out LoadedInstance loadedInstance))
            {
                InstanceID instanceId = loadedInstance.InstanceId;
                UriLoader uriLoader = loadedInstance.UriLoader;

                uriLoader.Unload(instanceId);

                m_LoadedInstances.Remove(nodeId);
            }
        }
    }
}
