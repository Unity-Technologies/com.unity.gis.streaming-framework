using System;
using System.Collections.Generic;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Interface to manage the loading / unloading of <see cref="NodeId"/> content via the <see cref="UGSystem"/> <see cref="ITaskManager"/>.
    /// </summary>
    public interface INodeContentManager:
        ILoaderActions
    {
        /// <summary>
        /// Create a new unique <see cref="ContentType"/> instance.
        /// </summary>
        /// <returns>The newly created <see cref="ContentType"/>.</returns>
        ContentType GenerateContentType();

        /// <summary>
        /// Get a list of <see cref="NodeId"/> defined as child of the given <paramref name="parent"/>.
        /// </summary>
        /// <param name="parent">Owner of the returned children.</param>
        /// <param name="children">Result of the search.</param>
        void GetChildren(NodeId parent, List<NodeId> children);

        /// <summary>
        /// The number of nodes currently in the process of being <see cref="Load">Loaded</see>. This includes nodes which are
        /// waiting on server requests or going through some internal processing.
        /// </summary>
        int LoadingCount { get; }

        /// <summary>
        /// Load the provided node and its corresponding content. Once loaded,
        /// it will still not be visible.
        /// </summary>
        /// <param name="nodeId">The node Id to be loaded</param>
        /// <param name="content">The content to be loaded</param>
        void Load(NodeId nodeId, NodeContent content);

        /// <summary>
        /// Register a new content loader which can be used to decode data.
        /// </summary>
        /// <param name="contentType">Associated unique content id for the given <paramref name="contentLoader"/>.</param>
        /// <param name="contentLoader">The <see cref="INodeContentLoader"/> to be registered.</param>
        void RegisterLoader(ContentType contentType, INodeContentLoader contentLoader);

        /// <summary>
        /// Append a task to the list to be executed.
        /// </summary>
        /// <param name="task">Task to be executed. Will be asynchronous if the <see cref="INodeContentManager"/> supports it.</param>
        void ScheduleTask(Action task);

        /// <summary>
        /// Unload the provided node
        /// </summary>
        /// <param name="nodeId">The node id to be unloaded.</param>
        void Unload(NodeId nodeId);

        /// <summary>
        /// Update the visibility of the nodes. This is all guaranteed to happen
        /// within the same frame to avoid visual transition artifacts.
        /// </summary>
        /// <param name="visible">The nodes which should be made visible.</param>
        /// <param name="hidden">The nodes which should be hidden.</param>
        void UpdateVisibility(IEnumerable<NodeId> visible, IEnumerable<NodeId> hidden);
    }

}
