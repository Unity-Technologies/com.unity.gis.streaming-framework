using UnityEditor;
using UnityEngine;
using Unity.Geospatial.HighPrecision;

namespace Unity.Geospatial.Streaming.Editor
{
    /// <summary>
    /// <see cref="UGCameraBehaviour"/> commands.
    /// </summary>
    public static class UGCameraBehaviourInspector
    {
        /// <summary>
        /// Create a new <see cref="UGCameraBehaviour"/> instance under the given parent.
        /// </summary>
        /// <param name="parent">Set this GameObject as the parent of the newly created GameObject.</param>
        /// <returns>The newly created object.</returns>
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
