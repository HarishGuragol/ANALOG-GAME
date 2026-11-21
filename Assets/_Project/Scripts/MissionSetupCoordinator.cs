using UnityEngine;

namespace Analog.Core
{
    [ExecuteAlways]
    public class MissionSetupCoordinator : MonoBehaviour
    {
        [Header("Quick Align Button")]
        [SerializeField] private bool snapNow = false;

        private void OnValidate()
        {
            if (snapNow)
            {
                snapNow = false;
                AlignMissionLayout();
            }
        }

        [ContextMenu("Align All Mission 1 Objects")]
        public void AlignMissionLayout()
        {
            // 1. LevelBuilder (Center anchor)
            var lb = FindFirstObjectByType<LevelBuilder>();
            if (lb != null)
            {
                lb.transform.position = Vector3.zero;
                lb.transform.rotation = Quaternion.identity;
                lb.transform.localScale = Vector3.one;
            }

            // 2. Door - Snapped exactly into the middle of the wall gap!
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

            // 4. Terminal Desk - Shifted deeper into the server vault room
            var terminal = GameObject.Find("TerminalDesk");
            if (terminal != null)
            {
                terminal.transform.position = new Vector3(-6.0f, 0.6f, 8.5f);
                terminal.transform.rotation = Quaternion.identity;
                terminal.transform.localScale = new Vector3(1.6f, 1.1f, 0.9f);
            }

            // 5. Patrol Guard - Placed between the server racks
            var guard = GameObject.Find("Guard");
            if (guard != null)
            {
                guard.transform.position = new Vector3(0.8f, 1.0f, 6.5f);
                guard.transform.rotation = Quaternion.Euler(0, 180f, 0);
            }

            // 6. Player Kael - Placed in the front lobby room
            var player = GameObject.Find("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(0.5f, 1.0f, -4.0f);
                player.transform.rotation = Quaternion.identity;
            }

            // 7. Ground Plane - Stretched to fill the entire concrete building
            var ground = GameObject.Find("Ground");
            if (ground != null)
            {
                ground.transform.position = Vector3.zero;
                ground.transform.localScale = new Vector3(2.5f, 1.0f, 3.0f);
            }

            Debug.Log("[MissionSetup] Layout perfectly aligned to the doorway and rooms!");
        }

        private void Start()
        {
            AlignMissionLayout();
        }
    }
}
