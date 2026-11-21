using UnityEngine;

namespace Analog.Core
{
    public class SimpleRainSystem : MonoBehaviour
    {
        [Header("Rain Settings")]
        [SerializeField] private bool enableRain = false;
        [SerializeField] private int maxParticles = 2000;
        [SerializeField] private float rainAreaSize = 25.0f;
        [SerializeField] private float rainHeight = 10.0f;
        [SerializeField] private float fallSpeed = 16.0f;

        private ParticleSystem rainParticleSystem;
        private Transform rainEmitterTransform;

        private void Start()
        {
            var existing = GameObject.Find("RainEmitter");
            if (existing != null) Destroy(existing);

            if (!enableRain) return;
            SetupRain();
        }

        private void SetupRain()
        {
            var pObj = new GameObject("RainEmitter");
            rainEmitterTransform = pObj.transform;
            rainEmitterTransform.position = new Vector3(0, rainHeight, 0);
            rainEmitterTransform.rotation = Quaternion.Euler(90f, 0f, 0f);

            rainParticleSystem = pObj.AddComponent<ParticleSystem>();
            var main = rainParticleSystem.main;
            main.maxParticles = maxParticles;
            main.startSpeed = fallSpeed;
            main.startLifetime = 0.7f;
            main.startSize = 0.04f;
            main.startColor = new Color(0.7f, 0.85f, 1f, 0.5f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var shape = rainParticleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(rainAreaSize, rainAreaSize, 1.0f);

            var emission = rainParticleSystem.emission;
            emission.rateOverTime = 800f;

            var renderer = pObj.GetComponent<ParticleSystemRenderer>();
            // Assign default particle material so it is never pink/missing!
            Material rainMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            rainMat.color = new Color(0.7f, 0.85f, 1.0f, 0.4f);
            renderer.material = rainMat;
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.05f;
            renderer.lengthScale = 1.0f;
            renderer.minParticleSize = 0f;
            renderer.maxParticleSize = 0.02f; // stops raindrops near the camera turning into fat bars
        }

        private void LateUpdate()
        {
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
