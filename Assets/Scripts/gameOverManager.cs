using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel;
    public Image backgroundFade;
    public TextMeshProUGUI youDiedText;

    [Header("Animation Settings")]
    public float fadeDuration = 1.5f;       
    public float textAppearDelay = 0.5f;   
    public float blackScreenDelay = 2.5f;   
    public float restartDelay = 1.5f;       

    [Header("Other References")]
    public AudioSource deathSFX;

    private bool isShowingGameOver = false;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (youDiedText != null) youDiedText.alpha = 0f;
        if (backgroundFade != null)
            backgroundFade.color = new Color(backgroundFade.color.r, backgroundFade.color.g, backgroundFade.color.b, 0f);
    }

    public void ShowGameOver()
    {
        if (isShowingGameOver)
        {
            Debug.LogWarning("[GameOver] Already showing game over, ignoring duplicate call");
            return;
        }
        
        isShowingGameOver = true;
        Debug.Log("[GameOver] Starting death sequence");

        if (BGMManager.Instance != null)
            BGMManager.Instance.MuteBGM();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
            Cursor.visible = false;
        }

        if (deathSFX != null) deathSFX.Play();

        StartCoroutine(PlayDeathSequence());
    }

    private IEnumerator PlayDeathSequence()
    {
        float t = 0f;
        Color startColor = backgroundFade.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0.7f);

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            backgroundFade.color = Color.Lerp(startColor, endColor, t / fadeDuration);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(textAppearDelay);
        
        Debug.Log($"[GameOver] Showing text. youDiedText is null? {youDiedText == null}");
        
        if (youDiedText != null)
        {
            Debug.Log("[GameOver] Setting youDiedText alpha");
            youDiedText.alpha = 0f;
            Vector3 startScale = Vector3.one * 1.3f;
            Vector3 endScale = Vector3.one;
            youDiedText.transform.localScale = startScale;

            float duration = 1.2f;
            t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                youDiedText.alpha = Mathf.Lerp(0f, 1f, t / duration);
                youDiedText.transform.localScale = Vector3.Lerp(startScale, endScale, t / duration);
                yield return null;
            }
            Debug.Log($"[GameOver] Text should be visible now. Alpha = {youDiedText.alpha}");
        }

        yield return new WaitForSecondsRealtime(blackScreenDelay);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        float blackT = 0f;
        while (blackT < 1f)
        {
            blackT += Time.unscaledDeltaTime / 1.0f; 
            backgroundFade.color = new Color(0, 0, 0, Mathf.Lerp(0.85f, 1f, blackT));
            youDiedText.alpha = Mathf.Lerp(1f, 0f, blackT); 
            yield return null;
        }

        yield return new WaitForSecondsRealtime(restartDelay);
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.ResetGameScore();
        }
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}