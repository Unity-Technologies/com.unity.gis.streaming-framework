using System;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.Assertions;
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// The UGCamera is a type of controller that can be used within the Unity Geospatial Framework
    /// that allows the framework to know what is currently visible through a camera, and to stream
    /// the corresponding geometry in while taking into consideration the camera's position, orientation
    /// field of view and resolution. 
    /// 
    /// Because it relies on the attached Camera parameters, there is nothing to configure for this
    /// class, other than ensuring that it has been added to the <see cref="UGSystemBehaviour"/>'s
    /// scene observers.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class UGCameraBehaviour : UGSceneObserverBehaviour, UGSimpleSceneObserver.IImplementation
    {
        /// <summary>
        /// The maximum screen space error used by the observer allowing to specify if the object should be displayed.
        /// This value is used to calculate the <see cref="DetailObserverData.ErrorMultiplier"/>.
        /// Higher value will result in loading less geometry to be loaded since it would require a smaller <see cref="UniversalDecoder.NodeData.GeometricError"/>;
        /// A lower value will result in displaying more geometry.
        /// </summary>
        public float ScreenSpaceError { get; set; } = 4.0f;

        /// <summary>
        /// Camera component this parent component refers to.
        /// </summary>
        private Camera m_Camera;

        /// <summary>
        /// Transform component this parent component refers to.
        /// This represent the local position / rotation / scale of the camera without the HPLocation localisation.
        /// </summary>
        private Transform m_Transform;

        /// <summary>
        /// HPRoot parent of this camera allowing to drive HPLocation parenting without
        /// depending on the object classic hierarchy.
        /// </summary>
        [SerializeField]
        private HPRoot m_Root;

        /// <summary>
        /// HPRoot parent of this camera allowing to drive HPLocation parenting without
        /// depending on the object classic hierarchy.
        /// </summary>
        public HPRoot Root
        {
            get { return m_Root; }
            set { m_Root = value; }
        }

        /// <summary>
        /// Create a <see cref="UGSimpleSceneObserver"/> instance based on this camera components.
        /// </summary>
        /// <param name="ugSystem">Create an observer based on this <see cref="UGSystemBehaviour"/>.</param>
        /// <returns>The newly created <see cref="UGSceneObserver"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the camera has no <see cref="Root"/> set and has no parent with a HPRoot component.
        /// </exception>
        public override UGSceneObserver Instantiate(UGSystemBehaviour ugSystem)
        {
            m_Camera = GetComponent<Camera>();
            m_Transform = GetComponent<Transform>();

            if (m_Root == null)
                m_Root = GetComponentInParent<HPRoot>();

            if (m_Root == null)
                throw new InvalidOperationException("Could not find UGSystem for UGCamera. Either assign one or place the UGCamera as a child of a UGSystem.");

            return new UGSimpleSceneObserver(this);
        }

        /// <summary>
        /// Get the required values allowing to calculate if a <see cref="UniversalDecoder.NodeData"/> can be
        /// loaded based on its <see cref="UniversalDecoder.NodeData.GeometricError"/>.
        /// </summary>
        /// <returns>A new set of observer values.</returns>
        public DetailObserverData GetDetailObserverData()
        {
            CameraData cameraData = ComputeCameraData();

            return ConvertCameraData(cameraData);
        }

        /// <summary>
        /// Get all the required data to calculate the <see cref="DetailObserverData"/> values.
        /// </summary>
        /// <returns>The required camera components values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CameraData ComputeCameraData()
        {
            float2 resolution = new float2(m_Camera.pixelWidth, m_Camera.pixelHeight);
            float fovVRad = m_Camera.fieldOfView * (float)Math.PI / 180.0f;
            float fovHRad = 2.0f * math.atan(resolution.x / resolution.y * math.tan(0.5f * fovVRad));
            float2 fovRad = new float2(fovHRad, fovVRad);

            double4x4 universeFromWorld = math.inverse(m_Root.WorldMatrix);
            double4x4 worldFromCamera = m_Transform.localToWorldMatrix.ToDouble4x4();

            double4x4 universeFromCamera = math.mul(universeFromWorld, worldFromCamera);

            universeFromCamera.GetTRS(out double3 translation, out quaternion rotation, out _);

            CameraData result;

            result.UniversePosition = translation;
            result.UniverseRotation = rotation;
            result.FovRad = fovRad;
            result.Resolution = resolution;

            result.ScreenSpaceError = ScreenSpaceError;
            result.NearClip = m_Camera.nearClipPlane;
            result.FarClip = m_Camera.farClipPlane;

            return result;
        }

        /// <summary>
        /// Convert the <see cref="CameraData"/> values to <see cref="DetailObserverData"/> values.
        /// </summary>
        /// <param name="cameraData">The values to convert.</param>
        /// <returns>The converted values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DetailObserverData ConvertCameraData(CameraData cameraData)
        {
            float f = cameraData.FarClip;
            float n = cameraData.NearClip;
            double c = (f + n) / (f - n);
            double d = -2 * f * n / (f - n);

            float2 halfDim = new float2(math.tan(0.5f * cameraData.FovRad.x), math.tan(0.5f * cameraData.FovRad.y));

            double4x4 cameraFromUniverse = math.inverse(HPMath.TRS(cameraData.UniversePosition, cameraData.UniverseRotation, new float3(1F)));
            double4x4 clipFromCamera = new double4x4
            (
                1 / halfDim.x,             0, 0, 0,
                            0, 1 / halfDim.y, 0, 0,
                            0,             0, c, d,
                            0,             0, 1, 0
            );

            double4x4 clipFromUniverse = math.mul(clipFromCamera, cameraFromUniverse);

            double3 forward = math.mul(cameraData.UniverseRotation, math.forward());
            DoublePlane clipPlane = new DoublePlane(forward, cameraData.UniversePosition + cameraData.NearClip * forward);

            float errorMultiplier = cameraData.ScreenSpaceError * (2 * math.tan(0.5f * cameraData.FovRad.y)) / cameraData.Resolution.y;

            return new DetailObserverData(in clipFromUniverse, in clipPlane, GetGeometricError, errorMultiplier);
        }

        /// <summary>
        /// Function used to evaluate the geometric error of a bound based on the given observer.
        /// This function is used by <see cref="DetailObserverData"/> constructor.
        /// </summary>
        /// <param name="data">Will get the observer information from this instance.</param>
        /// <param name="clipBounds">Get the geometric error for these double precision bounds.</param>
        /// <returns>Will load the <see cref="UniversalDecoder.NodeContent"/> if this value is lower than the
        /// corresponding <see cref="UniversalDecoder.NodeData.GeometricError"/>.</returns>
        private static float GetGeometricError(in DetailObserverData data, in DoubleBounds clipBounds)
        {
            //
            //  TODO - Can we simplify this further?
            //
            double c = (data.ClipFromUniverse.c0.z + data.ClipFromUniverse.c1.z + data.ClipFromUniverse.c2.z) /
                       (data.ClipFromUniverse.c0.w + data.ClipFromUniverse.c1.w + data.ClipFromUniverse.c2.w);

            double d = data.ClipFromUniverse.c3.z - c * data.ClipFromUniverse.c3.w;

            Assert.IsTrue(clipBounds.Extents.z >= 0);

            double min = clipBounds.Center.z - clipBounds.Extents.z;

            if (min < -1.0)
                min = -1.0;

            float distance = (float)(d / (min - c));

            return data.ErrorMultiplier * distance;
        }
    }
}
