using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(Collider))]
public class RandomFlame : MonoBehaviour
{
    [Header("Timing & Chance")]
    float popChance = 0.25f;   // 25% chance per roll
    float checkInterval = 5.0f;                // how often to roll (when OFF)
    float activeDuration = 3.0f;               // how long flame stays ON

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

    // Per-instance RNG (prevents synchronization between flames)
    private System.Random rng;

    void Awake()
    {
        // Seed with position + instance + time → unique per flame
        int seed = (int)(
            (DateTime.UtcNow.Ticks & 0x7FFFFFFF) ^
            (GetInstanceID() << 7) ^
            (transform.position.GetHashCode() * 397)
        );
        rng = new System.Random(seed);

        ps = GetComponent<ParticleSystem>();
        triggerCol = GetComponent<Collider>();
        triggerCol.isTrigger = true;

        // Auto-find common children if not assigned
        if (extraVisualRoots == null || extraVisualRoots.Length == 0)
        {
            var list = new System.Collections.Generic.List<GameObject>();
            var fe = transform.Find("FireEmbers");
            if (fe) list.Add(fe.gameObject);
            var lightNode = transform.Find("Light");
            if (lightNode) list.Add(lightNode.gameObject);
            extraVisualRoots = list.ToArray();
        }

        // Start OFF and desync initial roll time a bit
        SetFlame(false, immediateStop: true);
        timer = (float)(rng.NextDouble() * checkInterval);
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (isOn)
        {
            if (timer <= 0f)
                SetFlame(false);
        }
        else
        {
            if (timer <= 0f)
            {
                if (rng.NextDouble() < popChance)
                    SetFlame(true);
                else
                    timer = checkInterval; // try again later
            }
        }
    }

    private void SetFlame(bool on, bool immediateStop = false)
    {
        isOn = on;

        // Toggle particle emission
        var em = ps.emission;
        em.enabled = on;

        if (on)
        {
            if (!ps.isPlaying) ps.Play();
            triggerCol.enabled = true;
            timer = activeDuration;
        }
        else
        {
            triggerCol.enabled = false;
            if (immediateStop)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            else
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            timer = checkInterval;
        }

        // Toggle extra visuals like embers/light
        if (extraVisualRoots != null)
        {
            foreach (var go in extraVisualRoots)
                if (go) go.SetActive(on);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isOn || !other.CompareTag(playerTag)) return;

        // Hard stop physics
        var rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // freeze movement
        }

        // Optionally disable your movement script (e.g., PlayerMovePhysicsSafe)
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
