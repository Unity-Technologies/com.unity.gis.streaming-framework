
namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    /// <summary>
    /// Top level schema interface to be used when deserializing an ogc3d tiles dataset.
    /// </summary>
    public interface ITilesetSchema
    {
        /// <summary>
        /// Metadata about the entire tileset.
        /// </summary>
        IAssetSchema Asset { get; }

        /// <summary>
        /// Extensions part of this tileset.
        /// </summary>
        IExtensionsSchema Extensions { get; }
        
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
        /// <typeparam name="TTile">Implementation of <see cref="ILeaf"/> class.</typeparam>
        /// <returns>The first <see cref="ILeaf"/> to load.</returns>
        public TTile GetRoot<TTile>()
            where TTile : ITileSchema<TTile>;
    }

    /// <summary>
    /// Top level schema interface to be used when deserializing an ogc3d tiles dataset with typed parameters
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
    public interface ITilesetSchema<out TAsset, TBoundingVolume, TContent, out TExtensions, out TExtras, out TProperties, out TTile> :
        ITilesetSchema
        where TAsset: IAssetSchema
        where TBoundingVolume: IBoundingVolumeSchema
        where TContent: IContentSchema
        where TExtensions: IExtensionsSchema
        where TProperties: IPropertiesSchema
        where TTile: ITileSchema<TBoundingVolume, TContent, TTile>
    {
        /// <inheritdoc cref="ITilesetSchema.Asset"/>
        new TAsset Asset { get; }

        /// <summary>
        /// Extensions part of this tileset.
        /// </summary>
        new TExtensions Extensions { get; }

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
