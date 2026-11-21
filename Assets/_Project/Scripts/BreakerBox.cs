using UnityEngine;

namespace Analog.Core
{
    public class BreakerBox : MonoBehaviour, IInteractable
    {
        [Header("Breaker State")]
        [SerializeField] private bool powerIsOn = true;

        [Header("Target Systems")]
        [SerializeField] private Light[] linkedLights;
        [SerializeField] private Door linkedMagneticDoor;

        [Header("Visual Indicator (Amber LED on Box)")]
        private Material boxMaterial;

        private void Start()
        {
            // Auto-find Directional Light if not linked manually
            if (linkedLights == null || linkedLights.Length == 0 || linkedLights[0] == null)
            {
                var sun = FindFirstObjectByType<Light>();
                if (sun != null)
                {
                    linkedLights = new Light[] { sun };
                }
            }

            // Create a custom material on the box so it visibly turns from glowing amber to dead grey
            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                boxMaterial = renderer.material;
                UpdateBoxVisual();
            }
        }

        public string GetInteractionPrompt()
        {
            return powerIsOn ? "Cut Circuit Power" : "Restore Circuit Power";
        }

        public void Interact(PlayerController player)
        {
            powerIsOn = !powerIsOn;

            // 1. Physically turn off the sun/lights
            if (linkedLights != null)
            {
                foreach (var light in linkedLights)
                {
                    if (light != null)
                    {
                        light.enabled = powerIsOn;
                    }
                }
            }

            // 2. Dim camera exposure to make the whole screen pitch dark
            if (Camera.main != null)
            {
                Camera.main.backgroundColor = powerIsOn ? new Color(0.19f, 0.3f, 0.47f) : Color.black;
                Camera.main.clearFlags = powerIsOn ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
            }

            // 3. Change box visual
            UpdateBoxVisual();

            // 4. Unlock or lock the door
            if (linkedMagneticDoor != null)
            {
                if (!powerIsOn)
                {
                    linkedMagneticDoor.Unlock();
                    Debug.Log("[Breaker] Power severed. Magnetic lock disabled — Door is now accessible!");
                }
                else
                {
                    linkedMagneticDoor.Lock();
                    Debug.Log("[Breaker] Power restored. Magnetic lock re-engaged.");
                }
            }
        }

        private void UpdateBoxVisual()
        {
            if (boxMaterial != null)
            {
                boxMaterial.color = powerIsOn ? new Color(1.0f, 0.65f, 0.1f) : new Color(0.15f, 0.15f, 0.15f);
            }
        }
    }
}
