
namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    public class ContentSchema :
        IContentSchema
    {
        /// <inheritdoc cref="IContentSchema.GetUri"/>
        public string Uri { get; set; }

        /// <inheritdoc cref="IContentSchema.GetUri"/>
        public string Url { get; set; }

        /// <inheritdoc cref="IContentSchema.GetUri"/>
        public string GetUri()
        {
            return Uri ?? Url;
        }
    }
}
