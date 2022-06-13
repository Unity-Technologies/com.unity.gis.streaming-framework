
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    public readonly struct DetailObserverData
    {
        public delegate float GeometricErrorFunction(in DetailObserverData detailObserverData, in DoubleBounds clipBounds);

        public DetailObserverData(in double4x4 clipFromUniverse, GeometricErrorFunction geometricErrorFunction, float errorMultiplier)
        {
            UseClipPlane = false;
            ClipPlane = default;
            ClipFromUniverse = clipFromUniverse;
            GeometricError = geometricErrorFunction;
            ErrorMultiplier = errorMultiplier;
        }

        public DetailObserverData(in double4x4 clipFromUniverse, in DoublePlane clipPlane, GeometricErrorFunction geometricErrorFunction, float errorMultiplier)
        {
            UseClipPlane = true;
            ClipPlane = clipPlane;
            ClipFromUniverse = clipFromUniverse;
            GeometricError = geometricErrorFunction;
            ErrorMultiplier = errorMultiplier;
        }

        public readonly double4x4 ClipFromUniverse;
        public readonly bool UseClipPlane;
        public readonly DoublePlane ClipPlane;
        public readonly GeometricErrorFunction GeometricError;
        public readonly float ErrorMultiplier;
        
        public float GetErrorSpecification(in DoubleBounds bounds)
        {
            return GeometricError.Invoke(in this, in bounds);
        }
    }
}
