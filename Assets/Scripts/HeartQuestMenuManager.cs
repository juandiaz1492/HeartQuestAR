
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartQuestMenuManager : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("ExploreHeart");
    }

    public void HeartQuiz()
    {
        SceneManager.LoadScene("QuizChallenge");
    }

    public void Minigames()
    {
        SceneManager.LoadScene("HeartMemory");
    }

    public void Exit()
    {
        Debug.Log("Closing HeartQuest AR");
        Application.Quit();
    }
}

