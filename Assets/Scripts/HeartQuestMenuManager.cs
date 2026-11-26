
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartQuestMenuManager : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("HeartQuestDemo");
    }

    public void HeartQuiz()
    {
        SceneManager.LoadScene("HeartQuiz");
    }

    public void Minigames()
    {
        SceneManager.LoadScene("HeartMemoryAR");
    }

    public void Exit()
    {
        Debug.Log("Closing HeartQuest AR");
        Application.Quit();
    }
}
