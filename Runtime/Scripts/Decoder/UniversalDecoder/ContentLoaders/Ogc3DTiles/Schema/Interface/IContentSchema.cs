
namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    public interface IContentSchema
    {
        /// <summary>
        /// A uri that points to tile content. When the uri is relative, it is relative to the referring tileset JSON file.
        /// </summary>
        string GetUri();
    }
}
