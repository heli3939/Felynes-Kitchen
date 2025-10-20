using UnityEngine;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public TestDialogue testDialogue; 
    public bool playOnStart = true;

    void Start()
    {
        if (playOnStart && testDialogue != null)
        {
            StartCoroutine(StartOpeningDialogue());
        }
    }

    private IEnumerator StartOpeningDialogue()
    {
        if (BGMManager.Instance != null)
            BGMManager.Instance.MuteBGM();

        Time.timeScale = 0f;
        InputBlocker.Lock(true);

        testDialogue.StartDialogue(testDialogue.lines);

        yield return new WaitUntil(() => testDialogue.IsFinished);

        InputBlocker.Lock(false);
        Time.timeScale = 1f;

        if (BGMManager.Instance != null)
            BGMManager.Instance.UnmuteBGM();
    }
}

