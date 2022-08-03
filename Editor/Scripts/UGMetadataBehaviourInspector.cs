using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEditor;
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.Editor
{
    /// <summary>
    /// Custom inspector for the <see cref="UGMetadataBehaviour"/>.
    /// </summary>
    [CustomEditor(typeof(UGMetadataBehaviour))]
    public class UGMetadataInspector : UnityEditor.Editor
    {
        /// <summary>
        /// Regular expression allowing to extract metadata key prefix.
        /// To group metadata values, the key must be prefixed by at least a character followed by a dot.
        /// </summary>
        private static readonly Regex k_KeyPrefixRegex = new Regex(@"^(?:(.+)\.|)([^.]+)$", RegexOptions.Compiled);

        /// <summary>
        /// Each prefixed keys can be foldout in the inspector. Those are the last expanded results.
        /// </summary>
        private static readonly Dictionary<string, bool> k_Foldout = new Dictionary<string, bool>();

        /// <summary>
        /// Editor wrapper allowing to draw the widgets.
        /// This wrapper allow to execute Unit Tests via Moq.
        /// </summary>
        internal EditorGUILayoutWrapper InspectorWrapper;

        /// <summary>
        /// Editor wrapper allowing to draw the scene gizmo.
        /// This wrapper allow to execute Unit Tests via Moq.
        /// </summary>
        internal SceneHandleWrapper SceneWrapper;

        /// <summary>
        /// Called when a <see cref="UGMetadataBehaviour"/> is selected and must be displayed in an inspector view.
        /// </summary>
        public override void OnInspectorGUI()
        {
            InspectorWrapper = new EditorGUILayoutWrapper();

            OnInspectorGUI(serializedObject.targetObject as UGMetadataBehaviour);
        }

        /// <summary>
        /// Custom IMGUI based GUI for the inspector for a given transform.
        /// </summary>
        /// <param name="target">Target to draw the inspector for.</param>
        internal virtual void OnInspectorGUI(UGMetadataBehaviour target)
        {
            Dictionary<string, object> metadata = target == null
                ? null
                : target.Metadata?.Properties;

            if (metadata is null)
                DrawEmptyMetadata();
            else
                DrawMetadata(metadata);
        }

        /// <summary>
        /// Draw in the inspector metadata values under a foldout group.
        /// <remarks>The foldout result is stored in <see cref="k_Foldout"/>.</remarks>
        /// </summary>
        /// <param name="name">Label to display in the foldout group.</param>
        /// <param name="metadata">Values to display in the group.</param>
        private void DrawInGroup(string name, IEnumerable<KeyValuePair<string, object>> metadata)
        {
            bool foldout = k_Foldout.TryGetValue(name, out bool value) ? value : true;
            k_Foldout[name] = foldout = InspectorWrapper.BeginFoldoutHeaderGroup(foldout, name);

            if (foldout) 
                DrawFields(metadata, key => key.Remove(0, name.Length + 1));

            InspectorWrapper.EndFoldoutHeaderGroup();
        }
        
        /// <summary>
        /// Draw multiple metadata values.
        /// </summary>
        /// <param name="metadata">Multiple key / values to draw.</param>
        /// <param name="labelGetter">Based on the <paramref name="metadata"/> keys, generate a label from this function.</param>
        private void DrawFields(IEnumerable<KeyValuePair<string, object>> metadata, Func<string, string> labelGetter)
        {
            foreach (KeyValuePair<string, object> kvp in metadata)
            {
                string key = labelGetter(kvp.Key);

                InspectorWrapper.DrawField(key, kvp.Value);
            }
        }

        /// <summary>
        /// Draw multiple metadata values and group them under foldout groups based on their keys.
        /// </summary>
        /// <param name="metadata">Multiple key / values to draw.</param>
        private void DrawMetadata(Dictionary<string, object> metadata)
        {
            foreach (IGrouping<string, KeyValuePair<string, object>> grp in metadata
                         .GroupBy(each => k_KeyPrefixRegex.Match(each.Key).Groups[1].Value))
            {
                if (string.IsNullOrWhiteSpace(grp.Key))
                    DrawFields(grp, key => key);
                else
                    DrawInGroup(grp.Key, grp);
            }
        }

        /// <summary>
        /// Draw information when the metadata is a empty dictionary or <see langword="null"/>
        /// </summary>
        private void DrawEmptyMetadata()
        {
            InspectorWrapper.Label("No Metadata");
        }

        /// <summary>
        /// Called when a <see cref="UGMetadataBehaviour"/> is selected and must be displayed in the scene view.
        /// </summary>
        private void OnSceneGUI()
        {
            SceneWrapper = new SceneHandleWrapper();
            
            OnSceneGUI(serializedObject.targetObject as UGMetadataBehaviour);
        }

        /// <summary>
        /// Called when a <see cref="UGMetadataBehaviour"/> is selected and must be displayed in the scene view.
        /// </summary>
        internal void OnSceneGUI(UGMetadataBehaviour target)
        {
            HPTransform hpTransform = target.GetComponent<HPTransform>();

            Dictionary<string, object> properties = target.Metadata?.Properties;
            
            if (properties != null 
                && hpTransform != null 
                && properties.TryGetValue(MetadataKeys.Bounds, out object boundsObject) 
                && boundsObject is SerializableDoubleBounds boundsSerialized)
            {
                double4x4 worldFromLocal = target.transform.localToWorldMatrix.ToDouble4x4();
                double4x4 universeFromLocal = hpTransform.UniverseMatrix;

                double4x4 worldFromUniverse = math.mul(worldFromLocal, math.inverse(universeFromLocal));

                DrawBounds(in worldFromUniverse, in boundsSerialized);
            }
        }

        /// <summary>
        /// Draw a <see cref="SerializableDoubleBounds"/> in the Scene view.
        /// </summary>
        /// <param name="transform">Offset to apply to the bounds.</param>
        /// <param name="bounds">Position and Size of the bounds to draw.</param>
        private unsafe void DrawBounds(in double4x4 transform, in SerializableDoubleBounds bounds)
        {
            double3 center = bounds.Center;
            double3 extent = bounds.Extents;

            double3* corners = stackalloc double3[8];

            corners[0] = center + new double3(-extent.x, -extent.y, -extent.z);
            corners[1] = center + new double3(-extent.x, -extent.y, extent.z);
            corners[2] = center + new double3(-extent.x, extent.y, -extent.z);
            corners[3] = center + new double3(-extent.x, extent.y, extent.z);

            corners[4] = center + new double3(extent.x, -extent.y, -extent.z);
            corners[5] = center + new double3(extent.x, -extent.y, extent.z);
            corners[6] = center + new double3(extent.x, extent.y, -extent.z);
            corners[7] = center + new double3(extent.x, extent.y, extent.z);

            for (int i = 0; i < 8; i++)
            {
                corners[i] = transform.HomogeneousTransformPoint(corners[i]);
            }

            for (int i = 0; i < 8; i++)
            {
                SceneWrapper.DrawLine((float3)corners[i], (float3)corners[i ^ 0x01]);
                SceneWrapper.DrawLine((float3)corners[i], (float3)corners[i ^ 0x02]);
                SceneWrapper.DrawLine((float3)corners[i], (float3)corners[i ^ 0x04]);
            }
        }
    }
}
