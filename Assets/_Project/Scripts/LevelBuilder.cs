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
            if (transform.Find("OfficeLayout") == null)
            {
                BuildRecordsOffice();
            }
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

            // Concrete Wall Material (Industrial slate noir)
            Material wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            wallMat.color = new Color(0.15f, 0.17f, 0.2f);
            wallMat.SetFloat("_Smoothness", 0.1f);

            // 1. Perimeter Walls
            CreateWall(officeRoot.transform, new Vector3(0, wallHeight / 2f, roomLength / 2f), new Vector3(roomWidth, wallHeight, wallThickness), wallMat, "NorthWall");
            CreateWall(officeRoot.transform, new Vector3(0, wallHeight / 2f, -roomLength / 2f), new Vector3(roomWidth, wallHeight, wallThickness), wallMat, "SouthWall");
            CreateWall(officeRoot.transform, new Vector3(-roomWidth / 2f, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, roomLength), wallMat, "WestWall");
            CreateWall(officeRoot.transform, new Vector3(roomWidth / 2f, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, roomLength), wallMat, "EastWall");

            // 2. Interior Partition Wall with Doorway Gap!
            CreateWall(officeRoot.transform, new Vector3(-4.5f, wallHeight / 2f, 3f), new Vector3(7f, wallHeight, wallThickness), wallMat, "LeftPartition");
            CreateWall(officeRoot.transform, new Vector3(5.5f, wallHeight / 2f, 3f), new Vector3(5f, wallHeight, wallThickness), wallMat, "RightPartition");

            // 3. Server Racks (Tall metal cover blocks you can sneak behind)
            Material rackMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            rackMat.color = new Color(0.06f, 0.07f, 0.08f);

            CreateCoverObject(officeRoot.transform, new Vector3(-4f, 1.4f, 6.5f), new Vector3(1.2f, 2.8f, 4.0f), rackMat, "ServerRack_01");
            CreateCoverObject(officeRoot.transform, new Vector3(-1.8f, 1.4f, 6.5f), new Vector3(1.2f, 2.8f, 4.0f), rackMat, "ServerRack_02");
            CreateCoverObject(officeRoot.transform, new Vector3(3.5f, 1.4f, 6.5f), new Vector3(1.2f, 2.8f, 4.0f), rackMat, "ServerRack_03");

            // 4. Amber & Security Blue Ceiling Point Lights
            CreateCeilingLight(officeRoot.transform, new Vector3(0, 3.8f, -2f), new Color(1.0f, 0.7f, 0.2f), 14f, "AmberLobbyLight");
            CreateCeilingLight(officeRoot.transform, new Vector3(0, 3.8f, 7f), new Color(0.35f, 0.65f, 1.0f), 12f, "ServerRoomLight");
        }

        private void CreateWall(Transform parent, Vector3 pos, Vector3 scale, Material mat, string name)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            if (mat != null) wall.GetComponent<MeshRenderer>().material = mat;
        }

        private void CreateCoverObject(Transform parent, Vector3 pos, Vector3 scale, Material mat, string name)
        {
            GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cover.name = name;
            cover.transform.SetParent(parent);
            cover.transform.position = pos;
            cover.transform.localScale = scale;
            if (mat != null) cover.GetComponent<MeshRenderer>().material = mat;
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
