
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// <see cref="UGSceneObserver"/> information allowing to calculate the geometric error specification against it.
    /// </summary>
    public readonly struct DetailObserverData
    {
        /// <summary>
        /// <see langword="delegate"/> defining a lambda for calculating a geometric error.
        /// </summary>
        /// <param name="detailObserverData"><see cref="UGSceneObserver"/> information allowing to calculate the geometric error against it.</param>
        /// <param name="clipBounds">Calculate the geometric error against this shape.</param>
        /// <returns>
        /// Value used to compare with the observer minimum allowed error. If the value is higher, the node represented
        /// by the given <paramref name="clipBounds"/> will be expanded allowing a higher resolution to be loaded.
        /// </returns>
        public delegate float GeometricErrorFunction(in DetailObserverData detailObserverData, in DoubleBounds clipBounds);

        /// <summary>
        /// Constructor without a <see cref="ClipPlane"/>.
        /// </summary>
        /// <param name="clipFromUniverse">Position of the observer.</param>
        /// <param name="geometricErrorFunction">Lambda used to calculate the geometric error.</param>
        /// <param name="errorMultiplier">
        /// Multiply the <see cref="GeometricError"/> result by this value.
        /// Higher the value, less chances the nodes will get expanded.
        /// Lower the value, higher will be the resolution of the loaded geometries.
        /// </param>
        public DetailObserverData(in double4x4 clipFromUniverse, GeometricErrorFunction geometricErrorFunction, float errorMultiplier)
        {
            UseClipPlane = false;
            ClipPlane = default;
            ClipFromUniverse = clipFromUniverse;
            GeometricError = geometricErrorFunction;
            ErrorMultiplier = errorMultiplier;
        }

        /// <summary>
        /// Constructor with a <see cref="ClipPlane"/>.
        /// </summary>
        /// <param name="clipFromUniverse">Position of the observer.</param>
        /// <param name="clipPlane">Do not evaluate the geometric error beyond this plane.</param>
        /// <param name="geometricErrorFunction">Lambda used to calculate the geometric error.</param>
        /// <param name="errorMultiplier">
        /// Multiply the <see cref="GeometricError"/> result by this value.
        /// Higher the value, less chances the nodes will get expanded.
        /// Lower the value, higher will be the resolution of the loaded geometries.
        /// </param>
        public DetailObserverData(in double4x4 clipFromUniverse, in DoublePlane clipPlane, GeometricErrorFunction geometricErrorFunction, float errorMultiplier)
        {
            UseClipPlane = true;
            ClipPlane = clipPlane;
            ClipFromUniverse = clipFromUniverse;
            GeometricError = geometricErrorFunction;
            ErrorMultiplier = errorMultiplier;
        }

        /// <summary>
        /// Universe position of the <see cref="UGSceneObserver">observer</see> allowing
        /// to know where the <see cref="UGSceneObserver">observer</see> look from.
        /// </summary>
        public readonly double4x4 ClipFromUniverse;
        
        /// <summary>
        /// <see langword="true"/> if the <see cref="UGSceneObserver">observer</see> does not look beyond te <see cref="ClipPlane"/>;
        /// <see langword="false"/> otherwise.
        /// </summary>
        public readonly bool UseClipPlane;
        
        /// <summary>
        /// Do not evaluate the geometric error beyond this plane.
        /// </summary>
        public readonly DoublePlane ClipPlane;
        
        /// <summary>
        /// Lambda used to calculate the geometric error.
        /// </summary>
        public readonly GeometricErrorFunction GeometricError;
        
        /// <summary>
        /// Multiply the <see cref="GeometricError"/> result by this value.
        /// Higher the value, less chances the nodes will get expanded.
        /// Lower the value, higher will be the resolution of the loaded geometries.
        /// </summary>
        public readonly float ErrorMultiplier;
        
        /// <summary>
        /// Calculate the geometric error for a specific bounding box.
        /// </summary>
        /// <param name="bounds">Calculate the geometric error against this shape.</param>
        /// <returns>Value used to compare with the observer minimum allowed error. If the value is higher, the
        /// node represented by the given <paramref name="bounds"/> will be expanded allowing a higher resolution to be loaded.</returns>
        public float GetErrorSpecification(in DoubleBounds bounds)
        {
            return GeometricError.Invoke(in this, in bounds);
        }
    }
}
