using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;
using UnityEngine.Assertions;

using Object = UnityEngine.Object;

namespace Unity.Geospatial.Streaming
{
    public class UGExtentModifier : UGModifier
    {
        private readonly struct ExtentPlane
        {
            public double3 Point { get; }
            public double3 Normal { get; }

            public ExtentPlane(double3 normal, double3 point)
            {
                Point = point;
                Normal = normal;
            }
        }

        private bool m_IsValid;

        /// <summary>
        /// List of data sources which will be cut out by the specified extent. This is usually
        /// comprised of a lower detail environment.
        /// </summary>
        private List<UGDataSourceID> m_DifferenceDataSources;

        /// <summary>
        /// List of data sources which will be cropped by the specified extent. This is
        /// usually comprised of a higher detail inset.
        /// </summary>
        private List<UGDataSourceID> m_IntersectionDataSources;

        private DoubleBounds m_EnuBounds;
        private double4x4 m_XzyEnuFromXzyEcef;

        private ExtentPlane[] m_EnuExtentPlanes;

        // TODO private DataAvailability m_LastUpstreamAvailability = DataAvailability.Idle;
        private Queue<InstanceCommand> m_InputQueue = new Queue<InstanceCommand>();
        private Queue<InstanceCommand> m_OutputQueue = new Queue<InstanceCommand>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="extent">Set the bounds and planes based on the points of this extent.</param>
        /// <param name="differenceDataSources">List of data sources which will be cut out by the specified extent. This is usually
        /// comprised of a lower detail environment.</param>
        /// <param name="intersectionDataSources">List of data sources which will be cropped by the specified extent. This is
        /// usually comprised of a higher detail inset.</param>

        public UGExtentModifier(GeodeticExtent extent, IEnumerable<UGDataSourceID> differenceDataSources, IEnumerable<UGDataSourceID> intersectionDataSources)
        {
            SetExtent(extent);

            m_DifferenceDataSources = new List<UGDataSourceID>(differenceDataSources);
            m_IntersectionDataSources = new List<UGDataSourceID>(intersectionDataSources);
        }

        protected override bool IsReadyForData
        {
            get { return (m_InputQueue.Count + m_OutputQueue.Count) == 0; }
        }

        public override bool ScheduleMainThread
        {
            get { return m_InputQueue.Count > 0; }
        }

        protected override bool IsProcessing
        {
            get { return m_InputQueue.Count > 0 || m_OutputQueue.Count > 0; }
        }

        /// <summary>
        /// Set the <see cref="m_XzyEnuFromXzyEcef"/> value based on the center of the given extent.
        /// </summary>
        /// <param name="extent">Get the ECEF center of this extent.</param>
        private void SetCenter(GeodeticExtent extent)
        {
            double2 geodeticCenter = extent.Center;

            double4x4 xzyEcefFromXzyEnu = Wgs84.GetXzyEcefFromXzyEnuMatrix(
                new GeodeticCoordinates(geodeticCenter.y, geodeticCenter.x, 0));

            m_XzyEnuFromXzyEcef = math.inverse(xzyEcefFromXzyEnu);
        }

        /// <summary>
        /// Based on a list of points, set <see cref="m_EnuBounds"/> value from the min / max coordinates.
        /// </summary>
        /// <param name="points">Get the min / max values of these coordinates.</param>
        private void SetBounds(IEnumerable<double3> points)
        {
            double3 enuMin = new double3(double.MaxValue, double.MaxValue, double.MaxValue);
            double3 enuMax = new double3(double.MinValue, double.MinValue, double.MinValue);
            foreach (double3 point in points)
            {
                enuMin = math.min(enuMin, point);
                enuMax = math.max(enuMax, point);
            }

            enuMax += new double3(0, 10000, 0);
            enuMin += new double3(0, -10000, 0);

            m_EnuBounds = new DoubleBounds(0.5 * (enuMax + enuMin), enuMax - enuMin);
        }

        /// <summary>
        /// Set the Bounds and Planes based on an extent.
        /// </summary>
        /// <param name="extent">Create the planes and bounds based on the points of this extent.</param>
        private void SetExtent(GeodeticExtent extent)
        {
            if (!extent.IsValid)
            {
                Debug.LogWarning("Extent Modifier isn't valid, ignoring");
                return;
            }

            List<double2> points = extent.Points;
            SetCenter(extent);

            double3[] xzyEnuExtent = points
                        .Select(p => Wgs84.GeodeticToXzyEcef(new GeodeticCoordinates(p.y, p.x, 0), float3.zero).Position)
                        .Select(p => m_XzyEnuFromXzyEcef.HomogeneousTransformPoint(p))
                        .ToArray();

            SetBounds(xzyEnuExtent);
            SetPlanes(xzyEnuExtent);

            m_IsValid = true;
        }

        /// <summary>
        /// Set <see cref="m_EnuExtentPlanes"/> value by creating an <see cref="ExtentPlane"/> from each edge part of the given <see cref="points"/>.
        /// </summary>
        /// <param name="points">Get the edges part of this list.</param>
        private void SetPlanes(IReadOnlyList<double3> points)
        {
            double3 up = math.up();

            m_EnuExtentPlanes = new ExtentPlane[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                double3 a = points[i];
                double3 b = points[(i + 1) % points.Count];
                double3 normal = math.cross(up, a - b);
                normal.y = 0;
                normal = math.normalizesafe(normal);

                ExtentPlane extentPlane = new ExtentPlane(normal, a);
                m_EnuExtentPlanes[i] = extentPlane;
            }
        }

        protected override void ProcessData(ref InstanceCommand instance)
        {
            Assert.AreEqual(0, m_InputQueue.Count);
            Assert.AreEqual(0, m_OutputQueue.Count);

            m_InputQueue.Enqueue(instance);
        }

        public override void Dispose()
        {
            //
            //  Method intentionally left blank
            //
        }

        public override void MainThreadUpKeep()
        {
            //
            //  Method left intentionally blank
            //
        }

        public override void MainThreadProcess()
        {
            if (m_InputQueue.Count > 0)
                m_OutputQueue.Enqueue(m_InputQueue.Dequeue());

            if (m_OutputQueue.Count > 0 && Output.IsReadyForData)
            {
                InstanceCommand data = m_OutputQueue.Dequeue();
                if (m_IsValid)
                    ApplyModification(ref data);
                Output.ProcessData(ref data);
            }
        }

        private void ApplyModification(ref InstanceCommand command)
        {
            if (command.Command != InstanceCommand.CommandType.Allocate)
                return;

            ApplyModification(command.Data, double4x4.identity);
        }

        private void ApplyModification(InstanceData instanceData, in double4x4 parentTransform)
        {
            double4x4 xzyEcefFromObject = math.mul(parentTransform, instanceData.Transform);

            if (instanceData.Children != null)
            {
                foreach (InstanceData child in instanceData.Children)
                {
                    ApplyModification(child, in xzyEcefFromObject);
                }
            }

            if (instanceData.Mesh == null)
                return;

            double4x4 xzyenuFromObject = math.mul(m_XzyEnuFromXzyEcef, xzyEcefFromObject);

            DoubleBounds meshBounds = NormalizeMeshBounds(instanceData.Mesh.bounds);

            DoubleBounds meshEnuBounds = DoubleBounds.Transform3x4(meshBounds, xzyenuFromObject);

            bool intersects = m_EnuBounds.Intersects(meshEnuBounds);

            if (m_DifferenceDataSources.Contains(instanceData.Source))
            {
                if (intersects)
                    ApplyDifference(instanceData, ref xzyenuFromObject);
            }
            else if (m_IntersectionDataSources.Contains(instanceData.Source))
            {
                if (intersects)
                    ApplyIntersection(instanceData, ref xzyenuFromObject);
                else
                    instanceData.Mesh = null;
            }

        }

        /// <summary>
        /// Normalize mesh bounds in order to remove any negative extent
        /// values.
        /// </summary>
        /// <param name="bounds">Bounds which may or may not have negative extent values</param>
        /// <returns>Normalized bounds</returns>
        private static DoubleBounds NormalizeMeshBounds(in Bounds bounds)
        {
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            return new DoubleBounds
            (
                new double3(center.x, center.y, center.z),
                new double3(
                    math.abs(extents.x) * 2,
                    math.abs(extents.y) * 2,
                    math.abs(extents.z) * 2)
            );
        }


        private void ApplyDifference(InstanceData instanceData, ref double4x4 xzyEnuFromObject)
        {
            double4x4 objectFromXzyenu = math.inverse(xzyEnuFromObject);

            Mesh mesh = instanceData.Mesh;

            Mesh.MeshDataArray readMeshDataArray = Mesh.AcquireReadOnlyMeshData(mesh);
            Mesh.MeshData readMeshData = readMeshDataArray[0];

            Mesh.MeshDataArray writeMeshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData writeMeshData = writeMeshDataArray[0];

            double skirtHeight = math.max(m_EnuBounds.Extents.x, m_EnuBounds.Extents.z) * 0.05;

            using MeshEditor meshEditor = new MeshEditor(ref readMeshData, mesh.GetVertexAttributes());
            bool first = true;
            TriangleCollectionIndex keep = default;
            TriangleCollectionIndex edit = meshEditor.GetFirstCollection();

            foreach (ExtentPlane enuPlane in m_EnuExtentPlanes)
            {
                Vector3 normal = objectFromXzyenu.HomogeneousTransformVector(enuPlane.Normal).ToVector3();
                Vector3 point = objectFromXzyenu.HomogeneousTransformPoint(enuPlane.Point).ToVector3();
                Plane cutPlane = new Plane(normal, point);

                List<Edge> extraEdgesFromCut = new List<Edge>();
                EdgeCollection extraEdgeCollection = new EdgeCollection(extraEdgesFromCut);
                meshEditor.Cut(edit, cutPlane, out TriangleCollectionIndex outside, out TriangleCollectionIndex inside, extraEdgeCollection);

                // Add skirting
                double3 direction = objectFromXzyenu.HomogeneousTransformVector(new double3(0, -skirtHeight, 0));
                meshEditor.EdgeExtrude(extraEdgeCollection, true, direction.ToVector3(), out TriangleCollectionIndex extrudeResult);


                if (first)
                {
                    keep = outside;
                    edit = inside;
                    first = false;
                }
                else
                {
                    keep = meshEditor.CombineAndDispose(keep, outside);
                    edit = inside;
                }
                keep = meshEditor.CombineAndDispose(keep, extrudeResult);

                if (!edit.IsValid())
                    break;
            }

            if (keep.IsValid())
            {
                meshEditor.AssignToMeshData(keep, ref writeMeshData);
                Mesh.ApplyAndDisposeWritableMeshData(writeMeshDataArray, mesh);
            }
            else
            {
                writeMeshDataArray.Dispose();

                //
                //  TODO - Keep mesh for later edits
                //
                Object.Destroy(instanceData.Mesh);
                instanceData.Mesh = null;
            }

            readMeshDataArray.Dispose();
        }

        private void ApplyIntersection(InstanceData instanceData, ref double4x4 xzyEnuFromObject)
        {
            double4x4 objectFromXzyenu = math.inverse(xzyEnuFromObject);

            Mesh mesh = instanceData.Mesh;

            Mesh.MeshDataArray readMeshDataArray = Mesh.AcquireReadOnlyMeshData(mesh);
            Mesh.MeshData readMeshData = readMeshDataArray[0];

            Mesh.MeshDataArray writeMeshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData writeMeshData = writeMeshDataArray[0];

            double skirtHeight = Math.Max(m_EnuBounds.Extents.x, m_EnuBounds.Extents.z) * 0.05;

            using MeshEditor meshEditor = new MeshEditor(ref readMeshData, mesh.GetVertexAttributes());
            bool first = true;
            TriangleCollectionIndex edit = meshEditor.GetFirstCollection();

            foreach (ExtentPlane enuPlane in m_EnuExtentPlanes)
            {
                Vector3 normal = objectFromXzyenu.HomogeneousTransformVector(enuPlane.Normal).ToVector3();
                Vector3 point = objectFromXzyenu.HomogeneousTransformPoint(enuPlane.Point).ToVector3();
                Plane cutPlane = new Plane(normal, point);
                Vector3 planeNormal = cutPlane.normal;


                Bounds meshBounds = mesh.bounds;
                Vector3 min = meshBounds.min;
                Vector3 max = meshBounds.max;
                Vector3 outsideMostPoint = new Vector3(
                    planeNormal.x < 0 ? math.min(min.x, max.x) : math.max(min.x, max.x),
                    planeNormal.y < 0 ? math.min(min.y, max.y) : math.max(min.y, max.y),
                    planeNormal.z < 0 ? math.min(min.z, max.z) : math.max(min.z, max.z));

                if (!cutPlane.GetSide(outsideMostPoint))
                    continue;

                List<Edge> extraEdgesFromCut = new List<Edge>();
                EdgeCollection extraEdgeCollection = new EdgeCollection(extraEdgesFromCut);
                meshEditor.Cut(edit, cutPlane, out TriangleCollectionIndex _, out TriangleCollectionIndex inside,
                    extraEdgeCollection);

                // Add skirting
                double3 direction = objectFromXzyenu.HomogeneousTransformVector(new double3(0, -skirtHeight, 0));
                meshEditor.EdgeExtrude(extraEdgeCollection, false, direction.ToVector3(), out TriangleCollectionIndex extrudeResult);

                if (first)
                {
                    edit = inside;
                    first = false;
                }
                else
                {
                    edit = inside;
                }

                if (!edit.IsValid())
                    break;

                edit = meshEditor.CombineAndDispose(edit, extrudeResult);
            }

            if (edit.IsValid())
            {
                meshEditor.AssignToMeshData(edit, ref writeMeshData);
                Mesh.ApplyAndDisposeWritableMeshData(writeMeshDataArray, mesh);
            }
            else
            {
                writeMeshDataArray.Dispose();


                //
                //  TODO - Keep mesh for later edits
                //
                Object.Destroy(instanceData.Mesh);
                instanceData.Mesh = null;
            }

            readMeshDataArray.Dispose();
        }
    }
}
