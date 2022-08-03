
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Extend the HPTransform methods with geodetic related methods.
    /// </summary>
    public static class HPTransformExtensions
    {
        /// <summary>
        /// Set current HPTransform's local position and rotation based on geodetic position coordinates, 
        /// and euler angles in degrees with range [-180, 180]
        /// </summary>
        /// <param name="transform">Set the transform values to this component.</param>
        /// <param name="position">Set the translation to this value.</param>
        /// <param name="eulerAngles">Set the orientation to this value.</param>
        public static void SetGeodeticCoordinates(this HPTransform transform, GeodeticCoordinates position, float3 eulerAngles)
        {
            EuclideanTR xzyecef = Wgs84.GeodeticToXzyEcef(position, eulerAngles);

            transform.LocalPosition = xzyecef.Position;
            transform.LocalRotation = xzyecef.Rotation;
        }

        /// <summary>
        /// Convert the <paramref name="transform"/> position / rotation to a <see cref="GeodeticTR">geodetic</see> format.
        /// </summary>
        /// <param name="transform">Get the transform values from this component.</param>
        /// <returns>The geodetic result.</returns>
        public static GeodeticTR GetGeodeticCoordinates(this HPTransform transform)
        {
            return Wgs84.XzyEcefToGeodetic(transform.LocalPosition, transform.LocalRotation);
        }
    }
}
