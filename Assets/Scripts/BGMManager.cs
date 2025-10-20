using UnityEngine;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [Header("Audio Source")]
    public AudioSource bgmSource;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (bgmSource != null)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void MuteBGM()
    {
        if (bgmSource != null)
            bgmSource.mute = true;
    }

    public void UnmuteBGM()
    {
        if (bgmSource != null)
            bgmSource.mute = false;
    }

    private IEnumerator FadeVolume(float targetVolume)
    {
        float startVolume = bgmSource.volume;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime; 
            bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, time / fadeDuration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }
}
