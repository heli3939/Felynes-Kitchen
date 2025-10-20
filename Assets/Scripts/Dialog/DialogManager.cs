using UnityEngine;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public TestDialogue testDialogue;
    public bool playOnStart = true;

    public PauseMenu pauseMenu;
    public Checklist checklist;

    void Start()
    {
        if (playOnStart && testDialogue != null)
        {
            StartCoroutine(StartOpeningDialogue());
        }
    }

    private IEnumerator StartOpeningDialogue()
    {
        Time.timeScale = 0f;
        InputBlocker.Lock(true);

        testDialogue.StartDialogue(testDialogue.lines);

        yield return new WaitUntil(() => testDialogue.IsFinished);

        InputBlocker.Lock(false);
        Time.timeScale = 1f;

        pauseMenu.OnGameStarted();
        if (pauseMenu.checklist != null)           // OR inside PauseMenu.OnGameStarted()
            pauseMenu.checklist.OnGameStarted();   // unlock the checklist button
    }
}

