using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button tutorialButton;
    public Button exitButton;
    public Button checklistButton;

    [Header("Tutorial Panel")]
    public GameObject tutorialPanel;
    public Button closeTutorialButton;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;

    [Header("Scene Settings")]
    public string startSceneName = "StartScene";
    public string mainSceneName = "MainScene";

    [Header("UI Hint")]
    public HintUI hintUI;

    private bool isPaused = false;
    private bool gameStarted = false;

    void Start()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(() => { OpenPause(); });
            AddHoverSound(pauseButton);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(() => { PlayClickSound(); ResumeGame(); });
            AddHoverSound(resumeButton);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => { PlayClickSound(); RestartGame(); });
            AddHoverSound(restartButton);
        }

        if (tutorialButton != null)
        {
            tutorialButton.onClick.RemoveAllListeners();
            tutorialButton.onClick.AddListener(() => { PlayClickSound(); OpenTutorial(); });
            AddHoverSound(tutorialButton);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(() => { PlayClickSound(); QuitGame(); });
            AddHoverSound(exitButton);
        }

        if (closeTutorialButton != null)
        {
            closeTutorialButton.onClick.RemoveAllListeners();
            closeTutorialButton.onClick.AddListener(() => { PlayClickSound(); CloseTutorial(); });
            AddHoverSound(closeTutorialButton);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        UpdateButtonStates();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (tutorialPanel != null && tutorialPanel.activeSelf)
            {
                CloseTutorial();
            }
            else if (!isPaused && gameStarted)
            {
                OpenPause();
            }
        }
    }

    public void OpenPause()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        if (checklistButton != null) checklistButton.interactable = false;
        Time.timeScale = 0f;

        foreach (MouseMovement mouse in FindObjectsOfType<MouseMovement>())
        {
            mouse.MuteMouseAudio();
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        UpdateButtonStates();
        Time.timeScale = 1f;

        foreach (MouseMovement mouse in FindObjectsOfType<MouseMovement>())
        {
            mouse.UnmuteMouseAudio();
        }
    }

    public void RestartGame()
    {
        PlayerPrefs.SetInt("SkipOpeningDialogue", 1);
        PlayerPrefs.Save();
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.ResetGameScore();
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainSceneName);
    }

    public void OpenTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
        if (hintUI != null)
        {
            hintUI.ShowHint("press ESC to close tutorial");
        }
    }

    public void CloseTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        
        if (hintUI != null)
        {
            hintUI.ShowHint("");
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(startSceneName);
    }

    public void OnChecklistOpened()
    {
        if (pauseButton != null) pauseButton.interactable = false;
    }

    public void OnChecklistClosed()
    {
        UpdateButtonStates();
    }

    public void OnGameStarted()
    {
        gameStarted = true;
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (pauseButton != null) pauseButton.interactable = gameStarted && !isPaused;
        if (checklistButton != null) checklistButton.interactable = gameStarted && !isPaused;
    }

    private void AddHoverSound(Button button)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener((eventData) => { PlayHoverSound(); });
        trigger.triggers.Add(entry);
    }

    private void PlayHoverSound()
    {
        if (uiAudioSource != null && hoverClip != null)
        {
            uiAudioSource.PlayOneShot(hoverClip);
        }
    }

    private void PlayClickSound()
    {
        if (uiAudioSource != null && clickClip != null)
        {
            uiAudioSource.pitch = 1f;
            uiAudioSource.PlayOneShot(clickClip);
        }
    }
}