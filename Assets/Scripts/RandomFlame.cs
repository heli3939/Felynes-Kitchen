using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(Collider))]
public class RandomFlame : MonoBehaviour
{
    [Header("Timing & Chance")]
    [SerializeField] float popChance = 0.25f;
    [SerializeField] float checkInterval = 3.0f;
    [SerializeField] float activeDuration = 3.0f;

    [Header("Damage Delay")]
    public float damageDelay = 0.5f;

    [Header("Ramp In (emission + transform)")]
    public float rampTime = 0.8f;
    public bool scaleTransform = true;
    public float minScale = 0.25f;
    Vector3 fullScale = new Vector3(0.39f, 0.39f, 0.39f);
    public float expK = 4.0f;

    [Header("Visual Soft-Start")]
    public float visualRampTime = 0.45f;
    public float minStartSizeMul = 0.05f;
    public float minStartAlphaMul = 0.0f;
    public float visualExpK = 5.0f;

    [Header("Brightness Fade-In")]
    public float brightnessRampTime = 0.6f;
    [Range(0f, 1f)] public float minAlphaMul = 0.0f;
    [Range(0f, 1f)] public float minEmissionMul = 0.1f;

    // Hard caps: keeps the flame small & dim even at full ramp
    [Header("Caps (limit final size/brightness)")]
    [Range(0.1f, 1f)] public float maxScaleMul = 0.6f;     // 60% of fullScale
    [Range(0.1f, 1f)] public float maxSizeMul = 0.6f;      // 60% of startSize
    [Range(0.05f, 1f)] public float maxEmissionMul = 0.4f; // 40% of emission color
    [Range(0.05f, 1f)] public float maxTintAlphaMul = 0.7f;// 70% of _Color alpha
    [Range(0.05f, 1f)] public float maxRateMul = 0.5f;     // 50% of emission rate

    [Header("Extras to hide when OFF (optional)")]
    public GameObject[] extraVisualRoots;

    [Header("Damage Height Gate")]
    [Tooltip("Local Y height above which damage is allowed (e.g., flame tip).")]
    public float damageMinLocalY = 0.6f;
    [Tooltip("Optional extra horizontal radius check in XZ (0 = skip).")]
    public float damageHorizontalRadius = 0.0f;

    // cached
    private ParticleSystem ps;
    private ParticleSystemRenderer psr;
    private Collider triggerCol;
    private Material instancedMat;

    private bool isOn;
    private float timer;
    private float damageArmTimer = 0f, rampT = 0f, visualRampT = 0f, brightnessT = 0f, baseRateOverTime = 0f;
    private float baseStartSizeMul = 1f;
    private Color baseStartColor, baseTintColor = Color.white, baseEmissionColor = Color.black;
    private bool hasColorProp, hasEmissionProp;
    private System.Random rng;

    // capped targets computed in Awake
    private Vector3 cappedFullScale;
    private float cappedStartSizeMul;
    private Color cappedTintColor;
    private Color cappedEmissionColor;
    private float cappedBaseRate;

    private static readonly int _ColorProp = Shader.PropertyToID("_Color");
    private static readonly int _EmissionProp = Shader.PropertyToID("_EmissionColor");

    private static float ExpIn(float k, float t01)
    {
        t01 = Mathf.Clamp01(t01);
        float denom = Mathf.Exp(k) - 1f;
        if (denom <= 1e-6f) return t01;
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

        psr = GetComponent<ParticleSystemRenderer>();
        if (psr != null && psr.material != null)
        {
            instancedMat = psr.material;
            hasColorProp = instancedMat.HasProperty(_ColorProp);
            hasEmissionProp = instancedMat.HasProperty(_EmissionProp);
            if (hasColorProp) baseTintColor = instancedMat.GetColor(_ColorProp);
            if (hasEmissionProp)
            {
                baseEmissionColor = instancedMat.GetColor(_EmissionProp);
                instancedMat.EnableKeyword("_EMISSION");
            }
        }

        if (extraVisualRoots == null || extraVisualRoots.Length == 0)
        {
            var list = new System.Collections.Generic.List<GameObject>();
            var fe = transform.Find("FireEmbers"); if (fe) list.Add(fe.gameObject);
            var lightNode = transform.Find("Light"); if (lightNode) list.Add(lightNode.gameObject);
            extraVisualRoots = list.ToArray();
        }

        // compute capped targets
        cappedFullScale = fullScale * Mathf.Clamp01(maxScaleMul);
        cappedStartSizeMul = baseStartSizeMul * Mathf.Clamp01(maxSizeMul);
        cappedTintColor = baseTintColor; cappedTintColor.a = baseTintColor.a * Mathf.Clamp01(maxTintAlphaMul);
        cappedEmissionColor = baseEmissionColor * Mathf.Clamp01(maxEmissionMul);
        cappedBaseRate = baseRateOverTime * Mathf.Clamp01(maxRateMul);

        SetFlame(false, true);
        timer = (float)(rng.NextDouble() * checkInterval);
    }

    void Update()
    {
        if (isOn && !triggerCol.enabled)
        {
            damageArmTimer -= Time.deltaTime;
            if (damageArmTimer <= 0f) triggerCol.enabled = true;
        }

        if (isOn && rampT < rampTime)
        {
            rampT += Time.deltaTime;
            float eased = ExpIn(expK, rampT / Mathf.Max(0.0001f, rampTime));
            SetEmissionRate(Mathf.Lerp(0f, cappedBaseRate, eased));

            if (scaleTransform)
            {
                Vector3 start = cappedFullScale * Mathf.Clamp01(minScale);
                transform.localScale = Vector3.Lerp(start, cappedFullScale, eased);
            }
        }

        if (isOn && visualRampT < visualRampTime)
        {
            visualRampT += Time.deltaTime;
            float ve = ExpIn(visualExpK, visualRampT / Mathf.Max(0.0001f, visualRampTime));
            var main = ps.main;
            main.startSizeMultiplier = Mathf.Lerp(
                cappedStartSizeMul * Mathf.Max(0f, minStartSizeMul),
                cappedStartSizeMul, ve);

            float a0 = Mathf.Clamp01(baseStartColor.a * Mathf.Max(0f, minStartAlphaMul));
            var c = baseStartColor; c.a = Mathf.Lerp(a0, baseStartColor.a, ve);
            main.startColor = c;
        }
        else if (isOn)
        {
            var main = ps.main;
            main.startSizeMultiplier = cappedStartSizeMul;
            main.startColor = baseStartColor;
        }

        if (isOn && brightnessT < brightnessRampTime && instancedMat != null)
        {
            brightnessT += Time.deltaTime;
            float be = ExpIn(4.5f, brightnessT / Mathf.Max(0.0001f, brightnessRampTime));

            if (hasColorProp)
            {
                float a0 = cappedTintColor.a * Mathf.Clamp01(minAlphaMul);
                var c = cappedTintColor; c.a = Mathf.Lerp(a0, cappedTintColor.a, be);
                instancedMat.SetColor(_ColorProp, c);
            }
            if (hasEmissionProp)
            {
                Color startE = baseEmissionColor * Mathf.Clamp01(minEmissionMul);
                Color targetE = cappedEmissionColor;
                instancedMat.SetColor(_EmissionProp, Color.Lerp(startE, targetE, be));
            }
        }

        // timer state machine
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
        var em = ps.emission; em.enabled = on;

        if (on)
        {
            if (!ps.isPlaying) ps.Play();
            rampT = 0f; visualRampT = 0f; brightnessT = 0f;
            SetEmissionRate(0f);

            if (scaleTransform) transform.localScale = cappedFullScale * Mathf.Clamp01(minScale);

            var main = ps.main;
            main.startSizeMultiplier = cappedStartSizeMul * Mathf.Max(0f, minStartSizeMul);
            var sc = baseStartColor;
            sc.a = Mathf.Clamp01(baseStartColor.a * Mathf.Max(0f, minStartAlphaMul));
            main.startColor = sc;

            if (instancedMat != null)
            {
                if (hasColorProp)
                {
                    var c = cappedTintColor;
                    c.a = cappedTintColor.a * Mathf.Clamp01(minAlphaMul);
                    instancedMat.SetColor(_ColorProp, c);
                }
                if (hasEmissionProp)
                {
                    Color e = baseEmissionColor * Mathf.Clamp01(minEmissionMul);
                    instancedMat.SetColor(_EmissionProp, e);
                }
            }

            triggerCol.enabled = false;
            damageArmTimer = damageDelay;
            timer = activeDuration;
        }
        else
        {
            triggerCol.enabled = false;
            if (immediateStop) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            else ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            SetEmissionRate(0f);

            if (scaleTransform) transform.localScale = cappedFullScale;

            var main = ps.main;
            main.startSizeMultiplier = cappedStartSizeMul;
            main.startColor = baseStartColor;

            if (instancedMat != null)
            {
                if (hasColorProp) instancedMat.SetColor(_ColorProp, cappedTintColor);
                if (hasEmissionProp) instancedMat.SetColor(_EmissionProp, cappedEmissionColor);
            }
            timer = checkInterval;
        }

        if (extraVisualRoots != null)
        {
            foreach (var go in extraVisualRoots)
                if (go) go.SetActive(on);
        }
    }

    private void SetEmissionRate(float r)
    {
        var em = ps.emission;
        var curve = em.rateOverTime;
        curve.mode = ParticleSystemCurveMode.Constant;
        curve.constant = Mathf.Max(0f, r);
        em.rateOverTime = curve;
    }

    // ---------- Public API used by PlayerHealth ----------
    public bool IsDamagingNow(Collider playerCollider)
    {
        return isOn && triggerCol.enabled && PassedHeightGate(playerCollider);
    }

    public Vector3 DamageDirection(Vector3 playerPosition)
    {
        return (playerPosition - transform.position).normalized;
    }
    // ----------------------------------------------------

    private bool PassedHeightGate(Collider player)
    {
        float planeY = transform.TransformPoint(new Vector3(0f, damageMinLocalY, 0f)).y;

        float topY;
        if (player is CapsuleCollider cap)
        {
            Vector3 centerW = player.transform.TransformPoint(cap.center);
            float half = Mathf.Max(0f, cap.height * 0.5f - cap.radius);
            topY = centerW.y + half + cap.radius;
        }
        else if (player.TryGetComponent<CharacterController>(out var cc))
        {
            Vector3 centerW = player.transform.TransformPoint(cc.center);
            float half = Mathf.Max(0f, cc.height * 0.5f - cc.radius);
            topY = centerW.y + half + cc.radius;
        }
        else
        {
            topY = player.bounds.max.y;
        }

        if (topY < planeY) return false;

        if (damageHorizontalRadius > 0f)
        {
            Vector3 p = player.bounds.center;
            Vector3 f = transform.position;
            Vector2 pXZ = new Vector2(p.x, p.z);
            Vector2 fXZ = new Vector2(f.x, f.z);
            if ((pXZ - fXZ).sqrMagnitude > damageHorizontalRadius * damageHorizontalRadius)
                return false;
        }

        return true;
    }

    // === Flame Sound Addon ===
    [Header("Sound Settings")]
    public AudioClip flameSound;
    public AudioSource flameAudioSource;
    public float playerDetectRadius = 6f;
    public float maxVolume = 1f;
    public float fadeSpeed = 2f;

    private static float globalVolumeTarget = 0f; 
    private static int activeFlameCount = 0;      
    private static RandomFlame[] allFlames;      

    private void LateUpdate()
    {
        HandleFlameSound();
    }

    private void HandleFlameSound()
    {
        if (flameSound == null) return;

        if (flameAudioSource == null)
        {
            flameAudioSource = GetComponent<AudioSource>();
            if (flameAudioSource == null)
                flameAudioSource = gameObject.AddComponent<AudioSource>();

            flameAudioSource.playOnAwake = false;
            flameAudioSource.loop = true;
            flameAudioSource.spatialBlend = 0f; 
            flameAudioSource.clip = flameSound;
            flameAudioSource.volume = 0f;
        }

        if (allFlames == null || allFlames.Length == 0)
            allFlames = FindObjectsOfType<RandomFlame>();

        activeFlameCount = 0;
        foreach (var f in allFlames)
            if (f != null && f.isOn) activeFlameCount++;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        bool playerNearby = false;
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            playerNearby = dist <= playerDetectRadius;
        }

        if (playerNearby && activeFlameCount > 0)
            globalVolumeTarget = Mathf.Clamp01(activeFlameCount / 5f) * maxVolume; 
        else
            globalVolumeTarget = 0f;

        float targetVol = globalVolumeTarget;
        flameAudioSource.volume = Mathf.MoveTowards(flameAudioSource.volume, targetVol, fadeSpeed * Time.deltaTime);

        if (flameAudioSource.volume > 0.01f && !flameAudioSource.isPlaying)
            flameAudioSource.Play();
        else if (flameAudioSource.volume <= 0.01f && flameAudioSource.isPlaying)
            flameAudioSource.Stop();
    }

}
