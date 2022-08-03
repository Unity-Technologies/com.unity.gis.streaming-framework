
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.TmsTerrain
{
    /// <summary>
    /// Define the coordinate limits of a tile based on the WGS84 system.
    /// </summary>
    public class ExtentSchema
    {
        /// <summary>
        /// Maximum north coordinate based on WGS84 system.
        /// </summary>
        public double MinLat { get; set; }

        /// <summary>
        /// Maximum south coordinate based on WGS84 system.
        /// </summary>
        public double MaxLat { get; set; }

        /// <summary>
        /// Maximum west coordinate based on WGS84 system.
        /// </summary>
        public double MinLon { get; set; }

        /// <summary>
        /// Maximum east coordinate based on WGS84 system.
        /// </summary>
        public double MaxLon { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="minLat">Maximum north coordinate based on WGS84 system.</param>
        /// <param name="maxLat">Maximum south coordinate based on WGS84 system.</param>
        /// <param name="minLon">Maximum west coordinate based on WGS84 system.</param>
        /// <param name="maxLon">Maximum east coordinate based on WGS84 system.</param>
        public ExtentSchema(double minLat, double maxLat, double minLon, double maxLon)
        {
            MinLat = minLat;
            MaxLat = maxLat;
            MinLon = minLon;
            MaxLon = maxLon;
        }

        /// TODO - support transform
        /// <summary>
        /// Convert this extent instance to a DoubleBounds instance.
        /// </summary>
        /// <param name="transform">Position of the root node allowing to offset the extent. This is still not supported.</param>
        /// <param name="minElevation">Lower elevation of the DoubleBounds Y part result.</param>
        /// <param name="maxElevation">Higher elevation of the DoubleBounds Y part result.</param>
        /// <returns></returns>
        public DoubleBounds ToBoundingVolume(double4x4 transform, double minElevation, double maxElevation)
        {
            return Wgs84.ConvertRegionBoundingVolume(
                MinLat, MaxLat, MinLon, MaxLon, minElevation, maxElevation);
        }
    }
}
