using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// The GLTF content loader interprets .gltf and .glb files for the content manager.
    /// </summary>
    public class GltfContentLoader : UriNodeContentLoader
    {
        /// <summary>
        /// <see cref="UriNodeContent"/> used for holding glTF loading directives.
        /// </summary>
        internal class NodeContent : UriNodeContent
        {
            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="type">Type identifier allowing to figure which <see cref="INodeContentLoader"/> to execute when loading this content.</param>
            /// <param name="dataSource">Unique identifier allowing to do indirect assignment.</param>
            /// <param name="bounds">Limits of the node in space that will be evaluated within each <see cref="UGSceneObserver"/>.</param>
            /// <param name="geometricError">If the evaluated screen space error is higher than this value, the node will not be expanded.</param>
            /// <param name="transform">Where to place the node once loaded.</param>
            /// <param name="uris"><see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> providing where the resource is accessible.</param>
            public NodeContent(
                ContentType type, UGDataSourceID dataSource, in DoubleBounds bounds, float geometricError, double4x4 transform, IUriCollection uris) :
                base(type, dataSource, bounds, geometricError, transform, uris) { }
        }

        /// <summary>
        /// Constructor with the adjustment matrix set to identity.
        /// </summary>
        /// <param name="loaderActions">
        /// Reference to the command manager the <see cref="UriLoader"/> should publish it's output to.
        /// </param>
        /// <param name="dataSource">Unique identifier allowing to do indirect assignment.</param>
        /// <param name="lighting">Lighting type to apply to the shading.</param>
        /// <param name="textureSettings">Texture options to apply for each imported texture.</param>
        public GltfContentLoader(ILoaderActions loaderActions, UGDataSourceID dataSource, UGLighting lighting, UGTextureSettings textureSettings) :
            base(new GltfUriLoader(loaderActions, dataSource, lighting, textureSettings, double4x4.identity)) { }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="loaderActions">
        /// Reference to the command manager the <see cref="UriLoader"/> should publish it's output to.
        /// </param>
        /// <param name="dataSource">Unique identifier allowing to do indirect assignment.</param>
        /// <param name="lighting">Lighting type to apply to the shading.</param>
        /// <param name="textureSettings">Texture options to apply for each imported texture.</param>
        /// <param name="adjustmentMatrix">
        /// Each time a node is loaded, multiply its transform by this matrix allowing axis alignments when the format is not left-handed, Y-Up.
        /// </param>
        public GltfContentLoader(ILoaderActions loaderActions, UGDataSourceID dataSource, UGLighting lighting, UGTextureSettings textureSettings, double4x4 adjustmentMatrix) :
            base(new GltfUriLoader(loaderActions, dataSource, lighting, textureSettings, adjustmentMatrix)) { }
    }
}
