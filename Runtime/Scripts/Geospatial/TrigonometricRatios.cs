using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    internal readonly struct TrigonometricRatios
    {
        public double4 Ratios { get; }

        public TrigonometricRatios(double4 ratios)
        {
            Ratios = ratios;
        }

        public TrigonometricRatios(GeodeticCoordinates coordinates):
            this(GeodeticCoordinatesToDouble4(coordinates)) { }

        [BurstCompile(CompileSynchronously = true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double4 GeodeticCoordinatesToDouble4(GeodeticCoordinates coordinates)
        {
            double4 rads = math.radians(coordinates.value.xxyy);

            return new double4(math.cos(rads.xz), math.sin(rads.yw));
        }
        
        public double CosLat
        {
            get { return Ratios.x; }
        }

        public double SinLat
        {
            get { return Ratios.z; }
        }

        public double CosLon
        {
            get { return Ratios.y; }
        }

        public double SinLon
        {
            get { return Ratios.w; }
        }
    }
}
