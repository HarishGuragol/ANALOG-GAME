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
            // 1. LevelBuilder (Center anchor)
            var lb = FindFirstObjectByType<LevelBuilder>();
            if (lb != null && lb.transform != null)
            {
                lb.transform.position = Vector3.zero;
                lb.transform.rotation = Quaternion.identity;
                lb.transform.localScale = Vector3.one;
            }

            // 2. Door - Placed in the partition wall opening
            var door = GameObject.Find("Door");
            if (door != null)
            {
                door.transform.position = new Vector3(0.5f, 1.3f, 3.0f);
                door.transform.rotation = Quaternion.identity;
                door.transform.localScale = new Vector3(2.0f, 2.8f, 0.15f);
            }

            // 3. Breaker Box - Mounted flat on the wall near the door
            var breaker = GameObject.Find("BreakerBox");
            if (breaker != null)
            {
                breaker.transform.position = new Vector3(3.2f, 1.5f, 2.75f);
                breaker.transform.rotation = Quaternion.identity;
                breaker.transform.localScale = new Vector3(0.6f, 0.8f, 0.25f);
            }

            // 4. Terminal Desk - Placed right in the center of the server archive room so it's easy to reach!
            var terminal = GameObject.Find("TerminalDesk");
            if (terminal != null)
            {
                terminal.transform.position = new Vector3(0.5f, 0.6f, 10.0f);
                terminal.transform.rotation = Quaternion.identity;
                terminal.transform.localScale = new Vector3(1.6f, 1.1f, 0.9f);
            }

            // 5. Patrol Guard - Pacing between server racks
            var guard = GameObject.Find("Guard");
            if (guard != null)
            {
                guard.transform.position = new Vector3(2.5f, 1.0f, 7.5f);
                guard.transform.rotation = Quaternion.Euler(0, 180f, 0);
            }

            // 6. Ground Floor - Sized to fill room
            var ground = GameObject.Find("Ground");
            if (ground != null)
            {
                ground.transform.position = Vector3.zero;
                ground.transform.localScale = new Vector3(2.5f, 1.0f, 3.0f);
            }

            // 7. Auto-link Camera to Player if missing
            var cam = Camera.main;
            if (cam != null)
            {
                var tpc = cam.GetComponent<ThirdPersonCamera>();
                var player = GameObject.Find("Player");
                if (tpc != null && player != null)
                {
                    tpc.SetTarget(player.transform);
                }
            }
        }
    }
}
