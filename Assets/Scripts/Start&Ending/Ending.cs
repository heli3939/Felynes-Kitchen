using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class EndSceneController : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup cgCanvas;         
    public TMP_Text endText;             
    public CanvasGroup blackCanvas;      

    [Header("Text Settings")]
    [TextArea] public string[] lines;   
    public float charDelay = 0.03f;      
    public float lineStayTime = 3f;      
    public float fadeOutDuration = 2f;

    [Header("Typing Audio")]
    public AudioSource audioSource;     
    public AudioClip typeSound;         
    public int charsPerSound = 2;        

    [Header("Scene Settings")]
    public string startSceneName = "StartScene";

    void Start()
    {
        if (cgCanvas != null)
            cgCanvas.alpha = 1f;
        if (blackCanvas != null)
            blackCanvas.alpha = 0f;
        if (endText != null)
            endText.text = "";

        StartCoroutine(PlayEndingSequence());
    }

    private IEnumerator PlayEndingSequence()
    {
        foreach (string line in lines)
        {
            yield return StartCoroutine(TypeText(line));
            yield return new WaitForSeconds(lineStayTime);
        }

        yield return StartCoroutine(FadeToBlack());

        SceneManager.LoadScene(startSceneName);
    }

    private IEnumerator TypeText(string text)
    {
        if (endText == null) yield break;

        endText.text = "";
        int charCount = 0;

        foreach (char c in text)
        {
            endText.text += c;
            charCount++;

            if (audioSource && typeSound && charCount % charsPerSound == 0)
                audioSource.PlayOneShot(typeSound);

            yield return new WaitForSeconds(charDelay);
        }
    }

    private IEnumerator FadeToBlack()
    {
        if (blackCanvas == null) yield break;

        float t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            blackCanvas.alpha = Mathf.Lerp(0f, 1f, t / fadeOutDuration);
            yield return null;
        }

        blackCanvas.alpha = 1f;
    }
}
