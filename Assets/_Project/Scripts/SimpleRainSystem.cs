using UnityEngine;

namespace Analog.Core
{
    // Keeps rain strictly outdoors over Meridian Street so the inside of the building stays dry!
    public class SimpleRainSystem : MonoBehaviour
    {
        [Header("Rain Settings")]
        [SerializeField] private bool enableOutdoorRain = true;
        [SerializeField] private int maxParticles = 3000;
        [SerializeField] private Vector3 streetCenter = new Vector3(0f, 14f, -22f);
        [SerializeField] private Vector2 streetArea = new Vector2(46f, 19f);
        [SerializeField] private float fallSpeed = 16.0f;

        private ParticleSystem rainParticleSystem;

        private void Start()
        {
            var existing = GameObject.Find("RainEmitter");
            if (existing != null) Destroy(existing);

            if (!enableOutdoorRain) return;
            SetupRain();
        }

        private void SetupRain()
        {
            var pObj = new GameObject("RainEmitter");
            pObj.transform.position = streetCenter;
            pObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            rainParticleSystem = pObj.AddComponent<ParticleSystem>();
            var main = rainParticleSystem.main;
            main.maxParticles = maxParticles;
            main.startSpeed = fallSpeed;
            main.startLifetime = 0.9f;
            main.startSize = 0.04f;
            main.startColor = new Color(0.7f, 0.85f, 1f, 0.45f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var shape = rainParticleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(streetArea.x, streetArea.y, 1.0f);

            var emission = rainParticleSystem.emission;
            emission.rateOverTime = 1100f;

            var renderer = pObj.GetComponent<ParticleSystemRenderer>();
            Material rainMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            rainMat.color = new Color(0.7f, 0.85f, 1.0f, 0.38f);
            renderer.material = rainMat;
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.05f;
            renderer.lengthScale = 1.0f;
            renderer.minParticleSize = 0f;
            renderer.maxParticleSize = 0.02f;
        }
    }
}
