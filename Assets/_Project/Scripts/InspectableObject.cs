using UnityEngine;

namespace Analog.Core
{
    public class InspectableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string promptLabel = "Inspect";
        [SerializeField] private string title = "NOTE";
        [TextArea(3, 8)]
        [SerializeField] private string bodyText = "";
        [SerializeField] private bool isVendingMachine = false;

        private bool isInspecting = false;
        private int cansDispensed = 0;
        private const int MaxCans = 5;

        public void Configure(string prompt, string headerTitle, string text, bool vending = false)
        {
            promptLabel = prompt;
            title = headerTitle;
            bodyText = text;
            isVendingMachine = vending;
        }

        public string GetInteractionPrompt()
        {
            if (isVendingMachine)
            {
                return cansDispensed >= MaxCans ? "Vending Machine Empty" : promptLabel;
            }
            return isInspecting ? "Close" : promptLabel;
        }

        public void Interact(PlayerController player)
        {
            if (isVendingMachine)
            {
                DispenseSodaCan();
                return;
            }

            isInspecting = !isInspecting;
        }

        private void DispenseSodaCan()
        {
            if (cansDispensed >= MaxCans) return;
            cansDispensed++;

            var can = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            can.name = $"SodaCan_{cansDispensed}";
            can.transform.position = transform.position + new Vector3(-0.75f, 0.7f, Random.Range(-0.25f, 0.25f));
            can.transform.localScale = new Vector3(0.11f, 0.14f, 0.11f);
            can.transform.rotation = Quaternion.Euler(75f, Random.Range(0f, 360f), 0f);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.75f, 0.12f, 0.14f);
            mat.SetFloat("_Smoothness", 0.7f);
            mat.SetFloat("_Metallic", 0.65f);
            can.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var rb = can.AddComponent<Rigidbody>();
            rb.mass = 0.35f;
            rb.AddForce(new Vector3(-1.8f, 0.8f, Random.Range(-0.4f, 0.4f)), ForceMode.Impulse);

            var prop = can.AddComponent<PhysicsProp>();
            prop.Configure("Soda Can", false, true, 10f);
        }

        private void OnGUI()
        {
            if (!isInspecting || isVendingMachine) return;

            float w = Mathf.Min(540f, Screen.width - 80f);
            float h = 240f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;

            Texture2D bg = new Texture2D(1, 1);
            bg.SetPixel(0, 0, new Color(0.05f, 0.06f, 0.08f, 0.94f));
            bg.Apply();

            GUIStyle box = new GUIStyle(GUI.skin.box);
            box.normal.background = bg;
            GUI.Box(new Rect(x, y, w, h), GUIContent.none, box);

            Texture2D accent = new Texture2D(1, 1);
            accent.SetPixel(0, 0, new Color(1f, 0.72f, 0.18f, 1f));
            accent.Apply();
            GUI.DrawTexture(new Rect(x, y, w, 3f), accent);

            GUIStyle header = new GUIStyle
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(20, 20, 16, 6)
            };
            header.normal.textColor = new Color(1f, 0.72f, 0.18f);
            GUI.Label(new Rect(x, y, w, 42f), title, header);

            GUIStyle body = new GUIStyle
            {
                fontSize = 15,
                wordWrap = true,
                padding = new RectOffset(22, 22, 6, 10)
            };
            body.normal.textColor = new Color(0.88f, 0.9f, 0.94f);
            GUI.Label(new Rect(x, y + 44f, w, h - 85f), bodyText, body);

            GUIStyle footer = new GUIStyle
            {
                fontSize = 13,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter
            };
            footer.normal.textColor = new Color(0.6f, 0.65f, 0.7f);
            GUI.Label(new Rect(x, y + h - 34f, w, 24f), "Press [E] to close", footer);
        }
    }
}
