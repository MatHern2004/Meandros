using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TitleScreenUI : MonoBehaviour
{
    public static bool isReturningFromPause = false;

    [Header("Scene Settings")]
    [SerializeField] private string gameplaySceneName = "Labyrinth";

    [Header("UI References")]
    [SerializeField] private TMP_Text playButtonText;
    [SerializeField] private GameObject quitButtonObject; // Optional: leave empty until needed

    private void Start()
    {
        Time.timeScale = 1f; // Ensures time isn't still frozen if coming back from a pause menu

        if (isReturningFromPause)
        {
            if (playButtonText != null) playButtonText.text = "RESUME";
            if (quitButtonObject != null) quitButtonObject.SetActive(true);
        }
        else
        {
            if (playButtonText != null) playButtonText.text = "PLAY";
            if (quitButtonObject != null) quitButtonObject.SetActive(false);
        }
    }

    public void StartGame()
    {
        isReturningFromPause = false;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}

