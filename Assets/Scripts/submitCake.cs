using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

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

    private void Start()
    {
        if (submitButtonPanel != null)
            submitButtonPanel.SetActive(true);
            
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
        
        if (submitPromptText != null)
            submitPromptText.text = "Can't find more and want to score your play?";
        
        if (confirmationText != null)
            confirmationText.text = "Are you sure you want to quit?";
        
        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitClicked);
            
        if (yesButton != null)
            yesButton.onClick.AddListener(OnYesClicked);
            
        if (noButton != null)
            noButton.onClick.AddListener(OnNoClicked);
    }

    private void OnSubmitClicked()
    {
        Debug.Log("[SubmitCakeUI] Submit button clicked - showing confirmation");
        
        if (confirmationPanel != null)
            confirmationPanel.SetActive(true);
    }

    private void OnYesClicked()
    {
        Debug.Log("[SubmitCakeUI] Yes clicked - triggering game ending");
        
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
            
        if (submitButtonPanel != null)
            submitButtonPanel.SetActive(false);
        
        StartCoroutine(TriggerGameEndingSequence());
    }

    private void OnNoClicked()
    {
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
            }
            else if (endingType == "HE")
            {
                hintUI.ShowHint("Congrats Felyne, enjoy your cake!");
                Debug.Log("[SubmitCakeUI] Happy Ending displayed");
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
}