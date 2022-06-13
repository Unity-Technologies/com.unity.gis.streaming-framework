
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Unity.Geospatial.Streaming.Editor
{
    /// <summary>
    /// UI library used to display the <see href="https://docs.unity3d.com/ScriptReference/Component.html">Component</see>
    /// in the scene view.
    /// </summary>
    public class SceneHandleWrapper
    {
        /// <summary>
        /// Draws a line from p1 to p2.
        /// </summary>
        /// <param name="p1">The position of the first line's end point in world space.</param>
        /// <param name="p2">The position of the second line's end point in world space.</param>
        /// <param name="thickness">Line thickness in UI points (zero thickness draws single-pixel line).</param>
        public virtual void DrawLine(float3 p1, float3 p2, float thickness = 0.0f)
        {
            Handles.DrawLine(p1, p2, thickness);
        }

        /// <summary>
        /// Make an unconstrained movement handle.
        /// This handle can move freely in all directions.
        /// Hold down Ctrl (Cmd on macOS) to snap to the grid (see PositioningGameObjects).
        /// Hold Ctrl-Shift (Cmd-Shift on macOS) to snap the object to any Collider surface under the mouse pointer.
        /// </summary>
        /// <param name="position">The position of the handle in the space of Handles.matrix.</param>
        /// <param name="size">The size of the handle in the space of Handles.matrix.
        /// Use HandleUtility.GetHandleSize if you want a constant screen-space size.</param>
        /// <param name="color">Draw the handle with this color.</param>
        /// <returns>Vector3 The new value modified by the user's interaction with the handle.
        /// If the user has not moved the handle, it will return the same value as you passed into the function.</returns>
        public virtual float3 FreeMoveHandle(Vector3 position, float size, Color color)
        {
            
            using (new Handles.DrawingScope(color))
#if UNITY_2020 || UNITY_2021
                return Handles.FreeMoveHandle(position, Quaternion.identity, size, Vector3.zero, Handles.DotHandleCap);
#else
                return Handles.FreeMoveHandle(position, size, Vector3.zero, Handles.DotHandleCap);
#endif
        }
    }
}
