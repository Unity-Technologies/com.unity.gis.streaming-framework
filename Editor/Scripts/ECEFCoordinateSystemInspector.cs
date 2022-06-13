using Unity.Geospatial.HighPrecision;
using Unity.Geospatial.HighPrecision.Editor;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.Editor
{
    /// <summary>
    /// Class to tells an Editor class how to display the coordinate values using <see cref="EuclideanTR"/>.
    /// </summary>
    public class EcefCoordinateSystemInspector : CoordinateSystemInspector
    {
        /// <summary>
        /// The UI wrapper used to draw the component in the inspector.
        /// </summary>
        internal EditorGUILayoutWrapper GUILayoutWrapper { get; set; }

        /// <summary>
        /// Unique name to display allowing to differentiate the related <see cref="LocalCoordinateSystem"/> from others.
        /// </summary>
        internal const string k_Name = "ECEF";

        /// <summary>
        /// Unique name to display allowing to differentiate the related <see cref="LocalCoordinateSystem"/> from others.
        /// </summary>
        public override string Name
        {
            get { return k_Name; }
        }

        /// <summary>
        /// Minimum width for each axis labels.
        /// </summary>
        private const int MinLabelWidth = 35;

        /// <summary>
        /// Custom IMGUI based GUI for the inspector for a given <see cref="HPNode"/>.
        /// </summary>
        /// <param name="target">Target to draw the inspector for.</param>
        public override void OnInspectorGUI(HPNode target)
        {
            GUILayoutWrapper ??= new EditorGUILayoutWrapper();
            OnInspectorGUI(GUILayoutWrapper, target);
        }

        /// <summary>
        /// Custom IMGUI based GUI for the inspector for a given <see cref="HPNode"/>.
        /// </summary>
        /// <param name="guiLayout">Custom IMGUI based GUI for the inspector for a given <see cref="HPNode"/>.</param>
        /// <param name="target">Target to draw the inspector for.</param>
        private void OnInspectorGUI(EditorGUILayoutWrapper guiLayout, HPNode target)
        {
            GetTRS(target, out double3 translation, out quaternion ecefRotation, out float3 scale);

            GeodeticTR geodetic = Wgs84.XzyEcefToGeodetic(translation, ecefRotation);

            double3 oldTranslation = translation;

            float3 rotation = geodetic.EulerAngles;
            float3 oldRotation = rotation;

            float3 oldScale = scale;

            guiLayout.Double3Field("Position", ref translation, subMinWidth: MinLabelWidth);
            guiLayout.Float3Field("Rotation", ref rotation, "Pitch", "Head", "Roll", subMinWidth: MinLabelWidth);

            switch (GetScaleType(target))
            {
                case ScaleTypes.Anisotropic:
                    guiLayout.Float3Field("Scale", ref scale, subMinWidth: MinLabelWidth);
                    break;

                case ScaleTypes.Isotropic:
                    guiLayout.FloatField("Isotropic Scale", ref scale.x);
                    break;
            }
            
            if (!oldTranslation.Equals(translation) || !rotation.Equals(oldRotation) || !scale.Equals(oldScale))
            {
                if (GetScaleType(target) == ScaleTypes.Isotropic)
                    scale.y = scale.z = scale.x;

                GeodeticTR newGeodetic = Wgs84.XzyEcefToGeodetic(translation, ecefRotation);

                EuclideanTR result = Wgs84.GeodeticToXzyEcef(newGeodetic.Position, rotation);

                SetTRS(target, result.Position, result.Rotation, scale);
            }
        }
    }
}
