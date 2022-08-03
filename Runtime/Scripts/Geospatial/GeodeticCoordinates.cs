
using System;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Geodetic position stored as latitude / longitude / elevation.
    /// </summary>
    [Serializable]
    public struct GeodeticCoordinates : IEquatable<GeodeticCoordinates>
    {
        /// <summary>
        /// The coordinates values store as a <see href="https://docs.unity3d.com/Packages/com.unity.mathematics@0.0/api/Unity.Mathematics.double3.html">double3</see>
        /// where the x is the <see cref="Latitude"/>, the y is the <see cref="Longitude"/> and the z is the <see cref="Elevation"/>.
        /// </summary>
        public readonly double3 value;
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="latitude">The angular distance of the vector (north / south) expressed in degrees and minutes.</param>
        /// <param name="longitude">The angular distance of the vector (east / west) expressed in degrees and minutes.</param>
        /// <param name="elevation">Distance in meter to geoid.</param>
        public GeodeticCoordinates(double latitude, double longitude, double elevation)
        {
            value = new double3(latitude, longitude, elevation);
        }

        /// <summary>
        /// The angular distance of the vector (north / south) expressed in degrees and minutes.
        /// </summary>
        public double Latitude
        {
            get { return value.x; }
        }
        
        /// <summary>
        /// The angular distance of the vector (east / west) expressed in degrees and minutes.
        /// </summary>
        public double Longitude
        {
            get { return value.y; }
        }
        
        /// <summary>
        /// Distance in meter to geoid.
        /// </summary>
        public double Elevation
        {
            get { return value.z; }
        }

        /// <summary>
        /// Returns a formatted string for the coordinates.
        /// </summary>
        /// <returns>The formatted string representing this instance.</returns>
        public override string ToString()
        {
            return $"long: {Longitude}, lat: {Latitude}, elev: {Elevation}";
        }

        /// <summary>
        /// Validate both <see cref="GeodeticCoordinates"/> have the same values.
        /// </summary>
        /// <param name="lhs">First instance to compare with.</param>
        /// <param name="rhs">Compare <paramref name="lhs"/> with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(GeodeticCoordinates lhs, GeodeticCoordinates rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Validate both <see cref="GeodeticCoordinates"/> have the different values.
        /// </summary>
        /// <param name="lhs">First instance to compare with.</param>
        /// <param name="rhs">Compare <paramref name="lhs"/> with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if at least one value is different on both instances;
        /// <see langword="false"/> if both instance have the same values.
        /// </returns>
        public static bool operator !=(GeodeticCoordinates lhs, GeodeticCoordinates rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Validate <paramref name="obj"/> is a <see cref="GeodeticCoordinates"/> instance and have the same values as this instance.
        /// </summary>
        /// <param name="obj">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is GeodeticCoordinates coordinates && Equals(coordinates);
        }

        /// <summary>
        /// Validate an other <see cref="GeodeticCoordinates"/> has the same values as this instance.
        /// </summary>
        /// <param name="other">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(GeodeticCoordinates other)
        {
            return
                Latitude == other.Latitude &&
                Longitude == other.Longitude &&
                Elevation == other.Elevation;
        }

        /// <summary>
        /// Compute a hash code for the object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <remarks>
        /// * You should not assume that equal hash codes imply object equality.
        /// * You should never persist or use a hash code outside the application domain in which it was created,
        ///   because the same object may hash differently across application domains, processes, and platforms.
        /// </remarks>
        public override int GetHashCode()
        {
            int hashCode = 3079;
            hashCode = hashCode * 2957 + Latitude.GetHashCode();
            hashCode = hashCode * 5003 + Longitude.GetHashCode();
            hashCode = hashCode * 1151 + Elevation.GetHashCode();
            return hashCode;
        }
    }
}
