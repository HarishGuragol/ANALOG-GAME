using UnityEngine;
using UnityEngine.InputSystem;

namespace Analog.Core
{
    public class AnalogNotebook : MonoBehaviour
    {
        [Header("Notebook State")]
        [SerializeField] private bool isOpen = false;

        [Header("Story Clues Collected")]
        private bool hasReadRiasFile = false;
        private bool hasCutPower = false;
        private bool hasSpottedGuard = false;

        public static AnalogNotebook Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Press TAB or J to open Kael's physical sketchbook!
            if (keyboard.tabKey.wasPressedThisFrame || keyboard.jKey.wasPressedThisFrame)
            {
                isOpen = !isOpen;
            }
        }

        public void LogClue(string clueType)
        {
            if (clueType == "Ria") hasReadRiasFile = true;
            if (clueType == "Breaker") hasCutPower = true;
            if (clueType == "Guard") hasSpottedGuard = true;
        }

        private void OnGUI()
        {
            // Small subtle HUD prompt in top-left
            GUIStyle hintStyle = new GUIStyle();
            hintStyle.fontSize = 14;
            hintStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            GUI.Label(new Rect(20, 20, 300, 25), "[TAB] Kael's Analog Sketchbook", hintStyle);

            if (!isOpen) return;

            // The Physical Sketchbook UI (Dev's architect paper)
            float bookWidth = Mathf.Min(680, Screen.width - 60);
            float bookHeight = Mathf.Min(500, Screen.height - 80);
            float startX = (Screen.width - bookWidth) / 2f;
            float startY = (Screen.height - bookHeight) / 2f;

            // Cream parchment / heavy sketch paper texture
            Texture2D paperTex = new Texture2D(1, 1);
            paperTex.SetPixel(0, 0, new Color(0.92f, 0.89f, 0.82f, 0.98f)); // Real sketchbook paper color!
            paperTex.Apply();

            GUIStyle paperBox = new GUIStyle(GUI.skin.box);
            paperBox.normal.background = paperTex;
            GUI.Box(new Rect(startX, startY, bookWidth, bookHeight), GUIContent.none, paperBox);

            // Sketchbook title (hand-written graphite pencil style)
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 22;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(0.2f, 0.2f, 0.22f); // Dark pencil lead
            titleStyle.padding = new RectOffset(35, 20, 25, 5);

            GUI.Label(new Rect(startX, startY, bookWidth, 40), "MERIDIAN ANNEX // KAEL'S FIELD SKETCHES", titleStyle);

            // Subtitle quote from Dev (Kael's architect father)
            GUIStyle quoteStyle = new GUIStyle();
            quoteStyle.fontSize = 13;
            quoteStyle.fontStyle = FontStyle.Italic;
            quoteStyle.normal.textColor = new Color(0.45f, 0.42f, 0.38f);
            quoteStyle.padding = new RectOffset(35, 20, 0, 10);

            GUI.Label(new Rect(startX, startY + 38, bookWidth, 25), "\"The machine predicts where a pencil goes. It can't predict why you lifted it.\" — Dev", quoteStyle);

            // Separator line
            Texture2D lineTex = new Texture2D(1, 1);
            lineTex.SetPixel(0, 0, new Color(0.6f, 0.58f, 0.52f, 0.6f));
            lineTex.Apply();
            GUI.DrawTexture(new Rect(startX + 35, startY + 68, bookWidth - 70, 2), lineTex);

            // Entries & Observations
            GUIStyle entryStyle = new GUIStyle();
            entryStyle.fontSize = 16;
            entryStyle.wordWrap = true;
            entryStyle.normal.textColor = new Color(0.18f, 0.18f, 0.2f);
            entryStyle.padding = new RectOffset(35, 35, 10, 10);

            string notes = "";

            notes += "• OBJECTIVE: Infiltrate municipal records and erase Drift file #992.\n\n";

            notes += "• OBSERVATION - INFRASTRUCTURE:\n";
            notes += "  Security doors run on magnetic coil locks. Cutting main circuit breaker severs the lock instantly.\n\n";

            notes += "• OBSERVATION - PATROLS:\n";
            notes += "  Corporate guards use forward-sweeping flashlights. Staying low (crouched) halves detection distance.\n\n";

            notes += "• RIA'S RECORD (CRIMINAL ARCHIVE):\n";
            notes += "  Found her file: \"Status: Reallocated.\" Directive signed by ORACLE Core. It wasn't an accident.\n";

            GUI.Label(new Rect(startX, startY + 85, bookWidth, bookHeight - 140), notes, entryStyle);

            // Close hint footer
            GUIStyle footerStyle = new GUIStyle();
            footerStyle.fontSize = 13;
            footerStyle.fontStyle = FontStyle.Italic;
            footerStyle.normal.textColor = new Color(0.4f, 0.38f, 0.35f);
            footerStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(startX, startY + bookHeight - 35, bookWidth, 25), "Press [TAB] to close sketchbook", footerStyle);
        }
    }
}
