using System;
using System.Runtime.CompilerServices;

using UnityEngine;
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// The Omni Observer is an omnidirectional observer which ensures that the same level of detail is
    /// uniformly streamed in all around the observer, with a level of detail which is inversely
    /// proportional to the distance objects are located from the observer. Using this scene observer
    /// will increase the amount of video memory required to load the given geometry but will
    /// eliminate noticeable loading when rotating the camera. The primary use-case for this observer
    /// is VR applications, where the camera turns frequently.
    /// </summary>
    public class UGOmniObserverBehaviour :
        UGSceneObserverBehaviour, 
        UGSimpleSceneObserver.IImplementation
    {

        /// <summary>
        /// The maximum distance at which an object should be visible. Note that objects
        /// further than this may still be visible, depending on the structure of the streamed
        /// data but it's visibility is not guaranteed. This setting is completely independent
        /// from the camera's clip planes, which may also impact object visibility.
        /// </summary>
        public float MaxViewDistance = 1e6f;

        /// <summary>
        /// The expected geometric error 1 meter away from the observer.
        /// </summary>
        public float NormalizedError = 1e-2f;
        
        /// <summary>
        /// Set the parent root of this instance.
        /// If no root is set, the parent <see cref="UGSystemBehaviour"/> root will be taken.
        /// </summary>
        public HPRoot Root;

        /// <summary>
        /// Cached transform component.
        /// </summary>
        private Transform m_Transform;

        /// <summary>
        /// Create a new <see cref="UGSceneObserver"/> of this camera and link it with the given <paramref name="ugSystem">system </paramref>.
        /// </summary>
        /// <param name="ugSystem">Create the instance under this parent node.</param>
        /// <returns>The instantiated observer.</returns>
        /// <exception cref="InvalidOperationException">Raised if no valid <see cref="Root"/> was found.</exception>
        public override UGSceneObserver Instantiate(UGSystemBehaviour ugSystem)
        {
            m_Transform = GetComponent<Transform>();

            if (Root == null)
                Root = GetComponentInParent<HPRoot>();

            if (Root == null)
                throw new InvalidOperationException( 
                    "Could not find UGSystem for UGSceneObserver. Either assign one or place the UGCamera as a child of a UGSystem.");

            return new UGSimpleSceneObserver(this);
        }
  
        /// <summary>
        /// Get a <see cref="DetailObserverData"/> representing this instance.
        /// </summary>
        /// <returns>see cref="UGSceneObserver"/> information allowing to calculate the geometric error against it.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DetailObserverData GetDetailObserverData()
        {
            double4x4 worldFromUniverse = Root.WorldMatrix;
            double4x4 objectFromWorld = HPMath.Translate(-m_Transform.position.ToDouble3());
            double4x4 clipFromObject = HPMath.TRS(double3.zero, quaternion.identity, 1.0f / MaxViewDistance * new float3(1F));

            double4x4 clipFromUniverse = math.mul(math.mul(clipFromObject, objectFromWorld), worldFromUniverse);

            return new DetailObserverData(in clipFromUniverse, GetGeometricError, NormalizedError * MaxViewDistance);
        }

        /// <summary>
        /// Calculates a geometric error by comparing a <see cref="UGOmniCameraBehaviour"/> with a bounding box.
        /// </summary>
        /// <param name="data"><see cref="UGSceneObserver"/> information allowing to calculate the geometric error against it.</param>
        /// <param name="clipBounds">Calculate the geometric error against this shape.</param>
        /// <returns>
        /// Value used to compare with the observer minimum allowed error. If the value is higher, the node represented
        /// by the given <paramref name="clipBounds"/> will be expanded allowing a higher resolution to be loaded.
        /// </returns>
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
