using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Analog.Core
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float regenRate = 6f;
        [SerializeField] private float regenDelay = 5f;

        [Header("Melee Strike")]
        [SerializeField] private float strikeDamage = 35f;
        [SerializeField] private float strikeCooldown = 0.45f;
        [SerializeField] private float strikeRange = 1.35f;

        public bool IsDead { get; private set; }
        public float CurrentHealth => currentHealth;

        private float currentHealth;
        private float regenDelayTimer = 0f;
        private float strikeCooldownTimer = 0f;
        private float damageFlashTimer = 0f;
        private float hitConfirmTimer = 0f;
        private string hitConfirmText = "";

        private PlayerController player;
        private AudioSource audioSource;
        private AudioClip whooshClip;
        private AudioClip hitClip;
        private AudioClip hurtClip;
        private Texture2D whiteTex;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            currentHealth = maxHealth;

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            whooshClip = BuildToneClip("StrikeWhoosh", 190f, 95f, 0.11f, 0.4f);
            hitClip = BuildToneClip("StrikeImpact", 120f, 55f, 0.14f, 0.7f);
            hurtClip = BuildToneClip("PlayerHurt", 95f, 45f, 0.22f, 0.8f);
        }

        private void Update()
        {
            if (damageFlashTimer > 0f) damageFlashTimer -= Time.deltaTime;
            if (hitConfirmTimer > 0f) hitConfirmTimer -= Time.deltaTime;
            if (strikeCooldownTimer > 0f) strikeCooldownTimer -= Time.deltaTime;

            var keyboard = Keyboard.current;

            if (IsDead)
            {
                if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.rKey.wasPressedThisFrame))
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                return;
            }

            // Slow passive health recovery when out of combat
            if (regenDelayTimer > 0f)
            {
                regenDelayTimer -= Time.deltaTime;
            }
            else if (currentHealth < maxHealth)
            {
                currentHealth = Mathf.Min(maxHealth, currentHealth + regenRate * Time.deltaTime);
            }

            // Skip combat while hiding, reading terminal, or after mission completion
            if (player != null && player.IsHiding) return;
            if (Terminal.IsAnyOpen) return;
            if (MissionManager.Instance != null && MissionManager.Instance.MissionComplete) return;

            var mouse = Mouse.current;
            bool strikePressed = (keyboard != null && keyboard.qKey.wasPressedThisFrame) ||
                                 (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState == CursorLockMode.Locked);

            if (strikePressed && strikeCooldownTimer <= 0f)
            {
                PerformStrike();
            }
        }

        private void PerformStrike()
        {
            strikeCooldownTimer = strikeCooldown;
            if (audioSource != null && whooshClip != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.1f);
                audioSource.PlayOneShot(whooshClip, 0.35f);
            }

            NoiseSystem.EmitNoise(transform.position, 6f);

            Vector3 hitCenter = transform.position + transform.forward * 1.2f + Vector3.up * 0.8f;
            Collider[] hits = Physics.OverlapSphere(hitCenter, strikeRange);

            bool landedHit = false;
            foreach (var col in hits)
            {
                var guard = col.GetComponentInParent<GuardAI>();
                if (guard != null && !guard.IsKnockedOut)
                {
                    guard.TakeDamage(strikeDamage, transform.forward);
                    landedHit = true;
                    hitConfirmText = guard.IsKnockedOut ? "KNOCKOUT!" : $"HIT! ({Mathf.CeilToInt(guard.CurrentHealth)} HP)";
                    hitConfirmTimer = 0.8f;
                    break;
                }
            }

            if (landedHit && audioSource != null && hitClip != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.05f);
                audioSource.PlayOneShot(hitClip, 0.55f);
            }
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            damageFlashTimer = 0.35f;
            regenDelayTimer = regenDelay;

            if (audioSource != null && hurtClip != null)
            {
                audioSource.pitch = Random.Range(0.92f, 1.05f);
                audioSource.PlayOneShot(hurtClip, 0.6f);
            }

            if (currentHealth <= 0f)
            {
                IsDead = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void EnsureTex()
        {
            if (whiteTex == null)
            {
                whiteTex = new Texture2D(1, 1);
                whiteTex.SetPixel(0, 0, Color.white);
                whiteTex.Apply();
            }
        }

        private void OnGUI()
        {
            EnsureTex();

            // Brief red flash when struck
            if (damageFlashTimer > 0f && !IsDead)
            {
                float alpha = (damageFlashTimer / 0.35f) * 0.32f;
                GUI.color = new Color(0.85f, 0.08f, 0.08f, alpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteTex);
                GUI.color = Color.white;
            }

            // Bottom-left health bar
            float barW = 210f;
            float barH = 14f;
            float x = 20f;
            float y = Screen.height - 54f;

            GUIStyle labelStyle = new GUIStyle { fontSize = 13, fontStyle = FontStyle.Bold };
            labelStyle.normal.textColor = new Color(0.9f, 0.92f, 0.95f);
            GUI.Label(new Rect(x, y - 20f, 260f, 20f), $"KAEL // HP {Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}", labelStyle);

            GUI.color = new Color(0.08f, 0.09f, 0.11f, 0.85f);
            GUI.DrawTexture(new Rect(x, y, barW, barH), whiteTex);

            float pct = Mathf.Clamp01(currentHealth / maxHealth);
            GUI.color = currentHealth < 35f ? new Color(0.95f, 0.2f, 0.18f) : new Color(1f, 0.72f, 0.18f);
            GUI.DrawTexture(new Rect(x + 2f, y + 2f, (barW - 4f) * pct, barH - 4f), whiteTex);
            GUI.color = Color.white;

            GUIStyle hintStyle = new GUIStyle { fontSize = 12 };
            hintStyle.normal.textColor = new Color(0.65f, 0.7f, 0.75f, 0.85f);
            GUI.Label(new Rect(x, y + 18f, 260f, 18f), "[Q / Click] Melee Strike", hintStyle);

            if (hitConfirmTimer > 0f && !string.IsNullOrEmpty(hitConfirmText))
            {
                GUIStyle hitStyle = new GUIStyle
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                hitStyle.normal.textColor = new Color(1f, 0.78f, 0.25f);
                GUI.Label(new Rect((Screen.width - 260f) / 2f, Screen.height / 2f + 40f, 260f, 30f), hitConfirmText, hitStyle);
            }

            if (IsDead)
            {
                GUI.color = new Color(0.03f, 0.03f, 0.05f, 0.9f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteTex);
                GUI.color = Color.white;

                GUIStyle title = new GUIStyle { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                title.normal.textColor = new Color(0.95f, 0.22f, 0.2f);
                GUI.Label(new Rect(0, Screen.height / 2f - 50f, Screen.width, 45f), "CAPTURED // OPTIMIZATION COMPLETE", title);

                GUIStyle retry = new GUIStyle { fontSize = 16, fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleCenter };
                retry.normal.textColor = new Color(0.8f, 0.85f, 0.9f);
                GUI.Label(new Rect(0, Screen.height / 2f + 10f, Screen.width, 30f), "Press [ENTER] or [R] to Retry", retry);
            }
        }

        private static AudioClip BuildToneClip(string name, float startFreq, float endFreq, float duration, float gain)
        {
            int rate = 44100;
            int len = Mathf.Max(1, Mathf.RoundToInt(rate * duration));
            float[] samples = new float[len];
            float phase = 0f;

            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += freq / rate;
                float env = 1f - t;
                float grit = Mathf.Sin(i * 43.12f) * 0.25f;
                samples[i] = (Mathf.Sin(phase * 2f * Mathf.PI) * 0.75f + grit) * env * gain;
            }

            var clip = AudioClip.Create(name, len, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
