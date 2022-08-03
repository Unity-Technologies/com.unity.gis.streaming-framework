using Unity.Geospatial.Streaming.UniversalDecoder;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    /// <summary>
    /// Interface when deserializing <see cref="ILeaf"/> to this type.
    /// </summary>
    /// <typeparam name="T">The <see cref="ILeaf"/> type to deserialize to.</typeparam>
    public interface ITileSchema<out T>:
        ILeaf
        where T: ITileSchema<T>
    {
        /// <summary>
        /// Metadata about the tile's content and a link to the content. When this is omitted the tile is just used for culling.
        /// </summary>
        IContentSchema Content { get; }

        /// <summary>
        /// Convert the tile transform value to a double4x4 matrix.
        /// </summary>
        /// <returns>The matrix created from the transform value.</returns>
        double4x4 TransformToDouble4X4();
    }

    /// <summary>
    /// Interface when deserializing <see cref="ILeaf"/> to this type.
    /// </summary>
    /// <typeparam name="TBoundingVolume">Bounding volume for each ILeaf part of the dataset.</typeparam>
    /// <typeparam name="TContent">
    /// <see cref="ILeaf"/> <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">Uri</see> getter allowing
    /// to retrieve where to get the data to load for this instance.
    /// </typeparam>
    /// <typeparam name="TTile">The <see cref="ILeaf"/> type to deserialize to.</typeparam>
    public interface ITileSchema<out TBoundingVolume, out TContent, out TTile> :
        ITileSchema<TTile>
        where TBoundingVolume: IBoundingVolumeSchema
        where TContent: IContentSchema
        where TTile: ITileSchema<TBoundingVolume, TContent, TTile>
    {
        /// <inheritdoc cref="TileSchema.Content"/>
        new TContent Content { get; }
    }
}
