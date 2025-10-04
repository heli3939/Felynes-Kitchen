using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button tutorialButton;
    public Button exitButton;

    private bool isPaused = false;

    void Start()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(() =>
            {
                Debug.Log("PauseButton clicked");
                OpenPause();
            });

        if (resumeButton != null)
            resumeButton.onClick.AddListener(() =>
            {
                Debug.Log("ResumeButton clicked");
                ResumeGame();
            });

        if (restartButton != null)
            restartButton.onClick.AddListener(() =>
            {
                Debug.Log("RestartButton clicked");
                RestartGame();
            });

        if (tutorialButton != null)
            tutorialButton.onClick.AddListener(() =>
            {
                Debug.Log("TutorialButton clicked");
                OpenTutorial();
            });

        if (exitButton != null)
            exitButton.onClick.AddListener(() =>
            {
                Debug.Log("ExitButton clicked");
                QuitGame();
            });

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                OpenPause();
        }
    }

    public void OpenPause()
    {
        isPaused = true;
        if (pausePanel != null)
            pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OpenTutorial()
    {
        Time.timeScale = 1f;
        Debug.Log("Open tutorial");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
