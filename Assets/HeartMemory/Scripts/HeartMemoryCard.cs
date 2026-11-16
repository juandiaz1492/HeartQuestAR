
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HeartQuestAR.HeartMemory
{
    public class HeartMemoryCard : MonoBehaviour
    {
        [Header("Card Settings")]
        [SerializeField] private TMP_Text cardLabel;
        [SerializeField] private Image cardImage;
        [SerializeField] private string hiddenText = "?";

        private Button button;
        private HeartMemoryGame game;
        private string faceText;

        public int PairId { get; private set; }
        public bool IsFaceUp { get; private set; }
        public bool IsMatched { get; private set; }


        private void Awake()
        {
            button = GetComponent<Button>();

            // Get the card background image
            if (cardImage == null)
            {
                cardImage = GetComponent<Image>();
            }

            // Get the TextMeshPro component
            if (cardLabel == null)
            {
                cardLabel = GetComponentInChildren<TMP_Text>(true);
            }

            if (cardImage == null)
            {
                Debug.LogError(
                    "No Image component found on this card.",
                    this
                );
            }

            if (cardLabel == null)
            {
                Debug.LogError(
                    "No TextMeshPro component found in this card.",
                    this
                );
            }

            if (button == null)
            {
                Debug.LogError(
                    "No Button component found on this card.",
                    this
                );

                return;
            }

            button.onClick.AddListener(HandleClick);
        }


        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        public void Initialize(
            HeartMemoryGame owner,
            int pairId,
            string content
        )
        {
            game = owner;
            PairId = pairId;
            faceText = content;

            IsFaceUp = false;
            IsMatched = false;

            ApplyHiddenStyle();

            button.interactable = true;
        }

        private void HandleClick()
        {
            if (game != null)
            {
                game.SelectCard(this);
            }
        }

        public void Reveal()
        {
            if (IsMatched)
                return;

            IsFaceUp = true;

            ApplyFaceStyle();

            button.interactable = false;
        }

        public void Hide()
        {
            if (IsMatched)
                return;

            IsFaceUp = false;

            ApplyHiddenStyle();

            button.interactable = true;
        }


        public void MarkAsMatched()
        {
            IsMatched = true;
            IsFaceUp = true;

            ApplyFaceStyle();

            cardLabel.color = new Color32(40, 123, 101, 255);

            cardImage.color = new Color32(189, 235, 217, 255);

            button.interactable = false;
        }


        // Style for hidden cards

        private void ApplyHiddenStyle()
        {
            cardLabel.enableAutoSizing = false;
            cardLabel.fontSize = 60;
            cardLabel.text = hiddenText;

            cardLabel.color = Color.white;

            cardImage.color = new Color32(35, 118, 185, 255);
        }

        // Style for revealed cards

        private void ApplyFaceStyle()
        {
            cardLabel.enableAutoSizing = true;
            cardLabel.fontSizeMin = 16;
            cardLabel.fontSizeMax = 28;
            cardLabel.text = faceText;

            // Dark navy text for better readability
            cardLabel.color = new Color32(57, 125, 178, 255);

            cardImage.color = new Color32(225, 239, 248, 255);
        }

    }
}
