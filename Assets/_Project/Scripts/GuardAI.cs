using UnityEngine;

namespace Analog.Core
{
    public class GuardAI : MonoBehaviour
    {
        [Header("Vision Settings")]
        [SerializeField] private float viewDistance = 10.0f;
        [SerializeField] private float viewAngle = 75.0f;

        [Header("Vision Cone Visual")]
        [SerializeField] private Light flashlight;

        private Transform player;
        private PlayerController playerController;
        private bool hasSpottedPlayer = false;

        private void Start()
        {
            FindPlayer();
            SetupFlashlight();
        }

        private void FindPlayer()
        {
            var pObj = GameObject.FindWithTag("Player");
            if (pObj == null)
            {
                pObj = FindFirstObjectByType<PlayerController>()?.gameObject;
            }

            if (pObj != null)
            {
                player = pObj.transform;
                playerController = pObj.GetComponent<PlayerController>();
            }
        }

        private void SetupFlashlight()
        {
            if (flashlight == null)
            {
                flashlight = GetComponentInChildren<Light>();
                if (flashlight == null)
                {
                    var lightObj = new GameObject("FlashlightCone");
                    lightObj.transform.SetParent(transform);
                    flashlight = lightObj.AddComponent<Light>();
                }
            }

            flashlight.transform.localPosition = new Vector3(0, 0.5f, 0.3f);
            flashlight.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);

            flashlight.type = LightType.Spot;
            flashlight.spotAngle = viewAngle;
            flashlight.range = viewDistance;
            flashlight.intensity = 35.0f;
            flashlight.color = Color.white;
        }

        private void Update()
        {
            if (player == null)
            {
                FindPlayer();
                if (player == null) return;
            }

            // Swivel guard smoothly
            float swivelAngle = Mathf.PingPong(Time.time * 30f, 80f) - 40f;
            transform.rotation = Quaternion.Euler(0, 180f + swivelAngle, 0);

            CheckDetection();
        }

        private void CheckDetection()
        {
            Vector3 guardEyePos = transform.position + Vector3.up * 0.8f;
            Vector3 playerTargetPos = player.position + Vector3.up * 0.8f;
            Vector3 dirToPlayer = playerTargetPos - guardEyePos;
            float dist = dirToPlayer.magnitude;

            // Detection range (reduced if crouching!)
            float maxDetectDist = playerController != null && playerController.IsCrouching ? viewDistance * 0.5f : viewDistance;

            if (dist <= maxDetectDist)
            {
                // Check if player is inside the forward vision cone angle
                float angle = Vector3.Angle(transform.forward, dirToPlayer);
                if (angle <= viewAngle / 2f)
                {
                    // Raycast: make sure guard's own collider doesn't block the ray!
                    Ray ray = new Ray(guardEyePos + transform.forward * 0.6f, dirToPlayer.normalized);
                    if (Physics.Raycast(ray, out RaycastHit hit, dist))
                    {
                        // Did we hit the player or something on the player?
                        if (hit.transform == player || hit.transform.IsChildOf(player))
                        {
                            TriggerAlert(true);
                            return;
                        }
                    }
                    else
                    {
                        // Clean line of sight
                        TriggerAlert(true);
                        return;
                    }
                }
            }

            TriggerAlert(false);
        }

        private void TriggerAlert(bool spotted)
        {
            if (spotted)
            {
                hasSpottedPlayer = true;
                if (flashlight != null)
                {
                    flashlight.color = Color.red;
                    flashlight.intensity = 50.0f;
                }
            }
            else
            {
                hasSpottedPlayer = false;
                if (flashlight != null)
                {
                    flashlight.color = Color.white;
                    flashlight.intensity = 35.0f;
                }
            }
        }

        private void OnGUI()
        {
            if (hasSpottedPlayer)
            {
                GUIStyle alertStyle = new GUIStyle();
                alertStyle.fontSize = 28;
                alertStyle.fontStyle = FontStyle.Bold;
                alertStyle.normal.textColor = Color.red;
                alertStyle.alignment = TextAnchor.MiddleCenter;

                GUI.Label(new Rect((Screen.width - 500) / 2f, 60, 500, 50), "🚨 INTRUDER DETECTED! PATROL ALERT! 🚨", alertStyle);
            }
        }
    }
}
