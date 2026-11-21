using UnityEngine;

namespace Analog.Core
{
    [ExecuteAlways]
    public class MissionSetupCoordinator : MonoBehaviour
    {
        private void Awake()
        {
            CleanComponents();
        }

        private void Start()
        {
            CleanComponents();
        }

        public void CleanComponents()
        {
            // Fix BreakerBox: REMOVE Door script if accidentally attached!
            var breaker = GameObject.Find("BreakerBox");
            if (breaker != null)
            {
                var accidentalDoorScript = breaker.GetComponent<Door>();
                if (accidentalDoorScript != null)
                {
                    if (Application.isPlaying) Destroy(accidentalDoorScript);
                    else DestroyImmediate(accidentalDoorScript);
                    Debug.Log("[Coordinator] Removed accidental Door script from BreakerBox!");
                }

                // Ensure it has BreakerBox script
                var bb = breaker.GetComponent<BreakerBox>();
                if (bb == null)
                {
                    breaker.AddComponent<BreakerBox>();
                }
            }

            // Fix Door: ensure it only has Door script
            var door = GameObject.Find("Door");
            if (door != null)
            {
                var accidentalBreaker = door.GetComponent<BreakerBox>();
                if (accidentalBreaker != null)
                {
                    if (Application.isPlaying) Destroy(accidentalBreaker);
                    else DestroyImmediate(accidentalBreaker);
                }

                var doorScript = door.GetComponent<Door>();
                if (doorScript == null)
                {
                    door.AddComponent<Door>();
                }
            }
        }
    }
}
