
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    internal static class UGMath
    {
        internal static bool ValidTRS(double4x4 matrix)
        {
            return matrix.c0.w == 0 && matrix.c1.w == 0 && matrix.c2.w == 0 && math.abs(matrix.c3.w) == 1;
        }
    }
}
