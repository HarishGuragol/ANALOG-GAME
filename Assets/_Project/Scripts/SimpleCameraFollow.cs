using UnityEngine;
using UnityEngine.InputSystem;

namespace Analog.Core
{
    // Third-person chase camera.
    //  - Mouse orbits and tilts the view
    //  - When Kael runs sideways or away, the camera swings round behind him
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
        [SerializeField] private float autoFollowSpeed = 2.2f;
        [SerializeField] private float collisionRadius = 0.25f;

        private float yaw = 0f;
        private float pitch = 15f;
        private float mouseIdleTimer = 1f;
        private bool initialized = false;
        private PlayerController playerController;

        // Camera heading shown on screen
        public float Yaw => yaw;
        // How much the mouse alone turned the camera this frame (the player uses this to steer)
        public float MouseYawDelta { get; private set; }

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
            if (target != null && playerController == null)
            {
                playerController = target.GetComponent<PlayerController>();
            }
        }

        private static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void LateUpdate()
        {
            if (target == null || playerController == null)
            {
                FindTarget();
                if (target == null) return;
            }

            float dt = Time.deltaTime;
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            MouseYawDelta = 0f;

            // Esc frees the mouse, clicking the game locks it again
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) LockCursor(false);
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked) LockCursor(true);

            if (!initialized)
            {
                yaw = target.eulerAngles.y;
                initialized = true;
            }

            // Mouse look
            mouseIdleTimer += dt;
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 delta = mouse.delta.ReadValue();
                if (delta.sqrMagnitude > 0.25f) mouseIdleTimer = 0f;

                MouseYawDelta = delta.x * mouseSensitivity;
                yaw += MouseYawDelta;
                pitch -= delta.y * mouseSensitivity;
                pitch = Mathf.Clamp(pitch, -15f, 60f);
            }

            // Auto follow: swing round behind Kael when he runs sideways or away.
            // (Skipped when he walks back toward the camera, so it never flips around.)
            if (playerController != null && mouseIdleTimer > 0.3f)
            {
                Vector3 vel = playerController.HorizontalVelocity;
                if (vel.magnitude > 0.8f)
                {
                    Vector3 camForward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
                    if (Vector3.Dot(vel.normalized, camForward) > -0.3f)
                    {
                        float heading = Mathf.Atan2(vel.x, vel.z) * Mathf.Rad2Deg;
                        yaw = Mathf.LerpAngle(yaw, heading, autoFollowSpeed * dt);
                    }
                }
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
