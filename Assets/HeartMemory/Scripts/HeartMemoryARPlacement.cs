
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

namespace HeartQuestAR.HeartMemory
{
    [DefaultExecutionOrder(-10000)]
    public class HeartMemoryARPlacement : MonoBehaviour
    {
        [Header("AR References")]
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private HeartMemoryARGame gameManager;
        [SerializeField] private TMP_Text instructionText;

        [Header("Desktop Test Mode")]
        [SerializeField] private bool desktopTestMode = true;
        [SerializeField] private GameObject desktopTestEnvironment;
        [SerializeField] private Transform desktopSpawnPoint;
        [SerializeField] private Camera desktopCamera;

        [Header("AR Objects")]
        [SerializeField] private GameObject arSession;
        [SerializeField] private GameObject xrOrigin;

        private static readonly List<ARRaycastHit> hits =
            new List<ARRaycastHit>();

        private bool cardsPlaced = false;

        private void Awake()
        {
#if UNITY_EDITOR
            if (desktopTestMode)
            {
                if (arSession != null)
                    arSession.SetActive(false);

                if (xrOrigin != null)
                    xrOrigin.SetActive(false);
            }
#else
            if (desktopTestEnvironment != null)
                desktopTestEnvironment.SetActive(false);
#endif
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (desktopTestMode)
            {
                if (desktopSpawnPoint == null ||
                    desktopCamera == null ||
                    gameManager == null)
                {
                    Debug.LogError(
                        "Desktop test mode: Missing references.",
                        this
                    );
                    return;
                }

                gameManager.SetInputCamera(desktopCamera);
                gameManager.StartGame(desktopSpawnPoint);

                cardsPlaced = true;

                if (instructionText != null)
                    instructionText.text =
                        "CLICK THE CARDS TO FIND THE PAIRS!";

                Debug.Log("Heart Memory: Desktop test started!");
                return;
            }
#endif
        }

        private void Update()
        {
            if (cardsPlaced ||
                raycastManager == null ||
                gameManager == null)
                return;

            if (!TryGetPressPosition(out Vector2 screenPosition))
                return;

            if (raycastManager.Raycast(
                screenPosition,
                hits,
                TrackableType.PlaneWithinPolygon))
            {
                Pose pose = hits[0].pose;

                GameObject root = new GameObject("ARCardsRoot");

                root.transform.SetPositionAndRotation(
                    pose.position,
                    pose.rotation
                );

                gameManager.StartGame(root.transform);

                cardsPlaced = true;

                if (instructionText != null)
                    instructionText.text =
                        "Find the 6 matching pairs!";
            }
        }

        private bool TryGetPressPosition(out Vector2 position)
        {
            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                position = Touchscreen.current.primaryTouch
                    .position.ReadValue();

                return true;
            }

            if (Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame)
            {
                position = Mouse.current.position.ReadValue();

                return true;
            }

            position = Vector2.zero;
            return false;
        }
    }
}
