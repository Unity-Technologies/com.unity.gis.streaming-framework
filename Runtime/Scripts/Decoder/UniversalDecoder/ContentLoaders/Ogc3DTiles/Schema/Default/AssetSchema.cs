
namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    public class AssetSchema :
        IAssetSchema
    {
        /// <summary>
        /// Application-specific version of this tileset, e.g., for when an existing tileset is updated.
        /// </summary>
        public string TilesetVersion { get; set; }

        /// <inheritdoc cref="IAssetSchema.Version"/>
        public string Version { get; set; }
    }
}
