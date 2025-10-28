using UnityEngine;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("Refs")]
    public TestDialogue testDialogue;
    public PauseMenu pauseMenu;
    public Checklist checklist;
    public GameObject cat;

    [Header("Behaviour")]
    public bool playOnStart = true;

    [Header("Tutorial After Dialogue")]
    public IntroTutorialManager introTutorialManager;  

    [Tooltip("Block input via InputBlocker.Lock(true/false). If your project doesn't use InputBlocker, untick this.")]
    public bool useInputBlocker = true;
    public TutorialManager tutorialManager;   

    void Start()
    {

        if (introTutorialManager != null && introTutorialManager.gameObject.activeSelf)
        {
            introTutorialManager.gameObject.SetActive(false);
        }

        if (PlayerPrefs.GetInt("SkipOpeningDialogue", 0) == 1)
        {
            Debug.Log("[DialogueManager] SkipOpeningDialogue detected — starting game immediately.");
            PlayerPrefs.DeleteKey("SkipOpeningDialogue"); 
            FinalizeStart(); 
            return;
        }

        if (!playOnStart) return;

        // Validate before starting to avoid NREs
        if (testDialogue == null)
        {
            Debug.LogError("[DialogueManager] Missing TestDialogue reference. Assign it in the Inspector.");
            return;
        }

        if (testDialogue.lines == null || testDialogue.lines.Length == 0)
        {
            Debug.LogWarning("[DialogueManager] testDialogue.lines is empty. Skipping opening dialogue.");
            // Still mark game started so other systems can proceed.
            FinalizeStart();
            return;
        }

        StartCoroutine(StartOpeningDialogue());
    }

    private IEnumerator StartOpeningDialogue()
    {
        if (BGMManager.Instance != null)
            BGMManager.Instance.MuteBGM();

        foreach (RandomFlame flame in FindObjectsOfType<RandomFlame>())
        {
            if (flame.flameAudioSource != null)
                flame.flameAudioSource.mute = true;
        }

        foreach (MouseMovement mouse in FindObjectsOfType<MouseMovement>())
        {
            mouse.MuteMouseAudio();
        }

        // Pause gameplay while opening dialogue plays
        var prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (useInputBlocker)
        {
            // Guard in case InputBlocker throws
            try { InputBlocker.Lock(true); }
            catch { Debug.LogWarning("[DialogueManager] InputBlocker not available; continuing without it."); }
        }

        // Start the dialogue safely
        try
        {
            testDialogue.StartDialogue(testDialogue.lines);
        }
        catch (System.Exception e)
        {
            // Restore state and bail
            if (useInputBlocker) { try { InputBlocker.Lock(false); } catch { } }
            Time.timeScale = prevTimeScale;
            yield break;
        }

        // Wait until dialogue finishes (guard against destroyed/disabled refs)
        yield return new WaitUntil(() =>
        {
            if (testDialogue == null)
            {
                Debug.LogWarning("[DialogueManager] TestDialogue was destroyed or unassigned during playback; ending early.");
                return true; // end wait
            }
            return testDialogue.IsFinished;
        });

        // Unblock and resume time
        if (useInputBlocker) { try { InputBlocker.Lock(false); } catch { } }
        Time.timeScale = 1f; // resume gameplay regardless of previous (assumes opening always starts from gameplay)

        if (BGMManager.Instance != null) BGMManager.Instance.UnmuteBGM();
        foreach (MouseMovement mouse in FindObjectsOfType<MouseMovement>()) mouse.UnmuteMouseAudio();
        yield return new WaitForSeconds(0.5f);
        foreach (RandomFlame flame in FindObjectsOfType<RandomFlame>())
        {
            if (flame.flameAudioSource != null)
                flame.flameAudioSource.mute = false;
        }

        FinalizeStart();
    }

    private void FinalizeStart()
    {
        // Notify PauseMenu the game has started (optional)
        if (pauseMenu != null)
        {
            try { pauseMenu.OnGameStarted(); }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[DialogueManager] pauseMenu.OnGameStarted() threw: {e.Message}");
            }
        }
        else
        {
            Debug.Log("[DialogueManager] pauseMenu not assigned (optional) — skipped OnGameStarted().");
        }

        // Enable checklist interactions after game start (optional)
        if (checklist != null)
        {
            try { checklist.SetGameStarted(true); }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[DialogueManager] checklist.SetGameStarted(true) threw: {e.Message}");
            }
        }
        else
        {
            Debug.Log("[DialogueManager] checklist not assigned (optional) — skipped SetGameStarted(true).");
        }

        if (testDialogue != null && testDialogue.dialoguePanel != null)
            testDialogue.dialoguePanel.SetActive(false);

        if (PlayerPrefs.GetInt("SkipOpeningDialogue", 0) == 1)
        {
            Debug.Log("[DialogueManager] Skipping cat shrink animation (restart detected).");

            PlayerPrefs.DeleteKey("SkipOpeningDialogue");

            if (cat != null)
                cat.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            return; 
        }

        StartCoroutine(PlayCatShrinkAndOpenIntroTutorial());
    }

    private IEnumerator PlayCatShrinkAnimation()
    {
        Transform catTransform = cat.transform;
        Vector3 startScale = new Vector3(2f, 2f, 2f);
        Vector3 endScale = new Vector3(0.4f, 0.4f, 0.4f);
        float duration = 1.0f;
        float elapsed = 0f;

        Debug.Log("[DialogueManager] Starting cat shrink animation (scale: 2 → 0.4)");

        catTransform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            catTransform.localScale = Vector3.Lerp(startScale, endScale, smoothT);

            yield return null;
        }

        catTransform.localScale = endScale;

        Debug.Log("[DialogueManager] Cat shrink animation completed.");
    }

    private IEnumerator PlayCatShrinkAndOpenIntroTutorial()
    {
        Transform catTransform = cat.transform;
        Vector3 startScale = new Vector3(2f, 2f, 2f);
        Vector3 endScale = new Vector3(0.4f, 0.4f, 0.4f);
        float duration = 1.0f;
        float elapsed = 0f;

        catTransform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            catTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        catTransform.localScale = endScale;

        yield return new WaitForSecondsRealtime(0.1f);

        if (introTutorialManager != null)
        {
            introTutorialManager.gameObject.SetActive(true);   
            introTutorialManager.OpenTutorial();               
        }
        else
        {
            Debug.LogWarning("[DialogueManager] introTutorialManager not assigned!");
        }
    }

    // Call this if you need to trigger the opening dialogue manually instead of playOnStart.
    public void StartOpeningDialogueSafe()
    {
        StopAllCoroutines();

        if (testDialogue == null)
        {
            return;
        }
        if (testDialogue.lines == null || testDialogue.lines.Length == 0)
        {
            FinalizeStart();
            return;
        }

        StartCoroutine(StartOpeningDialogue());
    }
}
