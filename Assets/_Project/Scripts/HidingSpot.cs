using UnityEngine;

namespace Analog.Core
{
    public class HidingSpot : MonoBehaviour, IInteractable
    {
        [SerializeField] private string spotName = "Storage Locker";
        [SerializeField] private Vector3 exitPositionOffset = new Vector3(1.1f, 0f, 0f);

        private bool isOccupied = false;
        private PlayerController occupyingPlayer;
        private Vector3 savedPlayerPos;

        public void Configure(string name, Vector3 exitOffset)
        {
            spotName = name;
            exitPositionOffset = exitOffset;
        }

        public string GetInteractionPrompt()
        {
            return isOccupied ? $"Exit {spotName}" : $"Hide in {spotName}";
        }

        public void Interact(PlayerController player)
        {
            if (player == null) return;

            if (!isOccupied)
            {
                EnterSpot(player);
            }
            else
            {
                ExitSpot();
            }
        }

        private void EnterSpot(PlayerController player)
        {
            isOccupied = true;
            occupyingPlayer = player;
            savedPlayerPos = player.transform.position;

            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.IsHiding = true;
            player.transform.position = new Vector3(transform.position.x, savedPlayerPos.y, transform.position.z);

            SetRenderersVisible(player, false);
        }

        private void ExitSpot()
        {
            if (occupyingPlayer != null)
            {
                Vector3 exitPos = new Vector3(
                    transform.position.x + exitPositionOffset.x,
                    savedPlayerPos.y,
                    transform.position.z + exitPositionOffset.z
                );
                occupyingPlayer.transform.position = exitPos;
                SetRenderersVisible(occupyingPlayer, true);

                var cc = occupyingPlayer.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = true;

                occupyingPlayer.IsHiding = false;
            }

            isOccupied = false;
            occupyingPlayer = null;
        }

        private static void SetRenderersVisible(PlayerController player, bool visible)
        {
            foreach (var r in player.GetComponentsInChildren<MeshRenderer>())
            {
                r.enabled = visible;
            }
        }

        private void OnGUI()
        {
            if (!isOccupied) return;

            GUIStyle style = new GUIStyle
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            style.normal.textColor = new Color(0.3f, 0.95f, 0.5f);

            float w = 620f;
            GUI.Label(new Rect((Screen.width - w) / 2f, 160f, w, 36f),
                $"[HIDDEN IN {spotName.ToUpper()}] Guard cannot see you — Press [E] to exit", style);
        }
    }
}
