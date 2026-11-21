using UnityEngine;
using UnityEngine.InputSystem;

namespace Analog.Core
{
    public class InteractionDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float reachDistance = 2.5f;
        [SerializeField] private LayerMask interactableLayer = ~0; // Everything by default
        [SerializeField] private Transform raycastOrigin;

        private IInteractable currentInteractable;
        private PlayerController player;

        public IInteractable CurrentInteractable => currentInteractable;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            if (raycastOrigin == null)
            {
                raycastOrigin = Camera.main != null ? Camera.main.transform : transform;
            }
        }

        private void Update()
        {
            DetectInteractable();
            HandleInput();
        }

        private void DetectInteractable()
        {
            if (raycastOrigin == null && Camera.main != null)
            {
                raycastOrigin = Camera.main.transform;
            }

            Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, reachDistance, interactableLayer))
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null && !string.IsNullOrEmpty(interactable.GetInteractionPrompt()))
                {
                    currentInteractable = interactable;
                    return;
                }
            }

            // Also check a small spherecast around player front if camera ray missed
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1.0f + Vector3.up * 0.8f, 1.2f, interactableLayer);
            foreach (var col in hits)
            {
                IInteractable interactable = col.GetComponentInParent<IInteractable>();
                if (interactable != null && !string.IsNullOrEmpty(interactable.GetInteractionPrompt()))
                {
                    currentInteractable = interactable;
                    return;
                }
            }

            currentInteractable = null;
        }

        private void HandleInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || currentInteractable == null) return;

            // Press E to interact!
            if (keyboard.eKey.wasPressedThisFrame)
            {
                currentInteractable.Interact(player);
            }
        }

        private void OnGUI()
        {
            // Simple, beautiful HUD prompt when looking at an interactable object
            if (currentInteractable != null)
            {
                GUIStyle style = new GUIStyle();
                style.fontSize = 22;
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = new Color(1f, 0.75f, 0.2f, 1f); // Warm amber cyber-noir color!
                style.alignment = TextAnchor.MiddleCenter;

                float width = 400;
                float height = 50;
                float x = (Screen.width - width) / 2f;
                float y = Screen.height - 120f;

                GUI.Label(new Rect(x, y, width, height), $"[E] {currentInteractable.GetInteractionPrompt()}", style);
            }
        }
    }
}
