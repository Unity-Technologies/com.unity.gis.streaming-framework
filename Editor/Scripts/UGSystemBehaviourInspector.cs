using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.Editor
{
    /// <summary>
    /// Custom inspector for the <see cref="UGSystemBehaviour"/>.
    /// </summary>
    [CustomEditor(typeof(UGSystemBehaviour))]
    public class UGSystemBehaviourInspector : UnityEditor.Editor
    {
        internal static UGSystemBehaviourInspector Inspector;
        
        /// <summary>
        /// The UI wrapper used to draw the component in the inspector.
        /// </summary>
        internal EditorGUILayoutWrapper GUILayoutWrapper { get; set; }
        
        /// <summary>
        /// Label with its <see href="https://docs.unity3d.com/ScriptReference/GUI-tooltip.html">tooltip</see> for <see cref="UGSystemBehaviour.streamingMode"/>.
        /// </summary>
        internal static readonly GUIContent StreamingModeContent = new GUIContent(
            "Streaming Mode",
            "Minimal Impact: Uses as little frame time as possible in order to minimize the impact on the application's frame rate.\n\n" +
            "Hold Frame: Hold the frame until everything in frame has been fully loaded. This will cause major hits on the framerate and should only be" +
            "used to produce videos in a deterministic fashion.");

        /// <summary>
        /// Label with its <see href="https://docs.unity3d.com/ScriptReference/GUI-tooltip.html">tooltip</see> for <see cref="m_MainThreadTimeLimit"/>.
        /// </summary>
        internal static readonly GUIContent MainThreadTimeLimitContent = new GUIContent(
            "Main Thread Time Limit",
            "Sets the amount of time, in milliseconds, that the main thread can be held for loading meshes and textures to the GPU. If the main thread needs to be held for more" +
            "than the specified amount of, the remaining work will be delayed to the next frame(s).");

        /// <summary>
        /// Label with its <see href="https://docs.unity3d.com/ScriptReference/GUI-tooltip.html">tooltip</see> for <see cref="m_MaximumSimultaneousContentRequests"/>.
        /// </summary>
        internal static readonly GUIContent MaximumSimultaneousContentRequestsContent = new GUIContent(
            "Maximum Simultaneous Loads",
            "Sets the limit of files that can be loaded simultaneously. If the limit has been reached, remaining files will be delayed until at least one file is loaded.");

        /// <summary>
        /// Label with its <see href="https://docs.unity3d.com/ScriptReference/GUI-tooltip.html">tooltip</see> for <see cref="m_PlanetRadius"/>.
        /// </summary>
        private static readonly GUIContent k_PlanetRadiusContent = new GUIContent(
            "Planet Radius",
            "Sets the planet's radius, which is used by various scripts to control things like clipping planes based off of altitude and the appearance of the skybox.");

        /// <summary>
        /// Horizontal offset specifying where the property modifier widget to be positioned compared to the left side of the label.
        /// </summary>
        private const float k_LabelOffset = 100.0f;

        /// <summary>
        /// List of <see cref="UGSceneObserverBehaviour"/> to use by this system and defining the order of evaluation.
        /// </summary>
        private SerializedProperty m_ObserversProperty;

        /// <summary>
        /// List of <see cref="UGSceneObserverBehaviour"/> instances allowing the user to change the order with the inspector.
        /// </summary>
        private ReorderableList m_ObserversReorderableList;

        /// <summary>
        /// List of <see cref="UGDataSourceObject"/> to use by this system and defining the order of evaluation.
        /// </summary>
        private SerializedProperty m_DataSourcesProperty;

        /// <summary>
        /// List of <see cref="UGDataSourceObject"/> instances allowing the user to change the order with the inspector.
        /// </summary>
        private ReorderableList m_DataSourcesReorderableList;

        /// <summary>
        /// Active <see cref="UGDataSourceObject"/> part of <see cref="UGSystemBehaviour.dataSources"/>.
        /// This <see langword="Array"/> is required to be handle when <see cref="UGSystemBehaviour.dataSources"/> is empty or <see langword="null"/>.
        /// </summary>
        private string[] m_DataSourceStrings;

        /// <summary>
        /// List of <see cref="UGModifierBehaviour"/> to use by this system and defining the order of evaluation.
        /// </summary>
        private SerializedProperty m_ModifiersProperty;

        /// <summary>
        /// List of <see cref="UGModifierBehaviour"/> instances allowing the user to change the order with the inspector.
        /// </summary>
        private ReorderableList m_ModifiersReorderableList;

        /// <summary>
        /// List of <see cref="UGSystemBehaviour.PresenterConfiguration"/> to use by this system and defining the order of evaluation.
        /// </summary>
        private SerializedProperty m_PresentersProperty;

        /// <summary>
        /// List of <see cref="UGSystemBehaviour.PresenterConfiguration"/> instances allowing the user to change the order with the inspector.
        /// </summary>
        private ReorderableList m_PresentersReorderableList;

        /// <summary>
        /// List of <see cref="UGMaterialFactoryObject"/> to use by this system.
        /// </summary>
        private SerializedProperty m_MaterialFactory;

        /// <summary>
        /// <see langword="true"/> if the advanced options section is expanded;
        /// <see langword="false"/> if collapsed.
        /// </summary>
        private bool m_AdvancedFoldout;

        /// <summary>
        /// Time limit in milliseconds to limit the main tread execution time.
        /// </summary>
        private SerializedProperty m_MainThreadTimeLimit;

        /// <summary>
        /// Used when creating instances of <see cref="UGDataSourceDecoder"/>.
        /// This is the amount of files that will be loaded simultaneously.
        /// If the limit has been reached, items part of the queue needs to be completed before new ones can be added.
        /// </summary>
        private SerializedProperty m_MaximumSimultaneousContentRequests;

        /// <summary>
        /// Sets the planet's radius, which is used by various scripts to control things like clipping planes based off of altitude and the appearance of the skybox.
        /// </summary>
        private SerializedProperty m_PlanetRadius;

        /// <summary>
        /// <see langword="List"/> of the <see href="https://docs.unity3d.com/ScriptReference/LayerMask.html">Layer</see> names.
        /// </summary>
        private List<string> m_UnityLayers;

        /// <summary>
        /// Message displayed when in play mode.
        /// </summary>
        internal const string k_MessagePlaying =
            "The UG System cannot be updated in play mode. Please exit play mode to make changes to this component";
        
        /// <summary>
        /// Message displayed when a list is null;
        /// </summary>
        internal const string k_MessageEmpty = "{0} list cannot be empty.";
        
        /// <summary>
        /// Message displayed when a null value is part of a list.
        /// </summary>
        internal const string k_MessageNull = "{0} list cannot contain empty values.";
        
        /// <summary>
        /// Message displayed when the <see cref="m_MaterialFactory"/> value is not set.
        /// </summary>
        internal const string k_MessageMaterialFactoryNull =
            "Please provide the material factory that corresponds to the render pipeline you are using.";
        
        /// <summary>
        /// <see href="https://docs.unity3d.com/ScriptReference/MenuItem.html">MenuItem</see> creating a default
        /// <see cref="UGSystemBehaviour"/> setup with all required
        /// <see href="https://docs.unity3d.com/ScriptReference/Component.html">Components</see> and a <see cref="FlyCamera"/> when executed.
        /// </summary>
        /// <param name="menuCommand">Context for the <see href="https://docs.unity3d.com/ScriptReference/MenuItem.html">MenuItem</see></param>
        [MenuItem("GameObject/Geospatial/Geospatial System", false, 10)]
        private static void CreateNewSimpleGeospatialSystem(MenuCommand menuCommand)
        {
            GameObject system = CreateNewUGSystem();
            GameObject camera = UGCameraBehaviourInspector.CreateNewUGCamera(system);

            HPTransform cameraTransform = camera.GetComponent<HPTransform>();

            UGSystemBehaviour ugSystem = system.GetComponent<UGSystemBehaviour>();

            ugSystem.sceneObservers = new List<UGSceneObserverBehaviour> { camera.GetComponent<UGSceneObserverBehaviour>() };

            var flyCamera = camera.AddComponent<FlyCamera>();
            flyCamera.Speed = 1743384;
            var lvcs = system.AddComponent<LocalVerticalCoordinateSystem>();

            lvcs.origin = cameraTransform;

            cameraTransform.UniversePosition = new double3(0, 0, -12e6);

            
            Selection.activeObject = system;


        }

        /// <summary>
        /// Create a <see cref="UGSystemBehaviour"/> and a HPRoot <see href="https://docs.unity3d.com/ScriptReference/Component.html">Components</see>
        /// under a new <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see> instance.
        /// </summary>
        /// <returns>The newly created <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see>.</returns>
        public static GameObject CreateNewUGSystem()
        {
            // Create a custom game object
            GameObject go = new GameObject("UG System")
            {
                transform =
                {
                    position = Vector3.zero,
                    rotation = Quaternion.identity,
                    localScale = Vector3.one
                }
            };

            go.AddComponent<HPRoot>();
            go.AddComponent<UGSystemBehaviour>();

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;

            return go;
        }

        /// <summary>
        /// Called when the object is loaded.
        /// <seealso href="https://docs.unity3d.com/ScriptReference/ScriptableObject.OnEnable.html">ScriptableObject.OnEnable</seealso>
        /// </summary>
        private void OnEnable()
        {
            Inspector = this;
            m_UnityLayers = InternalEditorUtility.layers.ToList();

            m_ObserversProperty = serializedObject.FindProperty("sceneObservers");
            m_ObserversReorderableList = new ReorderableList(serializedObject, m_ObserversProperty, true, true, true, true)
            {
                drawHeaderCallback = DrawHeader("Scene Observers"),
                drawElementCallback = DrawSceneObserverElement
            };

            m_DataSourcesProperty = serializedObject.FindProperty("dataSources");
            m_DataSourcesReorderableList = new ReorderableList(serializedObject, m_DataSourcesProperty, true, true, true, true)
            {
                drawHeaderCallback = DrawHeader("Data Sources"),
                drawElementCallback = DrawDataSourceElement
            };

            m_ModifiersProperty = serializedObject.FindProperty("modifiers");
            m_ModifiersReorderableList = new ReorderableList(serializedObject, m_ModifiersProperty, true, true, true, true)
            {
                drawHeaderCallback = DrawHeader("Modifiers"),
                drawElementCallback = DrawModifierElement
            };

            m_PresentersProperty = serializedObject.FindProperty("presenters");
            m_PresentersReorderableList = new ReorderableList(serializedObject, m_PresentersProperty, true, true, true, true)
            {
                drawHeaderCallback = DrawHeader("Presenters"),
                drawElementCallback = DrawPresenterElement,
                elementHeight = 4.5f * EditorGUIUtility.singleLineHeight
            };

            m_MaterialFactory = serializedObject.FindProperty("materialFactory");
            m_MainThreadTimeLimit = serializedObject.FindProperty("mainThreadTimeLimitMS");
            m_MaximumSimultaneousContentRequests = serializedObject.FindProperty("maximumSimultaneousContentRequests");
            m_PlanetRadius = serializedObject.FindProperty("planetRadius");
        }

        /// <summary>
        /// Custom inspector IMGUI.
        /// <seealso href="https://docs.unity3d.com/ScriptReference/Editor.OnInspectorGUI.html">Editor.OnInspectorGUI</seealso>
        /// </summary>
        public override void OnInspectorGUI()
        {
            GUILayoutWrapper ??= new EditorGUILayoutWrapper();

            if (Application.isPlaying)
            {
                GUILayoutWrapper.HelpBox(k_MessagePlaying, MessageType.Info);
                return;
            }

            OnInspectorGUI((UGSystemBehaviour)serializedObject.targetObject);
        }

        /// <summary>
        /// Custom inspector IMGUI.
        /// <seealso href="https://docs.unity3d.com/ScriptReference/Editor.OnInspectorGUI.html">Editor.OnInspectorGUI</seealso>
        /// </summary>
        internal void OnInspectorGUI(UGSystemBehaviour system)
        {
            GUILayoutWrapper.DoLayoutList(m_ObserversReorderableList);

            if (m_ObserversReorderableList.count == 0)
                GUILayoutWrapper.HelpBox(string.Format(k_MessageEmpty, "Scene Observer"), MessageType.Error);

            if (ContainsNull(m_ObserversReorderableList))
                GUILayoutWrapper.HelpBox(string.Format(k_MessageNull, "Scene Observer"), MessageType.Error);

            GUILayoutWrapper.DoLayoutList(m_DataSourcesReorderableList);

            if (ContainsNull(m_DataSourcesReorderableList))
                GUILayoutWrapper.HelpBox(string.Format(k_MessageNull, "Data Source"), MessageType.Error);

            GUILayoutWrapper.DoLayoutList(m_ModifiersReorderableList);

            if (ContainsNull(m_ModifiersReorderableList))
                GUILayoutWrapper.HelpBox(string.Format(k_MessageNull, "Modifier"), MessageType.Error);

            if (system.modifiers is null)
                system.modifiers = new List<UGModifierBehaviour>();

            for (int i = 0; i < system.modifiers.Count; i++)
            {
                if (system.modifiers[i] == null)
                    continue;

                if (!system.modifiers[i].Validate(system, out string errorMsg))
                {
                    GUILayoutWrapper.HelpBox($"Modifier {i}: {errorMsg}", MessageType.Error);
                    break;
                }
            }

            string Selector(UGDataSourceObject ds) => ds is null 
                ? "Null Data Source" 
                : ds.name;

            m_DataSourceStrings = system.dataSources is null || system.dataSources.Count == 0
                ? new string[] { "Null Data Source" }
                : system.dataSources.Select(Selector).ToArray();


            if (m_DataSourcesReorderableList.count > 0)
                GUILayoutWrapper.DoLayoutList(m_PresentersReorderableList);
            
            GUILayoutWrapper.Space(20);

            GUILayoutWrapper.ObjectField(m_MaterialFactory, typeof(UGMaterialFactoryObject));

            if (m_MaterialFactory.objectReferenceValue == null)
                GUILayoutWrapper.HelpBox(k_MessageMaterialFactoryNull, MessageType.Error);

            GUILayoutWrapper.Space(15.0f);

            m_AdvancedFoldout = GUILayoutWrapper.BeginFoldoutHeaderGroup(m_AdvancedFoldout, "Advanced Options");
            if (m_AdvancedFoldout)
            {
                UGSystem.StreamingModes streamingMode = (UGSystem.StreamingModes)GUILayoutWrapper.EnumPopup(StreamingModeContent, system.streamingMode);
                if (streamingMode != system.streamingMode)
                {
                    Undo.RecordObject(target, "Change Streaming Mode");
                    system.streamingMode = streamingMode;
                    EditorUtility.SetDirty(system);
                }

                m_MainThreadTimeLimit.floatValue = GUILayoutWrapper.Slider(
                    MainThreadTimeLimitContent,
                    m_MainThreadTimeLimit.floatValue,
                    0.0f,
                    200.0f);

                m_MaximumSimultaneousContentRequests.intValue = GUILayoutWrapper.IntSlider(
                    MaximumSimultaneousContentRequestsContent,
                    m_MaximumSimultaneousContentRequests.intValue,
                    1,
                    100);

                // TODO
                // This value is not yet used, removed to prevent users complaining it doesn't work.
                // m_PlanetRadius.floatValue = EditorGUILayout.FloatField(m_PlanetRadiusContent, m_PlanetRadius.floatValue);

            }
            GUILayoutWrapper.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Get if a <see langword="null"/> value is present in the given list.
        /// </summary>
        /// <param name="list">List to evaluate its content.</param>
        /// <returns>
        /// <see langword="true"/> if a null value is present;
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

        /// <summary>
        /// Draw the <see cref="UGSceneObserverBehaviour"/> part.
        /// </summary>
        /// <param name="rect">Zone where to draw the GUI elements.</param>
        /// <param name="index"><see cref="UGSceneObserverBehaviour"/> id to draw.</param>
        /// <param name="isActive">
        /// <see langword="true"/> if the GUI has an active state;
        /// <see langword="false"/> if it is deactivated.
        /// </param>
        /// <param name="isFocused">
        /// <see langword="true"/> if the current focus is on this item;
        /// <see langword="false"/> otherwise.
        /// </param>
        private void DrawSceneObserverElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            DrawElement("Observer", m_ObserversReorderableList, rect, index, isActive, isFocused);
        }

        /// <summary>
        /// Draw the <see cref="UGDataSource"/> part.
        /// </summary>
        /// <param name="rect">Zone where to draw the GUI elements.</param>
        /// <param name="index"><see cref="UGDataSource"/> id to draw.</param>
        /// <param name="isActive">
        /// <see langword="true"/> if the GUI has an active state;
        /// <see langword="false"/> if it is deactivated.
        /// </param>
        /// <param name="isFocused">
        /// <see langword="true"/> if the current focus is on this item;
        /// <see langword="false"/> otherwise.
        /// </param>
        private void DrawDataSourceElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            DrawElement("Data Source", m_DataSourcesReorderableList, rect, index, isActive, isFocused);
        }

        /// <summary>
        /// Draw the <see cref="UGModifierBehaviour"/> part.
        /// </summary>
        /// <param name="rect">Zone where to draw the GUI elements.</param>
        /// <param name="index"><see cref="UGModifierBehaviour"/> id to draw.</param>
        /// <param name="isActive">
        /// <see langword="true"/> if the GUI has an active state;
        /// <see langword="false"/> if it is deactivated.
        /// </param>
        /// <param name="isFocused">
        /// <see langword="true"/> if the current focus is on this item;
        /// <see langword="false"/> otherwise.
        /// </param>
        private void DrawModifierElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            DrawElement("Modifier", m_ModifiersReorderableList, rect, index, isActive, isFocused);
        }

        /// <summary>
        /// Draw an item part of a given <paramref name="list"/>.
        /// </summary>
        /// <param name="label">Display this text before the index number.</param>
        /// <param name="list">Pick the element to draw from this list.</param>
        /// <param name="rect">Zone where to draw the GUI elements.</param>
        /// <param name="index"><see cref="UGSceneObserverBehaviour"/> id to draw.</param>
        /// <param name="isActive">
        /// <see langword="true"/> if the GUI has an active state;
        /// <see langword="false"/> if it is deactivated.
        /// </param>
        /// <param name="isFocused">
        /// <see langword="true"/> if the current focus is on this item;
        /// <see langword="false"/> otherwise.
        /// </param>
        private void DrawElement(string label, ReorderableList list, Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);

            GUILayoutWrapper.ObjectField($"{label} {index}", element, rect.x, rect.y, rect.width);
        }


        /// <summary>
        /// Draw the <see cref="UGBehaviourPresenter"/> part.
        /// </summary>
        /// <param name="rect">Zone where to draw the GUI elements.</param>
        /// <param name="index"><see cref="UGBehaviourPresenter"/> id to draw.</param>
        /// <param name="isActive">
        /// <see langword="true"/> if the GUI has an active state;
        /// <see langword="false"/> if it is deactivated.
        /// </param>
        /// <param name="isFocused">
        /// <see langword="true"/> if the current focus is on this item;
        /// <see langword="false"/> otherwise.
        /// </param>
        private void DrawPresenterElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;

            SerializedProperty element = m_PresentersReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            
            GUILayoutWrapper.Label(rect.x, rect.y, k_LabelOffset, lineHeight, new GUIContent($"Presenter {index}"));

            SerializedProperty dataSources = element.FindPropertyRelative("dataSources");
            int sourceValue = dataSources.intValue;
            GUILayoutWrapper.MaskField(
                "Sources",
                ref sourceValue, 
                m_DataSourceStrings,
                rect.x, 
                rect.y + lineHeight, 
                rect.width);
            dataSources.intValue = sourceValue;

            SerializedProperty parent = element.FindPropertyRelative("outputRoot");
            GUILayoutWrapper.ObjectField("Parent", parent, rect.x, rect.y + 2 * lineHeight + 1, rect.width);
            
            SerializedProperty outputLayer = element.FindPropertyRelative("outputLayer");
            string layerName = LayerMask.LayerToName(outputLayer.intValue);
            int layerIndex = m_UnityLayers.IndexOf(layerName);

            if (layerIndex < 0)
                layerIndex = 0;

            GUILayoutWrapper.Popup(
                "Unity Layer",
                ref layerIndex,
                InternalEditorUtility.layers,
                rect.x, 
                rect.y + 3 * lineHeight + 1, 
                rect.width);
            outputLayer.intValue = LayerMask.NameToLayer(m_UnityLayers[layerIndex]);
        }

        /// <summary>
        /// Callback to draw a <see href="https://docs.unity3d.com/ScriptReference/EditorGUI.LabelField.html">EditorGUI.LabelField</see>.
        /// </summary>
        /// <param name="headerString">Text to display in the header.</param>
        /// <returns>
        /// A <see langword="delegate"/> returning a new
        /// <see href="https://docs.unity3d.com/ScriptReference/EditorGUI.LabelField.html">EditorGUI.LabelField</see> when executed.
        /// </returns>
        private ReorderableList.HeaderCallbackDelegate DrawHeader(string headerString)
        {
            return rect => GUILayoutWrapper.Label(rect, new GUIContent(headerString));
        }

    }

}
