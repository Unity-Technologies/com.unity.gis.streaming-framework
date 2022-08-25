using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.UIElements;
using Unity.Geospatial.HighPrecision;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{

    /// <summary>
    /// The Global Fly Camera components is a simple camera movement controller that was built
    /// to test the Unity Geospatial Framework. It provides basic controls that allows
    /// the user to fly around the globe with ease and with few restrictions.
    ///
    /// The primary reason for a custom camera controller is to manage the "up" vector
    /// of the camera adequately. This controller does not use the scene's up-axis to
    /// orient itself but, rather, uses the vector linking the center of the planet to
    /// the current position of the camera.
    ///
    /// The camera's controls are as follows:\n
    /// <list>
    ///     <item>`w` `a` `s` `d` : Move and Strafe</item>
    ///     <item>`e` : Move Up</item>
    ///     <item>`q` : Move Down</item>
    ///     <item>`Right Mouse Button` : Pan / Tilt</item>
    ///     <item>`Shift` : Fast Move</item>
    ///     <item>`Scroll` : Increase/Decrease movement speed</item>
    /// </list>
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(HPTransform))]
    public class GlobalFlyCamera : MonoBehaviour
    {
        /// <summary>
        /// Keyboard and mouse information representing the requested camera changes from the user.
        /// </summary>
        private struct UserInput
        {
            /// <summary>
            /// Direction to move based on keyboard key pressed compared to the previous registered state.
            /// </summary>
            public float3 Movement;

            /// <summary>
            /// Mouse movement from the previous mouse position if the mouse button is held.
            /// </summary>
            public Vector2 Mouse;

            /// <summary>
            /// The current mouse scroll delta for the Y axis.
            /// <see href="https://docs.unity3d.com/ScriptReference/Input-mouseScrollDelta.html">Input.mouseScrollDelta</see>
            /// </summary>
            public float Scroll;

            /// <summary>
            /// <see langword="true"/> if the shift key is pressed;
            /// <see langword="false"/> otherwise.
            /// </summary>
            public bool ShiftKey;
        }

        /// <summary>
        /// When this is set to true, the controller automatically adjusts the clip planes
        /// based on the camera's altitude. For some use-cases, this adjustment may not
        /// be adequate.
        /// </summary>
        [SerializeField]
        private bool m_UpdateClipPlanes = true;

        /// <summary>
        /// <inheritdoc cref="m_UpdateClipPlanes"/>
        /// </summary>
        public bool UpdateClipPlanes
        {
            get { return m_UpdateClipPlanes; }
            set { m_UpdateClipPlanes = value; }
        }

        /// <summary>
        /// A factor by which the camera's speed is multiplied if the `shift` key is pressed
        /// </summary>
        [SerializeField]
        private float m_HighSpeedMultiplier = 5.0f;

        /// <summary>
        /// <inheritdoc cref="m_HighSpeedMultiplier"/>
        /// </summary>
        public float HighSpeedMultiplier
        {
            get { return m_HighSpeedMultiplier; }
            set { m_HighSpeedMultiplier = value; }
        }


        /// <summary>
        /// The speed of the camera at the start. This value will change if the user scrolls
        /// up or down.
        /// </summary>
        [SerializeField]
        private float m_Speed = 10.0f;

        /// <summary>
        /// <inheritdoc cref="m_Speed"/>
        /// </summary>
        public float Speed
        {
            get { return m_Speed; }
            set { m_Speed = value; }
        }

        /// <summary>
        /// How much the scrool wheel affects the camera's speed. Higher numbers means a
        /// larger change in velocity for each click of the scroll wheel.
        /// </summary>
        [SerializeField]
        private float m_ScrollSensitivity = 30.0f;

        /// <summary>
        /// <inheritdoc cref="m_ScrollSensitivity"/>
        /// </summary>
        public float ScrollSensitivity
        {
            get { return m_ScrollSensitivity; }
            set { m_ScrollSensitivity = value; }
        }



        /// <summary>
        /// The sensitivity of the mouse. Higher values will cause the camera to rotate faster
        /// when the mouse is moved.
        /// </summary>
        [SerializeField]
        private float m_MouseSensitivity = 1.0f;

        /// <summary>
        /// <inheritdoc cref="m_MouseSensitivity"/>
        /// </summary>
        public float MouseSensitivity
        {
            get { return m_MouseSensitivity; }
            set { m_MouseSensitivity = value; }
        }

        /// <summary>
        /// The radius of the planet. This is used only to calculate the altitude of the camera
        /// to set the clip planes.
        /// </summary>
        [SerializeField]
        private float m_PlanetRadius = 6_371_000;

        /// <summary>
        /// <inheritdoc cref="m_PlanetRadius"/>
        /// </summary>
        public float PlanetRadius
        {
            get { return m_PlanetRadius; }
            set { m_PlanetRadius = value; }
        }


        /// <summary>
        /// The maximum pitch of the camera before it is clamped, in degrees
        /// </summary>
        [SerializeField]
        private float m_MaximumPitch = 85.0f;

        /// <summary>
        /// <inheritdoc cref="m_MaximumPitch"/>
        /// </summary>
        public float MaximumPitch
        {
            get { return m_MaximumPitch; }
            set { m_MaximumPitch = value; }
        }

        /// <summary>
        /// Multiply the mouse movement by this factor.
        /// Lower value will reduce the distance.
        /// Higher value will augment the distance.
        /// </summary>
        private const float k_BaseMouseSensitivity = 0.1f;

        /// <summary>
        /// <see langword="true"/> If the mouse right button was held down at the previous state;
        /// <see langword="false"/> otherwise.
        /// </summary>
        private bool m_LastMouseButton;

        /// <summary>
        /// Last know mouse position allowing to calculate the mouse direction and distance by subtracting this value
        /// from the current mouse position.
        /// </summary>
        private Vector2 m_LastMousePosition;

        /// <summary>
        /// The <see href="https://docs.unity3d.com/ScriptReference/Camera.html">Camera</see> <see href="https://docs.unity3d.com/ScriptReference/Component.html">Component</see>
        /// attached to the <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see>.
        /// </summary>
        private Camera m_Camera;

        /// <summary>
        /// The HPTransform <see href="https://docs.unity3d.com/ScriptReference/Component.html">Component</see>
        /// attached to the <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see>.
        /// </summary>
        private HPTransform m_Transform;

        /// <summary>
        /// Called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// <seealso href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html"/>
        /// </summary>
        private void Start()
        {
            m_Camera = GetComponent<Camera>();
            m_Transform = GetComponent<HPTransform>();
        }

        /// <summary>
        /// Called every frame, if the MonoBehaviour is enabled.
        /// <seealso href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html"/>
        /// </summary>
        private void Update()
        {
            UserInput userInput = GetUserInput();

            float currentSpeed = GetAndAdjustSpeed(ref userInput);
            SetPosition(ref userInput, currentSpeed);
            SetRotation(ref userInput);

            if (m_UpdateClipPlanes)
                SetClipPlanes();
        }

        /// <summary>
        /// Get the user input result for the current frame by getting the movement based on the keyboard and mouse inputs.
        /// </summary>
        /// <returns>Get if the mouse wheel was scrolled vertically, if the shift key is pressed, the difference between
        /// the current mouse position and its previous position and the keyboard movement directives.</returns>
        private UserInput GetUserInput()
        {
            UserInput result;

            result.Scroll = Input.mouseScrollDelta.y;
            result.ShiftKey = Input.GetKey(KeyCode.LeftShift);

            //
            // TODO
            // Will need options to support none QWERTY keyboard layouts
            //
            result.Movement = new float3(
                    (Input.GetKey(KeyCode.D) ? 0.0f : -1.0f) + (Input.GetKey(KeyCode.A) ? 0.0f : 1.0f),
                    (Input.GetKey(KeyCode.E) ? 0.0f : -1.0f) + (Input.GetKey(KeyCode.Q) ? 0.0f : 1.0f),
                    (Input.GetKey(KeyCode.W) ? 0.0f : -1.0f) + (Input.GetKey(KeyCode.S) ? 0.0f : 1.0f));

            result.Mouse = m_LastMouseButton
                ? (Vector2)Input.mousePosition - m_LastMousePosition
                : Vector2.zero;

            m_LastMouseButton = Input.GetMouseButton((int)MouseButton.RightMouse);
            m_LastMousePosition = Input.mousePosition;

            return result;
        }

        /// <summary>
        /// Get the speed multiplier based on the options and if the user is holding the shift key.
        /// </summary>
        /// <param name="userInput">Will retrieve if the user is pressing the shift key from this structure.</param>
        /// <returns>Get the speed multiplier result.</returns>
        private float GetAndAdjustSpeed(ref UserInput userInput)
        {
            m_Speed *= math.pow(0.01f * m_ScrollSensitivity + 1.0f, Input.mouseScrollDelta.y);

            float result = m_Speed;
            if (userInput.ShiftKey)
                result *= m_HighSpeedMultiplier;

            return result;
        }

        /// <summary>
        /// Change the HPTransform <see href="https://docs.unity3d.com/ScriptReference/Component.html">Component</see>
        /// position based on the user input.
        /// </summary>
        /// <param name="userInput">The user input values for the current frame.</param>
        /// <param name="multiplier">Multiply the distance by this value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetPosition(ref UserInput userInput, float multiplier)
        {
            float3 dx = multiplier * (userInput.Movement.z * m_Transform.Forward + userInput.Movement.x * m_Transform.Right + userInput.Movement.y * m_Transform.Up) * Time.deltaTime;
            m_Transform.UniversePosition += dx;
        }

        /// <summary>
        /// Update the HPTransform.UniverseRotation based on the user input.
        /// </summary>
        /// <param name="userInput">The user input values for the current frame.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetRotation(ref UserInput userInput)
        {
            float3 up = (float3)math.normalizesafe(m_Transform.UniversePosition);
            float3 right = -math.cross(up, m_Transform.Forward);
            if (right.Equals(float3.zero))
                right = -math.normalizesafe(math.cross(up, m_Transform.Up));
            float3 forward = math.cross(up, right);

            float maximumPitchRad = math.radians(m_MaximumPitch);
            float pitch = math.asin(math.dot(up, m_Transform.Forward));
            pitch += math.radians(k_BaseMouseSensitivity) * m_MouseSensitivity * userInput.Mouse.y;
            pitch = math.clamp(pitch, -maximumPitchRad, maximumPitchRad);

            quaternion yawQuat = HPMath.AxisAngleDegrees(up, k_BaseMouseSensitivity * m_MouseSensitivity * userInput.Mouse.x);

            // TODO - convert to mathematics
            quaternion pitchQuat = Quaternion.AngleAxis(math.degrees(pitch), right);

            m_Transform.UniverseRotation = quaternion.LookRotationSafe(math.mul(math.mul(yawQuat, pitchQuat), forward), up);
        }

        /// <summary>
        /// Adapt the <see href="https://docs.unity3d.com/ScriptReference/Camera-nearClipPlane.html">Camera Near Clip Plane</see>
        /// and the <see href="https://docs.unity3d.com/ScriptReference/Camera-farClipPlane.html">Camera Far Clip Plane</see>
        /// based on the distance of the camera from the water level. This allow to have an adaptive clipping plane.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetClipPlanes()
        {
            float height = (float)(math.length(m_Transform.UniversePosition) - m_PlanetRadius);
            float viewDistance = math.clamp(height, 10.0f, 10000.0f);

            m_Camera.nearClipPlane = 0.1f * viewDistance;
            m_Camera.farClipPlane = 1000000.0f * viewDistance;
        }
    }

}
