using System.Runtime.CompilerServices;

using UnityEngine;
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// The Omni Camera is an omnidirectional camera which ensures that the same level of detail is
    /// uniformly streamed in all around the camera, with a level of detail which is inversely
    /// proportional to the distance objects are located from the camera. Using this scene observer
    /// will increase the amount of video memory required to load the given geometry but will
    /// eliminate noticeable loading when rotating the camera. The primary use-case for this observer
    /// is VR applications, where the camera turns frequently.
    /// </summary>
    public class UGOmniCameraBehaviour : UGSceneObserverBehaviour, UGSimpleSceneObserver.IImplementation
    {

        /// <summary>
        /// The maximum distance at which an object should be visible. Note that objects
        /// further than this may still be visible, depending on the structure of the streamed
        /// data but it's visibility is not garanteed. This setting is completely independant
        /// from the camera's clip planes, which may also impact object visibility.
        /// </summary>
        public float MaxViewDistance = 1e6f;

        /// <summary>
        /// The expected geometric error 1 meter away from the observer.
        /// </summary>
        public float NormalizedError = 1e-2f;
        
        public HPRoot Root;

        private Transform m_Transform;

        public override UGSceneObserver Instantiate(UGSystemBehaviour ugSystem)
        {
            m_Transform = GetComponent<Transform>();

            if (Root == null)
                Root = GetComponentInParent<HPRoot>();

            if (Root == null)
                throw new System.InvalidOperationException("Could not find UGSystem for UGCamera. Either assign one or place the UGCamera as a child of a UGSystem.");

            return new UGSimpleSceneObserver(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DetailObserverData GetDetailObserverData()
        {
            double4x4 worldFromUniverse = Root.WorldMatrix;
            double4x4 objectFromWorld = HPMath.Translate(-m_Transform.position.ToDouble3());
            double4x4 clipFromObject = HPMath.TRS(double3.zero, quaternion.identity, 1.0f / MaxViewDistance * new float3(1F));

            double4x4 clipFromUniverse = math.mul(math.mul(clipFromObject, objectFromWorld), worldFromUniverse);

            return new DetailObserverData(in clipFromUniverse, GetGeometricError, NormalizedError * MaxViewDistance);
        }

        private static float GetGeometricError(in DetailObserverData data, in DoubleBounds clipBounds)
        {
            double3 min = clipBounds.Min;
            double3 max = clipBounds.Max;

            double dx = IntervalDistanceFromZero(min.x, max.x);
            double dy = IntervalDistanceFromZero(min.y, max.y);
            double dz = IntervalDistanceFromZero(min.z, max.z);

            double distance = math.length(new double3(dx, dy, dz));

            return data.ErrorMultiplier * (float)distance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double IntervalDistanceFromZero(double a, double b)
        {
            if (0.0 < a)
                return a;

            if (0.0 < b)
                return 0.0;

            return -b;
        }
    }
}
