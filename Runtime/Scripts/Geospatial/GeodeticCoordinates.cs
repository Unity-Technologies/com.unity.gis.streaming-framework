
using System;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    [Serializable]
    public struct GeodeticCoordinates : IEquatable<GeodeticCoordinates>
    {
        public readonly double3 value;
        
        public GeodeticCoordinates(double latitude, double longitude, double elevation)
        {
            value = new double3(latitude, longitude, elevation);
        }

        public double Latitude
        {
            get { return value.x; }
        }
        public double Longitude
        {
            get { return value.y; }
        }
        public double Elevation
        {
            get { return value.z; }
        }

        public override string ToString()
        {
            return $"long: {Longitude}, lat: {Latitude}, elev: {Elevation}";
        }

        public static bool operator ==(GeodeticCoordinates a, GeodeticCoordinates b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(GeodeticCoordinates a, GeodeticCoordinates b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is GeodeticCoordinates coordinates && Equals(coordinates);
        }

        public bool Equals(GeodeticCoordinates other)
        {
            return
                Latitude == other.Latitude &&
                Longitude == other.Longitude &&
                Elevation == other.Elevation;
        }

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
