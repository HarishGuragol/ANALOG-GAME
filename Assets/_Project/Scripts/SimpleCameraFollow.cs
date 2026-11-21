using UnityEngine;
using UnityEngine.InputSystem;

namespace Analog.Core
{
    public class SimpleCameraFollow : MonoBehaviour
    {
        [Header("Target to Follow")]
        [SerializeField] private Transform target;

        [Header("Camera Offset Behind Kael")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 2.2f, -3.8f);
        [SerializeField] private float smoothSpeed = 10f;

        [Header("Mouse Orbit")]
        [SerializeField] private float mouseSensitivity = 0.15f;

        [Header("Auto Follow (camera swings behind Kael when he turns)")]
        [SerializeField] private float autoFollowSpeed = 1.6f;
        [SerializeField] private float collisionRadius = 0.25f;

        private float yaw = 0f;
        private float pitch = 15f;
        private Vector3 lastTargetPos;
        private float mouseIdleTimer = 1f;
        private bool initialized = false;

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
                lastTargetPos = target.position;
                initialized = true;
            }

            // Mouse orbit
            mouseIdleTimer += dt;
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 delta = mouse.delta.ReadValue();
                if (delta.sqrMagnitude > 0.25f) mouseIdleTimer = 0f;
                yaw += delta.x * mouseSensitivity;
                pitch -= delta.y * mouseSensitivity;
                pitch = Mathf.Clamp(pitch, -15f, 60f);
            }

            // Auto follow: if Kael is running away from the camera and the mouse is idle,
            // swing the camera round to sit behind him as he turns
            if (dt > 0f)
            {
                Vector3 vel = (target.position - lastTargetPos) / dt;
                vel.y = 0f;
                lastTargetPos = target.position;

                if (mouseIdleTimer > 0.4f && vel.magnitude > 0.5f)
                {
                    Vector3 camForward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
                    if (Vector3.Dot(vel.normalized, camForward) > 0.2f)
                    {
                        yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, autoFollowSpeed * dt);
                    }
                }
            }

            // Position behind Kael's shoulder
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + Vector3.up * 1.4f;
            Vector3 desired = pivot + rotation * new Vector3(offset.x, 0f, offset.z);

            // Don't let walls get between the camera and Kael
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
