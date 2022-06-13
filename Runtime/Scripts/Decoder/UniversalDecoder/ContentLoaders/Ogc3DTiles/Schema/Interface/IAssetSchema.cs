
namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    public interface IAssetSchema
    {
        /// <summary>
        /// The 3D Tiles version. The version defines the JSON schema for the tileset JSON and the base set of tile formats.
        /// </summary>
        string Version { get; }
    }
}
