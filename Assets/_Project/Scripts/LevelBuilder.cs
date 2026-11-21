using System.Collections.Generic;
using UnityEngine;

namespace Analog.Core
{
    [ExecuteAlways]
    public class LevelBuilder : MonoBehaviour
    {
        [Header("Office Dimensions")]
        [SerializeField] private float roomWidth = 28f;
        [SerializeField] private float roomLength = 32f;   // Interior z: -12 .. +20
        [SerializeField] private float wallHeight = 5.8f;
        [SerializeField] private float wallThickness = 0.4f;

        [Header("Build Trigger")]
        [SerializeField] private bool generateNow = false;

        // Building bounds (Z from -12 to +20, center Z = +4)
        private const float SouthZ = -12f;
        private const float NorthZ = 20f;
        private const float CenterZ = 4f;
        private const float PartitionZ = 3f;

        // Main security doorway (x = 0..2 on PartitionZ)
        private const float DoorLeftX = 0f;
        private const float DoorWidth = 2f;
        private const float DoorHeight = 2.6f;

        private Dictionary<string, Material> materials;
        private Transform root;

        private void Awake()
        {
            if (Application.isPlaying)
            {
                PlacePlayActors();
            }
        }

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

        private void PlacePlayActors()
        {
            var player = GameObject.Find("Player");
            if (player != null)
            {
                var cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                player.transform.position = new Vector3(0.5f, 1.0f, -20.0f);
                player.transform.rotation = Quaternion.identity;
                if (cc != null) cc.enabled = true;
            }

            var terminal = GameObject.Find("TerminalDesk");
            if (terminal != null)
            {
                terminal.transform.position = new Vector3(0.5f, 0.6f, 14.5f);
                terminal.transform.rotation = Quaternion.identity;
                terminal.transform.localScale = new Vector3(1.6f, 1.1f, 0.9f);
            }

            var guard = GameObject.Find("Guard");
            if (guard != null)
            {
                guard.transform.position = new Vector3(2.5f, 1.0f, 10.0f);
                guard.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
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
            BuildOutdoorStreet();
            BuildLobby();
            BuildServerRoom();
            BuildLighting();
            BuildFloorDetail();
            ApplyFloorMaterial();
            FitDoorToDoorway();
        }

        // Spacious shell (28m x 32m x 5.8m) with front entrance, back-alley entrance, and crouch vent
        private void BuildShell()
        {
            var wall = Mat("wall", new Color(0.15f, 0.17f, 0.2f), 0.1f);
            var ceilingMat = Mat("ceiling", new Color(0.09f, 0.1f, 0.12f), 0.1f);
            var trim = Mat("trim", new Color(0.08f, 0.09f, 0.1f), 0.3f, 0.6f);
            var ventSign = Mat("ventsign", new Color(1f, 0.65f, 0.1f), 0.4f, 0f, 1.1f);
            var facadeSign = Mat("facadesign", new Color(0.25f, 0.7f, 0.85f), 0.6f, 0f, 1.3f);

            float h = wallHeight;
            float edge = roomWidth / 2f; // 14

            // North & West perimeter walls
            Prop(PrimitiveType.Cube, "NorthWall", new Vector3(0, h / 2f, NorthZ), new Vector3(roomWidth, h, wallThickness), wall);
            Prop(PrimitiveType.Cube, "WestWall", new Vector3(-edge, h / 2f, CenterZ), new Vector3(wallThickness, h, roomLength), wall);

            // East wall with Back Alley Maintenance Doorway (z = -5.5 .. -3.5)
            Prop(PrimitiveType.Cube, "EastWall_North", new Vector3(edge, h / 2f, 8.25f), new Vector3(wallThickness, h, 23.5f), wall);
            Prop(PrimitiveType.Cube, "EastWall_South", new Vector3(edge, h / 2f, -8.75f), new Vector3(wallThickness, h, 6.5f), wall);
            Prop(PrimitiveType.Cube, "EastAlley_Lintel", new Vector3(edge, 2.6f + (h - 2.6f) / 2f, -4.5f), new Vector3(wallThickness, h - 2.6f, 2.0f), wall);

            // South Facade (z = -12) with 5m wide Grand Front Entrance (x = -2.5 .. +2.5, height 3.6m)
            Prop(PrimitiveType.Cube, "SouthWall_Left", new Vector3(-8.25f, h / 2f, SouthZ), new Vector3(11.5f, h, wallThickness), wall);
            Prop(PrimitiveType.Cube, "SouthWall_Right", new Vector3(8.25f, h / 2f, SouthZ), new Vector3(11.5f, h, wallThickness), wall);
            Prop(PrimitiveType.Cube, "FrontEntrance_Lintel", new Vector3(0f, 3.6f + (h - 3.6f) / 2f, SouthZ), new Vector3(5.0f, h - 3.6f, wallThickness), wall);
            Prop(PrimitiveType.Cube, "FrontEntrance_Sign", new Vector3(0f, 4.15f, SouthZ - 0.23f), new Vector3(4.2f, 0.45f, 0.06f), facadeSign, false);
            Prop(PrimitiveType.Cube, "FrontPillarL", new Vector3(-2.6f, 1.8f, SouthZ), new Vector3(0.3f, 3.6f, 0.65f), trim);
            Prop(PrimitiveType.Cube, "FrontPillarR", new Vector3(2.6f, 1.8f, SouthZ), new Vector3(0.3f, 3.6f, 0.65f), trim);

            // Tall Ceiling Slab (collider off so camera never clips)
            Prop(PrimitiveType.Cube, "Ceiling", new Vector3(0, h + 0.1f, CenterZ), new Vector3(roomWidth, 0.2f, roomLength), ceilingMat, false);

            // Partition Wall (z = +3): Main Door (x = 0..2) + Crouch-Only Vent Crawlway (x = 11.2..13.2)
            Prop(PrimitiveType.Cube, "LeftPartition", new Vector3(-7f, h / 2f, PartitionZ), new Vector3(14f, h, wallThickness), wall);
            Prop(PrimitiveType.Cube, "MiddlePartition", new Vector3(6.6f, h / 2f, PartitionZ), new Vector3(9.2f, h, wallThickness), wall);
            Prop(PrimitiveType.Cube, "RightPartitionStub", new Vector3(13.6f, h / 2f, PartitionZ), new Vector3(0.8f, h, wallThickness), wall);

            float lintelH = h - DoorHeight;
            Prop(PrimitiveType.Cube, "DoorLintel", new Vector3(1f, DoorHeight + lintelH / 2f, PartitionZ), new Vector3(DoorWidth, lintelH, wallThickness), wall);
            Prop(PrimitiveType.Cube, "DoorFrameL", new Vector3(DoorLeftX - 0.06f, DoorHeight / 2f, PartitionZ), new Vector3(0.12f, DoorHeight, wallThickness + 0.1f), trim);
            Prop(PrimitiveType.Cube, "DoorFrameR", new Vector3(DoorLeftX + DoorWidth + 0.06f, DoorHeight / 2f, PartitionZ), new Vector3(0.12f, DoorHeight, wallThickness + 0.1f), trim);

            // Crouch-height maintenance opening (0 .. 1.35m open; wall above 1.35m .. 5.8m)
            float ventClearance = 1.35f;
            float ventWallH = h - ventClearance;
            Prop(PrimitiveType.Cube, "VentOverhang", new Vector3(12.2f, ventClearance + ventWallH / 2f, PartitionZ), new Vector3(2.0f, ventWallH, wallThickness), wall);
            Prop(PrimitiveType.Cube, "VentTrimL", new Vector3(11.16f, ventClearance / 2f, PartitionZ), new Vector3(0.08f, ventClearance, wallThickness + 0.08f), trim);
            Prop(PrimitiveType.Cube, "VentTrimR", new Vector3(13.24f, ventClearance / 2f, PartitionZ), new Vector3(0.08f, ventClearance, wallThickness + 0.08f), trim);
            Prop(PrimitiveType.Cube, "VentWarningStrip", new Vector3(12.2f, ventClearance + 0.08f, PartitionZ - 0.22f), new Vector3(1.8f, 0.12f, 0.04f), ventSign, false);

            // Baseboards along exterior side walls
            var board = Mat("baseboard", new Color(0.07f, 0.08f, 0.09f), 0.2f);
            Prop(PrimitiveType.Cube, "BaseboardW", new Vector3(-edge + 0.22f, 0.12f, CenterZ), new Vector3(0.05f, 0.24f, roomLength - 0.4f), board, false);
        }

        // Zone 1: Outdoor Meridian Street, courtyard, cyber-cars, alley dumpster, skyline (z = -30 .. -12)
        private void BuildOutdoorStreet()
        {
            var asphalt = Mat("asphalt", new Color(0.05f, 0.055f, 0.065f), 0.72f, 0.05f);
            var sidewalk = Mat("sidewalk", new Color(0.16f, 0.17f, 0.19f), 0.25f);
            var roadStripe = Mat("roadstripe", new Color(0.85f, 0.6f, 0.1f), 0.2f);
            var poleMat = Mat("pole", new Color(0.1f, 0.11f, 0.12f), 0.5f, 0.75f);
            var amberBulb = Mat("amberbulb", new Color(1f, 0.68f, 0.18f), 0.8f, 0f, 3.0f);
            var carBody1 = Mat("car1", new Color(0.08f, 0.09f, 0.11f), 0.75f, 0.5f);
            var carBody2 = Mat("car2", new Color(0.14f, 0.1f, 0.09f), 0.65f, 0.4f);
            var headLight = Mat("headlight", new Color(0.9f, 0.95f, 1f), 0.9f, 0f, 2.5f);
            var tailLight = Mat("taillight", new Color(1f, 0.12f, 0.1f), 0.9f, 0f, 2.2f);
            var dumpsterMat = Mat("dumpster", new Color(0.11f, 0.2f, 0.14f), 0.2f, 0.4f);
            var kioskMat = Mat("kiosk", new Color(0.1f, 0.12f, 0.15f), 0.4f, 0.6f);
            var kioskScreen = Mat("kioskscreen", new Color(1f, 0.6f, 0.15f), 0.8f, 0f, 1.5f);
            var skylineMat = Mat("skyline", new Color(0.04f, 0.045f, 0.055f), 0.1f);
            var crate = Mat("crate", new Color(0.3f, 0.22f, 0.12f), 0.15f);
            var glass = Mat("glassbottle", new Color(0.15f, 0.35f, 0.2f), 0.9f);

            // Sidewalk & wet street
            Prop(PrimitiveType.Cube, "Sidewalk", new Vector3(0f, 0.06f, -15.5f), new Vector3(44f, 0.12f, 7f), sidewalk);
            Prop(PrimitiveType.Cube, "AsphaltRoad", new Vector3(0f, 0.01f, -24.5f), new Vector3(52f, 0.02f, 11f), asphalt, false);

            // Dashed road center markings
            for (int i = -3; i <= 3; i++)
            {
                Prop(PrimitiveType.Cube, "RoadLine" + i, new Vector3(i * 6f, 0.025f, -24.5f), new Vector3(3.0f, 0.01f, 0.22f), roadStripe, false);
            }

            // Streetlights along sidewalk
            float[] lampX = { -11f, -3.2f, 8.5f };
            for (int i = 0; i < lampX.Length; i++)
            {
                float lx = lampX[i];
                Prop(PrimitiveType.Cylinder, "StreetPole" + i, new Vector3(lx, 2.75f, -18.5f), new Vector3(0.14f, 2.75f, 0.14f), poleMat);
                Prop(PrimitiveType.Cube, "StreetArm" + i, new Vector3(lx, 5.45f, -19.1f), new Vector3(0.12f, 0.12f, 1.4f), poleMat, false);
                Prop(PrimitiveType.Cube, "StreetBulb" + i, new Vector3(lx, 5.35f, -19.7f), new Vector3(0.32f, 0.08f, 0.22f), amberBulb, false);
                Light(new Vector3(lx, 5.1f, -19.7f), new Color(1f, 0.65f, 0.2f), 22f, 14f, "StreetLight_" + i, i == 2);
            }

            // 2 Parked Cyber-Sedans on the street
            BuildCyberCar("ParkedSedan_1", new Vector3(-9.5f, 0f, -21.8f), 6f, carBody1, headLight, tailLight);
            BuildCyberCar("ParkedSedan_2", new Vector3(11.0f, 0f, -27.2f), 184f, carBody2, headLight, tailLight);

            // Street Curfew Kiosk (Interactive!)
            var kiosk = Prop(PrimitiveType.Cube, "CurfewKiosk", new Vector3(-4.2f, 1.05f, -14.8f), new Vector3(0.9f, 2.1f, 0.55f), kioskMat);
            kiosk.AddComponent<InspectableObject>().Configure(
                "Read Curfew Terminal",
                "MERIDIAN TRANSIT // CURFEW NOTICE",
                "SECTOR 4 ALERT: Mandatory curfew 22:00 - 05:00.\nAll Drift anomalies flagged in Old Quarter will be purged by dawn.\n\nTIP: Main Annex front entrance is open, or check the East Alley maintenance side door.");
            Prop(PrimitiveType.Cube, "KioskDisplay", new Vector3(-4.2f, 1.35f, -15.09f), new Vector3(0.65f, 0.55f, 0.03f), kioskScreen, false);

            // East Back Alley Dumpster, Crates & Bottles leading to Side Doorway (x = +14, z = -4.5)
            Prop(PrimitiveType.Cube, "AlleyDumpster", new Vector3(16.8f, 0.65f, -8.5f), new Vector3(1.6f, 1.3f, 2.4f), dumpsterMat);
            Prop(PrimitiveType.Cube, "AlleyCrate", new Vector3(15.8f, 0.35f, -6.2f), new Vector3(0.7f, 0.7f, 0.7f), crate, true, true, 5f);
            Prop(PrimitiveType.Cylinder, "AlleyBottle", new Vector3(15.5f, 0.2f, -10.5f), new Vector3(0.14f, 0.2f, 0.14f), glass, true, true, 0.5f);
            Light(new Vector3(16.2f, 3.6f, -4.5f), new Color(1f, 0.68f, 0.22f), 11f, 10f, "AlleyDoorLight", true);

            // Sidewalk distraction bottles + Outdoor Street Patrol Guard (carries Vault Keycard!)
            Prop(PrimitiveType.Cylinder, "SidewalkBottle1", new Vector3(-2.5f, 0.25f, -16.8f), new Vector3(0.14f, 0.2f, 0.14f), glass, true, true, 0.5f);
            Prop(PrimitiveType.Cylinder, "SidewalkBottle2", new Vector3(4.5f, 0.25f, -16.8f), new Vector3(0.14f, 0.2f, 0.14f), glass, true, true, 0.5f);

            var enforcerMat = Mat("enforcer", new Color(0.08f, 0.09f, 0.11f), 0.35f, 0.3f);
            var guardVisorMat = Mat("guardvisor", new Color(0.9f, 0.15f, 0.12f), 0.6f, 0f, 1.6f);
            var streetGuard = Prop(PrimitiveType.Capsule, "StreetGuard", new Vector3(-7f, 1.0f, -15.5f), Vector3.one, enforcerMat);
            ChildProp(streetGuard.transform, PrimitiveType.Cube, "EnforcerVisor", new Vector3(0f, 0.52f, 0.42f), new Vector3(0.55f, 0.14f, 0.18f), guardVisorMat, false);
            var guardAI = streetGuard.AddComponent<GuardAI>();
            guardAI.ConfigureGuard(
                new Vector3[] { new Vector3(-7f, 1.0f, -15.5f), new Vector3(7f, 1.0f, -15.5f) },
                hasKeycard: true
            );

            // Distant brutalist skyline blocks framing the street so the world feels huge
            Prop(PrimitiveType.Cube, "SkylineBlock_W", new Vector3(-24f, 9f, -6f), new Vector3(8f, 18f, 38f), skylineMat);
            Prop(PrimitiveType.Cube, "SkylineBlock_E", new Vector3(24f, 10f, -6f), new Vector3(8f, 20f, 38f), skylineMat);
            Prop(PrimitiveType.Cube, "SkylineBlock_SW", new Vector3(-14f, 11f, -36f), new Vector3(14f, 22f, 8f), skylineMat);
            Prop(PrimitiveType.Cube, "SkylineBlock_SE", new Vector3(12f, 13f, -36f), new Vector3(16f, 26f, 8f), skylineMat);
            Prop(PrimitiveType.Cube, "StreetBarrierW", new Vector3(-20f, 0.6f, -22f), new Vector3(0.5f, 1.2f, 18f), poleMat);
            Prop(PrimitiveType.Cube, "StreetBarrierE", new Vector3(20f, 0.6f, -22f), new Vector3(0.5f, 1.2f, 18f), poleMat);
        }

        private void BuildCyberCar(string name, Vector3 pos, float yaw, Material bodyMat, Material headMat, Material tailMat)
        {
            var carRoot = new GameObject(name).transform;
            carRoot.SetParent(root);
            carRoot.position = pos;
            carRoot.rotation = Quaternion.Euler(0f, yaw, 0f);

            var wheelMat = Mat("tire", new Color(0.03f, 0.03f, 0.035f), 0.15f);

            // Lower chassis, angled cabin, 4 wheels, light bars
            ChildProp(carRoot, PrimitiveType.Cube, "Chassis", new Vector3(0f, 0.45f, 0f), new Vector3(1.9f, 0.55f, 4.3f), bodyMat);
            ChildProp(carRoot, PrimitiveType.Cube, "Cabin", new Vector3(0f, 0.92f, -0.2f), new Vector3(1.65f, 0.45f, 2.2f), bodyMat);
            ChildProp(carRoot, PrimitiveType.Cube, "HeadlightBar", new Vector3(0f, 0.52f, 2.16f), new Vector3(1.7f, 0.08f, 0.05f), headMat, false);
            ChildProp(carRoot, PrimitiveType.Cube, "TaillightBar", new Vector3(0f, 0.54f, -2.16f), new Vector3(1.7f, 0.08f, 0.05f), tailMat, false);

            Vector3[] wheels =
            {
                new Vector3(-0.92f, 0.28f, 1.35f), new Vector3(0.92f, 0.28f, 1.35f),
                new Vector3(-0.92f, 0.28f, -1.35f), new Vector3(0.92f, 0.28f, -1.35f)
            };
            foreach (var w in wheels)
            {
                ChildProp(carRoot, PrimitiveType.Cylinder, "Wheel", w, new Vector3(0.56f, 0.14f, 0.56f), wheelMat, false, new Vector3(0, 0, 90));
            }
        }

        // Zone 2: Spacious Grand Lobby (z = -12 .. +3, x = -14 .. +14)
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

            // West Wing: L-shaped Reception Desk + Monitors + Filing Cabinets + Lobby Locker
            var desk = Prop(PrimitiveType.Cube, "ReceptionDesk", new Vector3(-8.5f, 0.55f, -5.5f), new Vector3(4.2f, 1.1f, 0.85f), wood);
            desk.AddComponent<InspectableObject>().Configure(
                "Check Reception Log",
                "RECEPTION NIGHT LOG // 23:40",
                "CLERK NOTE: Patrol guard on Vault Duty is jittery tonight — keeps jumping at every rattle in the hallway.\n\nIf the main server door locks again, check the yellow breaker panel beside the partition door—or slip through the East Maintenance Vent.");
            Prop(PrimitiveType.Cube, "ReceptionReturn", new Vector3(-10.45f, 0.55f, -4.0f), new Vector3(0.85f, 1.1f, 3.2f), wood);
            Prop(PrimitiveType.Cube, "DeskTop", new Vector3(-8.5f, 1.12f, -5.5f), new Vector3(4.3f, 0.05f, 0.95f), darkMetal, false);
            Prop(PrimitiveType.Cube, "Monitor1", new Vector3(-9.2f, 1.45f, -5.45f), new Vector3(0.6f, 0.38f, 0.04f), screen, false);
            Prop(PrimitiveType.Cube, "Monitor2", new Vector3(-7.8f, 1.45f, -5.45f), new Vector3(0.6f, 0.38f, 0.04f), screen, false);

            for (int i = 0; i < 4; i++)
            {
                var cab = Prop(PrimitiveType.Cube, "FilingCabinet" + i, new Vector3(-12.5f + i * 0.95f, 0.9f, -11.4f), new Vector3(0.85f, 1.8f, 0.55f), metal);
                if (i == 0)
                {
                    cab.AddComponent<InspectableObject>().Configure(
                        "Search Citizen Archives",
                        "ORACLE DRIFT WATCHLIST // DRAWER A",
                        "FLAGGED SUBJECT: KAEL VARN (#992)\nSYNC VARIANCE: CRITICAL\nORDER: Purge Team dispatched for 06:00.\n\nNOTE IN MARGIN: Older records moved to Server Vault terminal.");
                }
            }

            // Lobby Storage Locker (West Wall)
            var lobbyLocker = Prop(PrimitiveType.Cube, "LobbyLocker", new Vector3(-13.2f, 1.15f, -1.5f), new Vector3(0.9f, 2.3f, 0.9f), metal);
            lobbyLocker.AddComponent<HidingSpot>().Configure("Lobby Locker", new Vector3(1.1f, 0f, 0f));

            // East Wing: Waiting lounge, Memo, Vending Machine
            Prop(PrimitiveType.Cube, "Bench1", new Vector3(8.5f, 0.25f, -8.5f), new Vector3(3.4f, 0.5f, 0.65f), fabric);
            Prop(PrimitiveType.Cube, "Bench2", new Vector3(8.5f, 0.25f, -5.5f), new Vector3(3.4f, 0.5f, 0.65f), fabric);
            Prop(PrimitiveType.Cube, "CoffeeTable", new Vector3(8.5f, 0.2f, -7.0f), new Vector3(1.3f, 0.4f, 0.65f), wood);
            var memoMat = Mat("memo", new Color(0.92f, 0.88f, 0.78f), 0.1f);
            var memo = Prop(PrimitiveType.Cube, "SecurityMemo", new Vector3(8.5f, 0.42f, -7.0f), new Vector3(0.32f, 0.03f, 0.42f), memoMat);
            memo.AddComponent<InspectableObject>().Configure(
                "Read Maintenance Memo",
                "MAINTENANCE MEMO // TWO WAYS INTO VAULT",
                "1) Magnetic Door failsafe: cutting the yellow Breaker Box unlocks the main door.\n2) East Maintenance Vent: service crawlway along the far right wall leads straight behind the server racks (Crouch [C] to slip through).");

            var vend = Prop(PrimitiveType.Cube, "VendingMachine", new Vector3(13.2f, 1.1f, -0.5f), new Vector3(0.9f, 2.2f, 1f), darkMetal);
            vend.AddComponent<InspectableObject>().Configure("Dispense Soda Can", "", "", true);
            Prop(PrimitiveType.Cube, "VendingFront", new Vector3(12.73f, 1.3f, -0.5f), new Vector3(0.04f, 1.3f, 0.75f), vendGlow, false);

            // 4 Atrium Concrete Pillars (wide spacing)
            var pillarMat = Mat("pillar", new Color(0.11f, 0.12f, 0.14f), 0.1f);
            Prop(PrimitiveType.Cube, "Pillar_NW", new Vector3(-5.2f, wallHeight / 2f, -1.5f), new Vector3(0.8f, wallHeight, 0.8f), pillarMat);
            Prop(PrimitiveType.Cube, "Pillar_NE", new Vector3(5.2f, wallHeight / 2f, -1.5f), new Vector3(0.8f, wallHeight, 0.8f), pillarMat);
            Prop(PrimitiveType.Cube, "Pillar_SW", new Vector3(-5.2f, wallHeight / 2f, -8.5f), new Vector3(0.8f, wallHeight, 0.8f), pillarMat);
            Prop(PrimitiveType.Cube, "Pillar_SE", new Vector3(5.2f, wallHeight / 2f, -8.5f), new Vector3(0.8f, wallHeight, 0.8f), pillarMat);

            // Potted plant
            Prop(PrimitiveType.Cylinder, "PlantPot", new Vector3(3.6f, 0.3f, -10.8f), new Vector3(0.6f, 0.3f, 0.6f), pot);
            Prop(PrimitiveType.Sphere, "PlantLeaves", new Vector3(3.6f, 0.95f, -10.8f), new Vector3(0.85f, 0.85f, 0.85f), leaf, false);

            // Loose physics props (crates, bottles, bin)
            Prop(PrimitiveType.Cube, "Crate1", new Vector3(-6.2f, 0.4f, -1.5f), new Vector3(0.8f, 0.8f, 0.8f), crate, true, true, 6f);
            Prop(PrimitiveType.Cube, "Crate2", new Vector3(-5.2f, 0.3f, -0.8f), new Vector3(0.6f, 0.6f, 0.6f), crate, true, true, 4f);
            Prop(PrimitiveType.Cylinder, "Bottle1", new Vector3(-6.5f, 0.2f, -5.5f), new Vector3(0.14f, 0.2f, 0.14f), glass, true, true, 0.5f);
            Prop(PrimitiveType.Cylinder, "Bottle2", new Vector3(-6.2f, 0.2f, -5.2f), new Vector3(0.14f, 0.2f, 0.14f), glass, true, true, 0.5f);
            Prop(PrimitiveType.Cylinder, "DoorBottle1", new Vector3(2.2f, 0.2f, 1.5f), new Vector3(0.14f, 0.2f, 0.14f), glass, true, true, 0.5f);
            Prop(PrimitiveType.Cylinder, "DoorBottle2", new Vector3(-1.5f, 0.2f, 1.8f), new Vector3(0.14f, 0.2f, 0.14f), glass, true, true, 0.5f);
            Prop(PrimitiveType.Cylinder, "Bin", new Vector3(-11.2f, 0.3f, -7.2f), new Vector3(0.45f, 0.3f, 0.45f), metal, true, true, 3f);

            // Overhead lobby pipes (high at y = 5.3m, no collision)
            var pipe = Mat("pipe", new Color(0.12f, 0.13f, 0.15f), 0.4f, 0.7f);
            Prop(PrimitiveType.Cylinder, "Pipe1", new Vector3(0f, 5.3f, -4f), new Vector3(0.16f, roomWidth / 2f - 0.3f, 0.16f), pipe, false, false, 0f, new Vector3(0, 0, 90));
            Prop(PrimitiveType.Cylinder, "Pipe2", new Vector3(0f, 5.1f, -8f), new Vector3(0.12f, roomWidth / 2f - 0.3f, 0.12f), pipe, false, false, 0f, new Vector3(0, 0, 90));
        }

        // Zone 4: Vast Server Archive Vault (z = +3 .. +20, x = -14 .. +14)
        private void BuildServerRoom()
        {
            var rack = Mat("rack", new Color(0.06f, 0.07f, 0.08f), 0.5f, 0.6f);
            var metal = Mat("metal", Color.gray);
            var ledGreen = Mat("ledgreen", new Color(0.1f, 1f, 0.3f), 0.5f, 0f, 2.5f);
            var ledBlue = Mat("ledblue", new Color(0.2f, 0.5f, 1f), 0.5f, 0f, 2.5f);
            var cable = Mat("cable", new Color(0.04f, 0.04f, 0.05f), 0.3f);
            var hazard = Mat("hazard", new Color(0.9f, 0.7f, 0.05f), 0.2f);
            var exitFrame = Mat("exitframe", new Color(0.1f, 0.11f, 0.12f), 0.3f, 0.6f);
            var exitSign = Mat("exitsign", new Color(0.1f, 0.8f, 0.3f), 0.4f, 0f, 1.2f);
            var crate = Mat("crate", new Color(0.3f, 0.22f, 0.12f), 0.15f);

            // 6 Server racks in two wide rows with open flanking aisles
            Vector3[] rackPos =
            {
                new Vector3(-7.5f, 1.6f, 7.5f),
                new Vector3(-4.0f, 1.6f, 7.5f),
                new Vector3(5.5f, 1.6f, 7.5f),
                new Vector3(-7.5f, 1.6f, 13.5f),
                new Vector3(-4.0f, 1.6f, 13.5f),
                new Vector3(5.5f, 1.6f, 13.5f)
            };

            for (int r = 0; r < rackPos.Length; r++)
            {
                Prop(PrimitiveType.Cube, "ServerRack_0" + (r + 1), rackPos[r], new Vector3(1.2f, 3.2f, 3.8f), rack);

                for (int i = 0; i < 7; i++)
                {
                    float y = 0.55f + i * 0.38f;
                    Material led = (i + r) % 2 == 0 ? ledGreen : ledBlue;
                    Prop(PrimitiveType.Cube, "LED", new Vector3(rackPos[r].x + 0.62f, y, rackPos[r].z - 1.2f + (i % 3) * 1.2f), new Vector3(0.04f, 0.06f, 0.18f), led, false);
                    Prop(PrimitiveType.Cube, "LED", new Vector3(rackPos[r].x - 0.62f, y, rackPos[r].z - 1.2f + ((i + 1) % 3) * 1.2f), new Vector3(0.04f, 0.06f, 0.18f), led, false);
                }
            }

            // Floor cable trunking & overhead cable trays
            Prop(PrimitiveType.Cube, "FloorCable1", new Vector3(-2.5f, 0.03f, 14.5f), new Vector3(10f, 0.06f, 0.2f), cable, false);
            Prop(PrimitiveType.Cube, "FloorCable2", new Vector3(-2.0f, 0.03f, 10.5f), new Vector3(0.2f, 0.06f, 8.2f), cable, false);
            Prop(PrimitiveType.Cube, "CableTray1", new Vector3(-5.5f, 4.9f, 10.5f), new Vector3(0.6f, 0.14f, 13f), cable, false);
            Prop(PrimitiveType.Cube, "CableTray2", new Vector3(4.5f, 4.9f, 10.5f), new Vector3(0.6f, 0.14f, 13f), cable, false);

            // Pushable cover crate inside doorway
            Prop(PrimitiveType.Cube, "CoverCrate", new Vector3(-0.5f, 0.45f, 5.2f), new Vector3(0.9f, 0.9f, 0.9f), crate, true, true, 8f);

            // 2 Hideable Vault Storage Lockers
            var eastLocker = Prop(PrimitiveType.Cube, "ServerLocker_East", new Vector3(13.2f, 1.15f, 9.0f), new Vector3(0.9f, 2.3f, 0.9f), metal);
            eastLocker.AddComponent<HidingSpot>().Configure("East Vault Locker", new Vector3(-1.1f, 0f, 0f));

            var westLocker = Prop(PrimitiveType.Cube, "ServerLocker_West", new Vector3(-13.2f, 1.15f, 13.5f), new Vector3(0.9f, 2.3f, 0.9f), metal);
            westLocker.AddComponent<HidingSpot>().Configure("West Vault Locker", new Vector3(1.1f, 0f, 0f));

            // Fire exit frame, hazard stripe & sign on expanded North wall (z = +20)
            Prop(PrimitiveType.Cube, "ExitDoorFrame", new Vector3(-6f, 1.35f, NorthZ - 0.35f), new Vector3(2.3f, 2.7f, 0.1f), exitFrame, false);
            Prop(PrimitiveType.Cube, "ExitHazardStripe", new Vector3(-6f, 0.012f, NorthZ - 1.3f), new Vector3(2.4f, 0.02f, 0.35f), hazard, false);
            Prop(PrimitiveType.Cube, "ExitSign", new Vector3(-6f, 3.2f, NorthZ - 0.3f), new Vector3(1.2f, 0.28f, 0.05f), exitSign, false);
        }

        private void BuildLighting()
        {
            // Lobby lights (elevated for 5.8m ceilings)
            Light(new Vector3(0f, 5.1f, -3f), new Color(1.0f, 0.7f, 0.2f), 26f, 16f, "AmberLobbyLight", true);
            Light(new Vector3(-8.5f, 5.1f, -5.5f), new Color(1.0f, 0.75f, 0.35f), 20f, 14f, "ReceptionLight", true);
            Light(new Vector3(8.5f, 5.1f, -6.5f), new Color(1.0f, 0.65f, 0.3f), 16f, 13f, "WaitingLight", false);
            Light(new Vector3(11.8f, 2.0f, -0.5f), new Color(0.7f, 0.9f, 1f), 7f, 6f, "VendingGlowLight", false);
            Light(new Vector3(12.2f, 2.2f, 2.0f), new Color(1.0f, 0.65f, 0.15f), 6f, 5f, "VentHintLight", false);

            // Server vault lights
            Light(new Vector3(0f, 5.2f, 8.5f), new Color(0.35f, 0.65f, 1.0f), 24f, 16f, "ServerRoomLight", true);
            Light(new Vector3(0f, 5.2f, 15.5f), new Color(0.3f, 0.55f, 0.95f), 22f, 16f, "ServerBackLight", false);
            Light(new Vector3(-6f, 2.8f, NorthZ - 0.9f), new Color(0.2f, 1f, 0.4f), 4f, 4.5f, "ExitSignGlow", false);
        }

        // Carpet runner & rain puddles
        private void BuildFloorDetail()
        {
            var runner = Mat("runner", new Color(0.16f, 0.05f, 0.06f), 0.05f);
            Prop(PrimitiveType.Cube, "HallwayRunner", new Vector3(0.5f, 0.011f, -4.5f), new Vector3(3.2f, 0.02f, 14.5f), runner, false);

            var puddle = Mat("puddle", new Color(0.05f, 0.07f, 0.1f), 0.97f, 0.4f);
            Prop(PrimitiveType.Cylinder, "Puddle1", new Vector3(-2.5f, 0.02f, -21f), new Vector3(2.4f, 0.005f, 1.7f), puddle, false);
            Prop(PrimitiveType.Cylinder, "Puddle2", new Vector3(4.5f, 0.02f, -23f), new Vector3(1.9f, 0.005f, 2.2f), puddle, false);
            Prop(PrimitiveType.Cylinder, "Puddle3", new Vector3(3.0f, 0.006f, -3.5f), new Vector3(1.3f, 0.005f, 1.1f), puddle, false);
            Prop(PrimitiveType.Cylinder, "Puddle4", new Vector3(1.5f, 0.006f, 9.5f), new Vector3(1.4f, 0.005f, 1.0f), puddle, false);
        }

        // Expand Ground to cover street + courtyard + entire annex (90m x 90m)
        private void ApplyFloorMaterial()
        {
            var ground = GameObject.Find("Ground");
            if (ground == null) return;

            ground.transform.position = new Vector3(0f, 0f, -5f);
            ground.transform.localScale = new Vector3(9f, 1f, 9f);

            var rend = ground.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                rend.sharedMaterial = Mat("floor", new Color(0.11f, 0.12f, 0.14f), 0.55f, 0.1f);
            }
        }

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

                var pp = go.AddComponent<PhysicsProp>();
                bool glass = name.Contains("Bottle");
                bool isMetal = name == "Bin";
                string displayName = glass ? "Glass Bottle" : (isMetal ? "Metal Bin" : "Crate");
                pp.Configure(displayName, glass, isMetal);
            }
            return go;
        }

        private void ChildProp(Transform parent, PrimitiveType type, string name, Vector3 localPos, Vector3 localScale,
                               Material mat, bool collider = true, Vector3 localEuler = default)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.transform.localRotation = Quaternion.Euler(localEuler);
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            if (!collider)
            {
                var c = go.GetComponent<Collider>();
                if (c != null) SafeDestroy(c);
            }
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

            if (flicker) obj.AddComponent<FlickerLight>();
        }

        private static void SafeDestroy(Object o)
        {
            if (Application.isPlaying) Destroy(o);
            else DestroyImmediate(o);
        }
    }
}
