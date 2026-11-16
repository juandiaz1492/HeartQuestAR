
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace HeartQuestAR.HeartMemory
{
    public class HeartMemoryGame : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private HeartMemoryCard cardPrefab;
        [SerializeField] private Transform boardParent;

        [Header("Individual Statistics")]
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text attemptsText;
        [SerializeField] private TMP_Text pairsText;

        [Header("Victory Screen")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TMP_Text victoryStats;
        [SerializeField] private Button restartButton;

        private readonly List<CardInfo> deck =
            new List<CardInfo>();

        private HeartMemoryCard firstSelected;
        private HeartMemoryCard secondSelected;

        private bool isResolvingPair = false;
        private bool timerStarted = false;
        private bool gameCompleted = false;

        private int matchedPairs = 0;
        private int attempts = 0;

        private float elapsedTime = 0f;

        private void Awake()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartGame);
            }
        }

        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartGame);
            }
        }

        private void Start()
        {
            if (cardPrefab == null ||
                boardParent == null ||
                timeText == null ||
                attemptsText == null ||
                pairsText == null ||
                victoryPanel == null ||
                victoryStats == null ||
                restartButton == null)
            {
                Debug.LogError(
                    "Heart Memory: Assign all references in the Inspector.",
                    this
                );

                enabled = false;
                return;
            }

            InitializeGame();
        }

        private void Update()
        {
            if (timerStarted && !gameCompleted)
            {
                elapsedTime += Time.deltaTime;
                UpdateStats();
            }
        }

        private void InitializeGame()
        {
            StopAllCoroutines();

            // Remove cards from the previous game.
            foreach (Transform child in boardParent)
            {
                Destroy(child.gameObject);
            }

            firstSelected = null;
            secondSelected = null;

            isResolvingPair = false;
            timerStarted = false;
            gameCompleted = false;

            matchedPairs = 0;
            attempts = 0;
            elapsedTime = 0f;

            victoryPanel.SetActive(false);

            // Show individual statistics.
            timeText.gameObject.SetActive(true);
            attemptsText.gameObject.SetActive(true);
            pairsText.gameObject.SetActive(true);

            BuildDeck();
            ShuffleDeck();
            SpawnCards();

            UpdateStats();

            Debug.Log("Heart Memory: New game started!");
        }

        private void BuildDeck()
        {
            deck.Clear();

            AddPair(
                0,
                "Aorta",
                "Carries oxygenated blood\nfrom the left ventricle\nto the body."
            );

            AddPair(
                1,
                "Pulmonary Artery",
                "Carries deoxygenated blood\nfrom the right ventricle\nto the lungs."
            );

            AddPair(
                2,
                "Left Atrium",
                "Receives oxygenated blood\nfrom the lungs."
            );

            AddPair(
                3,
                "Right Atrium",
                "Receives deoxygenated blood\nfrom the body."
            );

            AddPair(
                4,
                "Left Ventricle",
                "Pumps oxygenated blood\ninto the aorta."
            );

            AddPair(
                5,
                "Right Ventricle",
                "Pumps deoxygenated blood\nto the lungs."
            );
        }

        private void AddPair(
            int pairId,
            string structure,
            string function
        )
        {
            deck.Add(new CardInfo(pairId, structure));
            deck.Add(new CardInfo(pairId, function));
        }

        private void ShuffleDeck()
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);

                CardInfo temporary = deck[i];
                deck[i] = deck[randomIndex];
                deck[randomIndex] = temporary;
            }
        }

        private void SpawnCards()
        {
            foreach (CardInfo cardInfo in deck)
            {
                HeartMemoryCard card =
                    Instantiate(cardPrefab, boardParent);

                card.Initialize(
                    this,
                    cardInfo.PairId,
                    cardInfo.Text
                );
            }
        }

        public void SelectCard(HeartMemoryCard card)
        {
            if (isResolvingPair || gameCompleted)
                return;

            if (card == null || card.IsFaceUp || card.IsMatched)
                return;

            if (!timerStarted)
            {
                timerStarted = true;
            }

            card.Reveal();

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

                Debug.Log(
                    $"Heart Memory: {matchedPairs}/6 pairs found."
                );

                ClearSelectedCards();

                UpdateStats();

                if (matchedPairs == 6)
                {
                    CompleteGame();
                }
            }
            else
            {
                UpdateStats();
                StartCoroutine(HideUnmatchedCards());
            }
        }

        private IEnumerator HideUnmatchedCards()
        {
            isResolvingPair = true;

            yield return new WaitForSeconds(1f);

            firstSelected.Hide();
            secondSelected.Hide();

            ClearSelectedCards();

            isResolvingPair = false;
        }

        private void CompleteGame()
        {
            gameCompleted = true;

            UpdateStats();

            // Hide individual statistics.
            timeText.gameObject.SetActive(false);
            attemptsText.gameObject.SetActive(false);
            pairsText.gameObject.SetActive(false);

            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);

            victoryStats.text =
                $"Time: {minutes:00}:{seconds:00}\n" +
                $"Attempts: {attempts}\n" +
                $"Pairs: {matchedPairs}/6";

            victoryPanel.SetActive(true);

            Debug.Log(
                "Heart Memory: All six pairs have been found!"
            );
        }

        private void UpdateStats()
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);

            timeText.text =
                $"TIME: {minutes:00}:{seconds:00}";

            attemptsText.text =
                $"ATTEMPTS: {attempts}";

            pairsText.text =
                $"PAIRS: {matchedPairs}/6";
        }

        public void RestartGame()
        {
            InitializeGame();
        }

        public void ReturnToMenu()
        {
            SceneManager.LoadScene("HeartQuestMenu");
        }

        private void ClearSelectedCards()
        {
            firstSelected = null;
            secondSelected = null;
        }

        private struct CardInfo
        {
            public int PairId;
            public string Text;

            public CardInfo(int pairId, string text)
            {
                PairId = pairId;
                Text = text;
            }
        }
    }
}
