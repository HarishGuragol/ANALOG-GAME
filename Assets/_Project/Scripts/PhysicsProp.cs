using UnityEngine;

namespace Analog.Core
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsProp : MonoBehaviour
    {
        [SerializeField] private string propName = "Object";
        [SerializeField] private bool isGlass = false;
        [SerializeField] private bool isMetal = false;
        [SerializeField] private float impactNoiseRadius = 11f;

        private AudioSource audioSource;
        private AudioClip cachedImpactClip;
        private float lastImpactTime = -1f;

        public string PropName => propName;
        public bool IsGlass => isGlass;
        public bool IsMetal => isMetal;

        public void Configure(string name, bool glass, bool metal, float noiseRadius = 11f)
        {
            propName = name;
            isGlass = glass;
            isMetal = metal;
            impactNoiseRadius = noiseRadius;
        }

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.spatialBlend = 0.85f;
            audioSource.playOnAwake = false;
        }

        private void OnCollisionEnter(Collision col)
        {
            float impactSpeed = col.relativeVelocity.magnitude;
            if (impactSpeed <= 1.8f) return;
            if (Time.time - lastImpactTime < 0.25f) return;

            lastImpactTime = Time.time;

            if (cachedImpactClip == null)
            {
                cachedImpactClip = BuildImpactClip();
            }

            if (audioSource != null && cachedImpactClip != null)
            {
                float vol = Mathf.Clamp01((impactSpeed - 1.5f) / 8f) * 0.65f + 0.15f;
                audioSource.pitch = Random.Range(0.92f, 1.08f);
                audioSource.PlayOneShot(cachedImpactClip, vol);
            }

            if (impactSpeed > 2.5f)
            {
                NoiseSystem.EmitNoise(transform.position, impactNoiseRadius);
            }
        }

        private AudioClip BuildImpactClip()
        {
            int rate = 44100;
            int len = isGlass ? rate / 5 : (isMetal ? rate / 4 : rate / 7);
            float[] samples = new float[len];

            for (int i = 0; i < len; i++)
            {
                float t = (float)i / rate;
                float env = Mathf.Exp(-t * (isGlass ? 22f : (isMetal ? 14f : 28f)));

                float wave;
                if (isGlass)
                {
                    // Crisp high-frequency glass clink
                    wave = Mathf.Sin(2f * Mathf.PI * 2400f * t) * 0.6f
                         + Mathf.Sin(2f * Mathf.PI * 3650f * t) * 0.4f;
                }
                else if (isMetal)
                {
                    // Hollow metallic ring
                    wave = Mathf.Sin(2f * Mathf.PI * 520f * t) * 0.55f
                         + Mathf.Sin(2f * Mathf.PI * 1180f * t) * 0.35f;
                }
                else
                {
                    // Low woody crate thud
                    float noise = Mathf.Sin(i * 12.9898f) * 0.25f;
                    wave = Mathf.Sin(2f * Mathf.PI * 135f * t) * 0.75f + noise;
                }

                samples[i] = wave * env * 0.7f;
            }

            var clip = AudioClip.Create($"Impact_{propName}", len, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
