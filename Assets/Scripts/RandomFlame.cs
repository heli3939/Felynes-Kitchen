using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(Collider))]
public class RandomFlame : MonoBehaviour
{
    [Header("Timing & Chance")]
    float popChance = 0.25f;                 // 25% chance per roll
    float checkInterval = 3.0f;              // how often to roll (when OFF)
    float activeDuration = 3.0f;             // how long flame stays ON

    [Header("Damage Delay")]
    public float damageDelay = 0.5f;         // no damage during this window

    [Header("Ramp In (emission + transform)")]
    public float rampTime = 1f;              // seconds to reach full strength
    public bool scaleTransform = true;       // scale during ramp
    public float minScale = 0.25f;           // fraction of fullScale to start from
    public Vector3 fullScale = new Vector3(0.2f, 0.3f, 0.2f); // target full size
    public float expK = 3.5f;                // NEW: exponential strength for emission/scale

    // Reverse-outro intro for first-born particles
    [Header("Visual Soft-Start (reverse outro)")]
    public float visualRampTime = 0.45f;     // grow/fade the first particles
    public float minStartSizeMul = 0.05f;    // start very tiny (5% of normal)
    public float minStartAlphaMul = 0.0f;    // start fully transparent
    public float visualExpK = 4.0f;          // NEW: exponential strength for size/alpha

    [Header("Player Freeze")]
    public string playerTag = "Player";
    [Tooltip("Optional: exact movement script type name to disable (e.g., PlayerMovePhysicsSafe). Leave empty to skip.")]
    public string movementScriptTypeName = "PlayerMovePhysicsSafe";

    [Header("Extras to hide when OFF (optional)")]
    [Tooltip("Assign child roots like FireEmbers, Light. If left empty, auto-finds children named 'FireEmbers' and 'Light'.")]
    public GameObject[] extraVisualRoots;

    private ParticleSystem ps;
    private Collider triggerCol;
    private bool isOn;
    private float timer;

    // timers/state
    private float damageArmTimer = 0f;
    private float rampT = 0f;            // emission/transform ramp
    private float visualRampT = 0f;      // start size/alpha ramp
    private float baseRateOverTime = 0f;

    // cache base visual settings
    private float baseStartSizeMul = 1f;
    private Color baseStartColor;

    // RNG
    private System.Random rng;

    // --- NEW: exponential ease-in ---
    private static float ExpIn(float k, float t01)
    {
        t01 = Mathf.Clamp01(t01);
        // normalized: (e^(k*t) - 1) / (e^k - 1)
        float denom = Mathf.Exp(k) - 1f;
        if (denom <= 1e-6f) return t01; // fallback to linear if k ~ 0
        return (Mathf.Exp(k * t01) - 1f) / denom;
    }

    void Awake()
    {
        int seed = (int)((DateTime.UtcNow.Ticks & 0x7FFFFFFF) ^ (GetInstanceID() << 7) ^ (transform.position.GetHashCode() * 397));
        rng = new System.Random(seed);

        ps = GetComponent<ParticleSystem>();
        triggerCol = GetComponent<Collider>();
        triggerCol.isTrigger = true;

        var em = ps.emission;
        baseRateOverTime = em.rateOverTime.constant;

        var main = ps.main;
        baseStartSizeMul = main.startSizeMultiplier;
        baseStartColor = main.startColor.color;

        if (extraVisualRoots == null || extraVisualRoots.Length == 0)
        {
            var list = new System.Collections.Generic.List<GameObject>();
            var fe = transform.Find("FireEmbers"); if (fe) list.Add(fe.gameObject);
            var lightNode = transform.Find("Light"); if (lightNode) list.Add(lightNode.gameObject);
            extraVisualRoots = list.ToArray();
        }

        SetFlame(false, immediateStop: true);
        timer = (float)(rng.NextDouble() * checkInterval);
    }

    void Update()
    {
        // arm collider after delay
        if (isOn && !triggerCol.enabled)
        {
            damageArmTimer -= Time.deltaTime;
            if (damageArmTimer <= 0f) triggerCol.enabled = true;
        }

        // emission + transform ramp with exponential ease-in
        if (isOn && rampT < rampTime)
        {
            rampT += Time.deltaTime;
            float k = Mathf.Clamp01(rampT / Mathf.Max(0.0001f, rampTime));
            float eased = ExpIn(expK, k); // <<< exponential

            SetEmissionRate(Mathf.Lerp(0f, baseRateOverTime, eased));

            if (scaleTransform)
            {
                Vector3 start = fullScale * Mathf.Clamp01(minScale);
                transform.localScale = Vector3.Lerp(start, fullScale, eased);
            }
        }

        // visual soft-start for first-born particles (size & alpha) with exponential ease-in
        if (isOn && visualRampT < visualRampTime)
        {
            visualRampT += Time.deltaTime;
            float vk = Mathf.Clamp01(visualRampT / Mathf.Max(0.0001f, visualRampTime));
            float ve = ExpIn(visualExpK, vk); // <<< exponential

            var main = ps.main;
            main.startSizeMultiplier = Mathf.Lerp(baseStartSizeMul * Mathf.Max(0f, minStartSizeMul),
                                                  baseStartSizeMul, ve);

            float a0 = Mathf.Clamp01(baseStartColor.a * Mathf.Max(0f, minStartAlphaMul));
            float a1 = baseStartColor.a;
            var c = baseStartColor; c.a = Mathf.Lerp(a0, a1, ve);
            main.startColor = c;
        }
        else if (isOn)
        {
            // hold base visuals after the intro
            var main = ps.main;
            main.startSizeMultiplier = baseStartSizeMul;
            main.startColor = baseStartColor;
        }

        // on/off timing
        timer -= Time.deltaTime;

        if (isOn)
        {
            if (timer <= 0f) SetFlame(false);
        }
        else
        {
            if (timer <= 0f)
            {
                if (rng.NextDouble() < popChance) SetFlame(true);
                else timer = checkInterval;
            }
        }
    }

    private void SetFlame(bool on, bool immediateStop = false)
    {
        isOn = on;

        var em = ps.emission;
        em.enabled = on;

        if (on)
        {
            if (!ps.isPlaying) ps.Play();

            rampT = 0f;
            visualRampT = 0f;

            SetEmissionRate(0f);

            if (scaleTransform)
                transform.localScale = fullScale * Mathf.Clamp01(minScale);

            // start the very first particles tiny & transparent
            var main = ps.main;
            main.startSizeMultiplier = baseStartSizeMul * Mathf.Max(0f, minStartSizeMul);
            var c = baseStartColor; c.a = Mathf.Clamp01(baseStartColor.a * Mathf.Max(0f, minStartAlphaMul));
            main.startColor = c;

            // damage delay
            triggerCol.enabled = false;
            damageArmTimer = damageDelay;

            timer = activeDuration;
        }
        else
        {
            triggerCol.enabled = false;
            if (immediateStop)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            else
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            SetEmissionRate(0f);
            if (scaleTransform) transform.localScale = fullScale;

            // restore base visuals
            var main = ps.main;
            main.startSizeMultiplier = baseStartSizeMul;
            main.startColor = baseStartColor;

            timer = checkInterval;
        }

        if (extraVisualRoots != null)
            foreach (var go in extraVisualRoots) if (go) go.SetActive(on);
    }

    private void SetEmissionRate(float r)
    {
        var em = ps.emission;
        var curve = em.rateOverTime;
        curve.mode = ParticleSystemCurveMode.Constant;
        curve.constant = Mathf.Max(0f, r);
        em.rateOverTime = curve;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isOn || !other.CompareTag(playerTag)) return;

        var rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log("Touch");
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (!string.IsNullOrEmpty(movementScriptTypeName))
        {
            var t = Type.GetType(movementScriptTypeName);
            if (t != null && typeof(MonoBehaviour).IsAssignableFrom(t))
            {
                var mover = other.GetComponent(t) as MonoBehaviour;
                if (mover != null) mover.enabled = false;
            }
        }
    }
}
