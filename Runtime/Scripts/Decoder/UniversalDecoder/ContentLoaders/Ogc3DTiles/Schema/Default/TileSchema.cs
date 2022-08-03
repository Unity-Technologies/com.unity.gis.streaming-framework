
using System;
using System.Collections.Generic;

using Unity.Geospatial.HighPrecision;
using Unity.Geospatial.Streaming.UniversalDecoder;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    /// <summary>
    /// Default tile class.
    /// Represent a single tile part of a ogc 3d tiles dataset. This contains all the information needed
    /// to load the data representing a single tile for a specific zoom level.
    /// </summary>
    public class TileSchema :
        TileSchema<BoundingVolumeSchema, ContentSchema, TileSchema> { }

    /// <summary>
    /// Base tile class with typed parameters.
    /// Represent a single tile part of a ogc 3d tiles dataset. This contains all the information needed
    /// to load the data representing a single tile for a specific zoom level.
    /// </summary>
    /// <typeparam name="TBoundingVolume">Type returned by <see cref="ITileSchema{TBoundingVolume, TContent, TTile}.GetBoundingVolume(double4x4)"/>.</typeparam>
    /// <typeparam name="TContent">Type returned by <see cref="ITileSchema{TBoundingVolume, TContent, TTile}.Content"/>.</typeparam>
    /// <typeparam name="TTile">Type returned by <see cref="ITileSchema{TBoundingVolume, TContent, TTile}.GetChildren()"/>.</typeparam>
    public abstract class TileSchema<TBoundingVolume, TContent, TTile> :
        ITileSchema<TBoundingVolume, TContent, TTile>
        where TBoundingVolume : IBoundingVolumeSchema
        where TContent : IContentSchema
        where TTile : ITileSchema<TBoundingVolume, TContent, TTile>
    {
        /// <summary>
        /// The bounding volume that encloses the tile.
        /// </summary>
        public TBoundingVolume BoundingVolume { get; set; }

        /// <summary>
        /// An array of objects that define child tiles. Each child tile content is fully enclosed by its parent tile's
        /// bounding volume and, generally, has a geometricError less than its parent tile's geometricError.
        /// For leaf tiles, the length of this array is zero, and children may not be defined.
        /// </summary>
        public TTile[] Children { get; set; }

        /// <inheritdoc cref="ITileSchema{TBoundingVolume, TContent, TTile}.Content"/>
        public TContent Content { get; set; }

        /// <inheritdoc cref="ITileSchema{T}.Content"/>
        IContentSchema ITileSchema<TTile>.Content
        {
            get { return Content; }
        }

        /// <summary>
        /// The error, in meters, introduced if this tile is rendered and its children are not.
        /// At runtime, the geometric error is used to compute screen space error (SSE), i.e., the error measured in pixels.
        /// </summary>
        public float GeometricError { get; set; }

        /// <inheritdoc cref="ILeaf.GetBoundingVolume(double4x4)"/>
        DoubleBounds ILeaf.GetBoundingVolume(double4x4 transform)
        {
            return BoundingVolume.ToDoubleBounds(transform);
        }

        /// <inheritdoc cref="ILeaf.GetChildren()"/>
        IEnumerable<ILeaf> ILeaf.GetChildren()
        {
            if (Children != null)
                for (int i = 0; i < Children.Length; i++)
                    yield return Children[i];
        }

        /// <inheritdoc cref="ILeaf.GetGeometricError()"/>
        float ILeaf.GetGeometricError()
        {
            return GeometricError;
        }

        /// <inheritdoc cref="ITileSchema{T}.GetRefinement"/>
        public RefinementMode GetRefinement(RefinementMode inherited)
        {
            return Refine?.ToUpper() switch
            {
                "ADD" => RefinementMode.Add,
                "REPLACE" => RefinementMode.Replace,
                null => inherited,
                "" => inherited,
                _ => throw new InvalidOperationException($"Invalid Refine Mode: {Refine}")
            };
        }

        /// <inheritdoc cref="ILeaf.GetTransform()"/>
        double4x4 ILeaf.GetTransform()
        {
            return TransformToDouble4X4();
        }

        /// <inheritdoc cref="ILeaf.GetUriCollection"/>
        IUriCollection ILeaf.GetUriCollection(Uri baseUri)
        {
            return new SingleUri(Content?.GetUri(), baseUri);
        }

        /// <summary>
        /// Specifies if additive or replacement refinement is used when traversing the tileset for rendering.
        /// This property is required for the root tile of a tileset; it is optional for all other tiles.
        /// The default is to inherit from the parent tile.
        /// </summary>
        public string Refine { get; set; }

        /// <summary>
        /// A double floating-point 4x4 affine transformation matrix, stored in column-major order, that transforms the tile's content
        /// --i.e., its features as well as content.boundingVolume, boundingVolume, and viewerRequestVolume--
        /// from the tile's local coordinate system to the parent tile's coordinate system, or, in the case of a root tile,
        /// from the tile's local coordinate system to the tileset's coordinate system.
        /// `transform` does not apply to any volume property when the volume is a region, defined in EPSG:4979 coordinates.
        /// `transform` scales the `geometricError` by the maximum scaling factor from the matrix.
        /// </summary>
        public double[] Transform { get; set; }

        /// <inheritdoc cref="ITileSchema{T}.TransformToDouble4X4"/>
        public double4x4 TransformToDouble4X4()
        {
            double[] transform = Transform;

            if (transform == null || transform.Length == 0)
                return double4x4.identity;

            if (transform.Length != 16)
                throw new InvalidOperationException("Invalid Transform: " + string.Join(",", transform));

            return new double4x4(
                transform[0], transform[4], transform[8],  transform[12],
                transform[1], transform[5], transform[9],  transform[13],
                transform[2], transform[6], transform[10], transform[14],
                transform[3], transform[7], transform[11], transform[15]);
        }
    }
}
