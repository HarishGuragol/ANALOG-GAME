using UnityEngine;
using UnityEngine.InputSystem;

namespace Analog.Core
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float sprintSpeed = 6.5f;
        [SerializeField] private float crouchSpeed = 1.8f;
        [SerializeField] private float rotationSmoothTime = 0.1f;
        [SerializeField] private float gravity = -15.0f;

        [Header("Visual Feedback")]
        [SerializeField] private Transform visualModel; // Scales the capsule visibly

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private Vector3 velocity;
        private float currentVelocity;
        private bool isCrouching = false;

        private float standingHeight = 2.0f;
        private float crouchingHeight = 1.0f;
        private Vector3 standingScale = Vector3.one;
        private Vector3 crouchingScale = new Vector3(1f, 0.5f, 1f);

        public bool IsGrounded => controller.isGrounded;
        public bool IsCrouching => isCrouching;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            standingHeight = controller.height;
            crouchingHeight = standingHeight * 0.5f;

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            HandleCrouch(keyboard);
            HandleMovement(keyboard);
        }

        private void HandleCrouch(Keyboard keyboard)
        {
            // Press C or Left Ctrl to TOGGLE crouch on and off!
            if (keyboard.cKey.wasPressedThisFrame || keyboard.leftCtrlKey.wasPressedThisFrame)
            {
                isCrouching = !isCrouching;

                // Adjust physical collider height
                controller.height = isCrouching ? crouchingHeight : standingHeight;
                controller.center = new Vector3(0, controller.height / 2f, 0);

                // Adjust visible shape so you see Kael physically duck and stand back up!
                transform.localScale = isCrouching ? crouchingScale : standingScale;
            }
        }

        private void HandleMovement(Keyboard keyboard)
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;

            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            float targetSpeed = walkSpeed;
            if (isCrouching)
            {
                targetSpeed = crouchSpeed;
            }
            else if (keyboard.leftShiftKey.isPressed)
            {
                targetSpeed = sprintSpeed;
            }

            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                if (cameraTransform != null)
                {
                    targetAngle += cameraTransform.eulerAngles.y;
                }

                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(moveDir.normalized * targetSpeed * Time.deltaTime);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
