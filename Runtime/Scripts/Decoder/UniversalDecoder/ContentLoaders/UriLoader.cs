using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Loader to use to load data based on a <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see>.
    /// This loader is intended to be used with a <see cref="INodeContentLoader"/> allowing to reuse <see cref="UriLoader"/>
    /// for multiple <see cref="INodeContentLoader"/>.
    /// </summary>
    public abstract class UriLoader
    {
        /// <summary>
        /// Returns the files types that are supported by this content loader.
        /// </summary>
        public abstract IEnumerable<FileType> SupportedFileTypes { get; }

        /// <summary>
        /// Reference to the command buffer the <see cref="UriLoader"/> should publish it's requests to.
        /// </summary>
        protected ILoaderActions LoaderActions { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="loaderActions">
        /// Reference to the command buffer the <see cref="UriLoader"/> should publish it's requests to.
        /// </param>
        protected UriLoader(ILoaderActions loaderActions)
        {
            LoaderActions = loaderActions;
        }

        /// <summary>
        /// Load the content specified by the uri. Returns a task which, when completed,
        /// will return the InstanceId of the generated instance.
        /// </summary>
        /// <param name="nodeId"><see cref="BoundingVolumeHierarchy{T}"/> <see cref="NodeId"/> requested to be loaded.</param>
        /// <param name="content">Content holding the <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> and transform information.</param>
        /// <param name="transform">The transform to be applied to the underlying geometry</param>
        /// <returns>The instance ID of the resulting geometry.</returns>
        public abstract Task<InstanceID> LoadAsync(NodeId nodeId, UriNodeContent content, double4x4 transform);

        /// <summary>
        /// Unload the specified instanceId
        /// </summary>
        /// <param name="instanceId">The instance Id to be unloaded</param>
        public abstract void Unload(InstanceID instanceId);
    }
}
