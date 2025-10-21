using UnityEngine;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("Refs")]
    public TestDialogue testDialogue;
    public PauseMenu pauseMenu;
    public Checklist checklist;

    [Header("Behaviour")]
    public bool playOnStart = true;
    [Tooltip("Block input via InputBlocker.Lock(true/false). If your project doesn't use InputBlocker, untick this.")]
    public bool useInputBlocker = true;

    void Start()
    {
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
            Debug.LogError($"[DialogueManager] Exception starting dialogue: {e.Message}\n{e.StackTrace}");
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
    }

    // Call this if you need to trigger the opening dialogue manually instead of playOnStart.
    public void StartOpeningDialogueSafe()
    {
        StopAllCoroutines();

        if (testDialogue == null)
        {
            Debug.LogError("[DialogueManager] Missing TestDialogue reference.");
            return;
        }
        if (testDialogue.lines == null || testDialogue.lines.Length == 0)
        {
            Debug.LogWarning("[DialogueManager] testDialogue.lines is empty. Skipping.");
            FinalizeStart();
            return;
        }

        StartCoroutine(StartOpeningDialogue());
    }
}
