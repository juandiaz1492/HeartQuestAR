
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HeartQuizManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string question;
        public string[] answers;
        public int correctAnswer;
    }

    public TMP_Text questionText;
    public TMP_Text scoreText;
    public TMP_Text feedbackText;

    public Button[] answerButtons;
    public TMP_Text[] answerLabels;

    public Button nextButton;
    public TMP_Text nextButtonText;

    // New: return to menu button
    public Button backToMenuButton;

    private int currentQuestion = 0;
    private int score = 0;
    private bool answered = false;

    private Question[] questions;

    private Image[] answerImages;
    private Color[] originalColors;

    private Color correctColor = new Color32(80, 180, 110, 255);
    private Color incorrectColor = new Color32(210, 75, 75, 255);

    void Start()
    {
        CreateQuestions();

        // Save original button colors
        answerImages = new Image[answerButtons.Length];
        originalColors = new Color[answerButtons.Length];

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;

            answerImages[i] = answerButtons[i].GetComponent<Image>();

            if (answerImages[i] != null)
                originalColors[i] = answerImages[i].color;

            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => CheckAnswer(index));
        }

        nextButton.gameObject.SetActive(false);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextQuestion);

        if (backToMenuButton != null)
        {
            backToMenuButton.gameObject.SetActive(false);
            backToMenuButton.onClick.RemoveAllListeners();
            backToMenuButton.onClick.AddListener(BackToMenu);
        }

        ShowQuestion();
    }

    void CreateQuestions()
    {
        questions = new Question[]
        {
            new Question
            {
                question = "Which chamber pumps oxygenated blood to the body?",
                answers = new string[]
                {
                    "Right Atrium",
                    "Left Ventricle",
                    "Left Atrium",
                    "Right Ventricle"
                },
                correctAnswer = 1
            },

            new Question
            {
                question = "Which chamber receives oxygenated blood from the lungs?",
                answers = new string[]
                {
                    "Right Ventricle",
                    "Right Atrium",
                    "Left Atrium",
                    "Left Ventricle"
                },
                correctAnswer = 2
            },

            new Question
            {
                question = "Which valve separates the left atrium and left ventricle?",
                answers = new string[]
                {
                    "Tricuspid Valve",
                    "Pulmonary Valve",
                    "Aortic Valve",
                    "Mitral Valve"
                },
                correctAnswer = 3
            },

            new Question
            {
                question = "Which blood vessel carries blood from the heart to the lungs?",
                answers = new string[]
                {
                    "Pulmonary Artery",
                    "Aorta",
                    "Pulmonary Vein",
                    "Vena Cava"
                },
                correctAnswer = 0
            },

            new Question
            {
                question = "What is the largest artery in the human body?",
                answers = new string[]
                {
                    "Pulmonary Artery",
                    "Aorta",
                    "Carotid Artery",
                    "Coronary Artery"
                },
                correctAnswer = 1
            }
        };
    }

    void ShowQuestion()
    {
        answered = false;
        nextButton.gameObject.SetActive(false);

        if (backToMenuButton != null)
            backToMenuButton.gameObject.SetActive(false);

        feedbackText.text = "";

        questionText.text = questions[currentQuestion].question;

        scoreText.text = "SCORE: " + score + "/" + questions.Length;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].gameObject.SetActive(true);
            answerLabels[i].text = questions[currentQuestion].answers[i];
            answerButtons[i].interactable = true;

            // Restore original button color
            if (answerImages[i] != null)
                answerImages[i].color = originalColors[i];
        }
    }

    void CheckAnswer(int selectedAnswer)
    {
        if (answered) return;

        answered = true;

        int correct = questions[currentQuestion].correctAnswer;

        if (selectedAnswer == correct)
        {
            score++;
            feedbackText.text = "CORRECT! Well done!";
        }
        else
        {
            feedbackText.text = "INCORRECT! Correct answer: " +
                questions[currentQuestion].answers[correct];
        }

        // Highlight correct and incorrect answers
        if (answerImages[correct] != null)
            answerImages[correct].color = correctColor;

        if (selectedAnswer != correct && answerImages[selectedAnswer] != null)
            answerImages[selectedAnswer].color = incorrectColor;

        scoreText.text = "SCORE: " + score + "/" + questions.Length;

        foreach (Button button in answerButtons)
        {
            button.interactable = false;
        }

        nextButton.gameObject.SetActive(true);
        nextButtonText.text = "NEXT";
    }

    void NextQuestion()
    {
        currentQuestion++;

        if (currentQuestion >= questions.Length)
        {
            ShowResults();
        }
        else
        {
            ShowQuestion();
        }
    }


    void ShowResults()
    {
        questionText.text = "QUIZ COMPLETED!";

        // Show final score separately
        scoreText.text = "FINAL SCORE: " + score + "/" + questions.Length;

        // Congratulations message
        feedbackText.text = "CONGRATULATIONS!";

        foreach (Button button in answerButtons)
        {
            button.gameObject.SetActive(false);
        }

        nextButtonText.text = "PLAY AGAIN";
        nextButton.gameObject.SetActive(true);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(RestartQuiz);

        if (backToMenuButton != null)
            backToMenuButton.gameObject.SetActive(true);
    }


    void RestartQuiz()
    {
        currentQuestion = 0;
        score = 0;

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextQuestion);

        ShowQuestion();
    }

    void BackToMenu()
    {
        SceneManager.LoadScene("HeartQuestMenu");
    }
}
