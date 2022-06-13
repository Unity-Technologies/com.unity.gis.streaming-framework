using System.Collections.Generic;

using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    public class GeodeticExtent
    {
        /// <summary>
        /// Indicates whether the extent is valid or not. Extents
        /// are expected to be convex shapes.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Returns the list of points that the extent is comprised
        /// of. These are guaranteed to be clockwise.
        /// </summary>
        private List<double2> m_Points;
        
        /// <summary>
        /// Returns the list of points that the extent is comprised
        /// of. These are guaranteed to be clockwise.
        /// </summary>
        public List<double2> Points 
        {
            get { return m_Points; }
            private set
            {
                IsValid = ValidateExtent(value);

                m_Points = new List<double2>(value);
                if (!IsClockwise(value))
                    m_Points.Reverse();

                Center = GetCenter(m_Points);
            } 
        }

        /// <summary>
        /// Returns the center of the extent, which is defined as the
        /// average coordinate.
        /// </summary>
        public double2 Center { get; private set; }

        public GeodeticExtent(List<double2> extent)
        {
            Points = extent;
        }

        private static bool ValidateExtent(IReadOnlyList<double2> points)
        {
            int count = points.Count;

            if (count < 3)
                return false;

            for (int i = 0; i < count; i++)
            {
                double2 a = points[i];
                double2 b = points[(i + 1) % count];

                if (math.abs(a.x - b.x) < double.Epsilon && math.abs(a.y - b.y) < double.Epsilon)
                    return false;
            }

            if (GetSignedArea(points) == 0)
                return false;

            double2 center = GetCenter(points);
            double4x4 xzyecefFromXzyenu = Wgs84.GetXzyEcefFromXzyEnuMatrix(new GeodeticCoordinates(center.y, center.x, 0));
            double4x4 xzyenuFromXzyecef = math.inverse(xzyecefFromXzyenu);

            double3[] xzyenuPoints = new double3[count];
            for (int i = 0; i < count; i++)
            {
                double3 xzyecefPoint = Wgs84.GeodeticToXzyEcef(new GeodeticCoordinates(points[i].y, points[i].x, 0), float3.zero).Position;
                double3 xzyenuPoint = xzyenuFromXzyecef.HomogeneousTransformPoint(xzyecefPoint);
                xzyenuPoint.y = 0;
                xzyenuPoints[i] = xzyenuPoint;
            }

            return IsConvex(xzyenuPoints);
        }

        private static double2 GetCenter(IReadOnlyCollection<double2> points)
        {
            if (points == null || points.Count == 0)
                return double2.zero;

            double2 min = new double2(double.MaxValue, double.MaxValue);
            double2 max = new double2(double.MinValue, double.MinValue);
            foreach (double2 point in points)
            {
                min = math.min(min, point);
                max = math.max(max, point);
            }


            return 0.5 * (min + max);
        }

        private static double GetSignedArea(IReadOnlyList<double2> points)
        {
            double sum = 0;
            for (int i = 0; i < points.Count; i++)
            {
                double2 a = points[i];
                double2 b = points[(i + 1) % points.Count];
                sum += 0.5 * (a.y + b.y) * (b.x - a.x);
            }
            return sum;
        }

        private static bool IsClockwise(IReadOnlyList<double2> points)
        {
            return GetSignedArea(points) > 0;
        }

        private static bool IsConvex(IReadOnlyList<double3> points)
        {
            bool hullOrientation = false;
            int checkCount = 0;

            for (int i = 0; i < points.Count; i++)
            {
                float3 a = (float3)points[i];
                float3 b = (float3)points[(i + 1) % points.Count];
                float3 c = (float3)points[(i + 2) % points.Count];

                float sineAngle = math.cross(a - b, b - c).y;

                if (sineAngle == 0)
                    continue;

                bool sign = sineAngle > 0;

                if (checkCount++ == 0)
                {
                    hullOrientation = sign;
                    continue;
                }

                if (sign != hullOrientation)
                    return false;

            }

            return true;
        }


    }
}
