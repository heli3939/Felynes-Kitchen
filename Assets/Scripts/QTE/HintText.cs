using UnityEngine;
using TMPro;
using System.Collections;

public class HintUI : MonoBehaviour
{
    public TMP_Text hintText;  
    public float displayTime = 2.0f;  

    private Coroutine hideRoutine;

    private void Awake()
    {
        if (hintText == null)
            hintText = GetComponent<TMP_Text>();

        gameObject.SetActive(false);
    }

    public void ShowHint(string message)
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hintText.text = message;
        gameObject.SetActive(true);
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        gameObject.SetActive(false);
        hideRoutine = null;
    }
}
