using System;
using UnityEngine;

namespace Analog.Core
{
    // Central event bus for sound-based stealth: footsteps, thrown bottles, and fallen crates.
    public static class NoiseSystem
    {
        public static event Action<Vector3, float> OnNoiseEmitted;

        public static void EmitNoise(Vector3 position, float radius)
        {
            OnNoiseEmitted?.Invoke(position, radius);
        }
    }
}
