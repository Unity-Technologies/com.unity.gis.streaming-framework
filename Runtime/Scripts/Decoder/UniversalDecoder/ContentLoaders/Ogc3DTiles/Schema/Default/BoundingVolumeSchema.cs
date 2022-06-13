
using System;
using System.Runtime.CompilerServices;

using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    public class BoundingVolumeSchema :
        IBoundingVolumeSchema
    {
        /// <summary>
        /// An array of 12 numbers that define an oriented bounding box. The first three elements define the x, y, and z values for the center of the box. The next three elements (with indices 3, 4, and 5) define the x axis direction and half-length. The next three elements (indices 6, 7, and 8) define the y axis direction and half-length. The last three elements (indices 9, 10, and 11) define the z axis direction and half-length.
        /// </summary>
        public double[] Box { get; set; }

        /// <summary>
        /// An array of six numbers that define a bounding geographic region in EPSG:4979 coordinates
        /// with the order [west, south, east, north, minimum height, maximum height].
        /// Longitudes and latitudes are in radians, and heights are in meters above (or below) the WGS84 ellipsoid.
        /// </summary>
        public double[] Region { get; set; }

        /// <summary>
        /// An array of four numbers that define a bounding sphere.
        /// The first three elements define the x, y, and z values for the center of the sphere.
        /// The last element (with index 3) defines the radius in meters.
        /// </summary>
        public double[] Sphere { get; set; }

        /// <inheritdoc cref="IBoundingVolumeSchema.ToDoubleBounds"/>
        public DoubleBounds ToDoubleBounds(double4x4 transform)
        {
            if (Box != null)
                return ConvertBoxToDoubleBounds(transform);

            if (Sphere != null)
                return ConvertSphereToDoubleBounds(transform);

            if (Region != null)
                return ConvertRegionToDoubleBounds();

            throw new InvalidOperationException("No bounding volume has been defined");
        }

        /// <summary>
        /// Convert the <see cref="BoundingVolumeSchema.Box"/> to a DoubleBounds.
        /// </summary>
        /// <param name="transform">Offset the result by this position.</param>
        /// <returns>The DoubleBounds result.</returns>
        /// <exception cref="InvalidOperationException">If the <see cref="Box"/> value is not conform.</exception>
        private DoubleBounds ConvertBoxToDoubleBounds(double4x4 transform)
        {
            double[] box = Box;

            if (box.Length != 12)
                throw new InvalidOperationException("Invalid Box Volume: " + string.Join(",", box));

            //
            //  TODO - Implement a tighter bounding volume by applying the rotation before building the
            //              axis aligned bounding volume.
            //
            double3 center = new double3(box[0], box[1], box[2]);
            double3 half1 = new double3(box[3], box[4], box[5]);
            double3 half2 = new double3(box[6], box[7], box[8]);
            double3 half3 = new double3(box[9], box[10], box[11]);

            DoubleBounds localBounds = new DoubleBounds(center, 2 * AbsMax(half1, half2, half3));

            return DoubleBounds.Transform3x4(localBounds, transform);
        }

        /// <summary>
        /// Convert the <see cref="BoundingVolumeSchema.Sphere"/> to a DoubleBounds.
        /// </summary>
        /// <param name="transform">Offset the result by this position.</param>
        /// <returns>The DoubleBounds result.</returns>
        /// <exception cref="InvalidOperationException">If the <see cref="Sphere"/> value is not conform.</exception>
        private DoubleBounds ConvertSphereToDoubleBounds(double4x4 transform)
        {
            double[] sphere = Sphere;

            if (sphere.Length != 4)
                throw new InvalidOperationException($"Invalid Sphere Volume: [{sphere.Length}] {string.Join(",", sphere)}");

            double3 center = new double3(sphere[0], sphere[1], sphere[2]);
            DoubleBounds localBounds = new DoubleBounds(center, new double3(sphere[3], sphere[3], sphere[3]));

            return DoubleBounds.Transform3x4(localBounds, transform);
        }

        /// <summary>
        /// Convert the <see cref="BoundingVolumeSchema.Region"/> to a DoubleBounds.
        /// </summary>
        /// <returns>The DoubleBounds result.</returns>
        /// <exception cref="InvalidOperationException">If the <see cref="Region"/> value is not conform.</exception>
        private DoubleBounds ConvertRegionToDoubleBounds()
        {
            double[] region = Region;

            if (region.Length != 6)
                throw new InvalidOperationException("Region bounding volume is expected to have exactly 6 values");

            double west = math.degrees(region[0]);
            double south = math.degrees(region[1]);
            double east = math.degrees(region[2]);
            double north = math.degrees(region[3]);
            double minHeight = region[4];
            double maxHeight = region[5];

            return Wgs84.ConvertRegionBoundingVolume(south, north, west, east, minHeight, maxHeight);
        }

        /// <summary>
        /// Get the maximum value of each given absolute double3.
        /// </summary>
        /// <param name="a">Vector to evaluate.</param>
        /// <param name="b">Vector to evaluate.</param>
        /// <param name="c">Vector to evaluate.</param>
        /// <returns>The absolute maximum result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double3 AbsMax(double3 a, double3 b, double3 c)
        {
            a = math.abs(a);
            b = math.abs(b);
            c = math.abs(c);
            return math.max(math.max(a, b), c);
        }
    }
}
