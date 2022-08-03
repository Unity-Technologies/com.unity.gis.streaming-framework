
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    /// <summary>
    /// Interface to get the bounding volume an ILeaf.
    /// </summary>
    public interface IBoundingVolumeSchema
    {
        /// <summary>
        /// Convert the <see cref="IBoundingVolumeSchema"/> to a DoubleBounds based on its values.
        /// </summary>
        /// <param name="transform">Offset the result by this position.</param>
        /// <returns>The DoubleBounds result.</returns>
        DoubleBounds ToDoubleBounds(double4x4 transform);
    }
}
