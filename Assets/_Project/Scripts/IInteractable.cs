using UnityEngine;

namespace Analog.Core
{
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void Interact(PlayerController player);
    }
}
