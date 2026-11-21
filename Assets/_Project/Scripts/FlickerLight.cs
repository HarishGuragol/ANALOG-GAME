using UnityEngine;

namespace Analog.Core
{
    // Subtle fluorescent-tube flicker for a worn, noir feel.
    [RequireComponent(typeof(Light))]
    public class FlickerLight : MonoBehaviour
    {
        [SerializeField] private float flickerAmount = 0.3f;
        [SerializeField] private float speed = 9f;

        private Light lamp;
        private float baseIntensity;
        private float seed;

        private void Awake()
        {
            lamp = GetComponent<Light>();
            baseIntensity = lamp.intensity;
            seed = Random.value * 100f;
        }

        private void Update()
        {
            if (lamp == null || !lamp.enabled) return;

            float noise = Mathf.PerlinNoise(Time.time * speed, seed);
            float dropout = Random.value < 0.004f ? 0.2f : 1f; // rare brief stutter
            lamp.intensity = baseIntensity * Mathf.Lerp(1f - flickerAmount, 1f, noise) * dropout;
        }
    }
}
