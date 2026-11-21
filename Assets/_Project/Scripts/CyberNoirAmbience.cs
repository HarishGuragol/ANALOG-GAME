using UnityEngine;

namespace Analog.Core
{
    public class CyberNoirAmbience : MonoBehaviour
    {
        private AudioSource rainAudioSource;

        private void Start()
        {
            SetupSynthesizedRainSound();
        }

        private void SetupSynthesizedRainSound()
        {
            rainAudioSource = gameObject.AddComponent<AudioSource>();
            rainAudioSource.loop = true;
            rainAudioSource.volume = 0.25f;

            // Generate procedural soft rain white noise so we have real audio without external files!
            int sampleRate = 44100;
            int length = sampleRate * 2; // 2 second seamless loop
            float[] samples = new float[length];
            System.Random rand = new System.Random();

            float lastSample = 0f;
            for (int i = 0; i < length; i++)
            {
                // Low-pass filtered pink/brown noise mimics gentle continuous rainfall
                float white = (float)(rand.NextDouble() * 2.0 - 1.0);
                lastSample = (lastSample + (0.04f * white)) / 1.04f;
                samples[i] = lastSample * 0.4f;
            }

            AudioClip rainClip = AudioClip.Create("ProceduralRain", length, 1, sampleRate, false);
            rainClip.SetData(samples, 0);

            rainAudioSource.clip = rainClip;
            rainAudioSource.Play();
        }
    }
}
