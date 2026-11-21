using UnityEngine;
using UnityEngine.InputSystem;

namespace Analog.Core
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Distance & Offsets")]
        [SerializeField] private float distance = 4.0f;
        [SerializeField] private float height = 1.8f;
        [SerializeField] private float shoulderOffset = 0.6f; // Over-the-shoulder look

        [Header("Sensitivity & Limits")]
        [SerializeField] private float mouseSensitivity = 0.2f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 70f;
        [SerializeField] private float smoothTime = 0.05f;

        private float yaw;
        private float pitch;
        private Vector3 currentVelocity;

        private void Start()
        {
            // Lock and hide cursor for smooth mouse control
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (target != null)
            {
                yaw = target.eulerAngles.y;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            var mouse = Mouse.current;
            var keyboard = Keyboard.current;

            // Press Escape to unlock cursor, click game view to lock back
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            // Mouse rotation
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * mouseSensitivity;
                pitch -= delta.y * mouseSensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            // Calculate rotation and position
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focusPoint = target.position + Vector3.up * height;
            Vector3 offset = new Vector3(shoulderOffset, 0f, -distance);
            Vector3 targetPosition = focusPoint + rotation * offset;

            // Smooth camera motion
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
            transform.LookAt(focusPoint + rotation * Vector3.right * (shoulderOffset * 0.5f));
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
