
namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    /// <summary>
    /// <see cref="ILeaf"/> <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">Uri</see> getter allowing
    /// to retrieve where to get the data to load for this instance.
    /// </summary>
    public class ContentSchema :
        IContentSchema
    {
        /// <summary>
        /// A uri that points to tile content. When the uri is relative, it is relative to the referring tileset JSON file.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// A uri that points to tile content. When the uri is relative, it is relative to the referring tileset JSON file.
        /// <remarks>This is resolved when the dataset is using "url" key instead of the standard "uri" key.</remarks>
        /// </summary>
        public string Url { get; set; }

        /// <inheritdoc cref="IContentSchema.GetUri"/>
        public string GetUri()
        {
            return Uri ?? Url;
        }
    }
}
