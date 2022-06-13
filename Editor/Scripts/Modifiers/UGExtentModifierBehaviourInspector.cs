using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.Geospatial.Streaming.Editor
{
    [CustomEditor(typeof(UGExtentModifierBehaviour))]
    public class UGExtentModifierBehaviourInspector : UnityEditor.Editor
    {
        internal static UGExtentModifierBehaviourInspector Inspector;

        internal const string k_MessageEmpty = "Data Source list cannot contain empty values.";

        /// <summary>
        /// The UI wrapper used to draw the component in the inspector.
        /// </summary>
        internal EditorGUILayoutWrapper GUILayoutWrapper { get; set; }

        private SerializedProperty m_DifferenceDataSourcesProperty;
        private ReorderableList m_DifferenceDataSourcesReorderableList;

        private SerializedProperty m_IntersectionDataSourcesProperty;
        private ReorderableList m_IntersectionDataSourcesReorderableList;

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

        public override void OnInspectorGUI()
        {
            GUILayoutWrapper = new EditorGUILayoutWrapper();
            OnInspectorGUI(target as UGExtentModifierBehaviour);
        }

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

        private void DrawDifferenceDataSourceElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            DrawDataSourceElements(m_DifferenceDataSourcesReorderableList, rect, index, isActive, isFocused);
        }

        private void DrawIntersectionDataSourceElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            DrawDataSourceElements(m_IntersectionDataSourcesReorderableList, rect, index, isActive, isFocused);
        }

        private void DrawDataSourceElements(ReorderableList elements, Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = elements.serializedProperty.GetArrayElementAtIndex(index);
            
            GUILayoutWrapper.ObjectField($"Data Source {index}", element, rect.x, rect.y, rect.width);
        }

        internal ReorderableList.HeaderCallbackDelegate DrawHeader(string headerString)
        {
            return rect => GUILayoutWrapper.Label(rect, new GUIContent(headerString));
        }

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
