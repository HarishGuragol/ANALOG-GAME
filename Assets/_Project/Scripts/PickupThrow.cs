using UnityEngine;
using UnityEngine.InputSystem;

namespace Analog.Core
{
    // Lets Kael pick up loose PhysicsProps ([F]), carry them physically in front of him,
    // drop them ([F]), or throw them ([G] or Right-Click) to create distractions.
    public class PickupThrow : MonoBehaviour
    {
        [SerializeField] private float detectRange = 2.4f;
        [SerializeField] private float holdDistance = 1.3f;
        [SerializeField] private float throwImpulse = 11f;

        private CharacterController playerController;
        private PhysicsProp candidateProp;
        private PhysicsProp heldProp;
        private Rigidbody heldRb;
        private Collider heldCollider;

        private void Awake()
        {
            playerController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (heldProp == null)
            {
                FindCandidateProp();
            }

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (keyboard != null && keyboard.fKey.wasPressedThisFrame)
            {
                if (heldProp != null) DropHeld();
                else if (candidateProp != null) PickUp(candidateProp);
            }

            if (heldProp != null &&
                ((keyboard != null && keyboard.gKey.wasPressedThisFrame) ||
                 (mouse != null && mouse.rightButton.wasPressedThisFrame)))
            {
                ThrowHeld();
            }
        }

        private void FixedUpdate()
        {
            if (heldProp == null || heldRb == null) return;

            Transform cam = Camera.main != null ? Camera.main.transform : transform;
            Vector3 forwardFlat = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            if (forwardFlat.sqrMagnitude < 0.01f) forwardFlat = transform.forward;

            Vector3 targetPos = transform.position + forwardFlat * holdDistance + Vector3.up * 0.35f;
            Vector3 toTarget = targetPos - heldRb.position;

            // Velocity steering keeps held props physical so they push against walls instead of clipping through
            heldRb.linearVelocity = Vector3.ClampMagnitude(toTarget * 16f, 12f);
            heldRb.angularVelocity *= 0.85f;
        }

        private void FindCandidateProp()
        {
            candidateProp = null;

            Transform cam = Camera.main != null ? Camera.main.transform : transform;
            if (Physics.Raycast(cam.position, cam.forward, out RaycastHit rayHit, detectRange + 3f))
            {
                var p = rayHit.collider.GetComponentInParent<PhysicsProp>();
                if (p != null && Vector3.Distance(transform.position, p.transform.position) <= detectRange)
                {
                    candidateProp = p;
                    return;
                }
            }

            Collider[] nearby = Physics.OverlapSphere(transform.position + transform.forward * 0.9f, detectRange);
            float bestDist = float.MaxValue;
            foreach (var col in nearby)
            {
                var p = col.GetComponentInParent<PhysicsProp>();
                if (p == null) continue;
                float d = Vector3.Distance(transform.position, p.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    candidateProp = p;
                }
            }
        }

        private void PickUp(PhysicsProp prop)
        {
            heldProp = prop;
            heldRb = prop.GetComponent<Rigidbody>();
            heldCollider = prop.GetComponent<Collider>();
            candidateProp = null;

            if (heldRb != null)
            {
                heldRb.useGravity = false;
                heldRb.linearDamping = 10f;
            }

            if (heldCollider != null && playerController != null)
            {
                Physics.IgnoreCollision(heldCollider, playerController, true);
            }
        }

        private void ReleaseCommon()
        {
            if (heldRb != null)
            {
                heldRb.useGravity = true;
                heldRb.linearDamping = 0.05f;
            }

            if (heldCollider != null && playerController != null)
            {
                Physics.IgnoreCollision(heldCollider, playerController, false);
            }
        }

        private void DropHeld()
        {
            ReleaseCommon();
            heldProp = null;
            heldRb = null;
            heldCollider = null;
        }

        private void ThrowHeld()
        {
            Rigidbody rb = heldRb;
            PhysicsProp prop = heldProp;
            ReleaseCommon();

            if (rb != null)
            {
                Transform cam = Camera.main != null ? Camera.main.transform : transform;
                Vector3 dir = (cam.forward + Vector3.up * 0.25f).normalized;

                // Bottles sail crisply; heavier crates need slightly less arc speed
                float scale = prop != null && prop.IsGlass ? 1.0f : (rb.mass > 2f ? 0.75f : 0.9f);
                rb.AddForce(dir * throwImpulse * scale * rb.mass, ForceMode.Impulse);
            }

            heldProp = null;
            heldRb = null;
            heldCollider = null;
        }

        private void OnGUI()
        {
            string prompt = null;
            if (heldProp != null)
            {
                prompt = "[F] Drop   [G / Right-Click] Throw";
            }
            else if (candidateProp != null)
            {
                prompt = $"[F] Pick up {candidateProp.PropName}";
            }

            if (string.IsNullOrEmpty(prompt)) return;

            GUIStyle style = new GUIStyle
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            style.normal.textColor = new Color(0.85f, 0.9f, 0.95f, 0.95f);

            float width = 460f;
            float x = (Screen.width - width) / 2f;
            float y = Screen.height - 155f;
            GUI.Label(new Rect(x, y, width, 35f), prompt, style);
        }
    }
}
