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

        private void Update()
        {
            // In editor mode (when not playing), continuously guarantee perfect alignment!
            if (!Application.isPlaying)
            {
                EnforceLayout();
            }
        }

        public void EnforceLayout()
        {
            // 1. LevelBuilder (Center anchor)
            var lb = FindFirstObjectByType<LevelBuilder>();
            if (lb != null && lb.transform != null)
            {
                lb.transform.position = Vector3.zero;
                lb.transform.rotation = Quaternion.identity;
                lb.transform.localScale = Vector3.one;
            }

            // 2. Door - Snapped flush into the middle partition doorway!
            var door = GameObject.Find("Door");
            if (door != null)
            {
                door.transform.position = new Vector3(0.5f, 1.3f, 3.0f);
                door.transform.rotation = Quaternion.identity;
                door.transform.localScale = new Vector3(2.0f, 2.8f, 0.15f);
            }

            // 3. Breaker Box - Mounted flat on the right wall near the door
            var breaker = GameObject.Find("BreakerBox");
            if (breaker != null)
            {
                breaker.transform.position = new Vector3(3.2f, 1.5f, 2.75f);
                breaker.transform.rotation = Quaternion.identity;
                breaker.transform.localScale = new Vector3(0.6f, 0.8f, 0.25f);
            }

            // 4. Terminal Desk - Placed deep inside the server archive room
            var terminal = GameObject.Find("TerminalDesk");
            if (terminal != null)
            {
                terminal.transform.position = new Vector3(-5.5f, 0.6f, 8.0f);
                terminal.transform.rotation = Quaternion.identity;
                terminal.transform.localScale = new Vector3(1.6f, 1.1f, 0.9f);
            }

            // 5. Patrol Guard - Between the server racks
            var guard = GameObject.Find("Guard");
            if (guard != null)
            {
                guard.transform.position = new Vector3(0.5f, 1.0f, 6.5f);
                guard.transform.rotation = Quaternion.Euler(0, 180f, 0);
            }

            // 6. Player Kael - In the front entrance lobby
            var player = GameObject.Find("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(0.5f, 1.0f, -4.0f);
                player.transform.rotation = Quaternion.identity;
            }

            // 7. Ground Floor - Sized to fill the room
            var ground = GameObject.Find("Ground");
            if (ground != null)
            {
                ground.transform.position = Vector3.zero;
                ground.transform.localScale = new Vector3(2.5f, 1.0f, 3.0f);
            }
        }
    }
}
