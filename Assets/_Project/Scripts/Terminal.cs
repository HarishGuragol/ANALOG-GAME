using UnityEngine;

namespace Analog.Core
{
    public class Terminal : MonoBehaviour, IInteractable
    {
        [Header("Terminal Data")]
        [SerializeField] private string terminalID = "ARCHIVE-NODE-01";
        [SerializeField] private string title = "MERIDIAN MUNICIPAL RECORDS // ARCHIVE LOG";
        
        [TextArea(4, 10)]
        [SerializeField] private string documentContent = 
            "RECORD ID: #8849-VARN-R\n" +
            "NAME: RIA VARN\n" +
            "STATUS: REALLOCATED\n" +
            "AUTHORIZATION: SYSTEM DIRECTIVE / ORACLE CORE\n" +
            "NOTES: Patient treatment reassessed under resource balance protocol.";

        [SerializeField] private bool opensTargetDoorOnRead = false;
        [SerializeField] private Door targetDoorToUnlock;

        private bool isOpen = false;

        public string GetInteractionPrompt()
        {
            return isOpen ? "Close Terminal" : "Access Terminal";
        }

        public void Interact(PlayerController player)
        {
            isOpen = !isOpen;

            if (isOpen && opensTargetDoorOnRead && targetDoorToUnlock != null)
            {
                targetDoorToUnlock.Unlock();
                Debug.Log($"[Terminal] Authorization granted. Door unlocked.");
            }
        }

        private void OnGUI()
        {
            if (!isOpen) return;

            // Cyber-noir CRT Terminal Display
            float boxWidth = Mathf.Min(600, Screen.width - 80);
            float boxHeight = Mathf.Min(380, Screen.height - 100);
            float startX = (Screen.width - boxWidth) / 2f;
            float startY = (Screen.height - boxHeight) / 2f;

            // Background box
            Texture2D bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0.04f, 0.05f, 0.07f, 0.95f)); // Dark industrial slate
            bgTex.Apply();

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = bgTex;
            GUI.Box(new Rect(startX, startY, boxWidth, boxHeight), GUIContent.none, boxStyle);

            // Amber border header
            GUIStyle headerStyle = new GUIStyle();
            headerStyle.fontSize = 18;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = new Color(1f, 0.7f, 0.15f); // Amber phosphor
            headerStyle.padding = new RectOffset(20, 20, 20, 10);

            GUI.Label(new Rect(startX, startY, boxWidth, 40), $"[SYSTEM] {title}", headerStyle);

            // Document body text
            GUIStyle bodyStyle = new GUIStyle();
            bodyStyle.fontSize = 16;
            bodyStyle.normal.textColor = new Color(0.85f, 0.9f, 0.95f);
            bodyStyle.wordWrap = true;
            bodyStyle.padding = new RectOffset(25, 25, 10, 10);

            GUI.Label(new Rect(startX, startY + 50, boxWidth - 40, boxHeight - 110), documentContent, bodyStyle);

            // Close prompt footer
            GUIStyle footerStyle = new GUIStyle();
            footerStyle.fontSize = 14;
            footerStyle.fontStyle = FontStyle.Italic;
            footerStyle.normal.textColor = new Color(0.5f, 0.6f, 0.65f);
            footerStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(startX, startY + boxHeight - 40, boxWidth, 30), "Press [E] to disconnect", footerStyle);
        }
    }
}
