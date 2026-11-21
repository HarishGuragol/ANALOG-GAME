using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Analog.Core
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        [TextArea(2, 4)]
        public string dialogueText;
        public Color speakerColor = new Color(0.2f, 0.8f, 1.0f); // Cyan default
    }

    public class RadioCommsManager : MonoBehaviour
    {
        public static RadioCommsManager Instance { get; private set; }

        [Header("Mission 1 Comms Sequence")]
        [SerializeField] private List<DialogueLine> introLines = new List<DialogueLine>();
        [SerializeField] private float autoAdvanceTime = 4.5f;

        private Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
        private DialogueLine currentLine;
        private bool isTransmitting = false;
        private float timer = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            SetupDefaultComms();
        }

        private void SetupDefaultComms()
        {
            introLines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    speakerName = "MIRA (UNKNOWN SIGNAL)",
                    dialogueText = "...Kael? Can you hear me? Don't speak, just listen.",
                    speakerColor = new Color(0.3f, 0.9f, 1.0f) // Mira's cyber-cyan
                },
                new DialogueLine
                {
                    speakerName = "MIRA (UNKNOWN SIGNAL)",
                    dialogueText = "I bypassed the annex comms relay. You're inside the municipal archive, right? ORACLE's purge team is 3 minutes out.",
                    speakerColor = new Color(0.3f, 0.9f, 1.0f)
                },
                new DialogueLine
                {
                    speakerName = "MIRA (UNKNOWN SIGNAL)",
                    dialogueText = "The server vault door has a magnetic coil lock. Find the circuit junction on the wall and cut the main feed to sever it.",
                    speakerColor = new Color(0.3f, 0.9f, 1.0f)
                },
                new DialogueLine
                {
                    speakerName = "KAEL",
                    dialogueText = "...Who are you? How are you tapping an analog earpiece?",
                    speakerColor = new Color(0.9f, 0.8f, 0.6f) // Kael's muted warm amber
                },
                new DialogueLine
                {
                    speakerName = "MIRA (UNKNOWN SIGNAL)",
                    dialogueText = "Call me a mutual friend. Just get to that terminal and wipe your record. And Kael... don't let the patrol catch you in their sweep.",
                    speakerColor = new Color(0.3f, 0.9f, 1.0f)
                }
            };
        }

        private void Start()
        {
            StartCoroutine(StartIntroAfterDelay(2.0f));
        }

        private IEnumerator StartIntroAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            TriggerCommsSequence(introLines);
        }

        public void TriggerCommsSequence(List<DialogueLine> lines)
        {
            dialogueQueue.Clear();
            foreach (var line in lines)
            {
                dialogueQueue.Enqueue(line);
            }

            NextLine();
        }

        private void NextLine()
        {
            if (dialogueQueue.Count > 0)
            {
                currentLine = dialogueQueue.Dequeue();
                isTransmitting = true;
                timer = 0f;
            }
            else
            {
                isTransmitting = false;
                currentLine = null;
            }
        }

        private void Update()
        {
            if (!isTransmitting) return;

            timer += Time.deltaTime;

            var keyboard = Keyboard.current;
            // Press SPACE or ENTER to advance dialogue faster
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
            {
                NextLine();
                return;
            }

            if (timer >= autoAdvanceTime)
            {
                NextLine();
            }
        }

        private void OnGUI()
        {
            if (!isTransmitting || currentLine == null) return;

            float boxWidth = Mathf.Min(620, Screen.width - 60);
            float boxHeight = 95f;
            float startX = (Screen.width - boxWidth) / 2f;
            float startY = 30f; // Sits neatly at the top of the screen

            // Dark semi-transparent radio HUD box
            Texture2D bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0.05f, 0.07f, 0.09f, 0.92f));
            bgTex.Apply();

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = bgTex;
            GUI.Box(new Rect(startX, startY, boxWidth, boxHeight), GUIContent.none, boxStyle);

            // Left decorative radio transmission bar
            Texture2D signalBar = new Texture2D(1, 1);
            signalBar.SetPixel(0, 0, currentLine.speakerColor);
            signalBar.Apply();
            GUI.DrawTexture(new Rect(startX + 2, startY + 2, 4, boxHeight - 4), signalBar);

            // Speaker Name + Waveform Icon
            GUIStyle speakerStyle = new GUIStyle();
            speakerStyle.fontSize = 14;
            speakerStyle.fontStyle = FontStyle.Bold;
            speakerStyle.normal.textColor = currentLine.speakerColor;
            GUI.Label(new Rect(startX + 20, startY + 10, boxWidth - 40, 20), $"[COMMS // 104.2 MHz] {currentLine.speakerName}", speakerStyle);

            // Subtitle / Dialogue Text
            GUIStyle textStyle = new GUIStyle();
            textStyle.fontSize = 15;
            textStyle.wordWrap = true;
            textStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(startX + 20, startY + 34, boxWidth - 40, 50), currentLine.dialogueText, textStyle);

            // Skip prompt in bottom corner of dialogue box
            GUIStyle skipStyle = new GUIStyle();
            skipStyle.fontSize = 11;
            skipStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            skipStyle.alignment = TextAnchor.LowerRight;
            GUI.Label(new Rect(startX, startY, boxWidth - 15, boxHeight - 8), "Press [SPACE] to advance", skipStyle);
        }
    }
}
