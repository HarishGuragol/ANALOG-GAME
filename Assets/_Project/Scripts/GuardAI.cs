using UnityEngine;

namespace Analog.Core
{
    public class GuardAI : MonoBehaviour, IInteractable
    {
        [Header("Vision Settings")]
        [SerializeField] private float viewDistance = 10.0f;
        [SerializeField] private float viewAngle = 75.0f;

        [Header("Patrol & Investigation")]
        [SerializeField] private float patrolSpeed = 1.9f;
        [SerializeField] private float investigateSpeed = 2.2f;
        [SerializeField] private float investigateDuration = 4.5f;
        [SerializeField] private bool carriesVaultKeycard = false;

        [Header("Vision Cone Visual")]
        [SerializeField] private Light flashlight;

        public bool IsKnockedOut { get; private set; }

        private Transform player;
        private PlayerController playerController;
        private bool hasSpottedPlayer = false;

        private Vector3 homePosition;
        private float homeYaw;
        private Vector3[] patrolPoints;
        private int patrolIndex = 0;
        private float patrolWaitTimer = 0f;

        private bool isInvestigating = false;
        private bool returningHome = false;
        private Vector3 investigateTarget;
        private float investigateTimer = 0f;

        private float toastTimer = 0f;
        private string toastMessage = "";

        public void ConfigureGuard(Vector3[] waypoints, bool hasKeycard)
        {
            patrolPoints = waypoints;
            carriesVaultKeycard = hasKeycard;
        }

        private void OnEnable()
        {
            NoiseSystem.OnNoiseEmitted += HandleNoiseHeard;
        }

        private void OnDisable()
        {
            NoiseSystem.OnNoiseEmitted -= HandleNoiseHeard;
        }

        private void Start()
        {
            homePosition = transform.position;
            homeYaw = transform.eulerAngles.y;
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

        private void HandleNoiseHeard(Vector3 noisePos, float radius)
        {
            if (IsKnockedOut || hasSpottedPlayer) return;
            if (Vector3.Distance(transform.position, noisePos) <= radius)
            {
                isInvestigating = true;
                returningHome = false;
                investigateTarget = new Vector3(noisePos.x, transform.position.y, noisePos.z);
                investigateTimer = investigateDuration;
                UpdateFlashlightColor();
            }
        }

        private void Update()
        {
            if (toastTimer > 0f) toastTimer -= Time.deltaTime;
            if (IsKnockedOut) return;

            if (player == null)
            {
                FindPlayer();
                if (player == null) return;
            }

            if (isInvestigating)
            {
                UpdateInvestigation();
            }
            else if (returningHome)
            {
                UpdateReturnHome();
            }
            else if (patrolPoints != null && patrolPoints.Length >= 2)
            {
                UpdatePatrol();
            }
            else
            {
                float swivelAngle = Mathf.PingPong(Time.time * 30f, 80f) - 40f;
                transform.rotation = Quaternion.Euler(0, homeYaw + swivelAngle, 0);
            }

            CheckDetection();
        }

        private void UpdatePatrol()
        {
            Vector3 target = new Vector3(patrolPoints[patrolIndex].x, transform.position.y, patrolPoints[patrolIndex].z);
            Vector3 toTarget = target - transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude > 0.3f)
            {
                Vector3 dir = toTarget.normalized;
                Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
                transform.position += dir * patrolSpeed * Time.deltaTime;
            }
            else
            {
                patrolWaitTimer += Time.deltaTime;
                if (patrolWaitTimer >= 2.0f)
                {
                    patrolWaitTimer = 0f;
                    patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                }
            }
        }

        private void UpdateInvestigation()
        {
            Vector3 toTarget = investigateTarget - transform.position;
            toTarget.y = 0f;
            float dist = toTarget.magnitude;

            if (dist > 1.4f)
            {
                Vector3 dir = toTarget.normalized;
                Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 6f);

                if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, 0.85f, ~0, QueryTriggerInteraction.Ignore))
                {
                    transform.position += dir * investigateSpeed * Time.deltaTime;
                }
                else
                {
                    investigateTimer -= Time.deltaTime;
                }
            }
            else
            {
                float scan = Mathf.Sin(Time.time * 2.5f) * 35f * Time.deltaTime;
                transform.Rotate(0f, scan, 0f);
                investigateTimer -= Time.deltaTime;
            }

            if (investigateTimer <= 0f)
            {
                isInvestigating = false;
                returningHome = true;
                UpdateFlashlightColor();
            }
        }

        private void UpdateReturnHome()
        {
            Vector3 returnPos = (patrolPoints != null && patrolPoints.Length > 0) ? patrolPoints[patrolIndex] : homePosition;
            Vector3 toHome = returnPos - transform.position;
            toHome.y = 0f;
            float dist = toHome.magnitude;

            if (dist > 0.25f)
            {
                Vector3 dir = toHome.normalized;
                Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);

                if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, 0.7f, ~0, QueryTriggerInteraction.Ignore))
                {
                    transform.position += dir * investigateSpeed * Time.deltaTime;
                    return;
                }
            }

            transform.position = new Vector3(returnPos.x, transform.position.y, returnPos.z);
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                transform.rotation = Quaternion.Euler(0f, homeYaw, 0f);
            }
            returningHome = false;
            UpdateFlashlightColor();
        }

        private void CheckDetection()
        {
            if (playerController != null && playerController.IsHiding)
            {
                TriggerAlert(false);
                return;
            }

            Vector3 guardEyePos = transform.position + Vector3.up * 0.8f;
            Vector3 playerTargetPos = player.position + Vector3.up * 0.8f;
            Vector3 dirToPlayer = playerTargetPos - guardEyePos;
            float dist = dirToPlayer.magnitude;

            float maxDetectDist = playerController != null && playerController.IsCrouching ? viewDistance * 0.5f : viewDistance;

            if (dist <= maxDetectDist)
            {
                float angle = Vector3.Angle(transform.forward, dirToPlayer);
                if (angle <= viewAngle / 2f)
                {
                    Ray ray = new Ray(guardEyePos + transform.forward * 0.6f, dirToPlayer.normalized);
                    if (Physics.Raycast(ray, out RaycastHit hit, dist))
                    {
                        if (hit.transform == player || hit.transform.IsChildOf(player))
                        {
                            TriggerAlert(true);
                            return;
                        }
                    }
                    else
                    {
                        TriggerAlert(true);
                        return;
                    }
                }
            }

            TriggerAlert(false);
        }

        private void TriggerAlert(bool spotted)
        {
            hasSpottedPlayer = spotted;
            UpdateFlashlightColor();
        }

        private void UpdateFlashlightColor()
        {
            if (flashlight == null) return;

            if (hasSpottedPlayer)
            {
                flashlight.color = Color.red;
                flashlight.intensity = 50.0f;
            }
            else if (isInvestigating)
            {
                flashlight.color = new Color(1f, 0.8f, 0.15f);
                flashlight.intensity = 42.0f;
            }
            else
            {
                flashlight.color = Color.white;
                flashlight.intensity = 35.0f;
            }
        }

        public string GetInteractionPrompt()
        {
            if (IsKnockedOut || hasSpottedPlayer) return "";
            return carriesVaultKeycard ? "Silent Takedown (Take Keycard)" : "Silent Takedown";
        }

        public void Interact(PlayerController p)
        {
            if (IsKnockedOut || hasSpottedPlayer) return;

            IsKnockedOut = true;
            isInvestigating = false;
            returningHome = false;
            hasSpottedPlayer = false;

            if (flashlight != null) flashlight.enabled = false;

            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
            transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);

            if (carriesVaultKeycard)
            {
                var door = FindFirstObjectByType<Door>();
                if (door != null) door.Unlock();
                toastMessage = "KEYCARD ACQUIRED — Server Vault Door Unlocked!";
                toastTimer = 4.0f;
            }
            else
            {
                toastMessage = "Guard neutralized silently.";
                toastTimer = 2.5f;
            }
        }

        private void OnGUI()
        {
            if (toastTimer > 0f && !string.IsNullOrEmpty(toastMessage))
            {
                GUIStyle toastStyle = new GUIStyle
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                toastStyle.normal.textColor = new Color(1f, 0.78f, 0.2f);
                GUI.Label(new Rect((Screen.width - 560f) / 2f, 195f, 560f, 36f), toastMessage, toastStyle);
            }

            if (IsKnockedOut) return;

            if (hasSpottedPlayer)
            {
                GUIStyle alertStyle = new GUIStyle
                {
                    fontSize = 28,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                alertStyle.normal.textColor = Color.red;

                GUI.Label(new Rect((Screen.width - 500) / 2f, 60, 500, 50), "🚨 INTRUDER DETECTED! PATROL ALERT! 🚨", alertStyle);
            }
            else if (isInvestigating)
            {
                GUIStyle invStyle = new GUIStyle
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                invStyle.normal.textColor = new Color(1f, 0.8f, 0.15f);

                GUI.Label(new Rect((Screen.width - 420) / 2f, 115, 420, 40), "? GUARD INVESTIGATING NOISE", invStyle);
            }
        }
    }
}
