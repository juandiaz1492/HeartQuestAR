
using TMPro;
using UnityEngine;

public class QuizVentricularWall : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HeartPlacement heartPlacement;
    [SerializeField] private TMP_Text buttonText;

    private GameObject currentHeart;
    private GameObject ventricularWall;

    private bool wallHidden = false;
    private bool originalActiveState = true;

    private void Update()
    {
        if (heartPlacement == null)
            return;

        GameObject heart = heartPlacement.CurrentHeart;

        // Detect when the placed heart changes
        if (heart != currentHeart)
        {
            RestoreWall();

            currentHeart = heart;
            ventricularWall = null;
            wallHidden = false;

            if (currentHeart != null)
            {
                Transform wall = FindPart(
                    currentHeart.transform,
                    "ventricular-wall"
                );

                if (wall != null)
                {
                    ventricularWall = wall.gameObject;
                    originalActiveState =
                        ventricularWall.activeSelf;
                }
            }

            UpdateButtonText();
        }
    }

    public void ToggleVentricularWall()
    {
        if (ventricularWall == null)
            return;

        wallHidden = !wallHidden;

        ventricularWall.SetActive(
            wallHidden ? false : originalActiveState
        );

        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        if (buttonText == null)
            return;

        buttonText.text = wallHidden
            ? "Show Ventricular Wall"
            : "Hide Ventricular Wall";
    }

    private Transform FindPart(Transform root, string name)
    {
        foreach (Transform child in
                 root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child;
        }

        return null;
    }

    private void RestoreWall()
    {
        if (ventricularWall != null)
        {
            ventricularWall.SetActive(originalActiveState);
        }
    }

    private void OnDisable()
    {
        RestoreWall();
    }
}
