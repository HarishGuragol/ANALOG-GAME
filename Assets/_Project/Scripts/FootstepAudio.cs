using UnityEngine;
using UnityEngine.InputSystem;

namespace Analog.Core
{
    [RequireComponent(typeof(PlayerController))]
    public class FootstepAudio : MonoBehaviour
    {
        private PlayerController player;
        private AudioSource audioSource;
        private AudioClip stepClip;
        private float distanceAccumulator = 0f;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        private void Start()
        {
            stepClip = BuildFootstepClip();
        }

        private void Update()
        {
            if (player == null || !player.IsGrounded) return;

            float speed = player.CurrentSpeed;
            if (speed <= 0.4f)
            {
                distanceAccumulator = 0f;
                return;
            }

            bool isCrouching = player.IsCrouching;
            bool isSprinting = !isCrouching && Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed && speed > 4.2f;

            float stepInterval = isCrouching ? 1.1f : (isSprinting ? 2.0f : 1.5f);
            distanceAccumulator += speed * Time.deltaTime;

            if (distanceAccumulator >= stepInterval)
            {
                distanceAccumulator -= stepInterval;

                float volume = isCrouching ? 0.08f : (isSprinting ? 0.42f : 0.22f);
                float basePitch = isCrouching ? 0.85f : (isSprinting ? 1.12f : 1.0f);

                if (audioSource != null && stepClip != null)
                {
                    audioSource.pitch = basePitch + Random.Range(-0.06f, 0.06f);
                    audioSource.PlayOneShot(stepClip, volume);
                }

                if (isSprinting)
                {
                    NoiseSystem.EmitNoise(transform.position, 6.5f);
                }
            }
        }

        private static AudioClip BuildFootstepClip()
        {
            int rate = 44100;
            int len = rate / 14; // ~70ms short shoe-on-concrete thud
            float[] samples = new float[len];

            for (int i = 0; i < len; i++)
            {
                float t = (float)i / rate;
                float env = Mathf.Exp(-t * 48f);
                float thud = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.7f;
                float grit = Mathf.Sin(i * 78.233f) * 0.3f;
                samples[i] = (thud + grit) * env * 0.65f;
            }

            var clip = AudioClip.Create("Footstep", len, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
