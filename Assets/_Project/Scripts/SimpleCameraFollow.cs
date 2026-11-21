using UnityEngine;
using UnityEngine.InputSystem;

namespace Analog.Core
{
    // Third-person chase camera.
    //  - A / D steer: the camera (and Kael) turn left / right
    //  - Mouse also orbits and tilts the view
    //  - Walls never get between the camera and Kael
    public class SimpleCameraFollow : MonoBehaviour
    {
        [Header("Target to Follow")]
        [SerializeField] private Transform target;

        [Header("Camera Offset Behind Kael")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 2.2f, -3.8f);
        [SerializeField] private float smoothSpeed = 10f;

        [Header("Turning")]
        [SerializeField] private float mouseSensitivity = 0.15f;
        [SerializeField] private float steerSpeed = 110f;   // degrees per second for A / D
        [SerializeField] private float collisionRadius = 0.25f;

        private float yaw = 0f;
        private float pitch = 15f;
        private bool initialized = false;

        public float Yaw => yaw;

        private void Start()
        {
            FindTarget();
            LockCursor(true);
        }

        private void FindTarget()
        {
            if (target == null)
            {
                var p = GameObject.Find("Player");
                if (p != null) target = p.transform;
            }
        }

        private static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindTarget();
                if (target == null) return;
            }

            float dt = Time.deltaTime;
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;

            // Esc frees the mouse, clicking the game locks it again
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) LockCursor(false);
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked) LockCursor(true);

            if (!initialized)
            {
                yaw = target.eulerAngles.y;
                initialized = true;
            }

            // A / D steer the view
            if (keyboard != null)
            {
                float steer = 0f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) steer += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) steer -= 1f;
                yaw += steer * steerSpeed * dt;
            }

            // Mouse look
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * mouseSensitivity;
                pitch -= delta.y * mouseSensitivity;
                pitch = Mathf.Clamp(pitch, -15f, 60f);
            }

            // Position behind Kael's shoulder
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + Vector3.up * 1.4f;
            Vector3 desired = pivot + rotation * new Vector3(offset.x, 0f, offset.z);

            // Pull the camera in if a wall is in the way
            Vector3 toCam = desired - pivot;
            float dist = toCam.magnitude;
            if (dist > 0.01f &&
                Physics.SphereCast(pivot, collisionRadius, toCam.normalized, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore) &&
                !hit.transform.IsChildOf(target))
            {
                desired = pivot + toCam.normalized * Mathf.Max(0.6f, hit.distance - 0.1f);
            }

            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-smoothSpeed * dt));
            transform.LookAt(pivot);
        }
    }
}
