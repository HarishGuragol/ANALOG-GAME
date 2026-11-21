using UnityEngine;

namespace Analog.Core
{
    [ExecuteAlways]
    public class LevelBuilder : MonoBehaviour
    {
        [Header("Office Dimensions")]
        [SerializeField] private float roomWidth = 16f;
        [SerializeField] private float roomLength = 22f;
        [SerializeField] private float wallHeight = 4.2f;
        [SerializeField] private float wallThickness = 0.4f;

        [Header("Build Trigger")]
        [SerializeField] private bool generateNow = false;

        // Doorway sits at x = 0..2 on the partition wall at z = 3
        private const float DoorLeftX = 0f;
        private const float DoorWidth = 2f;
        private const float DoorHeight = 2.6f;
        private const float PartitionZ = 3f;

        private void OnValidate()
        {
            if (generateNow)
            {
                generateNow = false;
                BuildRecordsOffice();
            }
        }

        private void Start()
        {
            // Always rebuild so layout fixes apply to the saved scene too
            BuildRecordsOffice();
        }

        [ContextMenu("Build Office Layout")]
        public void BuildRecordsOffice()
        {
            var oldOffice = transform.Find("OfficeLayout");
            if (oldOffice != null)
            {
                if (Application.isPlaying) Destroy(oldOffice.gameObject);
                else DestroyImmediate(oldOffice.gameObject);
            }

            GameObject officeRoot = new GameObject("OfficeLayout");
            officeRoot.transform.SetParent(transform);
            officeRoot.transform.localPosition = Vector3.zero;

            Material wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            wallMat.color = new Color(0.15f, 0.17f, 0.2f);
            wallMat.SetFloat("_Smoothness", 0.1f);

            // Perimeter walls
            CreateWall(officeRoot.transform, new Vector3(0, wallHeight / 2f, roomLength / 2f), new Vector3(roomWidth, wallHeight, wallThickness), wallMat, "NorthWall");
            CreateWall(officeRoot.transform, new Vector3(0, wallHeight / 2f, -roomLength / 2f), new Vector3(roomWidth, wallHeight, wallThickness), wallMat, "SouthWall");
            CreateWall(officeRoot.transform, new Vector3(-roomWidth / 2f, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, roomLength), wallMat, "WestWall");
            CreateWall(officeRoot.transform, new Vector3(roomWidth / 2f, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, roomLength), wallMat, "EastWall");

            // Partition wall with a doorway EXACTLY as wide as the door (no walking around it)
            float leftEnd = DoorLeftX;                       // left wall runs -8 .. 0
            float rightStart = DoorLeftX + DoorWidth;        // right wall runs 2 .. 8
            float edge = roomWidth / 2f;

            CreateWall(officeRoot.transform,
                new Vector3((-edge + leftEnd) / 2f, wallHeight / 2f, PartitionZ),
                new Vector3(leftEnd + edge, wallHeight, wallThickness), wallMat, "LeftPartition");
            CreateWall(officeRoot.transform,
                new Vector3((rightStart + edge) / 2f, wallHeight / 2f, PartitionZ),
                new Vector3(edge - rightStart, wallHeight, wallThickness), wallMat, "RightPartition");

            // Lintel above the door so the gap closes up to the ceiling
            float lintelHeight = wallHeight - DoorHeight;
            CreateWall(officeRoot.transform,
                new Vector3(DoorLeftX + DoorWidth / 2f, DoorHeight + lintelHeight / 2f, PartitionZ),
                new Vector3(DoorWidth, lintelHeight, wallThickness), wallMat, "DoorLintel");

            // Server racks (tall cover)
            Material rackMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            rackMat.color = new Color(0.06f, 0.07f, 0.08f);

            CreateCoverObject(officeRoot.transform, new Vector3(-4f, 1.4f, 6.5f), new Vector3(1.2f, 2.8f, 4.0f), rackMat, "ServerRack_01");
            CreateCoverObject(officeRoot.transform, new Vector3(-1.8f, 1.4f, 6.5f), new Vector3(1.2f, 2.8f, 4.0f), rackMat, "ServerRack_02");
            CreateCoverObject(officeRoot.transform, new Vector3(3.5f, 1.4f, 6.5f), new Vector3(1.2f, 2.8f, 4.0f), rackMat, "ServerRack_03");

            // Ceiling lights
            CreateCeilingLight(officeRoot.transform, new Vector3(0, 3.8f, -2f), new Color(1.0f, 0.7f, 0.2f), 14f, "AmberLobbyLight");
            CreateCeilingLight(officeRoot.transform, new Vector3(0, 3.8f, 7f), new Color(0.35f, 0.65f, 1.0f), 12f, "ServerRoomLight");

            FitDoorToDoorway();
        }

        // Put the existing Door object into the doorway so it fills it exactly
        private void FitDoorToDoorway()
        {
            var door = GameObject.Find("Door");
            if (door == null) return;

            door.transform.position = new Vector3(DoorLeftX + DoorWidth / 2f, DoorHeight / 2f, PartitionZ);
            door.transform.rotation = Quaternion.identity;
            door.transform.localScale = new Vector3(DoorWidth, DoorHeight, 0.15f);
        }

        private void CreateWall(Transform parent, Vector3 pos, Vector3 scale, Material mat, string name)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            if (mat != null) wall.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private void CreateCoverObject(Transform parent, Vector3 pos, Vector3 scale, Material mat, string name)
        {
            GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cover.name = name;
            cover.transform.SetParent(parent);
            cover.transform.position = pos;
            cover.transform.localScale = scale;
            if (mat != null) cover.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private void CreateCeilingLight(Transform parent, Vector3 pos, Color color, float intensity, string name)
        {
            GameObject lightObj = new GameObject(name);
            lightObj.transform.SetParent(parent);
            lightObj.transform.position = pos;

            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.range = 9.0f;
        }
    }
}
