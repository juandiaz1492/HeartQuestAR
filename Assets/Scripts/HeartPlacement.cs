


using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class HeartPlacement : MonoBehaviour
{
    public GameObject heartPrefab;

    [Header("Lock Button")]
    [SerializeField] private TMP_Text lockButtonText;

    private ARRaycastManager raycastManager;
    private GameObject currentHeart;
    private bool isLocked = false;

    public bool HasHeart => currentHeart != null;
    public bool IsLocked => isLocked;
    public GameObject CurrentHeart => currentHeart;

    private static readonly List<ARRaycastHit> hits =
        new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Start()
    {
        UpdateLockButton();
    }

    void Update()
    {
        if (heartPrefab == null || raycastManager == null || isLocked)
            return;

        Vector2 screenPosition = Vector2.zero;
        bool pressed = false;

#if UNITY_EDITOR
        // Ratón para XR Simulation
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPosition = Mouse.current.position.ReadValue();
            pressed = true;
        }
#else
        // Pantalla táctil en iPhone
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPosition =
                Touchscreen.current.primaryTouch.position.ReadValue();

            pressed = true;
        }
#endif

        if (!pressed)
            return;

        if (IsOverUI(screenPosition))
            return;

        if (raycastManager.Raycast(
            screenPosition,
            hits,
            TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;

            // Conservar la orientacion original del prefab
            Quaternion rotation =
                pose.rotation * heartPrefab.transform.rotation;

            if (currentHeart == null)
            {
                currentHeart = Instantiate(
                    heartPrefab,
                    pose.position,
                    rotation
                );
            }
            else
            {
                currentHeart.transform.SetPositionAndRotation(
                    pose.position,
                    rotation
                );
            }

            Debug.Log("Heart placed successfully!");
        }
    }

    // Evita colocar el corazon al pulsar un boton
    private bool IsOverUI(Vector2 position)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData data =
            new PointerEventData(EventSystem.current);

        data.position = position;

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(data, results);

        return results.Count > 0;
    }

    public void ToggleLock()
    {
        if (currentHeart == null)
        {
            Debug.Log("Place the heart first.");
            return;
        }

        isLocked = !isLocked;

        UpdateLockButton();

        Debug.Log(
            isLocked
                ? "Heart position LOCKED"
                : "Heart position UNLOCKED"
        );
    }

    private void UpdateLockButton()
    {
        if (lockButtonText != null)
        {
            lockButtonText.text = isLocked
                ? "Unlock Position"
                : "Lock Position";
        }
    }
}
