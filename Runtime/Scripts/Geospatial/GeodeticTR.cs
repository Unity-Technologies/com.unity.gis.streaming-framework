
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Geodetic transformation (coordinate transformation) stored as latitude / longitude / elevation / rotation of a single vector.
    /// </summary>
    public struct GeodeticTR
    {
        /// <summary>
        /// Projected position relative to a center expressed in degrees and minutes..
        /// </summary>
        public GeodeticCoordinates Position;
        
        /// <summary>
        /// Orientation of the vector where zero (0, 0, 0) is pointing to the same direction as the <see cref="Position"/> normal.
        /// </summary>
        public float3 EulerAngles;
    }
}
