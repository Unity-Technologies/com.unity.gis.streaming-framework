
using System;

using Unity.Geospatial.HighPrecision;
using Unity.Geospatial.Streaming.UniversalDecoder;
using Unity.Mathematics;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Geospatial.Streaming.Editor
{
    /// <summary>
    /// UI library used to display the Streaming <see href="https://docs.unity3d.com/ScriptReference/Component.html">Component</see> fields.
    /// </summary>
    public class EditorGUILayoutWrapper
    {
        /// <summary>
        /// Force the labels to take at least this amount of width space by default.
        /// </summary>
        private const int LabelMinWidth = 100;

        /// <summary>
        /// Begin a horizontal group and get its rect back.
        /// </summary>
        /// <param name="options">An optional list of layout options that specify extra layout
        ///         properties. Any values passed in here will override settings defined by the style.&lt;br&gt;
        /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight,
        /// GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.</param>
        /// <returns>Get the rectangle of the new item position.</returns>
        public virtual Rect BeginHorizontal(params GUILayoutOption[] options)
        {
            return EditorGUILayout.BeginHorizontal(options);
        }

        /// <summary>
        /// Begin a horizontal group and get its rect back.
        /// </summary>
        /// <param name="style">Optional GUIStyle.</param>
        /// <param name="options">An optional list of layout options that specify extra layout
        ///         properties. Any values passed in here will override settings defined by the style.&lt;br&gt;
        /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight,
        /// GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.</param>
        /// <returns>Get the rectangle of the new item position.</returns>
        public virtual Rect BeginHorizontal(GUIStyle style, params GUILayoutOption[] options)
        {
            return EditorGUILayout.BeginHorizontal(style, options);
        }

        /// <summary>
        /// Close a group started with BeginHorizontal.
        /// </summary>
        public virtual void EndHorizontal()
        {
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Begin a vertical group and get its rect back.
        /// </summary>
        /// <param name="options">An optional list of layout options that specify extra layout
        ///         properties. Any values passed in here will override settings defined by the style.&lt;br&gt;
        /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight,
        /// GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.</param>
        /// <returns>Get the rectangle of the new item position.</returns>
        public virtual Rect BeginVertical(params GUILayoutOption[] options)
        {
            return EditorGUILayout.BeginVertical(options);
        }

        /// <summary>
        /// Begin a vertical group and get its rect back.
        /// </summary>
        /// <param name="style">Optional GUIStyle.</param>
        /// <param name="options">An optional list of layout options that specify extra layout
        ///         properties. Any values passed in here will override settings defined by the style.&lt;br&gt;
        /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight,
        /// GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.</param>
        /// <returns>Get the rectangle of the new item position.</returns>
        public virtual Rect BeginVertical(GUIStyle style, params GUILayoutOption[] options)
        {
            return EditorGUILayout.BeginVertical(style, options);
        }

        /// <summary>
        /// Close a group started with BeginVertical.
        /// </summary>
        public virtual void EndVertical()
        {
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Make a label with a foldout arrow to the left of it.
        /// This is useful for folder-like structures, where child objects only appear if you've unfolded the parent folder.
        /// This control cannot be nested in another BeginFoldoutHeaderGroup.
        /// To use multiple of these foldouts, you must end each method with <see cref="EndFoldoutHeaderGroup"/>.
        /// </summary>
        /// <param name="foldout">The shown foldout state.</param>
        /// <param name="content">The label to show.</param>
        /// <returns>The foldout state selected by the user.
        /// <see langword="true"/> if you should render sub-objects.;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public virtual bool BeginFoldoutHeaderGroup(bool foldout, string content)
        {
            return EditorGUILayout.BeginFoldoutHeaderGroup(foldout, content);
        }

        /// <summary>
        /// Closes a group started with <see cref="BeginFoldoutHeaderGroup"/>.
        /// </summary>
        public virtual void EndFoldoutHeaderGroup()
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// Make text fields for entering two double precision values.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Set the sub-label for the X axis.</param>
        /// <param name="labelY">Set the sub-label for the Y axis.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="subMinWidth">Force the sub-labels to take at least this amount of width space.</param>
        internal void Double2Field(string label, ref double2 value, string labelX = "X", string labelY = "Y", int labelMinWidth = LabelMinWidth, int subMinWidth = 0)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);

            DoubleField(labelX, ref value.x, subMinWidth);
            DoubleField(labelY, ref value.y, subMinWidth);

            EndHorizontal();
        }

        /// <summary>
        /// Make text fields for entering three double precision values.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Set the sub-label for the X axis.</param>
        /// <param name="labelY">Set the sub-label for the Y axis.</param>
        /// <param name="labelZ">Set the sub-label for the Z axis.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="subMinWidth">Force the sub-labels to take at least this amount of width space.</param>
        internal void Double3Field(string label, ref double3 value, string labelX = "X", string labelY = "Y", string labelZ = "Z", int labelMinWidth = LabelMinWidth, int subMinWidth = 0)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);

            DoubleField(labelX, ref value.x, subMinWidth);
            DoubleField(labelY, ref value.y, subMinWidth);
            DoubleField(labelZ, ref value.z, subMinWidth);

            EndHorizontal();
        }

        /// <summary>
        /// Make text fields for entering four double precision values.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Set the sub-label for the X axis.</param>
        /// <param name="labelY">Set the sub-label for the Y axis.</param>
        /// <param name="labelZ">Set the sub-label for the Z axis.</param>
        /// <param name="labelW">Set the sub-label for the W axis.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="subMinWidth">Force the sub-labels to take at least this amount of width space.</param>
        internal void Double4Field(string label, ref double4 value, string labelX = "X", string labelY = "Y", string labelZ = "Z", string labelW = "W", int labelMinWidth = LabelMinWidth, int subMinWidth = 0)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);

            DoubleField(labelX, ref value.x, subMinWidth);
            DoubleField(labelY, ref value.y, subMinWidth);
            DoubleField(labelZ, ref value.z, subMinWidth);
            DoubleField(labelW, ref value.w, subMinWidth);

            EndHorizontal();
        }

        /// <summary>
        /// Make text fields for entering two single precision values.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Set the sub-label for the X axis.</param>
        /// <param name="labelY">Set the sub-label for the Y axis.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="subMinWidth">Force the sub-labels to take at least this amount of width space.</param>
        internal void Float2Field(string label, ref float2 value, string labelX = "X", string labelY = "Y", int labelMinWidth = LabelMinWidth, int subMinWidth = 0)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);

            FloatField(labelX, ref value.x, subMinWidth);
            FloatField(labelY, ref value.y, subMinWidth);

            EndHorizontal();
        }

        /// <summary>
        /// Make text fields for entering three single precision values.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Set the sub-label for the X axis.</param>
        /// <param name="labelY">Set the sub-label for the Y axis.</param>
        /// <param name="labelZ">Set the sub-label for the Z axis.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="subMinWidth">Force the sub-labels to take at least this amount of width space.</param>
        internal void Float3Field(string label, ref float3 value, string labelX = "X", string labelY = "Y", string labelZ = "Z", int labelMinWidth = LabelMinWidth, int subMinWidth = 0)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);

            FloatField(labelX, ref value.x, subMinWidth);
            FloatField(labelY, ref value.y, subMinWidth);
            FloatField(labelZ, ref value.z, subMinWidth);

            EndHorizontal();
        }

        /// <summary>
        /// Make text fields for entering four single precision values.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Set the sub-label for the X axis.</param>
        /// <param name="labelY">Set the sub-label for the Y axis.</param>
        /// <param name="labelZ">Set the sub-label for the Z axis.</param>
        /// <param name="labelW">Set the sub-label for the W axis.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="subMinWidth">Force the sub-labels to take at least this amount of width space.</param>
        internal void Float4Field(string label, ref float4 value, string labelX = "X", string labelY = "Y", string labelZ = "Z", string labelW = "W", int labelMinWidth = LabelMinWidth, int subMinWidth = 0)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);

            FloatField(labelX, ref value.x, subMinWidth);
            FloatField(labelY, ref value.y, subMinWidth);
            FloatField(labelZ, ref value.z, subMinWidth);
            FloatField(labelW, ref value.w, subMinWidth);

            EndHorizontal();
        }

        /// <summary>
        /// Make a help box with a message to the user.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="type">The type of message.</param>
        /// <param name="wide">
        /// <see langword="true"/> the box will cover the whole width of the window;
        /// <see langword="false"/> otherwise it will cover the controls part only.
        /// </param>
        public virtual void HelpBox(string message, MessageType type, bool wide = true)
        {
            EditorGUILayout.HelpBox(message, type, wide);
        }

        /// <summary>
        /// Make text fields for entering two integer values.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Set the sub-label for the X axis.</param>
        /// <param name="labelY">Set the sub-label for the Y axis.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="subMinWidth">Force the sub-labels to take at least this amount of width space.</param>
        internal void Integer2Field(string label, ref int2 value, string labelX = "X", string labelY = "Y", int labelMinWidth = LabelMinWidth, int subMinWidth = 0)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);

            IntegerField(labelX, ref value.x, subMinWidth);
            IntegerField(labelY, ref value.y, subMinWidth);

            EndHorizontal();
        }

        /// <summary>
        /// Make text fields for entering three integer values.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Set the sub-label for the X axis.</param>
        /// <param name="labelY">Set the sub-label for the Y axis.</param>
        /// <param name="labelZ">Set the sub-label for the Z axis.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="subMinWidth">Force the sub-labels to take at least this amount of width space.</param>
        internal void Integer3Field(string label, ref int3 value, string labelX = "X", string labelY = "Y", string labelZ = "Z", int labelMinWidth = LabelMinWidth, int subMinWidth = 0)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);

            IntegerField(labelX, ref value.x, subMinWidth);
            IntegerField(labelY, ref value.y, subMinWidth);
            IntegerField(labelZ, ref value.z, subMinWidth);

            EndHorizontal();
        }

        /// <summary>
        /// Make text fields for entering four integer values.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Set the sub-label for the X axis.</param>
        /// <param name="labelY">Set the sub-label for the Y axis.</param>
        /// <param name="labelZ">Set the sub-label for the Z axis.</param>
        /// <param name="labelW">Set the sub-label for the W axis.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="subMinWidth">Force the sub-labels to take at least this amount of width space.</param>
        internal void Integer4Field(string label, ref int4 value, string labelX = "X", string labelY = "Y", string labelZ = "Z", string labelW = "W", int labelMinWidth = LabelMinWidth, int subMinWidth = 0)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);

            IntegerField(labelX, ref value.x, subMinWidth);
            IntegerField(labelY, ref value.y, subMinWidth);
            IntegerField(labelZ, ref value.z, subMinWidth);
            IntegerField(labelW, ref value.w, subMinWidth);

            EndHorizontal();
        }

        /// <summary>
        /// Make text fields for entering four single precision values for a quaternion struct.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Set the sub-label for the X axis.</param>
        /// <param name="labelY">Set the sub-label for the Y axis.</param>
        /// <param name="labelZ">Set the sub-label for the Z axis.</param>
        /// <param name="labelW">Set the sub-label for the W axis.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="subMinWidth">Force the sub-labels to take at least this amount of width space.</param>
        internal void QuaternionField(string label, ref quaternion value, string labelX = "X", string labelY = "Y", string labelZ = "Z", string labelW = "W", int labelMinWidth = LabelMinWidth, int subMinWidth = 0)
        {
            Float4Field(label, ref value.value, labelX, labelY, labelZ, labelW, labelMinWidth, subMinWidth);
        }

        /// <summary>
        /// Make text fields for entering four single precision values for a quaternion struct.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Set the sub-label for the X axis.</param>
        /// <param name="labelY">Set the sub-label for the Y axis.</param>
        /// <param name="labelZ">Set the sub-label for the Z axis.</param>
        /// <param name="labelW">Set the sub-label for the W axis.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="subMinWidth">Force the sub-labels to take at least this amount of width space.</param>
        internal void QuaternionField(string label, ref Quaternion value, string labelX = "X", string labelY = "Y", string labelZ = "Z", string labelW = "W", int labelMinWidth = LabelMinWidth, int subMinWidth = 0)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);

            FloatField(labelX, ref value.x, subMinWidth);
            FloatField(labelY, ref value.y, subMinWidth);
            FloatField(labelZ, ref value.z, subMinWidth);
            FloatField(labelW, ref value.w, subMinWidth);

            EndHorizontal();
        }

        /// <summary>
        /// Make text fields for entering two single precision values.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Set the sub-label for the X axis.</param>
        /// <param name="labelY">Set the sub-label for the Y axis.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="subMinWidth">Force the sub-labels to take at least this amount of width space.</param>
        internal void Vector2Field(string label, ref Vector2 value, string labelX = "X", string labelY = "Y", int labelMinWidth = LabelMinWidth, int subMinWidth = 0)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);

            FloatField(labelX, ref value.x, subMinWidth);
            FloatField(labelY, ref value.y, subMinWidth);

            EndHorizontal();
        }

        /// <summary>
        /// Make text fields for entering three single precision values.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Set the sub-label for the X axis.</param>
        /// <param name="labelY">Set the sub-label for the Y axis.</param>
        /// <param name="labelZ">Set the sub-label for the Z axis.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="subMinWidth">Force the sub-labels to take at least this amount of width space.</param>
        internal void Vector3Field(string label, ref Vector3 value, string labelX = "X", string labelY = "Y", string labelZ = "Z", int labelMinWidth = LabelMinWidth, int subMinWidth = 0)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);

            FloatField(labelX, ref value.x, subMinWidth);
            FloatField(labelY, ref value.y, subMinWidth);
            FloatField(labelZ, ref value.z, subMinWidth);

            EndHorizontal();
        }

        /// <summary>
        /// Make text fields for entering four single precision values.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Set the sub-label for the X axis.</param>
        /// <param name="labelY">Set the sub-label for the Y axis.</param>
        /// <param name="labelZ">Set the sub-label for the Z axis.</param>
        /// <param name="labelW">Set the sub-label for the W axis.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="subMinWidth">Force the sub-labels to take at least this amount of width space.</param>
        internal void Vector4Field(string label, ref Vector4 value, string labelX = "X", string labelY = "Y", string labelZ = "Z", string labelW = "W", int labelMinWidth = LabelMinWidth, int subMinWidth = 0)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);

            FloatField(labelX, ref value.x, subMinWidth);
            FloatField(labelY, ref value.y, subMinWidth);
            FloatField(labelZ, ref value.z, subMinWidth);
            FloatField(labelW, ref value.w, subMinWidth);

            EndHorizontal();
        }

        /// <summary>
        /// Make text fields for displaying the center and the extent of bounds with double precision.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to display.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        internal void DoubleBoundsField(string label, in DoubleBounds value, int labelMinWidth = LabelMinWidth)
        {
            double3 center = value.Center;
            double3 extents = value.Extents;

            DoubleBoundsField(label, ref center, ref extents, labelMinWidth);
        }

        /// <summary>
        /// Make text fields for displaying the center and the extent of bounds with double precision.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to display.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        internal void DoubleBoundsField(string label, in SerializableDoubleBounds value, int labelMinWidth = LabelMinWidth)
        {
            double3 center = value.Center;
            double3 extents = value.Extents;

            DoubleBoundsField(label, ref center, ref extents, labelMinWidth);
        }

        /// <summary>
        /// Make text fields for displaying the center and the extent of bounds with double precision.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="center">The center part of the bounds to display.</param>
        /// <param name="extents">The extents part of the bounds to display.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        private void DoubleBoundsField(string label, ref double3 center, ref double3 extents, int labelMinWidth = LabelMinWidth)
        {
            Label(label, labelMinWidth);
            Double3Field("   Center", ref center, labelMinWidth: labelMinWidth);
            Double3Field("   Extents", ref extents, labelMinWidth: labelMinWidth);
        }

        private delegate void DrawFieldInRect<T>(ref T value, Rect position);

        /// <summary>
        /// Make a field inside a specified rectangle.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="x">Left side position of the field.</param>
        /// <param name="y">Bottom position of the field.</param>
        /// <param name="width">Limit the field to this width.</param>
        /// <param name="height">Limit the field to this height.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        private void RectField<T>(
            string label,
            float x,
            float y,
            float width,
            float height,
            ref T value,
            DrawFieldInRect<T> drawField,
            int labelMinWidth = LabelMinWidth)
        {
            height = float.IsNaN(height) ? EditorGUIUtility.singleLineHeight : height;

            GUIContent labelContent = GetLabelContent(label, out float labelWidth, labelMinWidth);

            Label(x, y, labelWidth, EditorGUIUtility.singleLineHeight, labelContent);
            drawField(ref value, new Rect(x + labelWidth, y + 1, width - labelWidth, height));
        }

        /// <summary>
        /// Make a text field for entering a double precision value.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="x">Left side position of the field.</param>
        /// <param name="y">Bottom position of the field.</param>
        /// <param name="width">Limit the field to this width.</param>
        /// <param name="height">Limit the field to this height.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        internal void DoubleField(
            string label,
            ref double value,
            float x,
            float y,
            float width,
            float height = float.NaN,
            GUIStyle style = null,
            int labelMinWidth = LabelMinWidth)
        {
            RectField(
                label,
                x,
                y,
                width,
                height,
                ref value,
                (ref double value, Rect position) => DoubleField(ref value, position),
                labelMinWidth);
        }

        /// <summary>
        /// Make a text field for entering a double precision value.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        internal void DoubleField(string label, ref double value, int labelMinWidth = LabelMinWidth)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);
            DoubleField(ref value);
            EndHorizontal();
        }

        /// <summary>
        /// Make a text field for entering a double precision value.
        /// </summary>
        /// <param name="value">The value to edit.</param>
        public virtual void DoubleField(ref double value)
        {
            value = EditorGUILayout.DoubleField(value, GUILayout.ExpandWidth(true));
        }

        /// <summary>
        /// Make a text field for entering a double precision value.
        /// </summary>
        /// <param name="value">The value to edit.</param>
        /// <param name="position">Rectangle on the screen to use for the field.</param>
        public virtual void DoubleField(ref double value, Rect position)
        {
            value = EditorGUI.DoubleField(position, value);
        }

        /// <summary>
        /// Make a text field for entering a single precision value.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        internal void FloatField(string label, ref float value, int labelMinWidth = LabelMinWidth)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);
            FloatField(ref value);
            EndHorizontal();
        }

        /// <summary>
        /// Make a text field for entering a single precision value.
        /// </summary>
        /// <param name="value">The value to edit.</param>
        public virtual void FloatField(ref float value)
        {
            value = EditorGUILayout.FloatField(value, GUILayout.ExpandWidth(true));
        }

        /// <summary>
        /// Make a text field for entering a integer value.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        internal void IntegerField(string label, ref int value, int labelMinWidth = LabelMinWidth)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);
            IntegerField(ref value);
            EndHorizontal();
        }


        /// <summary>
        /// Make a toggle checkbox field.
        /// </summary>
        /// <param name="label">Optional label in front of the toggle.</param>
        /// <param name="value">The shown state of the toggle.</param>
        /// <param name="style">Optional GUIStyle.</param>
        /// <param name="options">An optional list of layout options that specify extra layout
        ///         properties. Any values passed in here will override settings defined by the style.
        /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight,
        /// GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.</param>
        /// <returns>
        /// The selected state of the toggle.
        /// </returns>
        public virtual bool BoolField(
          GUIContent label,
          bool value,
          GUIStyle style = null,
          params GUILayoutOption[] options)
        {
            style ??= EditorStyles.toggle;
            return EditorGUILayout.Toggle(label, value, style, options);
        }

        /// <summary>
        /// Make a text field for entering a integer value.
        /// </summary>
        /// <param name="value">The value to edit.</param>
        public virtual void IntegerField(ref int value)
        {
            value = EditorGUILayout.IntField(value, GUILayout.ExpandWidth(true));
        }

        /// <summary>
        /// Make a slider the user can drag to change a value between a min and a max.
        /// </summary>
        /// <param name="label">Optional label in front of the slider.</param>
        /// <param name="value">The value the slider shows. This determines the position of the draggable thumb.</param>
        /// <param name="leftValue">The value at the left end of the slider.</param>
        /// <param name="rightValue">The value at the right end of the slider.</param>
        /// <param name="options">An optional list of layout options that specify extra layout properties. Any values passed in here will override settings defined by the style.&lt;br&gt;
        /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight,
        /// GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.</param>
        /// <returns>The value that has been set by the user.</returns>
        public virtual float Slider(GUIContent label, float value, float leftValue, float rightValue, params GUILayoutOption[] options)
        {
            return EditorGUILayout.Slider(label, value, leftValue, rightValue, options);
        }

        /// <summary>
        /// Make a slider the user can drag to change a value between a min and a max.
        /// </summary>
        /// <param name="label">Optional label in front of the slider.</param>
        /// <param name="value">The value the slider shows. This determines the position of the draggable thumb.</param>
        /// <param name="leftValue">The value at the left end of the slider.</param>
        /// <param name="rightValue">The value at the right end of the slider.</param>
        /// <param name="options">An optional list of layout options that specify extra layout properties. Any values passed in here will override settings defined by the style.&lt;br&gt;
        /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight,
        /// GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.</param>
        /// <returns>The value that has been set by the user.</returns>
        public virtual int IntSlider(GUIContent label, int value, int leftValue, int rightValue, params GUILayoutOption[] options)
        {
            return EditorGUILayout.IntSlider(label, value, leftValue, rightValue, options);
        }

        /// <summary>
        /// Make an enum popup selection field.
        /// </summary>
        /// <param name="label">Optional label in front of the field.</param>
        /// <param name="selected">The enum option the field shows.</param>
        /// <param name="options">An optional list of layout options that specify extra layout properties. Any values passed in here will override settings defined by the style.&lt;br&gt;
        /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight,
        /// GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.</param>
        /// <returns>
        /// The enum option that has been selected by the user.
        /// </returns>
        public virtual Enum EnumPopup(GUIContent label, Enum selected, params GUILayoutOption[] options)
        {
            return EditorGUILayout.EnumPopup(label, selected, options);
        }

        /// <summary>
        /// Makes a generic popup selection field.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="x">Left side position of the field.</param>
        /// <param name="y">Bottom position of the field.</param>
        /// <param name="width">Limit the field to this width.</param>
        /// <param name="height">Limit the field to this height.</param>
        /// <param name="selectedIndex">The index of the option the field shows.</param>
        /// <param name="displayedOptions">An array with the options shown in the popup.</param>
        internal void Popup(
            string label,
            ref int selectedIndex,
            string[] displayedOptions,
            float x,
            float y,
            float width,
            float height = float.NaN,
            int labelMinWidth = LabelMinWidth)
        {
            RectField(
                label,
                x,
                y,
                width,
                height,
                ref selectedIndex,
                (ref int value, Rect position) => Popup(position, ref value, displayedOptions),
                labelMinWidth);
        }

        /// <summary>
        /// Makes a generic popup selection field.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the field.</param>
        /// <param name="selectedIndex">The index of the option the field shows.</param>
        /// <param name="displayedOptions">An array with the options shown in the popup.</param>
        public virtual void Popup(Rect position, ref int selectedIndex, string[] displayedOptions)
        {
            selectedIndex = EditorGUI.Popup(position, selectedIndex, displayedOptions);
        }

        /// <summary>
        /// Makes an object field. You can assign objects either by drag and drop objects or by selecting an object using the Object Picker.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="x">Left side position of the field.</param>
        /// <param name="y">Bottom position of the field.</param>
        /// <param name="width">Limit the field to this width.</param>
        /// <param name="height">Limit the field to this height.</param>
        /// <param name="property">The object reference property the field shows.</param>
        /// <param name="objType">The type of the objects that can be assigned.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        internal void ObjectField(
            string label,
            SerializedProperty property,
            float x,
            float y,
            float width,
            float height = float.NaN,
            Type objType = null,
            int labelMinWidth = LabelMinWidth)
        {
            RectField(
                label,
                x,
                y,
                width,
                height,
                ref objType,
                (ref Type type, Rect position) => ObjectField(position, property, type, GUIContent.none),
                labelMinWidth);
        }

        /// <summary>
        /// Make a field to receive any object type.
        /// </summary>
        /// <param name="label">Optional label in front of the field.</param>
        /// <param name="obj">The object the field shows.</param>
        /// <param name="x">Left side position of the field.</param>
        /// <param name="y">Bottom position of the field.</param>
        /// <param name="width">Limit the field to this width.</param>
        /// <param name="height">Limit the field to this height.</param>
        /// <param name="objType">The type of the objects that can be assigned.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <returns>
        /// The object that has been set by the user.
        /// </returns>
        internal Object ObjectField(
            string label,
            Object obj,
            float x,
            float y,
            float width,
            float height = float.NaN,
            Type objType = null,
            int labelMinWidth = LabelMinWidth)
        {
            Object result = obj;

            RectField(
                label,
                x,
                y,
                width,
                height,
                ref objType,
                (ref Type type, Rect position) => result = ObjectField(position, obj, type),
                labelMinWidth);

            return result;
        }

        /// <summary>
        /// Makes an object field. You can assign objects either by drag and drop objects or by selecting an object using the Object Picker.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the field.</param>
        /// <param name="property">The object reference property the field shows.</param>
        /// <param name="objType">The type of the objects that can be assigned.</param>
        /// <param name="label">Optional label to display in front of the field. Pass GUIContent.none to hide the label.</param>
        public virtual void ObjectField(Rect position, SerializedProperty property, Type objType = null, GUIContent label = null)
        {
            EditorGUI.ObjectField(position, property, objType, label);
        }

        /// <summary>
        /// Make a field to receive any object type.
        /// </summary>
        /// <param name="label">Optional label in front of the field.</param>
        /// <param name="obj">The object the field shows.</param>
        /// <param name="objType">The type of the objects that can be assigned.</param>
        /// <param name="allowSceneObjects">Allow assigning Scene objects. See Description for more info.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="options">An optional list of layout options that specify extra layout properties. Any values passed in here will override settings defined by the style.
        /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight,
        /// GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.</param>
        /// <returns>
        /// The object that has been set by the user.
        /// </returns>
        internal Object ObjectField(
            string label,
            Object obj,
            Type objType = null,
            bool allowSceneObjects = true,
            int labelMinWidth = LabelMinWidth,
            params GUILayoutOption[] options)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);
            Object result = ObjectField(obj, objType, allowSceneObjects, options);
            EndHorizontal();

            return result;
        }

        /// <summary>
        /// Make a field to receive any object type.
        /// </summary>
        /// <param name="obj">The object the field shows.</param>
        /// <param name="objType">The type of the objects that can be assigned.</param>
        /// <param name="allowSceneObjects">Allow assigning Scene objects. See Description for more info.</param>
        /// <param name="options">An optional list of layout options that specify extra layout properties. Any values passed in here will override settings defined by the style.
        /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight,
        /// GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.</param>
        /// <returns>
        /// The object that has been set by the user.
        /// </returns>
        public virtual Object ObjectField(
            Object obj,
            Type objType = null,
            bool allowSceneObjects = true,
            params GUILayoutOption[] options)
        {
            return EditorGUILayout.ObjectField("", obj, objType, allowSceneObjects, options);
        }

        /// <summary>
        /// Make a field to receive any object type.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the field.</param>
        /// <param name="obj">The object the field shows.</param>
        /// <param name="objType">The type of the objects that can be assigned.</param>
        /// <param name="allowSceneObjects">Allow assigning Scene objects. See Description for more info.</param>
        /// <returns>
        /// The object that has been set by the user.
        /// </returns>
        public virtual Object ObjectField(
            Rect position,
            Object obj,
            Type objType = null,
            bool allowSceneObjects = true)
        {
            return EditorGUI.ObjectField(position, "", obj, objType, allowSceneObjects);
        }

        /// <summary>
        /// Make a field to receive any object type.
        /// </summary>
        /// <param name="label">Optional label in front of the field. Pass GUIContent.none to hide the label.</param>
        /// <param name="property">The object reference property the field shows.</param>
        /// <param name="objType">The type of the objects that can be assigned.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        /// <param name="options">An optional list of layout options that specify extra layout properties. Any values passed in here will override settings defined by the style.&lt;br&gt;
        /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight,
        /// GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.</param>
        internal void ObjectField(
            string label,
            SerializedProperty property,
            Type objType = null,
            int labelMinWidth = LabelMinWidth,
            params GUILayoutOption[] options)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);
            ObjectField(property, objType, GUIContent.none, options);
            EndHorizontal();
        }

        /// <summary>
        /// Make a field to receive any object type.
        /// </summary>
        /// <param name="property">The object reference property the field shows.</param>
        /// <param name="objType">The type of the objects that can be assigned.</param>
        /// <param name="label">Optional label in front of the field. Pass GUIContent.none to hide the label.</param>
        /// <param name="options">An optional list of layout options that specify extra layout properties. Any values passed in here will override settings defined by the style.&lt;br&gt;
        /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight,
        /// GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.</param>
        public virtual void ObjectField(
            SerializedProperty property,
            Type objType = null,
            GUIContent label = null,
            params GUILayoutOption[] options)
        {
            EditorGUILayout.ObjectField(property, objType, label, options);
        }

        /// <summary>
        /// Makes a field for masks.
        /// </summary>
        /// <param name="label">Optional label in front of the field.</param>
        /// <param name="mask">The current mask to display.</param>
        /// <param name="displayedOptions">A string array containing the labels for each flag.</param>
        /// <param name="x">Left side position of the field.</param>
        /// <param name="y">Bottom position of the field.</param>
        /// <param name="width">Limit the field to this width.</param>
        /// <param name="height">Limit the field to this height.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        internal void MaskField(
            string label,
            ref int mask,
            string[] displayedOptions,
            float x,
            float y,
            float width,
            float height = float.NaN,
            int labelMinWidth = LabelMinWidth)
        {
            RectField(
                label,
                x,
                y,
                width,
                height,
                ref mask,
                (ref int value, Rect position) => MaskField(ref value, displayedOptions, position),
                labelMinWidth);
        }

        /// <summary>
        /// Makes a field for masks.
        /// </summary>
        /// <param name="mask">The current mask to display.</param>
        /// <param name="displayedOptions">A string array containing the labels for each flag.</param>
        /// <param name="position">Rectangle on the screen to use for the field.</param>
        public virtual void MaskField(ref int mask, string[] displayedOptions, Rect position)
        {
            mask = EditorGUI.MaskField(position, mask, displayedOptions);
        }

        /// <summary>
        /// Make an auto-layout label.
        /// Labels have no user interaction, do not catch mouse clicks and are always rendered in normal style.
        /// </summary>
        /// <param name="label">Text to display on the label.</param>
        /// <param name="minWidth">Force the label to take at least this amount of width space.</param>
        public virtual void Label(string label, int minWidth = 0)
        {
            GUIContent content = GetLabelContent(label, out float width, minWidth);
            GUILayout.Label(content, GUILayout.Width(width));
        }

        /// <summary>
        /// Calculate the size a label should take in horizontal space.
        /// </summary>
        /// <param name="label">Text to calculate the width.</param>
        /// <param name="width">Returns the width the label should take.</param>
        /// <param name="minWidth">Force the label to take at least this amount of width space.</param>
        /// <returns>Returns a new content with the given label as its text.</returns>
        public virtual GUIContent GetLabelContent(string label, out float width, int minWidth = 0)
        {

            GUIContent content = new GUIContent(label);
            width = math.max(minWidth, GUI.skin.GetStyle("Label").CalcSize(content).x);

            return content;
        }

        /// <summary>
        /// Makes a label field. (Useful for showing read-only info.)
        /// </summary>
        /// <param name="x">Left side position of the label.</param>
        /// <param name="y">Bottom position of the label.</param>
        /// <param name="width">Limit the label to this width.</param>
        /// <param name="height">Limit the label to this height.</param>
        /// <param name="label">Label in front of the label field.</param>
        /// <param name="style">Style information (color, etc) for displaying the label.</param>
        internal void Label(float x, float y, float width, float height, GUIContent label, GUIStyle style = null)
        {
            Label(new Rect(x, y, width, height), label, style ?? EditorStyles.label);
        }

        /// <summary>
        /// Makes a label field. (Useful for showing read-only info.)
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the label field.</param>
        /// <param name="label">Label in front of the label field.</param>
        /// <param name="style">Style information (color, etc) for displaying the label.</param>
        public virtual void Label(Rect position, GUIContent label, GUIStyle style = null)
        {
            EditorGUI.LabelField(position, label, style ?? EditorStyles.label);
        }

        /// <summary>
        /// Make an auto-layout label.
        /// Labels have no user interaction, do not catch mouse clicks and are always rendered in normal style.
        /// </summary>
        /// <param name="label">Text to display on the label.</param>
        /// <param name="value">Text to display after the label.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        internal void Label(string label, string value, int labelMinWidth = LabelMinWidth)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);
            Label(value);
            EndHorizontal();
        }

        /// <summary>
        /// Make a text field for entering a string value.
        /// </summary>
        /// <param name="label">Optional label to display in front of the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        internal void TextField(string label, ref string value, int labelMinWidth = LabelMinWidth)
        {
            BeginHorizontal();
            Label(label, labelMinWidth);
            TextField(ref value);
            EndHorizontal();
        }

        /// <summary>
        /// Make a text field for entering a string value.
        /// </summary>
        /// <param name="value">The value to edit.</param>
        public virtual void TextField(ref string value)
        {
            value = EditorGUILayout.TextField(value, GUILayout.ExpandWidth(true));
        }

        /// <summary>
        /// Make a single press button.
        /// </summary>
        /// <param name="text">Text to display on the button.</param>
        /// <param name="options">An optional list of layout options that specify extra layouting properties. Any values passed in here will override settings defined by the style.&lt;br&gt;
        /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight,
        /// GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.</param>
        /// <returns>
        /// true when the users clicks the button.
        /// </returns>
        public virtual bool Button(string text, params GUILayoutOption[] options)
        {
            return GUILayout.Button(text, options);
        }

        /// <summary>
        /// Draw the given <paramref name="list"/> in the current layout.
        /// </summary>
        /// <param name="list">List requested to be draw.</param>
        public virtual void DoLayoutList(ReorderableList list)
        {
            list.DoLayoutList();
        }

        /// <summary>
        /// Make a small space between the previous control and the following.
        /// </summary>
        /// <param name="width">Space taken.</param>
        public virtual void Space(float width)
        {
            EditorGUILayout.Space(width);
        }

        /// <summary>
        /// Make a label and a value field.
        /// </summary>
        /// <param name="key">Label in front of the field.</param>
        /// <param name="value">Value to show to the right.</param>
        /// <param name="labelMinWidth">Force the label to take at least this amount of width space.</param>
        internal void DrawField(string key, object value, int labelMinWidth = LabelMinWidth)
        {
            switch (value)
            {
                case double d:
                    DoubleField(key, ref d, labelMinWidth);
                    break;
                case float f:
                    FloatField(key, ref f, labelMinWidth);
                    break;
                case int i:
                    IntegerField(key, ref i, labelMinWidth);
                    break;
                case string s:
                    TextField(key, ref s, labelMinWidth);
                    break;
                case SerializableDoubleBounds sBounds:
                    DoubleBoundsField(key, in sBounds, labelMinWidth);
                    break;
                case DoubleBounds bounds:
                    DoubleBoundsField(key, in bounds, labelMinWidth);
                    break;
                case NodeId nodeId:
                    int id = nodeId.Id;
                    IntegerField(key, ref id, labelMinWidth);
                    break;
                case double2 d2:
                    Double2Field(key, ref d2, labelMinWidth: labelMinWidth);
                    break;
                case float2 f2:
                    Float2Field(key, ref f2, labelMinWidth: labelMinWidth);
                    break;
                case int2 i2:
                    Integer2Field(key, ref i2, labelMinWidth: labelMinWidth);
                    break;
                case Vector2 v2:
                    Vector2Field(key, ref v2, labelMinWidth: labelMinWidth);
                    break;
                case double3 d3:
                    Double3Field(key, ref d3, labelMinWidth: labelMinWidth);
                    break;
                case float3 f3:
                    Float3Field(key, ref f3, labelMinWidth: labelMinWidth);
                    break;
                case int3 i3:
                    Integer3Field(key, ref i3, labelMinWidth: labelMinWidth);
                    break;
                case Vector3 v3:
                    Vector3Field(key, ref v3, labelMinWidth: labelMinWidth);
                    break;
                case double4 d4:
                    Double4Field(key, ref d4, labelMinWidth: labelMinWidth);
                    break;
                case float4 f4:
                    Float4Field(key, ref f4, labelMinWidth: labelMinWidth);
                    break;
                case int4 i4:
                    Integer4Field(key, ref i4, labelMinWidth: labelMinWidth);
                    break;
                case Vector4 v4:
                    Vector4Field(key, ref v4, labelMinWidth: labelMinWidth);
                    break;
                case quaternion q1:
                    QuaternionField(key, ref q1, labelMinWidth: labelMinWidth);
                    break;
                case Quaternion q2:
                    QuaternionField(key, ref q2, labelMinWidth: labelMinWidth);
                    break;
                case SerializedProperty property:
                    ObjectField(key, property, labelMinWidth: labelMinWidth);
                    break;
                case Object obj:
                    ObjectField(key, obj, labelMinWidth: labelMinWidth);
                    break;
                default:
                    string nullValue = value is null
                        ? "NULL"
                        : value.ToString();
                    TextField(key, ref nullValue, labelMinWidth);
                    break;

            }
        }
    }
}
