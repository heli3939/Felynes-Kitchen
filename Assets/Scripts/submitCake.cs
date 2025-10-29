using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SubmitCakeUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject submitButtonPanel;
    public Button submitButton;
    public TextMeshProUGUI submitPromptText;
    
    [Header("Confirmation Panel")]
    public GameObject confirmationPanel;
    public TextMeshProUGUI confirmationText;
    public Button yesButton;
    public Button noButton;

    [Header("Game References")]
    public QTEManager qteManager;
    public HintUI hintUI;

    [Header("Player Control Scripts")]
    public MonoBehaviour[] playerControlScripts;

    [Header("Audio Settings")]
    public AudioSource uiAudioSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;

    private void Start()
    {
        if (submitButtonPanel != null)
            submitButtonPanel.SetActive(true);
            
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
        
        if (submitPromptText != null)
            submitPromptText.text = "Can't find more and want to score your play?";
        
        //if (confirmationText != null)
        //    confirmationText.text = "Are you sure you want to quit?";

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
            AddHoverSound(submitButton);
        }

        if (yesButton != null)
        {
            yesButton.onClick.AddListener(OnYesClicked);
            AddHoverSound(yesButton);
        }

        if (noButton != null)
        {
            noButton.onClick.AddListener(OnNoClicked);
            AddHoverSound(noButton);
        }
    }

    private void OnSubmitClicked()
    {
        PlayClickSound();
        Debug.Log("[SubmitCakeUI] Submit button clicked - showing confirmation");
        
        if (confirmationPanel != null)
            confirmationPanel.SetActive(true);
    }

    private void OnYesClicked()
    {
        PlayClickSound();
        Debug.Log("[SubmitCakeUI] Yes clicked - triggering game ending");
        
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
            
        if (submitButtonPanel != null)
            submitButtonPanel.SetActive(false);
        
        StartCoroutine(TriggerGameEndingSequence());
    }

    private void OnNoClicked()
    {
        PlayClickSound();
        Debug.Log("[SubmitCakeUI] No clicked - returning to game");
        
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }

    private IEnumerator TriggerGameEndingSequence()
    {
        Debug.Log("[SubmitCakeUI] Starting game ending sequence...");
        
        if (qteManager != null)
        {
            qteManager.SetPlayerControls(false);
        }
        else
        {
            SetPlayerControls(false);
        }
        
        yield return new WaitForSecondsRealtime(0.5f);
        
        Debug.Log("[SubmitCakeUI] Pausing game...");
        Time.timeScale = 0f;
        
        string endingType = ScoreSystem.Instance.GetEndingType();
        int finalScore = ScoreSystem.Instance.score;
        
        Debug.Log($"[SubmitCakeUI] Final Score: {finalScore} | Ending Type: {endingType}");
        
        if (hintUI != null)
        {
            if (endingType == "BE")
            {
                hintUI.ShowHint("Sorry Felyne, you failed...");
                Debug.Log("[SubmitCakeUI] Bad Ending displayed");
                StartCoroutine(LoadEndingScene("BadEnding"));
            }
            else if (endingType == "HE")
            {
                hintUI.ShowHint("Congrats Felyne, enjoy your cake!");
                Debug.Log("[SubmitCakeUI] Happy Ending displayed");
                StartCoroutine(LoadEndingScene("HappyEnding"));
            }
            else
            {
                hintUI.ShowHint("Cake complete! Score: " + finalScore);
                Debug.Log("[SubmitCakeUI] Neutral Ending displayed");
            }
        }
        else
        {
            Debug.LogWarning("[SubmitCakeUI] HintUI reference is missing!");
        }
    }

    private void SetPlayerControls(bool enabled)
    {
        foreach (var script in playerControlScripts)
        {
            if (script != null)
                script.enabled = enabled;
        }
        Debug.Log($"[SubmitCakeUI] Player controls set to {(enabled ? "ENABLED" : "DISABLED")}");
    }
    
    private void OnDestroy()
    {
        if (submitButton != null)
            submitButton.onClick.RemoveListener(OnSubmitClicked);
            
        if (yesButton != null)
            yesButton.onClick.RemoveListener(OnYesClicked);
            
        if (noButton != null)
            noButton.onClick.RemoveListener(OnNoClicked);
    }

    private IEnumerator LoadEndingScene(string sceneName)
    {
        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private void AddHoverSound(Button button)
    {
        if (button == null) return;
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
            uiAudioSource.pitch = 1f;
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