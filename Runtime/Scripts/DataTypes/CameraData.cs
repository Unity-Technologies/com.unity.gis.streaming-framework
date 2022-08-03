using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Store the necessary data to generate a <see cref="DetailObserverData"/> instance
    /// from a <see href="https://docs.unity3d.com/ScriptReference/Camera.html">Camera</see> component. 
    /// </summary>
    public struct CameraData
    {
        /// <summary>
        /// Position in double float precision of the camera in Unity world space.
        /// </summary>
        public double3 UniversePosition;

        /// <summary>
        /// Orientation of the camera. This gives where the camera is looking.
        /// </summary>
        public quaternion UniverseRotation;

        /// <summary>
        /// Output pixel resolution.
        /// </summary>
        public float2 Resolution;

        /// <summary>
        /// Horizontal and vertical field of view of the camera stored in radians.
        /// </summary>
        public float2 FovRad;
        
        /// <summary>
        /// The maximum screen space error used by the observer allowing to specify if the object should be displayed.
        /// This value is used to calculate the <see cref="DetailObserverData.ErrorMultiplier"/>.
        /// Higher value will result in loading less geometry to be loaded since it would require a smaller <see cref="UniversalDecoder.NodeData.GeometricError"/>;
        /// A lower value will result in displaying more geometry.
        /// </summary>
        public float ScreenSpaceError;

        /// <summary>
        /// The distance of the near clipping plane from the the Camera, in world units.
        /// The <see href="https://docs.unity3d.com/ScriptReference/Camera-nearClipPlane.html">near clipping plane</see> is nearest point of the Camera's view frustum.
        /// The Camera cannot see geometry that is closer this distance.
        /// </summary>
        public float NearClip;

        /// <summary>
        /// The distance of the far clipping plane from the Camera, in world units.
        /// The <see href="https://docs.unity3d.com/ScriptReference/Camera-farClipPlane.html">far clipping plane</see> is furthest point of the Camera's view frustum.
        /// The Camera cannot see geometry that is further this distance.
        /// </summary>
        public float FarClip;
    }
}
