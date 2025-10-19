using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(Collider))]
public class RandomFlame : MonoBehaviour
{
    [Header("Timing & Chance")]
    [SerializeField] private float checkInterval = 3.0f;
    [Range(0f,1f)] [SerializeField] private float popChance = 0.25f;
    [SerializeField] private float activeDuration = 3.0f;

    [Header("Warning Light (Built-in)")]
    [SerializeField] private float warnLead = 0.3f;
    [SerializeField] private Light warnLight;
    [SerializeField] private float warnLightIntensity = 8f;
    [SerializeField] private float warnLightRange = 5f;
    [SerializeField] private Color warnLightColor = new Color(1f,0.8f,0.5f);
    [Range(0f,1f)] [SerializeField] private float warnEase = 0.2f;
    [SerializeField] private bool autoCreateWarnLight = true;

    [Header("Particles & Visuals")]
    [SerializeField] private ParticleSystem flameParticles;
    [SerializeField] private bool scaleTransform = true;
    [SerializeField] private float rampTime = 0.2f;
    [SerializeField] private Vector3 activeScale = new Vector3(1.1f,1.1f,1.1f);

    private Collider triggerCol;
    private bool damagingNow;
    private bool sequenceRunning;
    private Vector3 initialScale;
    private System.Random rng;

    private void Awake()
    {
        triggerCol = GetComponent<Collider>();
        triggerCol.isTrigger = true;

        if (!flameParticles) flameParticles = GetComponent<ParticleSystem>();

        if (flameParticles.isPlaying)
            flameParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // independent per-instance RNG
        int seed = unchecked(GetInstanceID() ^ System.Environment.TickCount);
        rng = new System.Random(seed);

        // seed once (safe)
#pragma warning disable 618
        flameParticles.useAutoRandomSeed = false;
        flameParticles.randomSeed = (uint)rng.Next(1, int.MaxValue);
#pragma warning restore 618

        var main = flameParticles.main;
        main.playOnAwake = false;
        main.loop = false;

        // ---- Built-in lighting setup ----
#if !UNITY_URP && !UNITY_RENDER_PIPELINE_UNIVERSAL
        QualitySettings.pixelLightCount = Mathf.Max(QualitySettings.pixelLightCount, 4);
#endif

        if (!warnLight && autoCreateWarnLight)
        {
            var go = new GameObject("WarnLight");
            go.transform.SetParent(transform, false);
            warnLight = go.AddComponent<Light>();
        }

        if (warnLight)
        {
            warnLight.type = LightType.Point;
            warnLight.range = warnLightRange;
            warnLight.color = warnLightColor;
            warnLight.intensity = 0f;
            warnLight.enabled = false;
            warnLight.renderMode = LightRenderMode.ForcePixel;
            warnLight.shadows = LightShadows.None;
            warnLight.cullingMask = ~0;
        }

        initialScale = transform.localScale;
    }

    private void OnEnable() => StartCoroutine(RollLoop());
    private void OnDisable()
    {
        StopAllCoroutines();
        SetDamaging(false);
        if (warnLight) { warnLight.enabled = false; warnLight.intensity = 0f; }
        if (flameParticles) flameParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (scaleTransform) transform.localScale = initialScale;
    }

    // -------- API used by PlayerHealth --------
    public bool IsDamagingNow(Collider _) => damagingNow;

    public Vector3 DamageDirection(Vector3 victimPos)
    {
        var dir = victimPos - transform.position;
        if (dir.sqrMagnitude < 1e-4f) dir = transform.forward;
        return dir.normalized;
    }

    // -------- core sequence --------
    private IEnumerator RollLoop()
    {
        var wait = new WaitForSeconds(checkInterval);
        yield return new WaitForSeconds((float)rng.NextDouble() * 0.5f); // desync

        while (enabled)
        {
            if (!sequenceRunning && rng.NextDouble() < popChance)
                yield return DoPopSequence();
            yield return wait;
        }
    }

    private IEnumerator DoPopSequence()
    {
        sequenceRunning = true;
        yield return WarningPhase();
        yield return ActivePhase();
        ResetVisuals();
        sequenceRunning = false;
    }

    private IEnumerator WarningPhase()
    {
        SetDamaging(false);

        if (warnLight)
        {
            warnLight.enabled = true;
            float t = 0f;
            while (t < warnLead)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / warnLead);
                float eased = 1f - Mathf.Pow(1f - u, Mathf.Lerp(1f, 4f, warnEase));
                warnLight.intensity = Mathf.Lerp(0f, warnLightIntensity, eased);
                yield return null;
            }
            warnLight.intensity = warnLightIntensity;
        }
        else yield return new WaitForSeconds(warnLead);
    }

    private IEnumerator ActivePhase()
    {
        // stop fully before play (avoid seed warnings)
        if (flameParticles.isPlaying)
            flameParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        yield return null; // 1 frame to clear
        flameParticles.Play();

        if (scaleTransform && rampTime > 0f)
            StartCoroutine(ScaleBump());

        SetDamaging(true);
        yield return new WaitForSeconds(activeDuration);
        SetDamaging(false);
    }

    private IEnumerator ScaleBump()
    {
        Vector3 from = initialScale;
        Vector3 to = Vector3.Scale(initialScale, activeScale);
        float t = 0f;

        while (t < rampTime)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(from, to, t / rampTime);
            yield return null;
        }

        float hold = Mathf.Max(0f, activeDuration - rampTime * 2f);
        if (hold > 0f) yield return new WaitForSeconds(hold);

        t = 0f;
        while (t < rampTime)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(to, from, t / rampTime);
            yield return null;
        }

        transform.localScale = from;
    }

    private void ResetVisuals()
    {
        if (warnLight)
        {
            warnLight.intensity = 0f;
            warnLight.enabled = false;
        }
        if (flameParticles)
            flameParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (scaleTransform)
            transform.localScale = initialScale;
    }

    private void SetDamaging(bool on) => damagingNow = on;

#if UNITY_EDITOR
    private void OnValidate()
    {
        warnLead = Mathf.Max(0f, warnLead);
        activeDuration = Mathf.Max(0.01f, activeDuration);
        checkInterval = Mathf.Max(0.1f, checkInterval);
    }
#endif
}
