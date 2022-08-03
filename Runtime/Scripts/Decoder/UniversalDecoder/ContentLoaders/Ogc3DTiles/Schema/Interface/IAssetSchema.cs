
namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    /// <summary>
    /// Interface to store information for an ogc3d tiles dataset.
    /// </summary>
    public interface IAssetSchema
    {
        /// <summary>
        /// The 3D Tiles version. The version defines the JSON schema for the tileset JSON and the base set of tile formats.
        /// </summary>
        string Version { get; }
    }
}
