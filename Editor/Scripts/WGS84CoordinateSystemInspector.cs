using Unity.Geospatial.HighPrecision;
using Unity.Geospatial.HighPrecision.Editor;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.Editor
{
    /// <summary>
    /// Class to tells an Editor class how to display the coordinate values using <see cref="Wgs84"/>.
    /// </summary>
    public class Wgs84CoordinateSystemInspector : CoordinateSystemInspector
    {
        /// <summary>
        /// The UI wrapper used to draw the component in the inspector.
        /// </summary>
        internal EditorGUILayoutWrapper GUILayoutWrapper { get; set; }

        /// <summary>
        /// Unique name to display allowing to differentiate the related <see cref="LocalCoordinateSystem"/> from others.
        /// </summary>
        internal const string k_Name = "Geodetic - WGS84";
        
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

            ref GeodeticCoordinates coordinates = ref geodetic.Position;
            GeodeticCoordinates oldCoordinates = coordinates;

            float3 rotation = geodetic.EulerAngles;
            float3 oldRotation = rotation;

            float3 oldScale = scale;

            Double3Field(guiLayout, ref coordinates);
            guiLayout.Float3Field("Rotation", ref rotation, "Pitch", "Yaw", "Roll", subMinWidth: MinLabelWidth);

            switch (GetScaleType(target))
            {
                case ScaleTypes.Anisotropic:
                    guiLayout.Float3Field("Scale", ref scale, subMinWidth: MinLabelWidth);
                    break;

                case ScaleTypes.Isotropic:
                    guiLayout.FloatField("Isotropic Scale", ref scale.x);
                    break;
            }

            if (!oldCoordinates.Equals(coordinates) || !rotation.Equals(oldRotation) || !scale.Equals(oldScale))
            {
                coordinates = new GeodeticCoordinates(
                    double.IsNaN(coordinates.Latitude)
                        ? 0.0
                        : coordinates.Latitude,
                    double.IsNaN(coordinates.Longitude)
                        ? 0.0
                        : coordinates.Longitude,
                    double.IsNaN(coordinates.Elevation)
                        ? 0.0
                        : coordinates.Elevation);

                if (GetScaleType(target) == ScaleTypes.Isotropic)
                    scale.y = scale.z = scale.x;

                EuclideanTR result = Wgs84.GeodeticToXzyEcef(coordinates, rotation);

                SetTRS(target, result.Position, result.Rotation, scale);
            }
        }

        /// <summary>
        /// Make text fields for entering three double precision values for a <see cref="GeodeticCoordinates"/>.
        /// </summary>
        /// <param name="guiLayout">Custom IMGUI based GUI for the inspector for a given <see cref="HPNode"/>.</param>
        /// <param name="value">The value to edit.</param>
        private static void Double3Field(EditorGUILayoutWrapper guiLayout, ref GeodeticCoordinates value)
        {
            double3 result = new double3(value.Latitude, value.Longitude, value.Elevation);
            guiLayout.Double3Field("Position", ref result, "Lat", "Long", "Elev", subMinWidth: MinLabelWidth);

            value = new GeodeticCoordinates(result.x, result.y, result.z);
        }
    }
}
