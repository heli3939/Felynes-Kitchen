using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartSceneBGM : MonoBehaviour
{
    [Header("BGM Settings")]
    public AudioSource bgmSource;          
    public AudioClip bgmClip;              
    public float fadeOutDuration = 1.5f;   

    private bool isFadingOut = false;

    void Start()
    {
        if (bgmSource == null)
            bgmSource = GetComponent<AudioSource>();

        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != gameObject.scene.name && !isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    private IEnumerator FadeOutAndDestroy()
    {
        isFadingOut = true;

        if (bgmSource != null)
        {
            float startVolume = bgmSource.volume;
            float t = 0f;
            while (t < fadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutDuration);
                yield return null;
            }
            bgmSource.Stop();
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
