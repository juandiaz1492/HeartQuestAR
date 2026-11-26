
using System.Collections;
using TMPro;
using UnityEngine;

namespace HeartQuestAR.HeartMemory
{
    public class HeartMemoryARCard : MonoBehaviour
    {
        [Header("Card References")]
        [SerializeField] private Renderer cardRenderer;
        [SerializeField] private TMP_Text cardLabel;
        [SerializeField] private TMP_Text faceLabel;

        [Header("Flip Animation")]
        [SerializeField] private float flipDuration = 0.45f;
        [SerializeField] private float liftHeight = 0.04f;

        public int PairId { get; private set; }
        public bool IsFaceUp { get; private set; }
        public bool IsMatched { get; private set; }

        private readonly Color hiddenColor =
            new Color32(35, 118, 185, 255);

        private readonly Color faceColor =
            new Color32(225, 239, 248, 255);

        private readonly Color matchedColor =
            new Color32(189, 235, 217, 255);

        private Vector3 originalPosition;
        private Quaternion originalRotation;

        private Coroutine flipRoutine;

        public void Initialize(
            HeartMemoryARGame owner,
            int pairId,
            string content)
        {
            PairId = pairId;

            if (cardRenderer == null)
            {
                Transform body = transform.Find("CardBody");

                if (body != null)
                    cardRenderer = body.GetComponent<Renderer>();
            }

            if (cardLabel == null)
            {
                Transform label = transform.Find("CardLabel3D");

                if (label != null)
                    cardLabel = label.GetComponent<TMP_Text>();
            }

            if (faceLabel == null)
            {
                Transform label = transform.Find("CardFaceLabel3D");

                if (label != null)
                    faceLabel = label.GetComponent<TMP_Text>();
            }

            if (cardRenderer == null ||
                cardLabel == null ||
                faceLabel == null)
            {
                Debug.LogError(
                    "HeartMemoryARCard: Missing card references!",
                    this
                );
                return;
            }

            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;

            IsFaceUp = false;
            IsMatched = false;

            ApplyHiddenStyle();
        }

        public void Reveal(string content)
        {
            if (IsFaceUp || IsMatched || flipRoutine != null)
                return;

            IsFaceUp = true;

            faceLabel.text = content;
            faceLabel.fontSize = 0.09f;
            faceLabel.enableAutoSizing = false;
            faceLabel.enableWordWrapping = true;
            faceLabel.overflowMode = TextOverflowModes.Truncate;
            faceLabel.alignment = TextAlignmentOptions.Center;
            faceLabel.color = new Color32(57, 125, 178, 255);

            flipRoutine = StartCoroutine(FlipCard(true));
        }

        public void Hide()
        {
            if (IsMatched || flipRoutine != null)
                return;

            IsFaceUp = false;

            flipRoutine = StartCoroutine(FlipCard(false));
        }

        private IEnumerator FlipCard(bool reveal)
        {
            float elapsed = 0f;

            Quaternion startRotation = transform.localRotation;

            Quaternion targetRotation =
                originalRotation *
                Quaternion.Euler(reveal ? 180f : 0f, 0f, 0f);

            bool appearanceChanged = false;

            while (elapsed < flipDuration)
            {
                elapsed += Time.deltaTime;

                float progress = Mathf.Clamp01(
                    elapsed / flipDuration
                );

                float smoothProgress =
                    Mathf.SmoothStep(0f, 1f, progress);

                transform.localRotation = Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    smoothProgress
                );

                // Lift the card above the table during the flip
                float lift =
                    Mathf.Sin(progress * Mathf.PI) * liftHeight;

                transform.localPosition =
                    originalPosition + Vector3.up * lift;

                // Change the visible side at the halfway point
                if (progress >= 0.5f && !appearanceChanged)
                {
                    UpdateAppearance(reveal);
                    appearanceChanged = true;
                }

                yield return null;
            }

            transform.localPosition = originalPosition;
            transform.localRotation = targetRotation;

            UpdateAppearance(reveal);

            flipRoutine = null;
        }

        private void UpdateAppearance(bool reveal)
        {
            if (reveal)
            {
                faceLabel.gameObject.SetActive(true);

                cardRenderer.material.color =
                    IsMatched ? matchedColor : faceColor;
            }
            else
            {
                faceLabel.text = "";

                cardRenderer.material.color = hiddenColor;
            }
        }

        public void MarkAsMatched()
        {
            IsMatched = true;
            IsFaceUp = true;

            if (faceLabel != null)
                faceLabel.color = new Color32(40, 123, 101, 255);

            if (cardRenderer != null)
                cardRenderer.material.color = matchedColor;
        }

        private void ApplyHiddenStyle()
        {
            if (cardRenderer == null ||
                cardLabel == null ||
                faceLabel == null)
                return;

            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;

            cardLabel.enableAutoSizing = false;
            cardLabel.fontSize = 0.33f;
            cardLabel.enableWordWrapping = false;
            cardLabel.text = "?";
            cardLabel.color = Color.white;

            faceLabel.enableAutoSizing = false;
            faceLabel.fontSize = 0.09f;
            faceLabel.text = "";
            faceLabel.enableWordWrapping = true;
            faceLabel.gameObject.SetActive(true);

            cardRenderer.material.color = hiddenColor;
        }
    }
}
