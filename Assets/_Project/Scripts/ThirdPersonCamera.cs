using UnityEngine;
using UnityEngine.InputSystem;

namespace Analog.Core
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Distance & Offsets")]
        [SerializeField] private float distance = 3.5f;
        [SerializeField] private float height = 1.6f;
        [SerializeField] private float shoulderOffset = 0.5f;

        [Header("Sensitivity & Limits")]
        [SerializeField] private float mouseSensitivity = 0.2f;
        [SerializeField] private float minPitch = -25f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float smoothTime = 0.03f;

        private float yaw;
        private float pitch = 10f;
        private Vector3 currentVelocity;

        private void Start()
        {
            FindPlayerTarget();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void FindPlayerTarget()
        {
            if (target == null)
            {
                var player = GameObject.Find("Player");
                if (player != null)
                {
                    target = player.transform;
                    yaw = target.eulerAngles.y;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindPlayerTarget();
                if (target == null) return;
            }

            var mouse = Mouse.current;
            var keyboard = Keyboard.current;

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

            // Read mouse rotation
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * mouseSensitivity;
                pitch -= delta.y * mouseSensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            // Always track right behind Kael's shoulder wherever he runs!
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focusPoint = target.position + Vector3.up * height;
            Vector3 offset = new Vector3(shoulderOffset, 0f, -distance);
            Vector3 targetPosition = focusPoint + (rotation * offset);

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
            transform.LookAt(focusPoint);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
