
using Newtonsoft.Json.Linq;

namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    /// <summary>
    /// Default top level schema to be used when deserializing an ogc3d tiles dataset with typed parameters
    /// allowing a more generic implementation and easier <see cref="IExtension"/> implementation.
    /// </summary>
    public class TilesetSchema :
        TilesetSchema<AssetSchema, BoundingVolumeSchema, ContentSchema, ExtensionsSchema, JToken, PropertiesSchema, TileSchema> { }


    /// <summary>
    /// Abstract class for top level schema to be used when deserializing an ogc3d tiles dataset with typed parameters
    /// allowing a more generic implementation and easier <see cref="IExtension"/> implementation.
    /// </summary>
    /// <typeparam name="TAsset">Information of the dataset.</typeparam>
    /// <typeparam name="TBoundingVolume">Bounding volume for each ILeaf part of the dataset.</typeparam>
    /// <typeparam name="TContent">
    /// <see cref="ILeaf"/> <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">Uri</see> getter allowing
    /// to retrieve where to get the data to load for this instance.
    /// </typeparam>
    /// <typeparam name="TExtensions">Extensions part of a dataset.</typeparam>
    /// <typeparam name="TExtras">Dictionary of extra information.</typeparam>
    /// <typeparam name="TProperties">A dictionary object of metadata about per-feature properties.</typeparam>
    /// <typeparam name="TTile">Deserialize <see cref="ILeaf"/> instances to this type.</typeparam>
    public abstract class TilesetSchema<TAsset, TBoundingVolume, TContent, TExtensions, TExtras, TProperties, TTile> :
        ITilesetSchema<TAsset, TBoundingVolume, TContent, TExtensions, TExtras, TProperties, TTile>
        where TAsset : IAssetSchema
        where TBoundingVolume : IBoundingVolumeSchema
        where TContent : IContentSchema
        where TExtensions: IExtensionsSchema
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

        /// <inheritdoc cref="ITilesetSchema.Extensions"/>
        IExtensionsSchema ITilesetSchema.Extensions
        {
            get { return Extensions; }
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
