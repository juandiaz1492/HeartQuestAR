
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class HeartPlacement : MonoBehaviour
{
    public GameObject heartPrefab;

    private ARRaycastManager raycastManager;
    private GameObject currentHeart;

    private static readonly List<ARRaycastHit> hits =
        new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        if (heartPrefab == null || raycastManager == null)
            return;

        Vector2 screenPosition = Vector2.zero;
        bool pressed = false;

        // Clic del ratón para probar XR Simulation
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPosition = Mouse.current.position.ReadValue();
            pressed = true;
        }

        // Pantalla táctil para la futura versión móvil
        if (!pressed && Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPosition =
                Touchscreen.current.primaryTouch.position.ReadValue();
            pressed = true;
        }

        if (!pressed)
            return;

        if (raycastManager.Raycast(
            screenPosition,
            hits,
            TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;

            // Conservar la orientación original del corazón
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
}
