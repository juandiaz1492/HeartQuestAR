
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace HeartQuest.QuizChallenge
{
    public class HeartQuizManager : MonoBehaviour
    {
        [Header("Heart references")]
        [SerializeField] private HeartPlacement heartPlacement;
        [SerializeField] private HeartRotation heartRotation;

        [Header("Quiz UI")]
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text feedbackText;

        [Header("Quiz settings")]
        [SerializeField] private int numberOfQuestions = 10;
        [SerializeField] private float feedbackDelay = 1.5f;
        [SerializeField] private int mistakesBeforeHint = 3;

        [Header("Highlight settings")]
        [SerializeField] private Color hintColor = Color.yellow;
        [SerializeField] private float blinkInterval = 0.4f;

        private readonly Dictionary<string, string> heartParts = new()
    {
        { "aortic-arch", "Aortic Arch" },
        { "ascending-aorta", "Ascending Aorta" },
        { "left-atrium", "Left Atrium" },
        { "right-atrium", "Right Atrium" },
        { "left-ventricle", "Left Ventricle" },
        { "right-ventricle", "Right Ventricle" },
        { "pulmonary-trunk", "Pulmonary Trunk" },
        { "superior-vena-cava", "Superior Vena Cava" },
        { "left-coronary", "Left Coronary Artery" },
        { "right-coronary", "Right Coronary Artery" },
        { "ventricular-wall", "Ventricular Wall" }
    };

        private readonly List<string> questions = new();

        private int questionIndex;
        private int score;
        private int mistakes;

        private bool waitingForNext;
        private bool quizFinished;
        private bool hintActive;
        private bool previousLockedState;

        // Highlight references
        private readonly Dictionary<Renderer, MaterialPropertyBlock>
            originalBlocks = new();

        private readonly List<Renderer> highlightedRenderers = new();

        private MaterialPropertyBlock workingBlock;

        private static readonly int BaseColorId =
            Shader.PropertyToID("_BaseColor");

        private static readonly int ColorId =
            Shader.PropertyToID("_Color");

        private void Start()
        {
            if (heartPlacement == null ||
                questionText == null ||
                scoreText == null ||
                progressText == null ||
                feedbackText == null)
            {
                Debug.LogError("Quiz references are missing!");
                enabled = false;
                return;
            }

            workingBlock = new MaterialPropertyBlock();

            StartNewQuiz();
        }

        private void StartNewQuiz()
        {
            StopAllCoroutines();
            StopHint();

            questions.Clear();

            foreach (string key in heartParts.Keys)
            {
                questions.Add(key);
            }

            // Randomize questions
            for (int i = questions.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);

                string temp = questions[i];
                questions[i] = questions[j];
                questions[j] = temp;
            }

            numberOfQuestions = Mathf.Clamp(
                numberOfQuestions, 1, questions.Count
            );

            questionIndex = 0;
            score = 0;
            mistakes = 0;

            waitingForNext = false;
            quizFinished = false;

            previousLockedState = heartPlacement.IsLocked;

            ShowQuestion();

            if (!previousLockedState)
            {
                ShowStartInstructions();
            }
        }

        private void ShowStartInstructions()
        {
            questionText.text = "Ready to Study?";

            feedbackText.color = Color.white;
            feedbackText.text =
                "Place the heart, then press Start Study Mode.";
        }

        private void ShowQuestion()
        {
            if (quizFinished ||
                questionIndex >= numberOfQuestions)
            {
                return;
            }

            mistakes = 0;

            string correctPart = questions[questionIndex];

            questionText.text =
                "Find the " + heartParts[correctPart];

            scoreText.text = "Score: " + score;

            progressText.text =
                "Question: " + (questionIndex + 1) +
                " / " + numberOfQuestions;

            feedbackText.color = Color.white;
            feedbackText.text = "Tap a heart structure";

            if (!heartPlacement.IsLocked)
            {
                ShowPausedInstructions();
            }
        }

        private void ShowPausedInstructions()
        {
            questionText.text = "Study Paused";

            feedbackText.color = Color.white;
            feedbackText.text =
                "Press Start Study Mode to continue.";
        }

        private void Update()
        {
            UpdateStudyMode();
            UpdateHintAnimation();

            if (waitingForNext || quizFinished)
            {
                return;
            }

            // Heart must exist and be locked
            if (heartPlacement == null ||
                !heartPlacement.HasHeart ||
                !heartPlacement.IsLocked)
            {
                return;
            }

            // Ignore answers while rotating
            if (heartRotation != null &&
                heartRotation.IsRotateMode)
            {
                return;
            }

            Vector2 screenPosition;

#if UNITY_EDITOR

            // Mouse input for Unity Editor
            if (Mouse.current == null ||
                !Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            screenPosition =
                Mouse.current.position.ReadValue();

#else

        // Touch input for iPhone
        if (Touchscreen.current == null ||
            !Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return;
        }

        screenPosition =
            Touchscreen.current.primaryTouch.position.ReadValue();

#endif

            // Ignore clicks on UI
            if (IsOverUI(screenPosition))
            {
                return;
            }

            Camera cam = Camera.main;

            if (cam == null)
            {
                return;
            }

            Ray ray = cam.ScreenPointToRay(screenPosition);

            RaycastHit[] hits =
                Physics.RaycastAll(ray, 10f);

            Array.Sort(
                hits,
                (a, b) => a.distance.CompareTo(b.distance)
            );

            foreach (RaycastHit hit in hits)
            {
                // Only recognize the placed heart
                if (!hit.collider.transform.IsChildOf(
                    heartPlacement.CurrentHeart.transform))
                {
                    continue;
                }

                string selectedPart =
                    hit.collider.gameObject.name;

                if (heartParts.ContainsKey(selectedPart))
                {
                    CheckAnswer(selectedPart);
                    return;
                }
            }
        }

        private void UpdateStudyMode()
        {
            if (heartPlacement == null || quizFinished)
            {
                return;
            }

            bool locked = heartPlacement.IsLocked;

            if (locked == previousLockedState)
            {
                return;
            }

            previousLockedState = locked;

            if (!locked)
            {
                // Pause without losing score or mistakes
                StopHint();

                ShowPausedInstructions();
                return;
            }

            // Do not interfere with a completed answer
            if (waitingForNext)
            {
                return;
            }

            string correctPart = questions[questionIndex];

            questionText.text =
                "Find the " + heartParts[correctPart];

            if (mistakes >= mistakesBeforeHint)
            {
                feedbackText.color = Color.yellow;
                feedbackText.text =
                    "Hint! Look for the yellow structure.";

                StartHint(correctPart);
            }
            else if (mistakes > 0)
            {
                feedbackText.color = Color.red;
                feedbackText.text =
                    "Incorrect! Try again.";
            }
            else
            {
                feedbackText.color = Color.white;
                feedbackText.text =
                    "Tap a heart structure";
            }
        }

        private bool IsOverUI(Vector2 position)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            PointerEventData data =
                new PointerEventData(EventSystem.current);

            data.position = position;

            List<RaycastResult> results =
                new List<RaycastResult>();

            EventSystem.current.RaycastAll(data, results);

            return results.Count > 0;
        }

        private void CheckAnswer(string selectedPart)
        {
            if (waitingForNext || quizFinished)
            {
                return;
            }

            string correctPart = questions[questionIndex];

            Debug.Log("Selected: " + selectedPart);
            Debug.Log("Expected: " + correctPart);

            if (selectedPart == correctPart)
            {
                // Only first-attempt answers earn points
                if (mistakes == 0)
                {
                    score++;
                    feedbackText.text =
                        "Correct! +1 point";
                }
                else
                {
                    feedbackText.text =
                        "Correct! Structure identified!";
                }

                feedbackText.color = Color.green;
                scoreText.text = "Score: " + score;

                waitingForNext = true;

                StopHint();

                StartCoroutine(NextQuestionAfterDelay());
            }
            else
            {
                mistakes++;

                // Remain on the same question
                if (mistakes >= mistakesBeforeHint)
                {
                    feedbackText.color = Color.yellow;

                    feedbackText.text =
                        "Hint! Find the yellow structure.";

                    if (!hintActive)
                    {
                        StartHint(correctPart);
                    }
                }
                else
                {
                    feedbackText.color = Color.red;

                    feedbackText.text =
                        "Incorrect! Try again (" +
                        mistakes + "/" +
                        mistakesBeforeHint + ")";
                }
            }
        }

        private IEnumerator NextQuestionAfterDelay()
        {
            yield return new WaitForSeconds(feedbackDelay);

            questionIndex++;

            if (questionIndex >= numberOfQuestions)
            {
                FinishQuiz();
                yield break;
            }

            StopHint();

            waitingForNext = false;

            ShowQuestion();
        }

        private void FinishQuiz()
        {
            StopHint();

            quizFinished = true;
            waitingForNext = false;

            questionText.text =
                "Quiz Completed!";

            scoreText.text =
                "Final Score: " + score;

            progressText.text =
                numberOfQuestions + " / " +
                numberOfQuestions;

            feedbackText.color = Color.white;

            feedbackText.text =
                "You scored " + score +
                " out of " + numberOfQuestions + "!";
        }

        // ----------------------------------------
        // Find anatomical structure
        // ----------------------------------------

        private Transform FindPart(
            Transform root,
            string partName)
        {
            foreach (Transform child in
                     root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == partName)
                {
                    return child;
                }
            }

            return null;
        }

        // ----------------------------------------
        // Yellow hint after three mistakes
        // ----------------------------------------

        private void StartHint(string partName)
        {
            if (hintActive ||
                heartPlacement == null ||
                heartPlacement.CurrentHeart == null)
            {
                return;
            }

            Transform target = FindPart(
                heartPlacement.CurrentHeart.transform,
                partName
            );

            if (target == null)
            {
                Debug.LogWarning(
                    "Quiz: part not found: " + partName
                );

                return;
            }

            originalBlocks.Clear();
            highlightedRenderers.Clear();

            foreach (Renderer renderer in
                     target.GetComponentsInChildren<Renderer>(true))
            {
                MaterialPropertyBlock original =
                    new MaterialPropertyBlock();

                renderer.GetPropertyBlock(original);

                originalBlocks[renderer] = original;

                highlightedRenderers.Add(renderer);
            }

            hintActive =
                highlightedRenderers.Count > 0;
        }

        private void UpdateHintAnimation()
        {
            if (!hintActive)
            {
                return;
            }

            bool bright =
                Mathf.FloorToInt(
                    Time.time / Mathf.Max(
                        0.1f, blinkInterval)
                ) % 2 == 0;

            foreach (Renderer renderer in highlightedRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                if (!bright)
                {
                    // Restore original color
                    renderer.SetPropertyBlock(
                        originalBlocks[renderer]
                    );

                    continue;
                }

                workingBlock.Clear();

                renderer.GetPropertyBlock(workingBlock);

                workingBlock.SetColor(
                    ColorId, hintColor
                );

                workingBlock.SetColor(
                    BaseColorId, hintColor
                );

                renderer.SetPropertyBlock(
                    workingBlock
                );
            }
        }

        private void StopHint()
        {
            foreach (Renderer renderer in highlightedRenderers)
            {
                if (renderer != null &&
                    originalBlocks.TryGetValue(
                        renderer, out var original))
                {
                    renderer.SetPropertyBlock(original);
                }
            }

            highlightedRenderers.Clear();
            originalBlocks.Clear();

            hintActive = false;
        }

        private void OnDisable()
        {
            StopHint();
        }

    }
}
