using System.Collections.Generic;


namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    internal interface ICurrentStateController
    {
        /// <summary>
        /// The number of nodes currently in the process of being loaded. This includes nodes which are
        /// waiting on server requests or going through some internal processing.
        /// </summary>
        public int LoadingCount { get; }

        /// <summary>
        /// Load the specified node.This operation will not be executed immediately
        /// but will be queued up.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the node which should be loaded</param>
        public void Load(NodeId nodeId);

        /// <summary>
        /// Unload the specified node. This operation will not be executed immediately
        /// but will be queued up.
        /// </summary>
        /// <param name="nodeId">The <see cref="NodeId"/> of the node which should be loaded</param>
        public void Unload(NodeId nodeId);

        /// <summary>
        /// Update the visibility of multiple nodes. This operation will not be executed immediately
        /// but will be queued up. However, the state of all the nodes listed is garanteed to be 
        /// executed within the same frame as one another.
        /// </summary>
        /// <param name="visible">The nodes to be made visible</param>
        /// <param name="hidden">The nodes to be hidden</param>
        public void UpdateVisibility(IEnumerable<NodeId> visible, IEnumerable<NodeId> hidden);
    }
}
