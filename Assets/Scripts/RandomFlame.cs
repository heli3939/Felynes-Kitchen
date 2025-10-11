using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(Collider))]
public class RandomFlame : MonoBehaviour
{
    [Header("Timing & Chance")]
    float popChance = 0.25f;
    float checkInterval = 3.0f;
    float activeDuration = 3.0f;

    [Header("Damage Delay")]
    public float damageDelay = 0.5f;

    [Header("Ramp In (emission + transform)")]
    public float rampTime = 0.8f;
    public bool scaleTransform = true;
    public float minScale = 0.25f;
    public Vector3 fullScale = new Vector3(0.2f, 0.3f, 0.2f);
    public float expK = 4.0f;

    [Header("Visual Soft-Start")]
    public float visualRampTime = 0.45f;
    public float minStartSizeMul = 0.05f;
    public float minStartAlphaMul = 0.0f;
    public float visualExpK = 5.0f;

    [Header("Brightness Fade-In")]
    public float brightnessRampTime = 0.6f;
    [Range(0f,1f)] public float minAlphaMul = 0.0f;
    [Range(0f,1f)] public float minEmissionMul = 0.1f;

    [Header("Player Freeze")]
    public string playerTag = "Player";
    public string movementScriptTypeName = "PlayerMovePhysicsSafe";

    [Header("Extras to hide when OFF (optional)")]
    public GameObject[] extraVisualRoots;

    // --- NEW: height gate so base touches don't count ---
    [Header("Damage Height Gate")]
    [Tooltip("Local Y height above which damage is allowed (e.g., where the flame tip is).")]
    public float damageMinLocalY = 0.6f;   // tweak per your model
    [Tooltip("Optional extra horizontal radius check in XZ (0 = skip).")]
    public float damageHorizontalRadius = 0.0f;

    private ParticleSystem ps;
    private Collider triggerCol;
    private bool isOn;
    private float timer;

    private float damageArmTimer = 0f, rampT = 0f, visualRampT = 0f, brightnessT = 0f, baseRateOverTime = 0f;
    private float baseStartSizeMul = 1f; private Color baseStartColor;
    private ParticleSystemRenderer psr; private Material instancedMat;
    private bool hasColorProp, hasEmissionProp;
    private Color baseTintColor = Color.white, baseEmissionColor = Color.black;
    private static readonly int _ColorProp = Shader.PropertyToID("_Color");
    private static readonly int _EmissionProp = Shader.PropertyToID("_EmissionColor");
    private System.Random rng;

    private static float ExpIn(float k, float t01) {
        t01 = Mathf.Clamp01(t01);
        float denom = Mathf.Exp(k) - 1f; if (denom <= 1e-6f) return t01;
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
        if (psr != null && psr.material != null) {
            instancedMat = psr.material;
            hasColorProp = instancedMat.HasProperty(_ColorProp);
            hasEmissionProp = instancedMat.HasProperty(_EmissionProp);
            if (hasColorProp) baseTintColor = instancedMat.GetColor(_ColorProp);
            if (hasEmissionProp) {
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

        SetFlame(false, true);
        timer = (float)(rng.NextDouble() * checkInterval);
    }

    void Update()
    {
        if (isOn && !triggerCol.enabled) { damageArmTimer -= Time.deltaTime; if (damageArmTimer <= 0f) triggerCol.enabled = true; }

        if (isOn && rampT < rampTime) {
            rampT += Time.deltaTime;
            float eased = ExpIn(expK, rampT / Mathf.Max(0.0001f, rampTime));
            SetEmissionRate(Mathf.Lerp(0f, baseRateOverTime, eased));
            if (scaleTransform) {
                Vector3 start = fullScale * Mathf.Clamp01(minScale);
                transform.localScale = Vector3.Lerp(start, fullScale, eased);
            }
        }

        if (isOn && visualRampT < visualRampTime) {
            visualRampT += Time.deltaTime;
            float ve = ExpIn(visualExpK, visualRampT / Mathf.Max(0.0001f, visualRampTime));
            var main = ps.main;
            main.startSizeMultiplier = Mathf.Lerp(baseStartSizeMul * Mathf.Max(0f, minStartSizeMul), baseStartSizeMul, ve);
            float a0 = Mathf.Clamp01(baseStartColor.a * Mathf.Max(0f, minStartAlphaMul));
            var c = baseStartColor; c.a = Mathf.Lerp(a0, baseStartColor.a, ve);
            main.startColor = c;
        } else if (isOn) {
            var main = ps.main; main.startSizeMultiplier = baseStartSizeMul; main.startColor = baseStartColor;
        }

        if (isOn && brightnessT < brightnessRampTime && instancedMat != null) {
            brightnessT += Time.deltaTime;
            float be = ExpIn(4.5f, brightnessT / Mathf.Max(0.0001f, brightnessRampTime));
            if (hasColorProp) {
                float a0 = baseTintColor.a * Mathf.Clamp01(minAlphaMul);
                var c = baseTintColor; c.a = Mathf.Lerp(a0, baseTintColor.a, be);
                instancedMat.SetColor(_ColorProp, c);
            }
            if (hasEmissionProp) {
                float eMul = Mathf.Lerp(Mathf.Clamp01(minEmissionMul), 1f, be);
                instancedMat.SetColor(_EmissionProp, baseEmissionColor * eMul);
            }
        }

        timer -= Time.deltaTime;
        if (isOn) { if (timer <= 0f) SetFlame(false); }
        else {
            if (timer <= 0f) {
                if (rng.NextDouble() < popChance) SetFlame(true);
                else timer = checkInterval;
            }
        }
    }

    private void SetFlame(bool on, bool immediateStop = false)
    {
        isOn = on;
        var em = ps.emission; em.enabled = on;

        if (on) {
            if (!ps.isPlaying) ps.Play();
            rampT = 0f; visualRampT = 0f; brightnessT = 0f;
            SetEmissionRate(0f);
            if (scaleTransform) transform.localScale = fullScale * Mathf.Clamp01(minScale);

            var main = ps.main;
            main.startSizeMultiplier = baseStartSizeMul * Mathf.Max(0f, minStartSizeMul);
            var sc = baseStartColor; sc.a = Mathf.Clamp01(baseStartColor.a * Mathf.Max(0f, minStartAlphaMul));
            main.startColor = sc;

            if (instancedMat != null) {
                if (hasColorProp) { var c = baseTintColor; c.a = baseTintColor.a * Mathf.Clamp01(minAlphaMul); instancedMat.SetColor(_ColorProp, c); }
                if (hasEmissionProp) { float eMul = Mathf.Clamp01(minEmissionMul); instancedMat.SetColor(_EmissionProp, baseEmissionColor * eMul); }
            }

            triggerCol.enabled = false;       // honor damage delay
            damageArmTimer = damageDelay;
            timer = activeDuration;
        }
        else {
            triggerCol.enabled = false;
            if (immediateStop) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            else ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            SetEmissionRate(0f);
            if (scaleTransform) transform.localScale = fullScale;
            var main = ps.main; main.startSizeMultiplier = baseStartSizeMul; main.startColor = baseStartColor;
            if (instancedMat != null) {
                if (hasColorProp) instancedMat.SetColor(_ColorProp, baseTintColor);
                if (hasEmissionProp) instancedMat.SetColor(_EmissionProp, baseEmissionColor);
            }
            timer = checkInterval;
        }

        if (extraVisualRoots != null) foreach (var go in extraVisualRoots) if (go) go.SetActive(on);
    }

    private void SetEmissionRate(float r)
    {
        var em = ps.emission;
        var curve = em.rateOverTime;
        curve.mode = ParticleSystemCurveMode.Constant;
        curve.constant = Mathf.Max(0f, r);
        em.rateOverTime = curve;
    }

    // --- NEW: HEIGHT-GATED DAMAGE ---
    private bool PassedHeightGate(Collider player)
    {
        // world Y of the allowed-damage plane
        float planeY = transform.TransformPoint(new Vector3(0f, damageMinLocalY, 0f)).y;

        // estimate player's top Y
        float topY;
        if (player is CapsuleCollider cap) {
            Vector3 centerW = player.transform.TransformPoint(cap.center);
            float half = Mathf.Max(0f, cap.height * 0.5f - cap.radius);
            topY = centerW.y + half + cap.radius;
        }
        else if (player.TryGetComponent<CharacterController>(out var cc)) {
            Vector3 centerW = player.transform.TransformPoint(cc.center);
            float half = Mathf.Max(0f, cc.height * 0.5f - cc.radius);
            topY = centerW.y + half + cc.radius;
        }
        else {
            topY = player.bounds.max.y;
        }

        if (topY < planeY) return false; // not high enough to touch flames

        // Optional horizontal radius check in XZ (helps avoid side triggers)
        if (damageHorizontalRadius > 0f) {
            Vector3 p = player.bounds.center;
            Vector3 f = transform.position;
            Vector2 pXZ = new Vector2(p.x, p.z);
            Vector2 fXZ = new Vector2(f.x, f.z);
            if ((pXZ - fXZ).sqrMagnitude > damageHorizontalRadius * damageHorizontalRadius)
                return false;
        }

        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isOn || !other.CompareTag(playerTag) || !triggerCol.enabled) return;
        if (!PassedHeightGate(other)) return;   // NEW: ignore low/base touches

        var rb = other.GetComponent<Rigidbody>();
        if (rb != null) {
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
