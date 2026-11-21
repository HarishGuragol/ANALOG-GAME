using System.Collections.Generic;
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

        private Dictionary<string, Material> materials;
        private Transform root;

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
            BuildRecordsOffice();
        }

        // ----------------------------------------------------------------- BUILD

        [ContextMenu("Build Office Layout")]
        public void BuildRecordsOffice()
        {
            var old = transform.Find("OfficeLayout");
            if (old != null) SafeDestroy(old.gameObject);

            materials = new Dictionary<string, Material>();
            root = new GameObject("OfficeLayout").transform;
            root.SetParent(transform);
            root.localPosition = Vector3.zero;

            BuildShell();
            BuildLobby();
            BuildServerRoom();
            BuildLighting();
            BuildFloorDetail();
            ApplyFloorMaterial();
            FitDoorToDoorway();
        }

        // Walls, partition and doorway
        private void BuildShell()
        {
            var wall = Mat("wall", new Color(0.15f, 0.17f, 0.2f), 0.1f);
            float h = wallHeight;

            Prop(PrimitiveType.Cube, "NorthWall", new Vector3(0, h / 2f, roomLength / 2f), new Vector3(roomWidth, h, wallThickness), wall);
            Prop(PrimitiveType.Cube, "SouthWall", new Vector3(0, h / 2f, -roomLength / 2f), new Vector3(roomWidth, h, wallThickness), wall);
            Prop(PrimitiveType.Cube, "WestWall", new Vector3(-roomWidth / 2f, h / 2f, 0), new Vector3(wallThickness, h, roomLength), wall);
            Prop(PrimitiveType.Cube, "EastWall", new Vector3(roomWidth / 2f, h / 2f, 0), new Vector3(wallThickness, h, roomLength), wall);

            var ceiling = Mat("ceiling", new Color(0.09f, 0.1f, 0.12f), 0.1f);
            Prop(PrimitiveType.Cube, "Ceiling", new Vector3(0, h + 0.1f, 0), new Vector3(roomWidth, 0.2f, roomLength), ceiling, false);

            float edge = roomWidth / 2f;
            float leftEnd = DoorLeftX;
            float rightStart = DoorLeftX + DoorWidth;

            Prop(PrimitiveType.Cube, "LeftPartition",
                new Vector3((-edge + leftEnd) / 2f, h / 2f, PartitionZ),
                new Vector3(leftEnd + edge, h, wallThickness), wall);
            Prop(PrimitiveType.Cube, "RightPartition",
                new Vector3((rightStart + edge) / 2f, h / 2f, PartitionZ),
                new Vector3(edge - rightStart, h, wallThickness), wall);

            float lintelH = h - DoorHeight;
            Prop(PrimitiveType.Cube, "DoorLintel",
                new Vector3(DoorLeftX + DoorWidth / 2f, DoorHeight + lintelH / 2f, PartitionZ),
                new Vector3(DoorWidth, lintelH, wallThickness), wall);

            // Door frame posts
            var trim = Mat("trim", new Color(0.08f, 0.09f, 0.1f), 0.3f, 0.6f);
            Prop(PrimitiveType.Cube, "DoorFrameL", new Vector3(DoorLeftX - 0.06f, DoorHeight / 2f, PartitionZ), new Vector3(0.12f, DoorHeight, wallThickness + 0.1f), trim);
            Prop(PrimitiveType.Cube, "DoorFrameR", new Vector3(DoorLeftX + DoorWidth + 0.06f, DoorHeight / 2f, PartitionZ), new Vector3(0.12f, DoorHeight, wallThickness + 0.1f), trim);

            // Baseboards along the long walls
            var board = Mat("baseboard", new Color(0.07f, 0.08f, 0.09f), 0.2f);
            Prop(PrimitiveType.Cube, "BaseboardW", new Vector3(-edge + 0.22f, 0.12f, 0), new Vector3(0.05f, 0.24f, roomLength - 0.4f), board, false);
            Prop(PrimitiveType.Cube, "BaseboardE", new Vector3(edge - 0.22f, 0.12f, 0), new Vector3(0.05f, 0.24f, roomLength - 0.4f), board, false);
        }

        // Reception area (z -11 .. 3)
        private void BuildLobby()
        {
            var wood = Mat("wood", new Color(0.22f, 0.14f, 0.09f), 0.35f);
            var metal = Mat("metal", new Color(0.2f, 0.22f, 0.25f), 0.45f, 0.7f);
            var darkMetal = Mat("darkmetal", new Color(0.09f, 0.1f, 0.12f), 0.4f, 0.6f);
            var fabric = Mat("fabric", new Color(0.12f, 0.16f, 0.2f), 0.05f);
            var screen = Mat("screen", new Color(0.2f, 0.7f, 0.8f), 0.8f, 0f, 1.6f);
            var vendGlow = Mat("vendglow", new Color(0.75f, 0.9f, 1f), 0.8f, 0f, 1.4f);
            var pot = Mat("pot", new Color(0.3f, 0.2f, 0.15f), 0.2f);
            var leaf = Mat("leaf", new Color(0.08f, 0.22f, 0.1f), 0.1f);
            var crate = Mat("crate", new Color(0.3f, 0.22f, 0.12f), 0.15f);
            var glass = Mat("glassbottle", new Color(0.15f, 0.35f, 0.2f), 0.9f);

            // Reception desk (L shape) with monitors
            Prop(PrimitiveType.Cube, "ReceptionDesk", new Vector3(-5.5f, 0.55f, -7.5f), new Vector3(4f, 1.1f, 0.8f), wood);
            Prop(PrimitiveType.Cube, "ReceptionReturn", new Vector3(-7.35f, 0.55f, -6.0f), new Vector3(0.8f, 1.1f, 3f), wood);
            Prop(PrimitiveType.Cube, "DeskTop", new Vector3(-5.5f, 1.12f, -7.5f), new Vector3(4.1f, 0.05f, 0.9f), darkMetal, false);
            Prop(PrimitiveType.Cube, "Monitor1", new Vector3(-6.2f, 1.45f, -7.45f), new Vector3(0.6f, 0.38f, 0.04f), screen, false);
            Prop(PrimitiveType.Cube, "Monitor2", new Vector3(-4.8f, 1.45f, -7.45f), new Vector3(0.6f, 0.38f, 0.04f), screen, false);

            // Filing cabinets along the south wall
            for (int i = 0; i < 3; i++)
            {
                Prop(PrimitiveType.Cube, "FilingCabinet" + i, new Vector3(-3f + i * 0.9f, 0.9f, -10.5f), new Vector3(0.8f, 1.8f, 0.5f), metal);
            }

            // Waiting benches
            Prop(PrimitiveType.Cube, "Bench1", new Vector3(5.5f, 0.25f, -8f), new Vector3(3f, 0.5f, 0.6f), fabric);
            Prop(PrimitiveType.Cube, "Bench2", new Vector3(5.5f, 0.25f, -5.5f), new Vector3(3f, 0.5f, 0.6f), fabric);
            Prop(PrimitiveType.Cube, "CoffeeTable", new Vector3(5.5f, 0.2f, -6.75f), new Vector3(1.2f, 0.4f, 0.6f), wood);

            // Support pillars
            Prop(PrimitiveType.Cube, "Pillar1", new Vector3(5f, wallHeight / 2f, -1.5f), new Vector3(0.7f, wallHeight, 0.7f), Mat("wall", Color.black));
            Prop(PrimitiveType.Cube, "Pillar2", new Vector3(5f, wallHeight / 2f, -9.8f), new Vector3(0.7f, wallHeight, 0.7f), Mat("wall", Color.black));
            Prop(PrimitiveType.Cube, "Pillar3", new Vector3(-4.5f, wallHeight / 2f, -2f), new Vector3(0.7f, wallHeight, 0.7f), Mat("wall", Color.black));

            // Vending machine with glowing front
            Prop(PrimitiveType.Cube, "VendingMachine", new Vector3(7.3f, 1.1f, -2f), new Vector3(0.9f, 2.2f, 1f), darkMetal);
            Prop(PrimitiveType.Cube, "VendingFront", new Vector3(6.83f, 1.3f, -2f), new Vector3(0.04f, 1.3f, 0.75f), vendGlow, false);

            // Potted plant
            Prop(PrimitiveType.Cylinder, "PlantPot", new Vector3(7f, 0.3f, -10f), new Vector3(0.6f, 0.3f, 0.6f), pot);
            Prop(PrimitiveType.Sphere, "PlantLeaves", new Vector3(7f, 0.95f, -10f), new Vector3(0.85f, 0.85f, 0.85f), leaf, false);

            // Loose physics props Kael can shove around
            Prop(PrimitiveType.Cube, "Crate1", new Vector3(-6.2f, 0.4f, -1.5f), new Vector3(0.8f, 0.8f, 0.8f), crate, true, true, 6f);
            Prop(PrimitiveType.Cube, "Crate2", new Vector3(-5.2f, 0.3f, -0.8f), new Vector3(0.6f, 0.6f, 0.6f), crate, true, true, 4f);
            Prop(PrimitiveType.Cylinder, "Bottle1", new Vector3(-3.5f, 0.2f, -5.8f), new Vector3(0.14f, 0.2f, 0.14f), glass, true, true, 0.5f);
            Prop(PrimitiveType.Cylinder, "Bottle2", new Vector3(-3.2f, 0.2f, -5.4f), new Vector3(0.14f, 0.2f, 0.14f), glass, true, true, 0.5f);
            Prop(PrimitiveType.Cylinder, "Bin", new Vector3(-7.2f, 0.3f, -9.5f), new Vector3(0.45f, 0.3f, 0.45f), metal, true, true, 3f);

            // Overhead pipes (decorative, no collision so the camera never snags)
            var pipe = Mat("pipe", new Color(0.12f, 0.13f, 0.15f), 0.4f, 0.7f);
            Prop(PrimitiveType.Cylinder, "Pipe1", new Vector3(0f, 3.9f, -6f), new Vector3(0.14f, roomWidth / 2f - 0.3f, 0.14f), pipe, false, false, 0f, new Vector3(0, 0, 90));
            Prop(PrimitiveType.Cylinder, "Pipe2", new Vector3(0f, 3.7f, -9f), new Vector3(0.1f, roomWidth / 2f - 0.3f, 0.1f), pipe, false, false, 0f, new Vector3(0, 0, 90));
        }

        // Server vault (z 3 .. 11)
        private void BuildServerRoom()
        {
            var rack = Mat("rack", new Color(0.06f, 0.07f, 0.08f), 0.5f, 0.6f);
            var ledGreen = Mat("ledgreen", new Color(0.1f, 1f, 0.3f), 0.5f, 0f, 2.5f);
            var ledBlue = Mat("ledblue", new Color(0.2f, 0.5f, 1f), 0.5f, 0f, 2.5f);
            var cable = Mat("cable", new Color(0.04f, 0.04f, 0.05f), 0.3f);
            var hazard = Mat("hazard", new Color(0.9f, 0.7f, 0.05f), 0.2f);
            var exitFrame = Mat("exitframe", new Color(0.1f, 0.11f, 0.12f), 0.3f, 0.6f);
            var exitSign = Mat("exitsign", new Color(0.1f, 0.8f, 0.3f), 0.4f, 0f, 1.2f);
            var crate = Mat("crate", new Color(0.3f, 0.22f, 0.12f), 0.15f);

            // Server racks (positions are tuned to the guard, so don't move these)
            Vector3[] rackPos =
            {
                new Vector3(-4f, 1.4f, 6.5f),
                new Vector3(-1.8f, 1.4f, 6.5f),
                new Vector3(3.5f, 1.4f, 6.5f)
            };

            for (int r = 0; r < rackPos.Length; r++)
            {
                Prop(PrimitiveType.Cube, "ServerRack_0" + (r + 1), rackPos[r], new Vector3(1.2f, 2.8f, 4.0f), rack);

                // Blinking-style status LEDs on both faces
                for (int i = 0; i < 7; i++)
                {
                    float y = 0.5f + i * 0.33f;
                    Material led = (i + r) % 2 == 0 ? ledGreen : ledBlue;
                    Prop(PrimitiveType.Cube, "LED", new Vector3(rackPos[r].x + 0.62f, y, rackPos[r].z - 1.2f + (i % 3) * 1.2f), new Vector3(0.04f, 0.06f, 0.18f), led, false);
                    Prop(PrimitiveType.Cube, "LED", new Vector3(rackPos[r].x - 0.62f, y, rackPos[r].z - 1.2f + ((i + 1) % 3) * 1.2f), new Vector3(0.04f, 0.06f, 0.18f), led, false);
                }
            }

            // Cable runs along the floor and overhead
            Prop(PrimitiveType.Cube, "FloorCable1", new Vector3(-3f, 0.03f, 9.0f), new Vector3(6f, 0.06f, 0.18f), cable, false);
            Prop(PrimitiveType.Cube, "FloorCable2", new Vector3(-1.2f, 0.03f, 6.5f), new Vector3(0.18f, 0.06f, 4.2f), cable, false);
            Prop(PrimitiveType.Cube, "CableTray1", new Vector3(-3f, 3.6f, 6.5f), new Vector3(0.5f, 0.12f, 6f), cable, false);
            Prop(PrimitiveType.Cube, "CableTray2", new Vector3(2.5f, 3.6f, 6.5f), new Vector3(0.5f, 0.12f, 6f), cable, false);

            // Cover crate just inside the doorway (pushable)
            Prop(PrimitiveType.Cube, "CoverCrate", new Vector3(-0.5f, 0.45f, 4.7f), new Vector3(0.9f, 0.9f, 0.9f), crate, true, true, 8f);

            // Fire exit frame, hazard stripe and sign (the glowing green door appears after the alarm)
            Prop(PrimitiveType.Cube, "ExitDoorFrame", new Vector3(-6f, 1.35f, 10.65f), new Vector3(2.3f, 2.7f, 0.1f), exitFrame, false);
            Prop(PrimitiveType.Cube, "ExitHazardStripe", new Vector3(-6f, 0.012f, 9.7f), new Vector3(2.4f, 0.02f, 0.35f), hazard, false);
            Prop(PrimitiveType.Cube, "ExitSign", new Vector3(-6f, 3.2f, 10.7f), new Vector3(1.2f, 0.28f, 0.05f), exitSign, false);
        }

        private void BuildLighting()
        {
            // Lobby
            Light(new Vector3(0f, 3.8f, -2f), new Color(1.0f, 0.7f, 0.2f), 14f, 9f, "AmberLobbyLight", true);
            Light(new Vector3(-5f, 3.8f, -6.5f), new Color(1.0f, 0.75f, 0.35f), 9f, 8f, "ReceptionLight", true);
            Light(new Vector3(5.5f, 3.8f, -7f), new Color(1.0f, 0.65f, 0.3f), 6f, 7f, "WaitingLight", false);
            Light(new Vector3(6.2f, 1.5f, -2f), new Color(0.7f, 0.9f, 1f), 5f, 4f, "VendingGlowLight", false);

            // Server room
            Light(new Vector3(0f, 3.8f, 7f), new Color(0.35f, 0.65f, 1.0f), 12f, 9f, "ServerRoomLight", true);
            Light(new Vector3(-4f, 3.8f, 9.4f), new Color(0.3f, 0.5f, 0.9f), 7f, 7f, "ServerBackLight", false);
            Light(new Vector3(-6f, 2.8f, 10f), new Color(0.2f, 1f, 0.4f), 3f, 3.5f, "ExitSignGlow", false);
        }

        // Rug runner, puddles and wet patches
        private void BuildFloorDetail()
        {
            var runner = Mat("runner", new Color(0.16f, 0.05f, 0.06f), 0.05f);
            Prop(PrimitiveType.Cube, "HallwayRunner", new Vector3(1f, 0.011f, -4f), new Vector3(2.4f, 0.02f, 14f), runner, false);

            var puddle = Mat("puddle", new Color(0.05f, 0.07f, 0.1f), 0.97f, 0.4f);
            Prop(PrimitiveType.Cylinder, "Puddle1", new Vector3(-2f, 0.006f, -7f), new Vector3(1.6f, 0.005f, 1.1f), puddle, false);
            Prop(PrimitiveType.Cylinder, "Puddle2", new Vector3(3f, 0.006f, -3.5f), new Vector3(1.1f, 0.005f, 1.4f), puddle, false);
            Prop(PrimitiveType.Cylinder, "Puddle3", new Vector3(1.2f, 0.006f, 5.5f), new Vector3(1.3f, 0.005f, 0.9f), puddle, false);
        }

        // Dark, slightly wet concrete floor
        private void ApplyFloorMaterial()
        {
            var ground = GameObject.Find("Ground");
            if (ground == null) return;

            var rend = ground.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                rend.sharedMaterial = Mat("floor", new Color(0.11f, 0.12f, 0.14f), 0.55f, 0.1f);
            }
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

        // ----------------------------------------------------------------- HELPERS

        private Material Mat(string key, Color color, float smoothness = 0.2f, float metallic = 0f, float emission = 0f)
        {
            if (materials.TryGetValue(key, out var existing)) return existing;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            if (emission > 0f)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * emission);
            }
            materials[key] = mat;
            return mat;
        }

        private GameObject Prop(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat,
                                bool collider = true, bool physics = false, float mass = 5f, Vector3 euler = default)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(root);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.transform.rotation = Quaternion.Euler(euler);
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            if (!collider)
            {
                var c = go.GetComponent<Collider>();
                if (c != null) SafeDestroy(c);
            }

            if (physics)
            {
                var rb = go.AddComponent<Rigidbody>();
                rb.mass = mass;
            }
            return go;
        }

        private void Light(Vector3 pos, Color color, float intensity, float range, string name, bool flicker)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(root);
            obj.transform.position = pos;

            var l = obj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.range = range;

            // Added after intensity is set so the flicker remembers the right base value
            if (flicker) obj.AddComponent<FlickerLight>();
        }

        private static void SafeDestroy(Object o)
        {
            if (Application.isPlaying) Destroy(o);
            else DestroyImmediate(o);
        }
    }
}
