

using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HeartRotation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HeartPlacement heartPlacement;
    [SerializeField] private TMP_Text rotateButtonText;

    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 0.3f;

    public bool IsRotateMode { get; private set; }

    private bool isDragging = false;
    private Vector2 previousPosition;

    void Start()
    {
        IsRotateMode = false;
        UpdateButtonText();
    }

    void Update()
    {
        if (heartPlacement == null)
            return;

        // Si se desbloquea, salir del modo rotacion
        if (!heartPlacement.IsLocked)
        {
            if (IsRotateMode)
            {
                IsRotateMode = false;
                UpdateButtonText();
            }

            isDragging = false;
            return;
        }

        if (!IsRotateMode || heartPlacement.CurrentHeart == null)
        {
            isDragging = false;
            return;
        }

        Vector2 position = Vector2.zero;
        bool pressed = false;
        bool justPressed = false;

#if UNITY_EDITOR

        // Control mediante raton en XR Simulation
        if (Mouse.current != null)
        {
            position = Mouse.current.position.ReadValue();

            pressed = Mouse.current.leftButton.isPressed;
            justPressed =
                Mouse.current.leftButton.wasPressedThisFrame;
        }

#else

        // Control tactil en iPhone
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            position = touch.position.ReadValue();

            pressed = touch.press.isPressed;
            justPressed = touch.press.wasPressedThisFrame;
        }

#endif

        if (!pressed)
        {
            isDragging = false;
            return;
        }

        if (justPressed)
        {
            // No girar al pulsar botones de la interfaz
            if (IsOverUI(position))
            {
                isDragging = false;
                return;
            }

            previousPosition = position;
            isDragging = true;

            return;
        }

        if (!isDragging)
            return;

        float deltaX = position.x - previousPosition.x;

        if (Mathf.Abs(deltaX) > 0.01f)
        {
            // Girar alrededor del eje vertical
            heartPlacement.CurrentHeart.transform.Rotate(
                Vector3.up,
                -deltaX * rotationSpeed,
                Space.World
            );
        }

        previousPosition = position;
    }

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

    public void ToggleRotateMode()
    {
        if (heartPlacement == null ||
            heartPlacement.CurrentHeart == null)
        {
            Debug.Log("Place the heart first.");
            return;
        }

        if (!heartPlacement.IsLocked)
        {
            Debug.Log("Lock the heart before rotating.");
            return;
        }

        IsRotateMode = !IsRotateMode;

        isDragging = false;

        UpdateButtonText();

        Debug.Log(
            IsRotateMode
                ? "ROTATE MODE ACTIVATED"
                : "EXPLORE MODE ACTIVATED"
        );
    }

    private void UpdateButtonText()
    {
        if (rotateButtonText != null)
        {
            rotateButtonText.text = IsRotateMode
                ? "Stop Rotating"
                : "Rotate Mode";
        }
    }
}
