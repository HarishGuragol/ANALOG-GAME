using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Analog.Core
{
    // Auto-creates itself when the scene loads. No manual setup needed.
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }

        public bool AlarmActive { get; private set; }
        public bool MissionComplete { get; private set; }

        private readonly List<Light> alarmLights = new List<Light>();
        private GameObject exitDoor;
        private Vector3 exitPosition = new Vector3(-6f, 1.3f, 19.3f);
        private Transform player;
        private AudioSource sirenSource;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (Instance == null)
            {
                new GameObject("MissionManager").AddComponent<MissionManager>();
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        public void TriggerPurge()
        {
            if (AlarmActive) return;
            AlarmActive = true;

            CreateAlarmLights();
            CreateExitDoor();
            StartSiren();

            if (RadioCommsManager.Instance != null)
            {
                var mira = new Color(0.3f, 0.9f, 1.0f);
                RadioCommsManager.Instance.TriggerCommsSequence(new List<DialogueLine>
                {
                    new DialogueLine { speakerName = "MIRA", speakerColor = mira,
                        dialogueText = "Kael, they caught the wipe! Strike team just breached the front lobby!" },
                    new DialogueLine { speakerName = "MIRA", speakerColor = mira,
                        dialogueText = "Front door is sealed. Fire exit, back wall of the server room, the green light. RUN!" }
                });
            }
        }

        private void CreateAlarmLights()
        {
            Vector3[] spots =
            {
                new Vector3(0f, 5.2f, -4f),
                new Vector3(0f, 5.2f, 8f),
                new Vector3(-6f, 5.2f, 16f),
                new Vector3(6f, 5.2f, 14f)
            };

            foreach (var pos in spots)
            {
                var go = new GameObject("AlarmLight");
                go.transform.position = pos;
                var l = go.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = Color.red;
                l.range = 18f;
                l.intensity = 28f;
                alarmLights.Add(l);
            }
        }

        private void CreateExitDoor()
        {
            exitDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exitDoor.name = "FireExit";
            exitDoor.transform.position = exitPosition;
            exitDoor.transform.localScale = new Vector3(2f, 2.6f, 0.15f);
            Destroy(exitDoor.GetComponent<Collider>());

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.1f, 0.9f, 0.3f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.1f, 1f, 0.3f) * 2f);
            exitDoor.GetComponent<MeshRenderer>().material = mat;

            var glow = new GameObject("ExitGlow");
            glow.transform.position = exitPosition + new Vector3(0, 0, -1f);
            var l = glow.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.2f, 1f, 0.4f);
            l.range = 8f;
            l.intensity = 15f;
        }

        private void StartSiren()
        {
            sirenSource = gameObject.AddComponent<AudioSource>();
            sirenSource.loop = true;
            sirenSource.volume = 0.2f;

            int rate = 44100;
            int len = rate * 2;
            float[] data = new float[len];
            float phase = 0f;
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float freq = 600f + 400f * Mathf.Sin(t * Mathf.PI * 2f);
                phase += freq / rate;
                data[i] = Mathf.Sin(phase * Mathf.PI * 2f) * 0.5f;
            }
            var clip = AudioClip.Create("Siren", len, 1, rate, false);
            clip.SetData(data, 0);
            sirenSource.clip = clip;
            sirenSource.Play();
        }

        private void Update()
        {
            if (player == null)
            {
                var pc = FindFirstObjectByType<PlayerController>();
                if (pc != null) player = pc.transform;
            }

            if (AlarmActive && !MissionComplete)
            {
                float pulse = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f;
                foreach (var l in alarmLights)
                {
                    if (l != null) l.intensity = Mathf.Lerp(2f, 30f, pulse);
                }

                if (player != null &&
                    Vector3.Distance(new Vector3(player.position.x, 0, player.position.z),
                                     new Vector3(exitPosition.x, 0, exitPosition.z)) < 1.6f)
                {
                    CompleteMission();
                }
            }

            if (MissionComplete && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void CompleteMission()
        {
            MissionComplete = true;
            if (sirenSource != null) sirenSource.Stop();
            foreach (var l in alarmLights) if (l != null) l.enabled = false;
        }

        private void OnGUI()
        {
            if (!MissionComplete) return;

            float w = Mathf.Min(560, Screen.width - 60);
            float h = 260f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;

            var bg = new Texture2D(1, 1);
            bg.SetPixel(0, 0, new Color(0.04f, 0.05f, 0.07f, 0.96f));
            bg.Apply();
            var box = new GUIStyle(GUI.skin.box);
            box.normal.background = bg;
            GUI.Box(new Rect(x, y, w, h), GUIContent.none, box);

            var title = new GUIStyle { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            title.normal.textColor = new Color(1f, 0.75f, 0.2f);
            GUI.Label(new Rect(x, y + 25, w, 40), "MISSION 1 COMPLETE", title);

            var sub = new GUIStyle { fontSize = 16, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            sub.normal.textColor = Color.white;
            GUI.Label(new Rect(x + 30, y + 80, w - 60, 100),
                "GHOST RECORD\n\nKael's file is gone. Ria's is not.\nSomeone reallocated her, and Kael wants to know who.", sub);

            var hint = new GUIStyle { fontSize = 13, fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleCenter };
            hint.normal.textColor = new Color(0.6f, 0.65f, 0.7f);
            GUI.Label(new Rect(x, y + h - 45, w, 30), "Press [ENTER] to replay", hint);
        }
    }
}
