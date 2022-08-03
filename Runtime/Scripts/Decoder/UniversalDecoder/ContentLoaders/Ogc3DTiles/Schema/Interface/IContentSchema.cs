
namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    /// <summary>
    /// Interface to specify where to retrieve the data to load for an <see cref="ILeaf"/> instance.
    /// </summary>
    public interface IContentSchema
    {
        /// <summary>
        /// A uri that points to tile content. When the uri is relative, it is relative to the referring tileset JSON file.
        /// </summary>
        /// <returns>The relative <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">Uri</see> of the data to load.</returns>
        string GetUri();
    }
}
