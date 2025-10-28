using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChecklistManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject checklistPanel;   
    public Button checklistButton;     
    public Button quitChecklistButton;  

    [Header("Audio (Optional)")]
    public AudioSource uiAudioSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;

    private bool isOpen = false;

    void Start()
    {
        if (checklistPanel != null)
            checklistPanel.SetActive(false); 

        if (checklistButton != null)
        {
            checklistButton.onClick.RemoveAllListeners();
            checklistButton.onClick.AddListener(() =>
            {
                PlayClickSound();
                OpenChecklist();
            });
            AddHoverSound(checklistButton);
        }

        if (quitChecklistButton != null)
        {
            quitChecklistButton.onClick.RemoveAllListeners();
            quitChecklistButton.onClick.AddListener(() =>
            {
                PlayClickSound();
                CloseChecklist();
            });
            AddHoverSound(quitChecklistButton);
        }
    }

    public void OpenChecklist()
    {
        if (isOpen) return;

        isOpen = true;
        if (checklistPanel != null)
            checklistPanel.SetActive(true);

        Time.timeScale = 0f;

        foreach (MouseMovement mouse in FindObjectsOfType<MouseMovement>())
        {
            mouse.MuteMouseAudio();
        }
    }

    public void CloseChecklist()
    {
        if (!isOpen) return;

        isOpen = false;
        if (checklistPanel != null)
            checklistPanel.SetActive(false);

        Time.timeScale = 1f;

        foreach (MouseMovement mouse in FindObjectsOfType<MouseMovement>())
        {
            mouse.UnmuteMouseAudio();
        }

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
            uiAudioSource.PlayOneShot(hoverClip);
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
