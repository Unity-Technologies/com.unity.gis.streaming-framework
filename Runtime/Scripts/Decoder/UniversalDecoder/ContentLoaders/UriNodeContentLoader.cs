using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Classes with <see cref="NodeId"/> <see cref="LoadNodeAsync">load</see> / <see cref="UnloadNode">unload</see> capabilities.
    /// based on a <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see>.
    /// </summary>
    public class UriNodeContentLoader : INodeContentLoader
    {
        /// <summary>
        /// <see cref="UriLoader"/> to use when loading / unloading nodes.
        /// </summary>
        private readonly UriLoader m_UriLoader;

        /// <summary>
        /// <see cref="InstanceID"/> currently loaded allowing to call <see cref="UnloadNode"/> with the <see cref="NodeId"/>.
        /// </summary>
        private readonly Dictionary<NodeId, InstanceID> m_Instances = new Dictionary<NodeId, InstanceID>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="uriLoader"><see cref="UriLoader"/> to use when loading / unloading nodes.</param>
        public UriNodeContentLoader(UriLoader uriLoader)
        {
            m_UriLoader = uriLoader;
        }

        /// <inheritdoc cref="INodeContentLoader.LoadNodeAsync"/>
        public async Task<InstanceID> LoadNodeAsync(NodeId nodeId, NodeContent nodeContent)
        {
            if (!(nodeContent is UriNodeContent uriNodeContent))
                throw new InvalidOperationException($"{nameof(UriNodeContentLoader)} can only be used to load node content of type {nameof(UriNodeContent)}");

            InstanceID instanceId = await m_UriLoader.LoadAsync(nodeId, uriNodeContent, uriNodeContent.Transform);

            m_Instances.Add(nodeId, instanceId);

            return instanceId;
        }

        /// <inheritdoc cref="INodeContentLoader.UnloadNode"/>
        public void UnloadNode(NodeId nodeId)
        {
            if (!m_Instances.TryGetValue(nodeId, out InstanceID instanceId))
                throw new InvalidOperationException("Cannot unload node that has not been previously loaded");

            m_UriLoader.Unload(instanceId);

            m_Instances.Remove(nodeId);
        }
    }
}
