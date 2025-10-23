using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;

public class StartMenuController : MonoBehaviour
{
    [Header("CanvasGroups")]
    public CanvasGroup startGroup;
    public CanvasGroup cgGroup;

    [Header("UI References")]
    public Button startButton;
    public Button skipButton;
    public Image cgImage1;
    public TMP_Text cgText;

    [TextArea] public string[] cgLines; 
    public float fadeDuration = 0.2f;
    public float textDisplayTime = 3f;

    [Header("Scene Settings")]
    public string mainSceneName = "MainScene";

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;

    private bool isSkipping = false;

    void Start()
    {
        cgImage1.gameObject.SetActive(false);

        SetGroup(startGroup, 1, true, true);
        SetGroup(cgGroup, 0, false, false);

        if (cgText != null)
            cgText.text = "";

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(OnStartClicked);
        AddHoverSound(startButton);

        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(SkipCG);
        AddHoverSound(skipButton);
    }

    void OnStartClicked()
    {
        StartCoroutine(TransitionToCG());
    }

    IEnumerator TransitionToCG()
    {
        // start → cg 
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / fadeDuration);
            startGroup.alpha = 1 - a;
            cgGroup.alpha = a;
            yield return null;
        }
        SetGroup(startGroup, 0, false, false);
        SetGroup(cgGroup, 1, true, true);

        cgImage1.gameObject.SetActive(true);
        for (int i = 0; i < cgLines.Length; i++)
        {
            if (isSkipping) yield break;
            yield return StartCoroutine(PlaySingleLine(cgImage1, cgLines[i]));
        }
        SceneManager.LoadScene(mainSceneName);
    }

    IEnumerator PlaySingleLine(Image img, string text)
    {
        cgText.text = "";

        if (img.color.a < 1f)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                if (isSkipping) yield break;
                t += Time.deltaTime;
                img.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, t / fadeDuration));
                yield return null;
            }
        }

        cgText.text = "";
        foreach (char c in text)
        {
            if (isSkipping) yield break;
            cgText.text += c;
            yield return new WaitForSeconds(0.03f); 
        }

        float stay = 0f;
        while (stay < textDisplayTime)
        {
            if (isSkipping) yield break;
            stay += Time.deltaTime;
            yield return null;
        }
    }

    void SkipCG()
    {
        if (!cgGroup.gameObject.activeInHierarchy) return;
        isSkipping = true;
        SceneManager.LoadScene(mainSceneName);
    }

    void SetGroup(CanvasGroup g, float alpha, bool interact, bool block)
    {
        g.alpha = alpha;
        g.interactable = interact;
        g.blocksRaycasts = block;
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
