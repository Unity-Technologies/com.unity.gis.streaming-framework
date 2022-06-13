
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Data source information required by the <see cref="GltfContentLoader"/> to decoder its content.
    /// </summary>
    public class GltfDataSource : UniversalDecoderDataSource
    {
        /// <summary>
        /// The largest possible bounds to containe everything.
        /// </summary>
        private static readonly DoubleBounds k_UniverseBounds = new DoubleBounds(double3.zero, new double3());

        /// <summary>
        /// Address of the .gltf / .glb file to decoder.
        /// </summary>
        public string Uri { get; private set; }

        /// <summary>
        /// Where to position, orient and resize the loaded data.
        /// </summary>
        public double4x4 Transform { get; private set; }

        /// <summary>
        /// Lighting type to apply to the shading.
        /// </summary>
        public UGLighting Lighting { get; private set; }

        /// <summary>
        /// Texture options to apply for each imported texture.
        /// </summary>
        public UGTextureSettings TextureSettings { get; private set; }


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="uri">Address of the .gltf / .glb file to decoder.</param>
        /// <param name="position">Where to position the loaded data.</param>
        /// <param name="rotation">Orientation to apply after loading the file.</param>
        /// <param name="scale">Resize the loaded content by this ratio by axis.</param>
        /// <param name="lighting">Lighting type to apply to the shading.</param>
        /// <param name="textureSettings">Texture options to apply for each imported texture.</param>
        /// <param name="dataSourceId">Scriptable object associated with this instance.</param>
        public GltfDataSource(string uri, double3 position, quaternion rotation, float3 scale, UGLighting lighting, UGTextureSettings textureSettings, UGDataSourceID dataSourceId) :
            base(dataSourceId)
        {
            Uri = uri;
            Transform = HPMath.TRS(position, rotation, scale);
            Lighting = lighting;
            TextureSettings = textureSettings;
        }

        /// <inheritdoc cref="UniversalDecoderDataSource.InitializerDecoder"/>
        public override void InitializerDecoder(NodeContentManager contentManager)
        {
            ContentType contentType = contentManager.ContentTypeGenerator.Generate();

            //
            //  Build and register the loader.
            //
            GltfContentLoader contentLoader = new GltfContentLoader(contentManager, DataSourceID, Lighting, TextureSettings);
            contentManager.RegisterLoader(contentType, contentLoader);

            //
            //  Build and add the root node to the hierarchy.
            //
            NodeContent content = new GltfContentLoader.NodeContent(contentType, DataSourceID, k_UniverseBounds, 0.0f, Transform, new SingleUri(Uri));
            NodeData nodeData = new NodeData(content, RefinementMode.Add);
            contentManager.AddNode(contentManager.GetRootNode(), nodeData, content);
        }
    }
}
