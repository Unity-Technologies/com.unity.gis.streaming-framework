using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Euclidean transformation (rigid motion) stored as a translation / rotation of a single vector.
    /// </summary>
    public struct EuclideanTR
    {
        /// <summary>
        /// Translation information of the vector.
        /// </summary>
        public double3 Position;
        
        /// <summary>
        /// Orientation information of the vector.
        /// </summary>
        public quaternion Rotation;
    }
}
