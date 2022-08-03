using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.Geospatial.Streaming.Editor
{
    /// <summary>
    /// Class to display the <see cref="UGExtentModifierBehaviour"/> fields inside the Unity inspector.
    /// </summary>
    [CustomEditor(typeof(UGExtentModifierBehaviour))]
    public class UGExtentModifierBehaviourInspector : UnityEditor.Editor
    {
        /// <summary>
        /// This is by our Unit Tests because <see cref="OnEnable"/> prevents us isolate execution.
        /// </summary>
        internal static UGExtentModifierBehaviourInspector Inspector;

        /// <summary>
        /// HelpBox message drawn when a null value is part of a list.
        /// </summary>
        internal const string k_MessageEmpty = "Data Source list cannot contain empty values.";

        /// <summary>
        /// The UI wrapper used to draw the component in the inspector.
        /// </summary>
        internal EditorGUILayoutWrapper GUILayoutWrapper { get; set; }

        private SerializedProperty m_DifferenceDataSourcesProperty;
        private ReorderableList m_DifferenceDataSourcesReorderableList;

        private SerializedProperty m_IntersectionDataSourcesProperty;
        private ReorderableList m_IntersectionDataSourcesReorderableList;

        /// <summary>
        /// This function is called when the object is loaded.
        /// </summary>
        public void OnEnable()
        { 
            Inspector = this;
            
            GUILayoutWrapper ??= new EditorGUILayoutWrapper();
            
            m_DifferenceDataSourcesProperty = serializedObject.FindProperty("m_DifferenceDataSources");
            m_DifferenceDataSourcesReorderableList = new ReorderableList(serializedObject, m_DifferenceDataSourcesProperty, true, true, true, true)
            {
                drawHeaderCallback = DrawHeader("Difference"),
                drawElementCallback = DrawDifferenceDataSourceElements
            };

            m_IntersectionDataSourcesProperty = serializedObject.FindProperty("m_IntersectionDataSources");
            m_IntersectionDataSourcesReorderableList = new ReorderableList(serializedObject, m_IntersectionDataSourcesProperty, true, true, true, true)
            {
                drawHeaderCallback = DrawHeader("Intersection"),
                drawElementCallback = DrawIntersectionDataSourceElements
            };
        }

        /// <summary>
        /// Custom IMGUI based GUI for the inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            GUILayoutWrapper = new EditorGUILayoutWrapper();
            OnInspectorGUI(target as UGExtentModifierBehaviour);
        }

        /// <summary>
        /// Custom IMGUI based GUI for the inspector.
        /// </summary>
        /// <param name="target">Display the fields of this extent.</param>
        internal void OnInspectorGUI(UGExtentModifierBehaviour target)
        {
            EditorGUI.BeginChangeCheck();

            GeodeticExtentObject extent = (GeodeticExtentObject)GUILayoutWrapper.ObjectField("Geodetic Extent", target.Extent, typeof(GeodeticExtentObject));

            GUILayoutWrapper.DoLayoutList(m_DifferenceDataSourcesReorderableList);
            if (ContainsNull(m_DifferenceDataSourcesReorderableList))
                GUILayoutWrapper.HelpBox(k_MessageEmpty, MessageType.Error);

            GUILayoutWrapper.DoLayoutList(m_IntersectionDataSourcesReorderableList);
            if (ContainsNull(m_IntersectionDataSourcesReorderableList))
                GUILayoutWrapper.HelpBox(k_MessageEmpty, MessageType.Error);

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Inspector");
                target.Extent = extent;
            }
        }

        /// <summary>
        /// Draw the coordinates for one point part of <see name="m_DifferenceDataSourcesReorderableList"/>.
        /// </summary>
        /// <param name="rect">Where to draw the element.</param>
        /// <param name="index">List index of the point to draw.</param>
        /// <param name="isActive">True if this element is enabled.</param>
        /// <param name="isFocused">True if this element has focus.</param>
        private void DrawDifferenceDataSourceElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            DrawDataSourceElements(m_DifferenceDataSourcesReorderableList, rect, index, isActive, isFocused);
        }

        /// <summary>
        /// Draw the coordinates for one point part of <see name="m_IntersectionDataSourcesReorderableList"/>.
        /// </summary>
        /// <param name="rect">Where to draw the element.</param>
        /// <param name="index">List index of the point to draw.</param>
        /// <param name="isActive">True if this element is enabled.</param>
        /// <param name="isFocused">True if this element has focus.</param>
        private void DrawIntersectionDataSourceElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            DrawDataSourceElements(m_IntersectionDataSourcesReorderableList, rect, index, isActive, isFocused);
        }

        /// <summary>
        /// Draw the coordinates for one point part of the given <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">Pick the element to draw from this list.</param>
        /// <param name="rect">Where to draw the element.</param>
        /// <param name="index">List index of the point to draw.</param>
        /// <param name="isActive">True if this element is enabled.</param>
        /// <param name="isFocused">True if this element has focus.</param>
        private void DrawDataSourceElements(ReorderableList elements, Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = elements.serializedProperty.GetArrayElementAtIndex(index);
            
            GUILayoutWrapper.ObjectField($"Data Source {index}", element, rect.x, rect.y, rect.width);
        }

        /// <summary>
        /// Draw the extent header at the top of the <see cref="m_DifferenceDataSourcesReorderableList"/> / <see cref="m_IntersectionDataSourcesReorderableList"/> list.
        /// </summary>
        /// <param name="headerString">Text to display.</param>
        internal ReorderableList.HeaderCallbackDelegate DrawHeader(string headerString)
        {
            return rect => GUILayoutWrapper.Label(rect, new GUIContent(headerString));
        }

        /// <summary>
        /// Get if a <see langword="null"/> value is part of the given <paramref name="list"/>
        /// </summary>
        /// <param name="list">List to validate.</param>
        /// <returns>
        /// <see langword="true"/> If the given <paramref name="list"/> has a <see langword="null"/> value;
        /// <see langword="false"/> otherwise.
        /// </returns>
        private static bool ContainsNull(ReorderableList list)
        {
            for (int i = 0; i < list.count; i++)
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue == null)
                    return true;
            }
            return false;
        }
    }

}
