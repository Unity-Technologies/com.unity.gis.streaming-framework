
namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    public interface ITilesetSchema
    {
        /// <summary>
        /// Metadata about the entire tileset.
        /// </summary>
        IAssetSchema Asset { get; }

        /// <summary>
        /// Names of 3D Tiles extensions used somewhere in this tileset.
        /// </summary>
        string[] ExtensionsUsed { get; }

        /// <summary>
        /// Names of 3D Tiles extensions required to properly load this tileset.
        /// </summary>
        string[] ExtensionsRequired { get; }

        /// <summary>
        /// The error, in meters, introduced if this tileset is not rendered.
        /// At runtime, the geometric error is used to compute screen space error (SSE), i.e., the error measured in pixels.
        /// </summary>
        float GeometricError { get; }

        /// <summary>
        /// A dictionary object of metadata about per-feature properties.
        /// </summary>
        IPropertiesSchema Properties { get; }

        /// <summary>
        /// The root tile.
        /// </summary>
        public TTile GetRoot<TTile>()
            where TTile : ITileSchema<TTile>;
    }

    public interface ITilesetSchema<out TAsset, TBoundingVolume, TContent, out TExtensions, out TExtras, out TProperties, out TTile> :
        ITilesetSchema
        where TAsset: IAssetSchema
        where TBoundingVolume: IBoundingVolumeSchema
        where TContent: IContentSchema
        where TProperties: IPropertiesSchema
        where TTile: ITileSchema<TBoundingVolume, TContent, TTile>
    {
        /// <inheritdoc cref="ITilesetSchema.Asset"/>
        new TAsset Asset { get; }

        /// <summary>
        /// Dictionary of the extensions part of this tileset.
        /// </summary>
        TExtensions Extensions { get; }

        /// <summary>
        /// Dictionary of extra information.
        /// </summary>
        TExtras Extras { get; }

        /// <inheritdoc cref="ITilesetSchema.Properties"/>
        new TProperties Properties { get; }

        /// <inheritdoc cref="ITilesetSchema.GetRoot{T}"/>
        public TTile Root { get; }
    }
}
