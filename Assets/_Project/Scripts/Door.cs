using UnityEngine;

namespace Analog.Core
{
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("Door State")]
        [SerializeField] private bool isOpen = false;
        [SerializeField] private bool isLocked = false;
        [SerializeField] private string lockedMessage = "Locked - Security Clearance Required";

        [Header("Movement Settings")]
        [SerializeField] private float openAngle = 90.0f;
        [SerializeField] private float smoothSpeed = 4.0f;

        private Quaternion closedRotation;
        private Quaternion openRotation;

        private void Start()
        {
            closedRotation = transform.localRotation;
            openRotation = Quaternion.Euler(transform.localEulerAngles + new Vector3(0, openAngle, 0));
        }

        private void Update()
        {
            Quaternion target = isOpen ? openRotation : closedRotation;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * smoothSpeed);
        }

        public string GetInteractionPrompt()
        {
            if (isLocked)
            {
                return lockedMessage;
            }
            return isOpen ? "Close Door" : "Open Door";
        }

        public void Interact(PlayerController player)
        {
            if (isLocked)
            {
                Debug.Log("[Door] Cannot open - Door is locked.");
                return;
            }

            isOpen = !isOpen;
        }

        public void Unlock()
        {
            isLocked = false;
        }

        public void Lock()
        {
            isLocked = true;
        }
    }
}
