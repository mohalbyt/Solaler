using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Solaler.Dungeon;
using Solaler.Dungeon.Core;

namespace Solaler.Editor
{
    /// <summary>
    /// Automated scene construction tool that assembles the complete 3-room dungeon,
    /// player character, camera, lights, walls, doorways, and puzzle mechanics.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneSetupTool
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";
        private const string SpritePath = "Assets/Sprites/Square.png";

        static SceneSetupTool()
        {
            EditorApplication.delayCall += CheckAndRebuildIfEmpty;
        }

        private static void CheckAndRebuildIfEmpty()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path == ScenePath && activeScene.rootCount <= 1)
            {
                Debug.Log("[SceneSetupTool] Empty scene detected. Automatically reconstructing complete dungeon scene...");
                RebuildCompleteScene();
            }
        }

        [MenuItem("Solaler/Rebuild Complete Scene", priority = 10)]
        public static void RebuildCompleteScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            // 1. Clear any leftover root objects
            var roots = scene.GetRootGameObjects();
            foreach (var go in roots)
            {
                Object.DestroyImmediate(go);
            }

            // 2. Load or create square sprite for clean visuals
            Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            Material litMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat");

            // 3. Setup Main Camera
            GameObject cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            Camera cam = cameraGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.11f, 1f);
            cameraGo.AddComponent<UniversalAdditionalCameraData>();

            // 4. Setup Global Light 2D
            GameObject lightGo = new GameObject("Global Light 2D");
            Light2D globalLight = lightGo.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.color = Color.white;
            globalLight.intensity = 1.0f;

            // 5. Instantiate Player
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            GameObject playerGo = null;
            if (playerPrefab != null)
            {
                playerGo = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerGo.name = "Player";
                playerGo.tag = "Player";
                playerGo.transform.position = Vector3.zero;
            }
            else
            {
                Debug.LogWarning("[SceneSetupTool] Player prefab not found at " + PlayerPrefabPath);
            }

            // 6. Assemble Rooms
            // Room 1: Starting Room (0, 0)
            Room room1 = CreateRoom("Room_Start", new Vector2Int(0, 0), Vector3.zero, new Vector2(18f, 10f), squareSprite, litMaterial);
            CreateWall(room1.gameObject, "Wall_North_Left", new Vector3(-5.5f, 5.2f, 0f), new Vector2(8f, 0.8f), squareSprite, litMaterial);
            CreateWall(room1.gameObject, "Wall_North_Right", new Vector3(5.5f, 5.2f, 0f), new Vector2(8f, 0.8f), squareSprite, litMaterial);
            CreateWall(room1.gameObject, "Wall_South", new Vector3(0f, -5.2f, 0f), new Vector2(19f, 0.8f), squareSprite, litMaterial);
            CreateWall(room1.gameObject, "Wall_West", new Vector3(-9.2f, 0f, 0f), new Vector2(0.8f, 11f), squareSprite, litMaterial);
            CreateWall(room1.gameObject, "Wall_East_Upper", new Vector3(9.2f, 3.5f, 0f), new Vector2(0.8f, 4.5f), squareSprite, litMaterial);
            CreateWall(room1.gameObject, "Wall_East_Lower", new Vector3(9.2f, -3.5f, 0f), new Vector2(0.8f, 4.5f), squareSprite, litMaterial);

            Doorway door1North = CreateDoorway(room1, "Door_North", Direction.North, new Vector3(0f, 5f, 0f), new Vector2(2f, 0.8f), squareSprite, litMaterial);
            Doorway door1East = CreateDoorway(room1, "Door_East", Direction.East, new Vector3(9f, 0f, 0f), new Vector2(0.8f, 2f), squareSprite, litMaterial);
            room1.AddDoorway(door1North);
            room1.AddDoorway(door1East);
            room1.Configure(new Vector2Int(0, 0), new Vector2(18f, 10f), null);

            // Room 2: North Puzzle Room (0, 14)
            Room room2 = CreateRoom("Room_NorthPuzzle", new Vector2Int(0, 1), new Vector3(0f, 14f, 0f), new Vector2(18f, 10f), squareSprite, litMaterial);
            CreateWall(room2.gameObject, "Wall_North", new Vector3(0f, 19.2f, 0f), new Vector2(19f, 0.8f), squareSprite, litMaterial);
            CreateWall(room2.gameObject, "Wall_South_Left", new Vector3(-5.5f, 8.8f, 0f), new Vector2(8f, 0.8f), squareSprite, litMaterial);
            CreateWall(room2.gameObject, "Wall_South_Right", new Vector3(5.5f, 8.8f, 0f), new Vector2(8f, 0.8f), squareSprite, litMaterial);
            CreateWall(room2.gameObject, "Wall_West", new Vector3(-9.2f, 14f, 0f), new Vector2(0.8f, 11f), squareSprite, litMaterial);
            CreateWall(room2.gameObject, "Wall_East", new Vector3(9.2f, 14f, 0f), new Vector2(0.8f, 11f), squareSprite, litMaterial);

            Doorway door2South = CreateDoorway(room2, "Door_South", Direction.South, new Vector3(0f, 9f, 0f), new Vector2(2f, 0.8f), squareSprite, litMaterial);
            room2.AddDoorway(door2South);

            // Connect Room 1 North <-> Room 2 South
            door1North.SetConnectedDoorway(door2South);
            door2South.SetConnectedDoorway(door1North);

            // Puzzle in Room 2: 2 Pressure Plates + 1 Pushable Block
            GameObject puzzleGo = new GameObject("Puzzle_PressurePlates");
            puzzleGo.transform.SetParent(room2.transform);
            puzzleGo.transform.localPosition = Vector3.zero;
            PuzzleController puzzle = puzzleGo.AddComponent<PuzzleController>();

            PressurePlate plateLeft = CreatePressurePlate(puzzleGo, "PressurePlate_Left", new Vector3(-3.5f, 14f, 0f), new Vector2(1.6f, 1.6f), squareSprite, litMaterial);
            PressurePlate plateRight = CreatePressurePlate(puzzleGo, "PressurePlate_Right", new Vector3(3.5f, 14f, 0f), new Vector2(1.6f, 1.6f), squareSprite, litMaterial);
            puzzle.Configure(new List<PressurePlate> { plateLeft, plateRight });

            CreatePushableBlock(room2.gameObject, "PushableBlock", new Vector3(0f, 14f, 0f), new Vector2(1.3f, 1.3f), squareSprite, litMaterial);
            room2.Configure(new Vector2Int(0, 1), new Vector2(18f, 10f), puzzle);

            // Room 3: East Room (24, 0)
            Room room3 = CreateRoom("Room_East", new Vector2Int(1, 0), new Vector3(24f, 0f, 0f), new Vector2(18f, 10f), squareSprite, litMaterial);
            CreateWall(room3.gameObject, "Wall_North", new Vector3(24f, 5.2f, 0f), new Vector2(19f, 0.8f), squareSprite, litMaterial);
            CreateWall(room3.gameObject, "Wall_South", new Vector3(24f, -5.2f, 0f), new Vector2(19f, 0.8f), squareSprite, litMaterial);
            CreateWall(room3.gameObject, "Wall_East", new Vector3(33.2f, 0f, 0f), new Vector2(0.8f, 11f), squareSprite, litMaterial);
            CreateWall(room3.gameObject, "Wall_West_Upper", new Vector3(14.8f, 3.5f, 0f), new Vector2(0.8f, 4.5f), squareSprite, litMaterial);
            CreateWall(room3.gameObject, "Wall_West_Lower", new Vector3(14.8f, -3.5f, 0f), new Vector2(0.8f, 4.5f), squareSprite, litMaterial);

            Doorway door3West = CreateDoorway(room3, "Door_West", Direction.West, new Vector3(15f, 0f, 0f), new Vector2(0.8f, 2f), squareSprite, litMaterial);
            room3.AddDoorway(door3West);

            // Connect Room 1 East <-> Room 3 West
            door1East.SetConnectedDoorway(door3West);
            door3West.SetConnectedDoorway(door1East);
            room3.Configure(new Vector2Int(1, 0), new Vector2(18f, 10f), null);

            // 7. Setup DungeonManager
            GameObject managerGo = new GameObject("DungeonManager");
            DungeonManager manager = managerGo.AddComponent<DungeonManager>();
            manager.Configure(room1, new List<Room> { room1, room2, room3 }, cam, 0.35f);

            // 8. Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("<color=green>[SceneSetupTool] Successfully reconstructed and saved SampleScene with all 3 rooms, player, camera, lights, and puzzle elements!</color>");
        }

        private static Room CreateRoom(string name, Vector2Int coords, Vector3 position, Vector2 size, Sprite sprite, Material mat)
        {
            GameObject roomGo = new GameObject(name);
            roomGo.transform.position = position;
            Room room = roomGo.AddComponent<Room>();

            // Floor visual
            GameObject floorGo = new GameObject("Floor");
            floorGo.transform.SetParent(roomGo.transform);
            floorGo.transform.localPosition = Vector3.zero;
            floorGo.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer floorSr = floorGo.AddComponent<SpriteRenderer>();
            floorSr.sprite = sprite;
            if (mat != null) floorSr.material = mat;
            floorSr.color = new Color(0.14f, 0.14f, 0.18f, 1f);
            floorSr.sortingOrder = -10;

            return room;
        }

        private static void CreateWall(GameObject parent, string name, Vector3 worldPos, Vector2 size, Sprite sprite, Material mat)
        {
            GameObject wallGo = new GameObject(name);
            wallGo.transform.position = worldPos;
            wallGo.transform.SetParent(parent.transform);

            BoxCollider2D col = wallGo.AddComponent<BoxCollider2D>();
            col.size = size;

            // Visual
            wallGo.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer sr = wallGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            if (mat != null) sr.material = mat;
            sr.color = new Color(0.28f, 0.29f, 0.35f, 1f);
            sr.sortingOrder = 0;
        }

        private static Doorway CreateDoorway(Room room, string name, Direction dir, Vector3 worldPos, Vector2 size, Sprite sprite, Material mat)
        {
            GameObject doorGo = new GameObject(name);
            doorGo.transform.position = worldPos;
            doorGo.transform.SetParent(room.transform);

            // Trigger collider
            BoxCollider2D triggerCol = doorGo.AddComponent<BoxCollider2D>();
            triggerCol.size = size;
            triggerCol.isTrigger = true;

            // Doorway component
            Doorway doorway = doorGo.AddComponent<Doorway>();

            // Barrier collider (child)
            GameObject barrierGo = new GameObject("Barrier");
            barrierGo.transform.SetParent(doorGo.transform);
            barrierGo.transform.localPosition = Vector3.zero;
            BoxCollider2D barrierCol = barrierGo.AddComponent<BoxCollider2D>();
            barrierCol.size = size;
            barrierCol.enabled = false;

            // Visual sprite (child)
            GameObject visualGo = new GameObject("Visual");
            visualGo.transform.SetParent(doorGo.transform);
            visualGo.transform.localPosition = Vector3.zero;
            visualGo.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer sr = visualGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            if (mat != null) sr.material = mat;
            sr.color = new Color(0.2f, 0.8f, 0.3f, 0.9f); // Open green
            sr.sortingOrder = 5;

            doorway.Configure(dir, room, barrierCol, sr);
            return doorway;
        }

        private static PressurePlate CreatePressurePlate(GameObject parent, string name, Vector3 worldPos, Vector2 size, Sprite sprite, Material mat)
        {
            GameObject plateGo = new GameObject(name);
            plateGo.transform.position = worldPos;
            plateGo.transform.SetParent(parent.transform);

            BoxCollider2D col = plateGo.AddComponent<BoxCollider2D>();
            col.size = size;
            col.isTrigger = true;

            plateGo.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer sr = plateGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            if (mat != null) sr.material = mat;
            sr.color = new Color(0.3f, 0.6f, 0.75f, 1f);
            sr.sortingOrder = -5;

            PressurePlate plate = plateGo.AddComponent<PressurePlate>();
            plate.Configure(sr, true);
            return plate;
        }

        private static void CreatePushableBlock(GameObject parent, string name, Vector3 worldPos, Vector2 size, Sprite sprite, Material mat)
        {
            GameObject blockGo = new GameObject(name);
            blockGo.transform.position = worldPos;
            blockGo.transform.SetParent(parent.transform);

            BoxCollider2D col = blockGo.AddComponent<BoxCollider2D>();
            col.size = size;

            Rigidbody2D rb = blockGo.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.linearDamping = 12f;
            rb.angularDamping = 10f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            blockGo.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer sr = blockGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            if (mat != null) sr.material = mat;
            sr.color = new Color(0.85f, 0.65f, 0.25f, 1f); // Bronze/Gold block
            sr.sortingOrder = 2;

            blockGo.AddComponent<PushableBlock>();
        }
    }
}
