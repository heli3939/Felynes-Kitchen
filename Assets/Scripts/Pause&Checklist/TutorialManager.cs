using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TutorialManager : MonoBehaviour
{
    [Header("Page Images")]
    public GameObject[] pages; 

    [Header("Buttons")]
    public Button nextButton;
    public Button prevButton;
    public Button exitButton;

    [Header("Audio")]
    public AudioSource uiAudioSource;  
    public AudioClip hoverClip;
    public AudioClip clickClip;

    private int currentPage = 0;

    void Start()
    {
        ShowPage(0);

        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PrevPage);
        exitButton.onClick.AddListener(CloseTutorial);

        AddHoverSound(nextButton);
        AddHoverSound(prevButton);
        AddHoverSound(exitButton);
    }

    void ShowPage(int index)
    {
        if (pages == null || pages.Length == 0) return;

        index = Mathf.Clamp(index, 0, pages.Length - 1);

        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == index);

        currentPage = index;

        prevButton.interactable = (index > 0);
        nextButton.interactable = (index < pages.Length - 1);
    }

    public void NextPage()
    {
        if (currentPage < pages.Length - 1)
        {
            ShowPage(currentPage + 1);
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            ShowPage(currentPage - 1);
        }
    }

    public void OpenTutorial()
    {
        gameObject.SetActive(true);
        ShowPage(0);
        Time.timeScale = 0f; 
    }

    public void CloseTutorial()
    {
        gameObject.SetActive(false);
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

