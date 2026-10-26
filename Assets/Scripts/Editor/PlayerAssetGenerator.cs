using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using Solaler.Player;
using Solaler.Player.Animation;
using Solaler.Player.Combat;
using Solaler.Player.Input;
using Solaler.Player.Movement;

namespace Solaler.Editor
{
    /// <summary>
    /// Editor automation tool that generates top-down animation clips, animator controller,
    /// projectile prefab, movement data, and the Player prefab (Isaac style).
    /// </summary>
    public static class PlayerAssetGenerator
    {
        private const string SpriteSheetPath = "Assets/Sprites/PlayerSpriteSheet.png";
        private const string AnimationsDir = "Assets/Animations";
        private const string PrefabsDir = "Assets/Prefabs";
        private const string SettingsDir = "Assets/Settings";

        [MenuItem("Solaler/Generate Top-Down Player & Assets", priority = 1)]
        public static void GenerateAll()
        {
            EnsureDirectories();

            Dictionary<string, Sprite> sprites = LoadSpriteSheet();
            if (sprites == null || sprites.Count == 0)
            {
                Debug.LogError("[PlayerAssetGenerator] Failed to find sprites in " + SpriteSheetPath);
                return;
            }

            // 1. Generate Animation Clips
            AnimationClip idleClip = CreateSpriteClip("Player_Idle", 6, true, new[]
            {
                GetSprite(sprites, "14-Sheet_16"),
                GetSprite(sprites, "14-Sheet_17"),
                GetSprite(sprites, "14-Sheet_18")
            });

            AnimationClip moveClip = CreateSpriteClip("Player_Move", 10, true, new[]
            {
                GetSprite(sprites, "14-Sheet_9"),
                GetSprite(sprites, "14-Sheet_10"),
                GetSprite(sprites, "14-Sheet_11"),
                GetSprite(sprites, "14-Sheet_12"),
                GetSprite(sprites, "14-Sheet_13"),
                GetSprite(sprites, "14-Sheet_14")
            });

            AnimationClip dashClip = CreateSpriteClip("Player_Dash", 2, true, new[]
            {
                GetSprite(sprites, "14-Sheet_20")
            });

            // Death animation uses the shattering frames
            AnimationClip dieClip = CreateSpriteClip("Player_Die", 10, false, new[]
            {
                GetSprite(sprites, "14-Sheet_22"),
                GetSprite(sprites, "14-Sheet_26"),
                GetSprite(sprites, "14-Sheet_30"),
                GetSprite(sprites, "14-Sheet_32")
            });

            AssetDatabase.SaveAssets();

            // 2. Generate Animator Controller
            AnimatorController controller = CreateAnimatorController(idleClip, moveClip, dashClip, dieClip);

            // 3. Generate Movement Data ScriptableObject
            PlayerMovementData movementData = CreateMovementDataAsset();

            // 4. Generate Projectile Prefab
            GameObject projectilePrefab = CreateProjectilePrefab(GetSprite(sprites, "14-Sheet_21"));

            // 5. Generate Player Prefab
            CreatePlayerPrefab(controller, movementData, projectilePrefab, GetSprite(sprites, "14-Sheet_16"));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green>[PlayerAssetGenerator] Top-down player, animations, projectile, and prefab created successfully!</color>");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder(AnimationsDir)) AssetDatabase.CreateFolder("Assets", "Animations");
            if (!AssetDatabase.IsValidFolder(PrefabsDir)) AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder(SettingsDir)) AssetDatabase.CreateFolder("Assets", "Settings");
        }

        private static Dictionary<string, Sprite> LoadSpriteSheet()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(SpriteSheetPath);
            var dict = new Dictionary<string, Sprite>();
            foreach (Object asset in assets)
            {
                if (asset is Sprite s)
                {
                    dict[s.name] = s;
                }
            }
            return dict;
        }

        private static Sprite GetSprite(Dictionary<string, Sprite> dict, string name)
        {
            if (dict.TryGetValue(name, out Sprite s)) return s;
            Debug.LogWarning($"[PlayerAssetGenerator] Sprite '{name}' not found in sheet.");
            return null;
        }

        private static AnimationClip CreateSpriteClip(string clipName, int frameRate, bool loop, Sprite[] frames)
        {
            string path = $"{AnimationsDir}/{clipName}.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            clip.frameRate = frameRate;

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            var validFrames = new List<Sprite>();
            foreach (var f in frames)
            {
                if (f != null) validFrames.Add(f);
            }

            if (validFrames.Count > 0)
            {
                ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[validFrames.Count];
                for (int i = 0; i < validFrames.Count; i++)
                {
                    keyframes[i] = new ObjectReferenceKeyframe
                    {
                        time = (float)i / frameRate,
                        value = validFrames[i]
                    };
                }

                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            }

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimatorController CreateAnimatorController(
            AnimationClip idle, AnimationClip move, AnimationClip dash, AnimationClip die)
        {
            string path = $"{AnimationsDir}/PlayerAnimatorController.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            }

            while (controller.parameters.Length > 0)
            {
                controller.RemoveParameter(0);
            }

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsDashing", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

            var rootStateMachine = controller.layers[0].stateMachine;

            var existingStates = rootStateMachine.states;
            foreach (var childState in existingStates)
            {
                rootStateMachine.RemoveState(childState.state);
            }

            var stateIdle = rootStateMachine.AddState("Idle", new Vector3(250, 0, 0));
            stateIdle.motion = idle;

            var stateMove = rootStateMachine.AddState("Move", new Vector3(250, 100, 0));
            stateMove.motion = move;

            var stateDash = rootStateMachine.AddState("Dash", new Vector3(500, 50, 0));
            stateDash.motion = dash;

            var stateDie = rootStateMachine.AddState("Die", new Vector3(250, -150, 0));
            stateDie.motion = die;

            rootStateMachine.defaultState = stateIdle;

            // Idle <-> Move
            var idleToMove = stateIdle.AddTransition(stateMove);
            idleToMove.hasExitTime = false;
            idleToMove.duration = 0.05f;
            idleToMove.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");

            var moveToIdle = stateMove.AddTransition(stateIdle);
            moveToIdle.hasExitTime = false;
            moveToIdle.duration = 0.05f;
            moveToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");

            // AnyState -> Dash
            var anyToDash = rootStateMachine.AddAnyStateTransition(stateDash);
            anyToDash.hasExitTime = false;
            anyToDash.duration = 0.02f;
            anyToDash.AddCondition(AnimatorConditionMode.If, 0, "IsDashing");

            var dashToMove = stateDash.AddTransition(stateMove);
            dashToMove.hasExitTime = false;
            dashToMove.duration = 0.05f;
            dashToMove.AddCondition(AnimatorConditionMode.IfNot, 0, "IsDashing");
            dashToMove.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");

            var dashToIdle = stateDash.AddTransition(stateIdle);
            dashToIdle.hasExitTime = false;
            dashToIdle.duration = 0.05f;
            dashToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsDashing");
            dashToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");

            // AnyState -> Die
            var anyToDie = rootStateMachine.AddAnyStateTransition(stateDie);
            anyToDie.hasExitTime = false;
            anyToDie.duration = 0.0f;
            anyToDie.AddCondition(AnimatorConditionMode.If, 0, "Die");

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static PlayerMovementData CreateMovementDataAsset()
        {
            string path = $"{SettingsDir}/DefaultPlayerMovementData.asset";
            PlayerMovementData data = AssetDatabase.LoadAssetAtPath<PlayerMovementData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<PlayerMovementData>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.moveSpeed = 4.8f;
            data.acceleration = 45f;
            data.deceleration = 50f;
            data.dashSpeed = 11f;
            data.dashDuration = 0.22f;
            data.dashCooldown = 0.6f;
            data.fireRate = 3.5f;
            data.projectileSpeed = 11f;
            data.projectileLifetime = 1.2f;
            data.projectileDamage = 10f;
            data.maxHealth = 6;
            data.invincibilityDuration = 1.0f;

            EditorUtility.SetDirty(data);
            return data;
        }

        private static GameObject CreateProjectilePrefab(Sprite projectileSprite)
        {
            string path = $"{PrefabsDir}/PlayerProjectile.prefab";

            GameObject projGO = new GameObject("PlayerProjectile");

            var sr = projGO.AddComponent<SpriteRenderer>();
            sr.sprite = projectileSprite;
            sr.sortingOrder = 15;

            var col = projGO.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.08f;

            var rb = projGO.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            projGO.AddComponent<PlayerProjectile>();

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(projGO, path);
            Object.DestroyImmediate(projGO);
            return savedPrefab;
        }

        private static void CreatePlayerPrefab(
            AnimatorController controller, PlayerMovementData data, GameObject projectilePrefab, Sprite defaultSprite)
        {
            string prefabPath = $"{PrefabsDir}/Player.prefab";

            GameObject playerGO = new GameObject("Player");

            var spriteRenderer = playerGO.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = defaultSprite;
            spriteRenderer.sortingOrder = 10;

            var animator = playerGO.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            var rb = playerGO.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Fitted top-down collision capsule / circle at base of character
            var col = playerGO.AddComponent<CircleCollider2D>();
            col.radius = 0.1f;
            col.offset = new Vector2(0f, 0.08f);

            var inputReader = playerGO.AddComponent<PlayerInputReader>();
            var movement = playerGO.AddComponent<PlayerMovement>();
            var combat = playerGO.AddComponent<PlayerCombat>();
            var health = playerGO.AddComponent<PlayerHealth>();
            var animController = playerGO.AddComponent<PlayerAnimationController>();
            var playerController = playerGO.AddComponent<PlayerController>();

            // Configure serialized references
            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            var inputSO = new SerializedObject(inputReader);
            inputSO.FindProperty("inputActions").objectReferenceValue = inputAsset;
            inputSO.ApplyModifiedPropertiesWithoutUndo();

            var combatSO = new SerializedObject(combat);
            combatSO.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
            combatSO.ApplyModifiedPropertiesWithoutUndo();

            var healthSO = new SerializedObject(health);
            healthSO.FindProperty("movementData").objectReferenceValue = data;
            healthSO.ApplyModifiedPropertiesWithoutUndo();

            var playerSO = new SerializedObject(playerController);
            playerSO.FindProperty("movementData").objectReferenceValue = data;
            playerSO.FindProperty("movement").objectReferenceValue = movement;
            playerSO.FindProperty("animationController").objectReferenceValue = animController;
            playerSO.FindProperty("inputReader").objectReferenceValue = inputReader;
            playerSO.FindProperty("combat").objectReferenceValue = combat;
            playerSO.FindProperty("health").objectReferenceValue = health;
            playerSO.ApplyModifiedPropertiesWithoutUndo();

            var animSO = new SerializedObject(animController);
            animSO.FindProperty("animator").objectReferenceValue = animator;
            animSO.FindProperty("spriteRenderer").objectReferenceValue = spriteRenderer;
            animSO.ApplyModifiedPropertiesWithoutUndo();

            var moveSO = new SerializedObject(movement);
            moveSO.FindProperty("rb").objectReferenceValue = rb;
            moveSO.FindProperty("visualTransform").objectReferenceValue = playerGO.transform;
            moveSO.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(playerGO, prefabPath);
            Object.DestroyImmediate(playerGO);

            Debug.Log($"[PlayerAssetGenerator] Saved top-down player prefab to {prefabPath}");
        }
    }
}
