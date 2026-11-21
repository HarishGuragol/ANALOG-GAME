using UnityEngine;

namespace Analog.Core
{
    [ExecuteAlways]
    public class MissionSetupCoordinator : MonoBehaviour
    {
        private void Awake()
        {
            EnforceLayout();
        }

        private void Start()
        {
            EnforceLayout();
        }

        public void EnforceLayout()
        {
            // 1. LevelBuilder
            var lb = FindFirstObjectByType<LevelBuilder>();
            if (lb != null && lb.transform != null)
            {
                lb.transform.position = Vector3.zero;
                lb.transform.rotation = Quaternion.identity;
                lb.transform.localScale = Vector3.one;
            }

            // 2. Door
            var door = GameObject.Find("Door");
            if (door != null)
            {
                door.transform.position = new Vector3(0.5f, 1.3f, 3.0f);
                door.transform.rotation = Quaternion.identity;
                door.transform.localScale = new Vector3(2.0f, 2.8f, 0.15f);
            }

            // 3. Breaker Box
            var breaker = GameObject.Find("BreakerBox");
            if (breaker != null)
            {
                breaker.transform.position = new Vector3(3.2f, 1.5f, 2.75f);
                breaker.transform.rotation = Quaternion.identity;
                breaker.transform.localScale = new Vector3(0.6f, 0.8f, 0.25f);
            }

            // 4. Terminal Desk - In server room
            var terminal = GameObject.Find("TerminalDesk");
            if (terminal != null)
            {
                terminal.transform.position = new Vector3(0.5f, 0.6f, 10.0f);
                terminal.transform.rotation = Quaternion.identity;
                terminal.transform.localScale = new Vector3(1.6f, 1.1f, 0.9f);
            }

            // 5. Patrol Guard
            var guard = GameObject.Find("Guard");
            if (guard != null)
            {
                guard.transform.position = new Vector3(2.5f, 1.0f, 7.5f);
                guard.transform.rotation = Quaternion.Euler(0, 180f, 0);
            }

            // 6. Player Kael
            var player = GameObject.Find("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(0.5f, 1.0f, -4.0f);
                player.transform.rotation = Quaternion.identity;
            }

            // 7. Ground
            var ground = GameObject.Find("Ground");
            if (ground != null)
            {
                ground.transform.position = Vector3.zero;
                ground.transform.localScale = new Vector3(2.5f, 1.0f, 3.0f);
            }

            // 8. Attach SimpleCameraFollow to Main Camera
            var cam = Camera.main;
            if (cam != null)
            {
                var oldCam = cam.GetComponent<ThirdPersonCamera>();
                if (oldCam != null) DestroyImmediate(oldCam);

                var newCam = cam.GetComponent<SimpleCameraFollow>();
                if (newCam == null)
                {
                    cam.gameObject.AddComponent<SimpleCameraFollow>();
                }
            }
        }
    }
}
