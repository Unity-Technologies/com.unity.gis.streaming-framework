using System;
using System.Runtime.CompilerServices;

using Unity.Burst;
using UnityEngine;
using Unity.Mathematics;
using Unity.Geospatial.HighPrecision;

namespace Unity.Geospatial.Streaming
{
    public static class Wgs84
    {
        private const double k_a = 6_378_137.0;

        private const double k_b = 6_356_752.314_140;

        private const double k_f = (k_a - k_b) / k_a;

        private const double k_ff = (1.0 - k_f) * (1.0 - k_f);

        private const double k_e2 = 1 - (k_b * k_b)/(k_a * k_a);

        internal static double MajorRadius
        {
            get { return k_a; }
        }

        private static double MinorRadius
        {
            get { return k_b; }
        }

        private static double3 GeodeticToXzyEcef(double latitude, double longitude, double elevation)
        {
            return GeodeticToXzyEcef(new GeodeticCoordinates(latitude, longitude, elevation));
        }

        internal static double3 GeodeticToXzyEcef(GeodeticCoordinates coords)
        {
            TrigonometricRatios ratios = new TrigonometricRatios(coords);
            return GeodeticToXzyEcef(in ratios, coords.Elevation);
        }

        [BurstCompile(CompileSynchronously = true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double3 GeodeticToXzyEcef(in TrigonometricRatios ratios, double elevation)
        {
            double c = 1.0 / math.sqrt(Square(ratios.CosLat) + k_ff * Square(ratios.SinLat));

            double s = c * k_ff;

            return new double3(
                (k_a * c + elevation) * ratios.CosLat * ratios.CosLon,
                (k_a * s + elevation) * ratios.SinLat,
                (k_a * c + elevation) * ratios.CosLat * ratios.SinLon);
        }

        public static EuclideanTR GeodeticToXzyEcef(GeodeticCoordinates position, float3 eulerAngles)
        {
            EuclideanTR result;

            double4x4 newMatrix = GetXzyEcefFromXzyEnuMatrix(position);

            newMatrix.GetTRS(out result.Position, out quaternion geodeticIdentityRotation, out _);

            result.Rotation = math.mul(geodeticIdentityRotation, Quaternion.Euler(FlipPrincipalAxes(eulerAngles)));

            return result;
        }

        public static double4x4 GetXzyEcefFromXzyEnuMatrix(GeodeticCoordinates origin)
        {
            TrigonometricRatios ratios = new TrigonometricRatios(origin);

            double3 xzyecefPosition = GeodeticToXzyEcef(in ratios, origin.Elevation);

            return new double4x4(
                -ratios.SinLon, ratios.CosLon * ratios.CosLat, -ratios.CosLon * ratios.SinLat, xzyecefPosition.x,
                           0.0,                 ratios.SinLat,                  ratios.CosLat, xzyecefPosition.y,
                 ratios.CosLon, ratios.SinLon * ratios.CosLat, -ratios.SinLon * ratios.SinLat, xzyecefPosition.z,
                           0.0,                           0.0,                            0.0,               1.0
            );
        }

        public static GeodeticCoordinates GetGeodeticCoordinates(double3 xzyEcef)
        {
            //
            //  Algorithm taken from:
            //      https://en.wikipedia.org/wiki/Geographic_coordinate_conversion#The_application_of_Ferrari's_solution
            //
            double x = xzyEcef.x;
            double y = xzyEcef.z;
            double z = xzyEcef.y;

            double x2 = Square(x);
            double y2 = Square(y);
            double z2 = Square(z);

            double a = k_a;
            double a2 = Square(k_a);
            double b2 = Square(k_b);

            double e2 = k_e2;
            double e4 = Square(e2);


            double r = math.sqrt(x2 + y2); double r2 = Square(r);
            double ep2 = (a2 - b2) / (b2);
            double F = 54 * b2 * z2;
            double G = r2 + (1.0 - e2) * z2 - e2 * (a2 - b2);

            //
            //  Guard against G going negative when altitudes are very small.
            //
            if (G < 1)
                G = 1;

            double c = (e4 * F * r2) / Cubic(G);
            double s = math.pow(1 + c + math.sqrt(Square(c) + 2 * c), 1.0 / 3.0);
            double P = F / (3 * Square(s + 1.0 + 1.0 / s) * Square(G));
            double Q = math.sqrt(1.0 + 2.0 * e4 * P);
            double r0 =
                ((-P * e2 * r) / (1.0 + Q)) +
                math.sqrt((a2 / 2.0 * (1.0 + 1.0 / Q)) - ((P * (1 - e2) * z2) / (Q * (1 + Q))) - (P * r2 / 2.0));
            double U = math.sqrt(Square(r - e2 * r0) + z2);
            double V = math.sqrt(Square(r - e2 * r0) + (1 - e2) * z2);
            double z0 = (b2 * z) / (a * V);

            double elevation = U * (1.0 - b2 / (a * V));
            double latitude = math.degrees(math.atan((z + ep2 * z0) / r));
            double longitude = math.degrees(math.atan2(y, x));

            if (double.IsNaN(latitude))
            {
                elevation = k_a;
                longitude = 0;
                latitude = 0;
            }

            return new GeodeticCoordinates(latitude, longitude, elevation);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Square(double d)
        {
            return d * d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Cubic(double d)
        {
            return d * d * d;
        }

        public static GeodeticTR XzyEcefToGeodetic(double3 position, quaternion rotation)
        {
            GeodeticTR result;

            result.Position = GetGeodeticCoordinates(position);

            double4x4 ecefFromEnu = GetXzyEcefFromXzyEnuMatrix(result.Position);

            quaternion ecefFromEnuRotation = UGMath.ValidTRS(ecefFromEnu)
                ? ecefFromEnu.GetRotation()
                : quaternion.identity;

            quaternion enuFromEcefRotation = math.inverse(ecefFromEnuRotation);
            quaternion enuRotation = math.mul(enuFromEcefRotation, rotation);

            result.EulerAngles = FlipPrincipalAxes(((Quaternion)enuRotation).eulerAngles);

            return result;
        }

        /// <summary>
        /// Validate a coordinate tile is in the valid WGS84 limits.
        /// </summary>
        /// <param name="minLat">Minimum latitude</param>
        /// <param name="maxLat">Maximum latitude</param>
        /// <param name="minLon">Minimum longitude</param>
        /// <param name="maxLon">Maximum longitude</param>
        /// <param name="minEle">Minimum elevation</param>
        /// <param name="maxEle">Maximum elevation</param>
        /// <exception cref="InvalidOperationException">If the coordinates are outside the limits.</exception>
        private static void ValidateCoordinates(double minLat, double maxLat, double minLon, double maxLon, double minEle, double maxEle)
        {
            if (minLon < -180.0 || minLon >= 180.0)
                throw new InvalidOperationException("Region Bounding Volume cannot have west value outside of -PI to PI range");

            if (maxLon <= -180.0 || maxLon > 180.0)
                throw new InvalidOperationException("Region Bounding Volume cannot have east value outside of -PI to PI range");

            if (maxLat < minLat)
                throw new InvalidOperationException("Region Bounding Volume cannot have north value smaller than south value");

            if (maxEle < minEle)
                throw new InvalidOperationException("Region Bounding Volume cannot have max height value smaller than min height");
        }

        /// <summary>
        /// Convert a region as defined by lat, lon and elevation into an axis aligned bounding volume
        /// in ECEF space.
        /// </summary>
        /// <param name="minLat">Minimum latitude, in degrees</param>
        /// <param name="maxLat">Maximum latitude, in degrees</param>
        /// <param name="minLon">Minimum longitude, in degrees</param>
        /// <param name="maxLon">Maximum longitude, in degrees</param>
        /// <param name="minEle">Minimum elevation, in meters</param>
        /// <param name="maxEle">Maximum elevation, in meters</param>
        /// <returns></returns>
        public static unsafe DoubleBounds ConvertRegionBoundingVolume(double minLat, double maxLat, double minLon, double maxLon, double minEle, double maxEle)
        {
            ValidateCoordinates(minLat, maxLat, minLon, maxLon, minEle, maxEle);

            while (maxLon < minLon)
                maxLon += 360.0;

            //
            //  What I'm calling prime latitude is the latitude that is closest
            //      to the equator in the region indicated within this bounding
            //      volume.
            //
            double primeLatitude;
            if (maxLat > 0 && minLat > 0)
                primeLatitude = minLat;
            else if (minLat < 0 && maxLat < 0)
                primeLatitude = maxLat;
            else
                primeLatitude = 0;

            double3* extremes = stackalloc double3[16];
            int extremesLength = 0;

            if (IsMonotonous(minLon, -180.0, maxLon) || IsMonotonous(minLon, 180.0, maxLon))
                extremes[extremesLength++] = GeodeticToXzyEcef(primeLatitude, -180.0, maxEle);

            if (IsMonotonous(minLon, -90.0, maxLon) || IsMonotonous(minLon, 270.0, maxLon))
                extremes[extremesLength++] = GeodeticToXzyEcef(primeLatitude, -90.0, maxEle);

            if (IsMonotonous(minLon, 0.0, maxLon) || IsMonotonous(minLon, 360.0, maxLon))
                extremes[extremesLength++] = GeodeticToXzyEcef(primeLatitude, 0.0, maxEle);

            if (IsMonotonous(minLon, 90.0, maxLon) || IsMonotonous(minLon, 450.0, maxLon))
                extremes[extremesLength++] = GeodeticToXzyEcef(primeLatitude, 90.0, maxEle);

            extremes[extremesLength++] = GeodeticToXzyEcef(minLat, minLon, minEle);
            extremes[extremesLength++] = GeodeticToXzyEcef(minLat, maxLon, minEle);
            extremes[extremesLength++] = GeodeticToXzyEcef(maxLat, minLon, minEle);
            extremes[extremesLength++] = GeodeticToXzyEcef(maxLat, maxLon, minEle);

            extremes[extremesLength++] = GeodeticToXzyEcef(minLat, minLon, maxEle);
            extremes[extremesLength++] = GeodeticToXzyEcef(minLat, maxLon, maxEle);
            extremes[extremesLength++] = GeodeticToXzyEcef(maxLat, minLon, maxEle);
            extremes[extremesLength++] = GeodeticToXzyEcef(maxLat, maxLon, maxEle);

            extremes[extremesLength++] = GeodeticToXzyEcef(primeLatitude, minLon, maxEle);
            extremes[extremesLength++] = GeodeticToXzyEcef(primeLatitude, maxLon, maxEle);

            double3 min = new double3(double.MaxValue, double.MaxValue, double.MaxValue);
            double3 max = new double3(double.MinValue, double.MinValue, double.MinValue);

            for (int i = 0; i < extremesLength; i++)
            {
                min = math.min(min, extremes[i]);
                max = math.max(max, extremes[i]);
            }

            return new DoubleBounds(0.5 * (min + max), max - min);
        }

        private static bool IsMonotonous(double a, double b, double c)
        {
            return (a <= b && b <= c);
        }

        private static float3 FlipPrincipalAxes(float3 input)
        {
            return new float3(input.x * -1, input.y, input.z * -1);
        }
    }
}
