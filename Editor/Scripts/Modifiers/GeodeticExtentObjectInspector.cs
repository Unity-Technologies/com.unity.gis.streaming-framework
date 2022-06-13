using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming.Editor
{
    [CustomEditor(typeof(GeodeticExtentObject))]
    public class GeodeticExtentObjectInspector : UnityEditor.Editor
    {
        internal static GeodeticExtentObjectInspector Inspector;
        
        /// <summary>
        /// The UI wrapper used to draw the component in the inspector.
        /// </summary>
        internal EditorGUILayoutWrapper GUILayoutWrapper { get; set; }

        /// <summary>
        /// Editor wrapper allowing to draw the scene gizmo.
        /// This wrapper allow to execute Unit Tests via Moq.
        /// </summary>
        internal SceneHandleWrapper SceneWrapper;

        internal const string k_InvalidExtentMessage = "The extent provided must be convex in shape and cannot be applied as-is.";

        internal const string k_ChangesMessage = "Changes have been made to the extent, make sure to apply them!";

        internal const string k_ChangesButtonLabel = "Validate & Apply";
        
        private Vector3[] m_WorldPoints;

        private bool m_HasBeenModified;
        private string m_ErrorString;

        private List<double2> m_InspectorExtent = new List<double2>();
        private readonly List<double2> m_SceneExtent = new List<double2>();
        
        private SerializedProperty m_ExtentProperty;
        private ReorderableList m_ReorderableExtent;

        public void OnEnable()
        {
            Inspector = this;
            
            GUILayoutWrapper ??= new EditorGUILayoutWrapper();

            OnEnable(serializedObject.targetObject as GeodeticExtentObject);
        }

        internal void OnEnable(GeodeticExtentObject target)
        {
            Assert.IsNotNull(target);

            target.Points ??= new List<double2>();

            m_InspectorExtent = new List<double2>(target.Points);

            m_ReorderableExtent = new ReorderableList(
                m_InspectorExtent,
                typeof(double2),
                draggable: true,
                displayHeader: true,
                displayAddButton: true,
                displayRemoveButton: true)
            {
                drawElementCallback = DrawExtentElements,
                drawHeaderCallback = DrawExtentHeader,
                onAddCallback = OnAddExtentPoint
            };

            Tools.hidden = true;

            SceneView.duringSceneGui += OnSceneGUI;
        }



        public void OnDisable()
        {
            Tools.hidden = false;

            SceneView.duringSceneGui -= OnSceneGUI;
        }


        public override void OnInspectorGUI()
        {
            GUILayoutWrapper ??= new EditorGUILayoutWrapper();
            OnInspectorGUI(serializedObject.targetObject as GeodeticExtentObject);
        }
        
        internal void OnInspectorGUI(GeodeticExtentObject target)
        {
            EditorGUI.BeginChangeCheck();

            GUILayoutWrapper.DoLayoutList(m_ReorderableExtent);

            if (EditorGUI.EndChangeCheck())
            {
                m_HasBeenModified = true;
            }

            if (!string.IsNullOrEmpty(m_ErrorString))
            {
                GUILayoutWrapper.HelpBox(m_ErrorString, MessageType.Error);
            }

            if (m_HasBeenModified)
            {
                GUILayoutWrapper.HelpBox(k_ChangesMessage, MessageType.Warning);
                m_ErrorString = null;
            }

            if (m_HasBeenModified && GUILayoutWrapper.Button(k_ChangesButtonLabel))
            {
                if (new GeodeticExtent(m_InspectorExtent).IsValid)
                {
                    m_HasBeenModified = false;
                    target.Points = new List<double2>(m_InspectorExtent);
                    m_ErrorString = null;
                    Undo.RecordObject(target, "Apply Extent");
                    EditorUtility.SetDirty(target);
                }
                else
                {
                    m_ErrorString = k_InvalidExtentMessage;
                }
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            OnSceneGUI(sceneView.camera);
        }

        internal void OnSceneGUI(Camera camera)
        {
            SceneWrapper ??= new SceneHandleWrapper();
            
            m_SceneExtent.Clear();
            m_SceneExtent.AddRange(m_InspectorExtent);

            EditorGUI.BeginChangeCheck();

            float3 cameraPosition = camera.transform.position;

            if (m_SceneExtent == null)
                return;

            if (m_SceneExtent.Count == 0)
                return;

            if (m_WorldPoints == null || m_WorldPoints.Length != m_SceneExtent.Count)
                m_WorldPoints = new Vector3[m_SceneExtent.Count];

            HPNode node = FindObjectOfType<HPRoot>();
            double4x4 worldFromParent = node != null ? node.WorldMatrix : double4x4.identity;
            double4x4 parentFromWorld = math.inverse(worldFromParent);

            //
            //  TODO - Eliminate allocation
            //
            double2 center = new GeodeticExtent(m_SceneExtent).Center;

            GeodeticCoordinates geoCenter = new GeodeticCoordinates(center.y, center.x, 0);
            double4x4 parentFromLocal = Wgs84.GetXzyEcefFromXzyEnuMatrix(geoCenter);
            double4x4 worldFromLocal = math.mul(worldFromParent, parentFromLocal);

            Plane projectionPlane = new Plane(
                worldFromLocal.HomogeneousTransformVector(math.up()).ToVector3(),
                worldFromLocal.HomogeneousTransformPoint(double3.zero).ToVector3());

            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < m_SceneExtent.Count; i++)
            {
                double2 geoPoint = m_SceneExtent[i];
                EuclideanTR point = Wgs84.GeodeticToXzyEcef(new GeodeticCoordinates(geoPoint.y, geoPoint.x, 0), float3.zero);

                Vector3 worldPoint = worldFromParent.HomogeneousTransformPoint(point.Position).ToVector3();


                float size = HandleUtility.GetHandleSize(worldPoint) * 0.05f;

                Color color = (m_ReorderableExtent.index == i) ? Color.green : Color.magenta;

                float3 newWorldPoint = SceneWrapper.FreeMoveHandle(worldPoint, size, color);

                float3 direction = math.normalizesafe(newWorldPoint - cameraPosition);
                projectionPlane.Raycast(new Ray(cameraPosition, direction), out float coef);
                newWorldPoint = cameraPosition + coef * direction;

                m_WorldPoints[i] = newWorldPoint;

                double3 newParentPoint = parentFromWorld.HomogeneousTransformPoint(newWorldPoint);

                GeodeticCoordinates newCoords = Wgs84.XzyEcefToGeodetic(newParentPoint, quaternion.identity).Position;

                m_SceneExtent[i] = new double2(newCoords.Longitude, newCoords.Latitude);
            }

            for (int i = 0; i < m_SceneExtent.Count; i++)
            {
                SceneWrapper.DrawLine(m_WorldPoints[i], m_WorldPoints[(i + 1) % m_SceneExtent.Count]);
            }

            if (EditorGUI.EndChangeCheck())
            {
                m_InspectorExtent.Clear();
                m_InspectorExtent.AddRange(m_SceneExtent);
                m_HasBeenModified = true;
                Repaint();
            }


            //// copy the target object's data to the handle
            // m_BoundsHandle.center = ugBoxExtent.bounds.center;
            // m_BoundsHandle.size = ugBoxExtent.bounds.size;
            // m_BoundsHandle.axes = PrimitiveBoundsHandle.Axes.None;

            //// draw the handle
            // EditorGUI.BeginChangeCheck();
            // using (new Handles.DrawingScope(Color.magenta, ugBoxExtent.transform.localToWorldMatrix))
            //     m_BoundsHandle.DrawHandle();

            // if (EditorGUI.EndChangeCheck())
            // {
            //     // record the target object before setting new values so changes can be undone/redone
            //     Undo.RecordObject(ugBoxExtent, "Change Bounds");
            //
            //     // copy the handle's updated data back to the target object
            //     Bounds newBounds = new Bounds();
            //     newBounds.center = m_BoundsHandle.center;
            //     newBounds.size = m_BoundsHandle.size;
            //     ugBoxExtent.bounds = newBounds;
            // }
        }

        internal void DrawExtentHeader(Rect rect)
        {
            GUILayoutWrapper.Label(rect, new GUIContent("Extent"));
        }

        internal void DrawExtentElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            double2 element = (double2)m_ReorderableExtent.list[index];

            const int labelWidth = 40;
            const int space = 12;
            float cellWidth = (rect.width - space) / 2;

            GUILayoutWrapper.DoubleField("Lat", ref element.y, rect.x, rect.y, cellWidth, labelMinWidth: labelWidth);
            GUILayoutWrapper.DoubleField("Lon", ref element.x, rect.x + cellWidth + space, rect.y, cellWidth, labelMinWidth: labelWidth);

            m_ReorderableExtent.list[index] = element;
        }

        internal static void OnAddExtentPoint(ReorderableList list)
        {
            int last = list.count;

            if (list.count == 0)
            {
                list.list.Add(new double2(0, 0));
            }
            else if (list.count < 2)
            {
                list.list.Add(list.list[last - 1]);
            }
            else
            {
                list.list.Add(0.5 * ((double2)list.list[0] + (double2)list.list[last - 1]));
            }
        }
    }
}
