
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class HeartPartSelector : MonoBehaviour
{
    [Header("Information card")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [SerializeField] private HeartRotation heartRotation;

    // Informacion de cada estructura anatomica
    private readonly Dictionary<string, (string title, string description)>
        heartParts = new()
    {
        {
            "aortic-arch",
            ("Aortic Arch",
             "The aortic arch carries oxygenated blood and gives rise to arteries that supply the head, neck and upper limbs.")
        },
        {
            "ascending-aorta",
            ("Ascending Aorta",
             "The ascending aorta carries oxygenated blood from the left ventricle toward the aortic arch.")
        },
        {
            "left-atrium",
            ("Left Atrium",
             "The left atrium receives oxygenated blood from the lungs through the pulmonary veins.")
        },
        {
            "right-atrium",
            ("Right Atrium",
             "The right atrium receives deoxygenated blood returning from the body through the venae cavae.")
        },
        {
            "left-ventricle",
            ("Left Ventricle",
             "The left ventricle pumps oxygenated blood into the aorta to supply the rest of the body.")
        },
        {
            "right-ventricle",
            ("Right Ventricle",
             "The right ventricle pumps deoxygenated blood into the pulmonary trunk toward the lungs.")
        },
        {
            "pulmonary-trunk",
            ("Pulmonary Trunk",
             "The pulmonary trunk carries deoxygenated blood from the right ventricle and divides into the pulmonary arteries.")
        },
        {
            "superior-vena-cava",
            ("Superior Vena Cava",
             "The superior vena cava returns deoxygenated blood from the head, neck, upper limbs and upper body to the right atrium.")
        },
        {
            "left-coronary",
            ("Left Coronary Artery",
             "The left coronary artery supplies oxygenated blood to important regions of the heart muscle.")
        },
        {
            "right-coronary",
            ("Right Coronary Artery",
             "The right coronary artery supplies oxygenated blood to parts of the heart muscle.")
        },
        {
            "ventricular-wall",
            ("Ventricular Wall",
             "The ventricular wall contains cardiac muscle that contracts to pump blood out of the ventricles.")
        }
    };

    private void Start()
    {
        // Hide information card at startup
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    private void Update()
    {
        // Do not select anatomical structures in Rotate Mode
        if (heartRotation != null && heartRotation.IsRotateMode)
            return;

        // Do not select parts while the information card is open
        if (infoPanel != null && infoPanel.activeSelf)
            return;

        Vector2 screenPosition;
        int pointerId = -1;

        // iPhone touch
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPosition =
                Touchscreen.current.primaryTouch.position.ReadValue();

            pointerId =
                Touchscreen.current.primaryTouch.touchId.ReadValue();
        }
        // Windows mouse
        else if (Mouse.current != null &&
                 Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPosition = Mouse.current.position.ReadValue();
        }
        else
        {
            return;
        }

        // Ignore clicks on UI
        if (EventSystem.current != null)
        {
            bool overUI = pointerId >= 0
                ? EventSystem.current.IsPointerOverGameObject(pointerId)
                : EventSystem.current.IsPointerOverGameObject();

            if (overUI)
                return;
        }

        Camera cam = Camera.main;

        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(screenPosition);

        // Find all colliders under the touch
        RaycastHit[] hits = Physics.RaycastAll(ray, 10f);

        // Process the closest hit first
        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit hit in hits)
        {
            string partName = hit.collider.gameObject.name;

            if (heartParts.TryGetValue(partName, out var part))
            {
                ShowInfoCard(part.title, part.description);
                Debug.Log("Selected: " + part.title);
                return;
            }
        }
    }

    private void ShowInfoCard(string title, string description)
    {
        if (infoPanel == null ||
            titleText == null ||
            descriptionText == null)
        {
            Debug.LogWarning("Information card references missing!");
            return;
        }

        titleText.text = title;
        descriptionText.text = description;

        infoPanel.SetActive(true);
    }

    public void CloseInfoCard()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }
}
