using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    public struct CameraData
    {
        public double3 UniversePosition;

        public quaternion UniverseRotation;

        public float2 Resolution;

        public float2 FovRad;

        public float ScreenSpaceError;

        public float NearClip;

        public float FarClip;
    }
}
