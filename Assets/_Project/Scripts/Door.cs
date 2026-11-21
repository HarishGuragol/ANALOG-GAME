using UnityEngine;

namespace Analog.Core
{
    // Hinged door: pivots on its left edge and always swings AWAY from the player.
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("Door State")]
        [SerializeField] private bool isOpen = false;
        [SerializeField] private bool isLocked = false;
        [SerializeField] private string lockedMessage = "Locked - Security Clearance Required";

        [Header("Movement Settings")]
        [SerializeField] private float openAngle = 90.0f;
        [SerializeField] private float smoothSpeed = 4.0f;

        private Vector3 closedPos;
        private Quaternion closedRot;
        private Vector3 hinge;
        private float currentAngle;
        private float targetAngle;
        private bool initialized;

        private void Init()
        {
            closedPos = transform.position;
            closedRot = transform.rotation;
            // Hinge sits on the door's left edge
            hinge = closedPos - closedRot * Vector3.right * (transform.lossyScale.x * 0.5f);
            isOpen = false;
            currentAngle = 0f;
            targetAngle = 0f;
            initialized = true;
        }

        private void Update()
        {
            if (!initialized) Init();

            currentAngle = Mathf.Lerp(currentAngle, targetAngle, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));

            Quaternion swing = Quaternion.Euler(0f, currentAngle, 0f);
            transform.position = hinge + swing * (closedPos - hinge);
            transform.rotation = swing * closedRot;
        }

        public string GetInteractionPrompt()
        {
            if (isLocked) return lockedMessage;
            return isOpen ? "Close Door" : "Open Door";
        }

        public void Interact(PlayerController player)
        {
            if (isLocked)
            {
                Debug.Log("[Door] Cannot open - Door is locked.");
                return;
            }

            if (!initialized) Init();

            isOpen = !isOpen;

            if (isOpen)
            {
                // Swing away from whichever side the player is standing on
                float side = 1f;
                if (player != null)
                {
                    Vector3 toPlayer = player.transform.position - closedPos;
                    side = Mathf.Sign(Vector3.Dot(toPlayer, closedRot * Vector3.forward));
                }
                targetAngle = side * openAngle;
            }
            else
            {
                targetAngle = 0f;
            }
        }

        public void Unlock() { isLocked = false; }
        public void Lock() { isLocked = true; }
    }
}
