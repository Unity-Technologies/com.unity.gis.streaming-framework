using UnityEngine;
using UnityEngine.UIElements;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming
{
    public class EuclideanFlyCamera : MonoBehaviour
    {
        private struct UserInput
        {
            public Vector3 Movement;
            public Vector2 Mouse;
            public float Scroll;
            public bool ShiftKey;
        }

        /// <summary>
        /// A factor by which the camera's speed is multiplied if the `shift` key is pressed
        /// </summary>
        private const float HighSpeedMultiplier = 5.0f;

        /// <summary>
        /// The speed of the camera at the start. This value will change if the user scrolls
        /// up or down.
        /// </summary>
        private float m_Speed = 10.0f;

        /// <summary>
        /// How much the scrool wheel affects the camera's speed. Higher numbers means a
        /// larger change in velocity for each click of the scroll wheel.
        /// </summary>
        private const float ScrollSensitivity = 30.0f;

        /// <summary>
        /// The sensitivity of the mouse. Higher values will cause the camera to rotate faster
        /// when the mouse is moved.
        /// </summary>
        private const float MouseSensitivity = 1.0f;

        private const float BaseMouseSensitivity = 0.1f;
        private bool m_LastMouseButton = false;

        private Vector2 m_LastMousePosition;
        private Camera m_Camera;
        private Transform m_Transform;


        private void Start()
        {
            m_Transform = GetComponent<Transform>();
        }

        // Update is called once per frame
        private void Update()
        {
            UserInput userInput = GetUserInput();

            float currentSpeed = GetAndAdjustSpeed(ref userInput);
            SetPosition(ref userInput, currentSpeed);
            SetRotation(ref userInput);

        }

        private UserInput GetUserInput()
        {
            UserInput result;

            result.Scroll = Input.mouseScrollDelta.y;
            result.ShiftKey = Input.GetKey(KeyCode.LeftShift);
            result.Movement = new Vector3(
                    (Input.GetKey(KeyCode.D) ? 0.0f : -1.0f) + (Input.GetKey(KeyCode.A) ? 0.0f : 1.0f),
                    (Input.GetKey(KeyCode.E) ? 0.0f : -1.0f) + (Input.GetKey(KeyCode.Q) ? 0.0f : 1.0f),
                    (Input.GetKey(KeyCode.W) ? 0.0f : -1.0f) + (Input.GetKey(KeyCode.S) ? 0.0f : 1.0f));

            result.Mouse = Vector2.zero;
            if (m_LastMouseButton)
                result.Mouse = (Vector2)Input.mousePosition - m_LastMousePosition;

            m_LastMouseButton = Input.GetMouseButton((int)MouseButton.RightMouse);
            m_LastMousePosition = Input.mousePosition;

            return result;
        }

        private float GetAndAdjustSpeed(ref UserInput userInput)
        {
            m_Speed *= math.pow((0.01f * ScrollSensitivity) + 1.0f, Input.mouseScrollDelta.y);

            float result = m_Speed;
            if (userInput.ShiftKey)
                result *= HighSpeedMultiplier;

            return result;
        }

        private void SetPosition(ref UserInput userInput, float speed)
        {
            Vector3 dx = (userInput.Movement.z * m_Transform.forward + userInput.Movement.x * m_Transform.right + userInput.Movement.y * m_Transform.up) * (speed * Time.deltaTime);
            m_Transform.position += dx;
        }

        private void SetRotation(ref UserInput userInput)
        {
            Vector3 rot = m_Transform.eulerAngles;

            rot.y += BaseMouseSensitivity * MouseSensitivity * userInput.Mouse.x;
            rot.x -= BaseMouseSensitivity * MouseSensitivity * userInput.Mouse.y;

            m_Transform.eulerAngles = rot;
        }
    }

}
