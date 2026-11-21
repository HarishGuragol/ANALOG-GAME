using UnityEngine;

namespace Analog.Core
{
    public class SimpleRainSystem : MonoBehaviour
    {
        [Header("Rain Settings")]
        [SerializeField] private int maxParticles = 3000;
        [SerializeField] private float rainAreaSize = 25.0f;
        [SerializeField] private float rainHeight = 12.0f;
        [SerializeField] private float fallSpeed = 18.0f;

        private ParticleSystem rainParticleSystem;
        private Transform rainEmitterTransform;

        private void Start()
        {
            SetupRain();
        }

        private void SetupRain()
        {
            var pObj = new GameObject("RainEmitter");
            // Do NOT parent to LevelBuilder so the building never moves!
            rainEmitterTransform = pObj.transform;
            rainEmitterTransform.position = new Vector3(0, rainHeight, 0);
            rainEmitterTransform.rotation = Quaternion.Euler(90f, 0f, 0f);

            rainParticleSystem = pObj.AddComponent<ParticleSystem>();
            var main = rainParticleSystem.main;
            main.maxParticles = maxParticles;
            main.startSpeed = fallSpeed;
            main.startLifetime = 0.8f;
            main.startSize = 0.05f;
            main.startColor = new Color(0.7f, 0.8f, 0.9f, 0.45f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var shape = rainParticleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(rainAreaSize, rainAreaSize, 1.0f);

            var emission = rainParticleSystem.emission;
            emission.rateOverTime = 1200f;

            var renderer = pObj.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.08f;
            renderer.lengthScale = 1.2f;
        }

        private void LateUpdate()
        {
            // Only move the floating rain cloud above the player, NOT the building!
            if (rainEmitterTransform != null)
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    rainEmitterTransform.position = new Vector3(player.transform.position.x, rainHeight, player.transform.position.z);
                }
            }
        }
    }
}
