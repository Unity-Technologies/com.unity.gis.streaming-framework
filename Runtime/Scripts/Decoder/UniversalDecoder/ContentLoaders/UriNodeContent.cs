
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// <see cref="NodeContent"/> associated with a specific <see cref="UriLoader"/>.
    /// </summary>
    public class UriNodeContent : NodeContent
    {
        /// <summary>
        /// The expected position of the node once loaded.
        /// </summary>
        public double4x4 Transform { get; }

        /// <summary>
        /// The URIs of the given content to load.
        /// </summary>
        public IUriCollection Uri { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="type">Type identifier allowing to figure which <see cref="INodeContentLoader"/> to execute when loading this content.</param>
        /// <param name="dataSource">The <see cref="UGDataSource"/> instance id this <see cref="NodeContent"/> refers to.</param>
        /// <param name="bounds">Limits of the node in space that will be evaluated within each <see cref="UGSceneObserver"/>.</param>
        /// <param name="geometricError">If the evaluated screen space error is higher than this value, the node will not be expanded.</param>
        /// <param name="transform">Where to place the node once loaded.</param>
        /// <param name="uris"><see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> providing where the resource is accessible.</param>
        public UriNodeContent(ContentType type, UGDataSourceID dataSource, in DoubleBounds bounds, float geometricError, double4x4 transform, IUriCollection uris) :
            base(type, dataSource, bounds, geometricError)
        {
            Transform = transform;
            Uri = uris;
        }
    }
}
