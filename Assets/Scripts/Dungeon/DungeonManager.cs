using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solaler.Dungeon
{
    /// <summary>
    /// Master coordinator for dungeon rooms and camera tracking (The Binding of Isaac style).
    /// Listens for doorway transitions, teleports player across thresholds with safe spacing,
    /// and smoothly transitions the camera to center on the active room.
    /// </summary>
    [DisallowMultipleComponent]
    public class DungeonManager : MonoBehaviour
    {
        [Header("Room Setup")]
        [SerializeField] private Room startingRoom;
        [SerializeField] private List<Room> allRooms = new List<Room>();

        [Header("Camera Control")]
        [SerializeField] private Camera dungeonCamera;
        [SerializeField] private float cameraPanDuration = 0.35f;

        [Header("Transition Settings")]
        [SerializeField] private float transitionCooldown = 0.5f;

        private Room _activeRoom;
        private float _lastTransitionTime = -999f;
        private Coroutine _cameraCoroutine;

        public Room ActiveRoom => _activeRoom;

        public void Configure(Room startRoom, List<Room> rooms, Camera cam, float panDuration = 0.35f)
        {
            startingRoom = startRoom;
            allRooms = new List<Room>(rooms);
            dungeonCamera = cam;
            cameraPanDuration = panDuration;
        }

        private void Reset()
        {
            dungeonCamera = Camera.main;
            allRooms.AddRange(FindObjectsByType<Room>(FindObjectsSortMode.None));
            if (allRooms.Count > 0) startingRoom = allRooms[0];
        }

        private void Awake()
        {
            if (dungeonCamera == null) dungeonCamera = Camera.main;

            if (allRooms.Count == 0)
            {
                allRooms.AddRange(FindObjectsByType<Room>(FindObjectsSortMode.None));
            }

            // Hook up doorway transition listeners
            foreach (var room in allRooms)
            {
                foreach (var door in room.Doorways)
                {
                    door.OnPlayerEnteredDoor += HandleDoorwayTransition;
                }
            }
        }

        private void Start()
        {
            if (startingRoom == null && allRooms.Count > 0)
            {
                startingRoom = allRooms[0];
            }

            if (startingRoom != null)
            {
                SwitchToRoom(startingRoom, instantCamera: true);
            }
        }

        private void OnDestroy()
        {
            foreach (var room in allRooms)
            {
                if (room == null) continue;
                foreach (var door in room.Doorways)
                {
                    if (door != null)
                    {
                        door.OnPlayerEnteredDoor -= HandleDoorwayTransition;
                    }
                }
            }
        }

        private void HandleDoorwayTransition(Doorway enteredDoor, GameObject player)
        {
            if (Time.time - _lastTransitionTime < transitionCooldown) return;
            if (enteredDoor == null || enteredDoor.ConnectedDoorway == null) return;

            Doorway destinationDoor = enteredDoor.ConnectedDoorway;
            Room destinationRoom = destinationDoor.ParentRoom;
            if (destinationRoom == null) return;

            _lastTransitionTime = Time.time;

            // Teleport player safely into destination room
            Vector2 targetPos = destinationDoor.SpawnPosition;
            if (player.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.position = targetPos;
                rb.linearVelocity = Vector2.zero; // Reset inertia when passing doorway
            }
            else
            {
                player.transform.position = targetPos;
            }

            SwitchToRoom(destinationRoom, instantCamera: false);
        }

        public void SwitchToRoom(Room room, bool instantCamera = false)
        {
            if (_activeRoom != null)
            {
                _activeRoom.OnPlayerExitRoom();
            }

            _activeRoom = room;
            _activeRoom.OnPlayerEnterRoom();

            Vector3 targetCamPos = new Vector3(_activeRoom.CenterPosition.x, _activeRoom.CenterPosition.y, dungeonCamera.transform.position.z);

            if (instantCamera)
            {
                dungeonCamera.transform.position = targetCamPos;
            }
            else
            {
                if (_cameraCoroutine != null) StopCoroutine(_cameraCoroutine);
                _cameraCoroutine = StartCoroutine(SmoothPanCamera(targetCamPos));
            }
        }

        private IEnumerator SmoothPanCamera(Vector3 targetPosition)
        {
            Vector3 startPos = dungeonCamera.transform.position;
            float elapsed = 0f;

            while (elapsed < cameraPanDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / cameraPanDuration);
                dungeonCamera.transform.position = Vector3.Lerp(startPos, targetPosition, t);
                yield return null;
            }

            dungeonCamera.transform.position = targetPosition;
            _cameraCoroutine = null;
        }
    }
}
