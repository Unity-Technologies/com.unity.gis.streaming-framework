
using Newtonsoft.Json.Linq;

namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    public class TilesetSchema :
        TilesetSchema<AssetSchema, BoundingVolumeSchema, ContentSchema, JToken, JToken, PropertiesSchema, TileSchema> { }

    public abstract class TilesetSchema<TAsset, TBoundingVolume, TContent, TExtensions, TExtras, TProperties, TTile> :
        ITilesetSchema<TAsset, TBoundingVolume, TContent, TExtensions, TExtras, TProperties, TTile>
        where TAsset : IAssetSchema
        where TBoundingVolume : IBoundingVolumeSchema
        where TContent : IContentSchema
        where TProperties : IPropertiesSchema
        where TTile : ITileSchema<TBoundingVolume, TContent, TTile>
    {
        /// <inheritdoc cref="ITilesetSchema{TAsset, TBoundingVolume, TContent, TExtensions, TExtras, TProperties, TTile}.Asset"/>
        public TAsset Asset { get; set; }

        /// <inheritdoc cref="ITilesetSchema.Asset"/>
        IAssetSchema ITilesetSchema.Asset
        {
            get { return Asset; }
        }

        /// <inheritdoc cref="ITilesetSchema.GeometricError"/>
        public float GeometricError { get; set; }

        /// <inheritdoc cref="ITilesetSchema.GetRoot{T}"/>
        T ITilesetSchema.GetRoot<T>()
        {
            return (T)(Root as ITileSchema<T>);
        }

        /// <inheritdoc cref="ITilesetSchema{TAsset, TBoundingVolume, TContent, TExtensions, TExtras, TProperties, TTile}.Extensions"/>
        public TExtensions Extensions { get; set; }

        /// <inheritdoc cref="ITilesetSchema.ExtensionsUsed"/>
        public string[] ExtensionsUsed { get; set; }

        /// <inheritdoc cref="ITilesetSchema.ExtensionsRequired"/>
        public string[] ExtensionsRequired { get; set; }

        /// <inheritdoc cref="ITilesetSchema{TAsset, TBoundingVolume, TContent, TExtensions, TExtras, TProperties, TTile}.Extras"/>
        public TExtras Extras { get; set; }

        /// <inheritdoc cref="ITilesetSchema{TAsset, TBoundingVolume, TContent, TExtensions, TExtras, TProperties, TTile}.Properties"/>
        public TProperties Properties { get; set; }

        /// <inheritdoc cref="ITilesetSchema.Properties"/>
        IPropertiesSchema ITilesetSchema.Properties
        {
            get { return Properties; }
        }

        /// <summary>
        /// The root tile.
        /// </summary>
        public TTile Root { get; set; }
    }
}
