
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HeartQuestAR.HeartMemory
{
    public class HeartMemoryARGame : MonoBehaviour
    {
        [Header("AR References")]
        [SerializeField] private HeartMemoryARCard cardPrefab;
        [SerializeField] private Camera arCamera;

        [Header("UI References")]
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text attemptsText;
        [SerializeField] private TMP_Text pairsText;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TMP_Text victoryStats;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button backToMenuButton;

        [Header("Card Layout")]
        [SerializeField] private float spacingX = 0.115f;
        [SerializeField] private float spacingZ = 0.085f;

        private Transform cardsParent;

        private readonly List<HeartMemoryARCard> cards = new();
        private readonly Dictionary<HeartMemoryARCard, string> contents = new();

        private HeartMemoryARCard firstSelected;
        private HeartMemoryARCard secondSelected;

        private int matchedPairs;
        private int attempts;
        private float elapsedTime;

        private bool timerStarted;
        private bool resolving;
        private bool completed;

        private void Awake()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);

            if (backToMenuButton != null)
                backToMenuButton.onClick.AddListener(ReturnToMenu);
        }

        private void OnDestroy()
        {
            if (restartButton != null)
                restartButton.onClick.RemoveListener(RestartGame);

            if (backToMenuButton != null)
                backToMenuButton.onClick.RemoveListener(ReturnToMenu);
        }

        private void Start()
        {
            if (victoryPanel != null)
                victoryPanel.SetActive(false);
        }

        public void StartGame(Transform parent)
        {
            if (cardPrefab == null || parent == null)
            {
                Debug.LogError("Heart Memory AR: Missing card prefab or parent.");
                return;
            }

            cardsParent = parent;
            InitializeGame();
        }

        private void Update()
        {
            if (timerStarted && !completed)
            {
                elapsedTime += Time.deltaTime;
                UpdateStats();
            }

            HandleCardInput();
        }

        private void HandleCardInput()
        {
            if (cardsParent == null || completed || resolving)
                return;

            Vector2 position;
            bool pressed = false;

            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                position = Touchscreen.current.primaryTouch.position.ReadValue();
                pressed = true;
            }
            else if (Mouse.current != null &&
                     Mouse.current.leftButton.wasPressedThisFrame)
            {
                position = Mouse.current.position.ReadValue();
                pressed = true;
            }
            else
            {
                return;
            }

            if (arCamera == null)
                return;

            Ray ray = arCamera.ScreenPointToRay(position);

            if (Physics.Raycast(ray, out RaycastHit hit, 5f))
            {
                HeartMemoryARCard card =
                    hit.collider.GetComponentInParent<HeartMemoryARCard>();

                SelectCard(card);
            }
        }

        private void InitializeGame()
        {
            StopAllCoroutines();

            foreach (Transform child in cardsParent)
                Destroy(child.gameObject);

            cards.Clear();
            contents.Clear();

            firstSelected = null;
            secondSelected = null;

            matchedPairs = 0;
            attempts = 0;
            elapsedTime = 0f;

            timerStarted = false;
            resolving = false;
            completed = false;

            if (victoryPanel != null)
                victoryPanel.SetActive(false);

            BuildDeck();
            UpdateStats();
        }

        private class CardInfo
        {
            public int pairId;
            public string text;

            public CardInfo(int id, string value)
            {
                pairId = id;
                text = value;
            }
        }

        private void BuildDeck()
        {
            List<CardInfo> deck = new();

            AddPair(deck, 0, "Aorta",
                "Carries oxygenated blood\nfrom the left ventricle\nto the body.");

            AddPair(deck, 1, "Pulmonary Artery",
                "Carries deoxygenated blood\nfrom the right ventricle\nto the lungs.");

            AddPair(deck, 2, "Left Atrium",
                "Receives oxygenated blood\nfrom the lungs.");

            AddPair(deck, 3, "Right Atrium",
                "Receives deoxygenated blood\nfrom the body.");

            AddPair(deck, 4, "Left Ventricle",
                "Pumps oxygenated blood\ninto the aorta.");

            AddPair(deck, 5, "Right Ventricle",
                "Pumps deoxygenated blood\nto the lungs.");

            for (int i = deck.Count - 1; i > 0; i--)
            {
                int random = Random.Range(0, i + 1);
                (deck[i], deck[random]) = (deck[random], deck[i]);
            }

            SpawnCards(deck);
        }

        private void AddPair(List<CardInfo> deck, int id,
            string structure, string function)
        {
            deck.Add(new CardInfo(id, structure));
            deck.Add(new CardInfo(id, function));
        }

        private void SpawnCards(List<CardInfo> deck)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                int column = i % 4;
                int row = i / 4;

                Vector3 localPosition = new Vector3(
                    (column - 1.5f) * spacingX,
                    0.003f,
                    (1f - row) * spacingZ
                );

                HeartMemoryARCard card =
                    Instantiate(cardPrefab, cardsParent);

                card.transform.localPosition = localPosition;
                card.transform.localRotation = Quaternion.identity;

                card.Initialize(this, deck[i].pairId, deck[i].text);

                cards.Add(card);
                contents.Add(card, deck[i].text);
            }
        }

        public void SelectCard(HeartMemoryARCard card)
        {
            if (card == null || resolving || completed ||
                card.IsFaceUp || card.IsMatched)
                return;

            if (!timerStarted)
                timerStarted = true;

            card.Reveal(contents[card]);

            if (firstSelected == null)
            {
                firstSelected = card;
                return;
            }

            secondSelected = card;
            attempts++;

            if (firstSelected.PairId == secondSelected.PairId)
            {
                firstSelected.MarkAsMatched();
                secondSelected.MarkAsMatched();

                matchedPairs++;

                firstSelected = null;
                secondSelected = null;

                if (matchedPairs == 6)
                    CompleteGame();
            }
            else
            {
                StartCoroutine(HideUnmatchedCards());
            }

            UpdateStats();
        }

        private IEnumerator HideUnmatchedCards()
        {
            resolving = true;

            yield return new WaitForSeconds(1f);

            firstSelected.Hide();
            secondSelected.Hide();

            firstSelected = null;
            secondSelected = null;

            resolving = false;
        }

        private void UpdateStats()
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);

            if (timeText != null)
                timeText.text = $"TIME: {minutes:00}:{seconds:00}";

            if (attemptsText != null)
                attemptsText.text = $"ATTEMPTS: {attempts}";

            if (pairsText != null)
                pairsText.text = $"PAIRS: {matchedPairs}/6";
        }

        private void CompleteGame()
        {
            completed = true;
            timerStarted = false;

            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);

            if (victoryStats != null)
            {
                victoryStats.text =
                    $"Time: {minutes:00}:{seconds:00}\n" +
                    $"Attempts: {attempts}\nPairs: {matchedPairs}/6";
            }

            if (victoryPanel != null)
                victoryPanel.SetActive(true);
        }

        public void RestartGame()
        {
            if (cardsParent != null)
                InitializeGame();
        }

        public void ReturnToMenu()
        {
            SceneManager.LoadScene("HeartQuestMenu");
        }

        public void SetInputCamera(Camera inputCamera)
        {
            if (inputCamera != null)
                arCamera = inputCamera;
        }
    }
}
