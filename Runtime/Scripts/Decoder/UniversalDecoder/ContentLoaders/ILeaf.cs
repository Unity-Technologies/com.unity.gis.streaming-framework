
using System;
using System.Collections.Generic;

using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Represent a single item available to be loaded by a <see cref="UriLoader"/>.
    /// </summary>
    public interface ILeaf
    {
        /// <summary>
        /// The bounding volume that encloses the item geometry.
        /// </summary>
        /// <param name="transform">Offset the result by this position.</param>
        /// <returns>The bounding volume to use when calculating the screen space error of this <see cref="ILeaf"/> instance.</returns>
        DoubleBounds GetBoundingVolume(double4x4 transform);

        /// <summary>
        /// Enumerate child items of this parent. Each child content is fully enclosed by its parent
        /// bounding volume and, generally, has a geometricError less than its parent geometricError.
        /// For leaf items, the length of this array is zero, and children may not be defined.
        /// </summary>
        /// <returns>Enumerate the children of this <see cref="ILeaf"/> instance.</returns>
        IEnumerable<ILeaf> GetChildren();

        /// <summary>
        /// The error, in meters, introduced if this item is rendered and its children are not.
        /// At runtime, the geometric error is used to compute screen space error (SSE), i.e., the error measured in pixels.
        /// </summary>
        /// <returns>The error value to compute the screen space error of the <see cref="ILeaf"/> instance.</returns>
        float GetGeometricError();

        /// <summary>
        /// Specifies if additive or replacement refinement is used when traversing the hierarchy for rendering.
        /// This property is required for the root item; it is optional for all other children.
        /// The default is to inherit from the parent item.
        /// </summary>
        /// <param name="inherited">The parent refine value in case the refine value on this item is not set.</param>
        /// <returns>The <see cref="RefinementMode"/> to use when loading the <see cref="ILeaf"/>.</returns>
        RefinementMode GetRefinement(RefinementMode inherited);

        /// <summary>
        /// Get a double4x4 matrix representing where the item content should be positioned, turned and scaled.
        /// </summary>
        /// <returns>The matrix of the item.</returns>
        double4x4 GetTransform();

        /// <summary>
        /// A uri collection that points to the all the content to load. When the uri are relative, it is relative to the referring top parent.
        /// </summary>
        /// <param name="baseUri">An absolute <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> of the root <see cref="ILeaf"/>.</param>
        /// <returns>Address of the content to be loaded.</returns>
        IUriCollection GetUriCollection(Uri baseUri);
    }
}
