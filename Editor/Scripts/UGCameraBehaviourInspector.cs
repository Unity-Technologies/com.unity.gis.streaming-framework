using UnityEditor;
using UnityEngine;
using Unity.Geospatial.HighPrecision;

namespace Unity.Geospatial.Streaming.Editor
{
    public static class UGCameraBehaviourInspector
    {
        public static GameObject CreateNewUGCamera(GameObject parent)
        {
            // Create a custom game object
            GameObject go = new GameObject("UG Camera");
            go.AddComponent<HPTransform>();
            GameObjectUtility.SetParentAndAlign(go, parent);

            Camera camera = go.AddComponent<Camera>();

            camera.nearClipPlane = 100.0f;
            camera.farClipPlane = 1_000_000_000.0f;

            go.AddComponent<UGCameraBehaviour>();


            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;

            return go;
        }
    }
}
