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
        private float yaw = 0f;
        private float pitch = 15f;

        private void Start()
        {
            FindTarget();
        }

        private void FindTarget()
        {
            if (target == null)
            {
                var p = GameObject.Find("Player");
                if (p != null) target = p.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindTarget();
                if (target == null) return;
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                // Read mouse delta directly without requiring cursor lock!
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * mouseSensitivity;
                pitch -= delta.y * mouseSensitivity;
                pitch = Mathf.Clamp(pitch, -15f, 60f);
            }

            // Calculate rotation and stick right behind Kael!
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPosition = target.position + rotation * offset;

            // Direct lock-follow
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
            transform.LookAt(target.position + Vector3.up * 1.2f);
        }
    }
}
