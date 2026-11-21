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

        private Material boxMaterial;

        private void Start()
        {
            // Auto-find door if not manually dragged into inspector
            if (linkedMagneticDoor == null)
            {
                linkedMagneticDoor = FindFirstObjectByType<Door>();
            }

            // Auto-find all directional & room lights
            if (linkedLights == null || linkedLights.Length == 0 || linkedLights[0] == null)
            {
                linkedLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            }

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

            // 1. Physically turn off ALL office lights and ceiling lamps!
            if (linkedLights != null)
            {
                foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
                {
                    // Don't turn off guard flashlight or player light
                    if (light != null && !light.name.Contains("Flashlight"))
                    {
                        light.enabled = powerIsOn;
                    }
                }
            }

            // 2. Kill skybox ambient light into pitch black noir
            if (!powerIsOn)
            {
                RenderSettings.ambientLight = new Color(0.01f, 0.01f, 0.02f);
                RenderSettings.ambientSkyColor = Color.black;
                RenderSettings.ambientEquatorColor = Color.black;
                RenderSettings.ambientGroundColor = Color.black;
                if (Camera.main != null)
                {
                    Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    Camera.main.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
                }
            }
            else
            {
                if (Camera.main != null)
                {
                    Camera.main.clearFlags = CameraClearFlags.Skybox;
                }
            }

            // 3. Turn box from glowing amber -> dead dark grey
            UpdateBoxVisual();

            // 4. UNLOCK the magnetic door!
            if (linkedMagneticDoor == null)
            {
                linkedMagneticDoor = FindFirstObjectByType<Door>();
            }

            if (linkedMagneticDoor != null)
            {
                if (!powerIsOn)
                {
                    linkedMagneticDoor.Unlock();
                    Debug.Log("[Breaker] Circuit cut! Magnetic coil lock severed. Door is UNLOCKED!");
                }
                else
                {
                    linkedMagneticDoor.Lock();
                    Debug.Log("[Breaker] Power restored. Door locked.");
                }
            }
        }

        private void UpdateBoxVisual()
        {
            if (boxMaterial != null)
            {
                boxMaterial.color = powerIsOn ? new Color(1.0f, 0.68f, 0.1f) : new Color(0.15f, 0.15f, 0.15f);
            }
        }
    }
}
