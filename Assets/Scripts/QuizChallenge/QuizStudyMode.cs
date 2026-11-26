
using TMPro;
using UnityEngine;

public class QuizStudyMode : MonoBehaviour
{
    [Header("Quiz references")]
    [SerializeField] private HeartPlacement heartPlacement;
    [SerializeField] private TMP_Text lockButtonText;

    private void LateUpdate()
    {
        if (heartPlacement == null || lockButtonText == null)
            return;

        // Only change the button label in Quiz Challenge
        if (heartPlacement.IsLocked)
        {
            lockButtonText.text = "Unlock Heart";
        }
        else
        {
            lockButtonText.text = "Start Study Mode";
        }
    }
}
