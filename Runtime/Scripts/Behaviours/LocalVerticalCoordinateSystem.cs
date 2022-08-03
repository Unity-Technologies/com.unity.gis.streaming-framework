using UnityEngine;
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// The Local Vertical Coordinate System aligns the universe space with the world
    /// space in such a way that a specified GameObject is always at the origin of
    /// world space. It also rotates the universe space in such a way that Vector3.up
    /// in world space corresponds to the earth's normal, relative to the WGS84
    /// ellipsoid.
    /// </summary>
    [RequireComponent(typeof(HPRoot))]
    public class LocalVerticalCoordinateSystem : MonoBehaviour
    {
        /// <summary>
        /// The HPTransform that will be located at the origin of the unity scene. This might
        /// be the camera or it might be a point of interest, such as a vehicle or a
        /// static object.
        /// </summary>
        public HPTransform origin;

        /// <summary>
        /// Setting this to true will make it so that the world origin is always at zero
        /// elevation. This means that the world space Y axis will directly correspond to
        /// elevation relative to the WGS84 ellipsoid. This is particularly usefull for
        /// out-of-the-box compatibility with t HDRP's procedural sky, for example.
        /// </summary>
        public bool worldHeightAsAltitude = true;

        private HPRoot m_Root;

        private double3 m_LastPosition;

        private void Start()
        {
            m_Root = GetComponent<HPRoot>();
        }


        private void LateUpdate()
        {
            if (origin == null)
                return;

            double3 position = origin.UniversePosition;

            if (!m_LastPosition.Equals(position))
            {
                GeodeticCoordinates coordinates = Wgs84.GetGeodeticCoordinates(position);

                if (worldHeightAsAltitude)
                    coordinates = new GeodeticCoordinates(coordinates.Latitude, coordinates.Longitude, 0.0);

                double4x4 xzyEcefFromXzyEnu = Wgs84.GetXzyEcefFromXzyEnuMatrix(coordinates);

                xzyEcefFromXzyEnu.GetTRS(out double3 translation, out quaternion rotation, out _);

                m_Root.SetRootTR(translation, rotation);

                m_LastPosition = position;
            }
        }
    }
}
