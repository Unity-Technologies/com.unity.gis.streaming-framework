
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{

    public static class HPTransformExtensions
    {
        /// <summary>
        /// Set current HPTransform's local position and rotation based on geodetic position coordinates, 
        /// and euler angles in degrees with range [-180, 180]
        /// </summary>
        public static void SetGeodeticCoordinates(this HPTransform transform, GeodeticCoordinates position, float3 eulerAngles)
        {
            EuclideanTR xzyecef = Wgs84.GeodeticToXzyEcef(position, eulerAngles);

            transform.LocalPosition = xzyecef.Position;
            transform.LocalRotation = xzyecef.Rotation;
        }

        public static GeodeticTR GetGeodeticCoordinates(this HPTransform transform)
        {
            return Wgs84.XzyEcefToGeodetic(transform.LocalPosition, transform.LocalRotation);
        }
    }
}
