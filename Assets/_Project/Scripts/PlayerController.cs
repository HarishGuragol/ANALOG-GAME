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
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float deceleration = 22f;
        [SerializeField] private float turnSpeed = 14f;
        [SerializeField] private float gravity = -15.0f;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private bool isCrouching = false;

        private float standingHeight = 2.0f;
        private float crouchingHeight = 1.0f;
        private Vector3 standingScale = Vector3.one;
        private Vector3 crouchingScale = new Vector3(1f, 0.5f, 1f);

        public bool IsGrounded => controller != null && controller.isGrounded;
        public bool IsCrouching => isCrouching;
        public float CurrentSpeed => horizontalVelocity.magnitude;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            standingHeight = controller.height;
            crouchingHeight = standingHeight * 0.5f;
            standingScale = transform.localScale;
            crouchingScale = new Vector3(standingScale.x, standingScale.y * 0.5f, standingScale.z);

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            if (transform.position.y < 0.5f)
            {
                transform.position = new Vector3(transform.position.x, 1.0f, transform.position.z);
            }

            BuildFacingMarkers();
        }

        // A capsule looks the same from every angle, so add a visor (front) and satchel (back)
        // so you can actually SEE Kael turn.
        private void BuildFacingMarkers()
        {
            if (transform.Find("KaelVisor") != null) return;

            var visorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            visorMat.color = new Color(1f, 0.7f, 0.15f);
            visorMat.EnableKeyword("_EMISSION");
            visorMat.SetColor("_EmissionColor", new Color(1f, 0.6f, 0.1f) * 1.5f);

            var visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "KaelVisor";
            visor.transform.SetParent(transform, false);
            visor.transform.localPosition = new Vector3(0f, 0.55f, 0.42f);
            visor.transform.localScale = new Vector3(0.55f, 0.14f, 0.2f);
            Destroy(visor.GetComponent<Collider>());
            visor.GetComponent<MeshRenderer>().material = visorMat;

            var satchelMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            satchelMat.color = new Color(0.25f, 0.18f, 0.12f);

            var satchel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            satchel.name = "KaelSatchel";
            satchel.transform.SetParent(transform, false);
            satchel.transform.localPosition = new Vector3(0.35f, 0.1f, -0.42f);
            satchel.transform.localScale = new Vector3(0.35f, 0.4f, 0.2f);
            Destroy(satchel.GetComponent<Collider>());
            satchel.GetComponent<MeshRenderer>().material = satchelMat;
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
            if (keyboard.cKey.wasPressedThisFrame || keyboard.leftCtrlKey.wasPressedThisFrame)
            {
                isCrouching = !isCrouching;
                controller.height = isCrouching ? crouchingHeight : standingHeight;
                controller.center = new Vector3(0, controller.height / 2f, 0);
            }

            // Smooth duck / stand instead of snapping
            Vector3 targetScale = isCrouching ? crouchingScale : standingScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 12f);
        }

        private void HandleMovement(Keyboard keyboard)
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;

            Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;

            float targetSpeed = walkSpeed;
            if (isCrouching) targetSpeed = crouchSpeed;
            else if (keyboard.leftShiftKey.isPressed) targetSpeed = sprintSpeed;

            // Work out the world-space direction relative to the camera
            Vector3 desiredVelocity = Vector3.zero;
            if (input.magnitude >= 0.1f)
            {
                float cameraYaw = cameraTransform != null ? cameraTransform.eulerAngles.y : 0f;
                Vector3 moveDir = Quaternion.Euler(0f, cameraYaw, 0f) * input;
                desiredVelocity = moveDir * targetSpeed;

                // Kael turns to face where he is going (smoothly)
                Quaternion lookRot = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
            }

            // Momentum: speed up and slow down gradually instead of instantly
            float rate = desiredVelocity.sqrMagnitude > 0.01f ? acceleration : deceleration;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desiredVelocity, rate * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 motion = horizontalVelocity + Vector3.up * verticalVelocity;
            controller.Move(motion * Time.deltaTime);
        }

        // Lets Kael shove loose physics objects (crates, bottles) as he walks into them
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            if (body == null || body.isKinematic) return;
            if (hit.moveDirection.y < -0.3f) return;

            Vector3 push = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
            body.AddForce(push * 4f, ForceMode.Impulse);
        }
    }
}
