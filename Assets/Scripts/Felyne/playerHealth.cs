using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 120;
    private int currentHealth;

    private bool isDead = false;
    private bool isFalling = false;
    public TextMeshProUGUI healthText;

    [Header("Fall Settings")]
    public float fallDuration = 1f;

    [SerializeField] private GameObject checklistButton;
    [SerializeField] private GameObject pauseButton;

    [SerializeField] private Collider myCollider;
    [SerializeField] private GameOverManager gameOverManager;

    private float lastDamageTime = -999f;
    public float damageCooldown = 1f;

    // DAMAGE FLASH >>> add these <<<
    [Header("Damage Flash")]
    [Tooltip("Renderer whose material will flash red on hit.")]
    public Renderer playerRenderer; // assign in Inspector (e.g. the cat mesh)
    public Color damageColor = Color.red;
    public float flashDuration = 0.15f;
    private Color originalColor;
    private bool isFlashing = false;
    // DAMAGE FLASH <<<

    void Update()
    {
        UpdateHealthUI();
    }

    void Awake()
    {
        currentHealth = maxHealth;

        if (myCollider == null)
            myCollider = GetComponent<Collider>() ?? GetComponentInChildren<Collider>();

        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<Renderer>();

        if (playerRenderer != null)
            originalColor = playerRenderer.material.color;
        else
            Debug.LogWarning("[PlayerHealth] No playerRenderer assigned for flash!");

        Debug.Log($"[PlayerHealth] Initialized. Health: {currentHealth}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[PlayerHealth] OnTriggerEnter with: {other.gameObject.name}");
        TryBurn(other);
    }

    private void OnTriggerStay(Collider other) => TryBurn(other);

    private void TryBurn(Collider other)
    {
        RandomFlame flame = other.GetComponent<RandomFlame>() ?? other.GetComponentInParent<RandomFlame>();
        if (flame == null || myCollider == null) return;

        if (Time.time - lastDamageTime < damageCooldown) return;

        if (!isDead && !isFalling && flame.IsDamagingNow(myCollider))
        {
            lastDamageTime = Time.time;
            Debug.Log($"[PlayerHealth] Taking fire damage!");
            TakeDamage(maxHealth / 3, flame.DamageDirection(transform.position));
        }
    }

    public void TakeDamage(int amount, Vector3? hitDirection = null)
    {
        if (isDead)
        {
            Debug.Log("[PlayerHealth] Already dead, ignoring damage");
            return;
        }

        currentHealth -= amount;
        Debug.Log($"[PlayerHealth] Took {amount} damage. Current health: {currentHealth}");

        if (!isFlashing)
            StartCoroutine(FlashRedThenRecover());

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("[PlayerHealth] Health reached 0, calling Die()");
            Die(hitDirection ?? -transform.forward);
        }
    }

    private IEnumerator FlashRedThenRecover()
    {
        if (playerRenderer == null) yield break;

        isFlashing = true;

        playerRenderer.material.color = damageColor;

        yield return new WaitForSeconds(flashDuration);

        float t = 0f;
        const float lerpTime = 0.15f;
        Color startColor = playerRenderer.material.color;

        while (t < lerpTime)
        {
            t += Time.deltaTime;
            float a = t / lerpTime;
            playerRenderer.material.color = Color.Lerp(startColor, originalColor, a);
            yield return null;
        }

        playerRenderer.material.color = originalColor;
        isFlashing = false;
    }

    public void Die(Vector3 hitDirection)
    {
        if (isDead || isFalling)
        {
            Debug.Log($"[PlayerHealth] Die() blocked - isDead: {isDead}, isFalling: {isFalling}");
            return;
        }

        Debug.Log("[PlayerHealth] Die() called - starting death sequence");
        isFalling = true;
        StopAllMice();
        DisablePlayerControls();
        checklistButton.SetActive(false);
        pauseButton.SetActive(false);

        StartCoroutine(FallAndDie(hitDirection));
    }

    IEnumerator FallAndDie(Vector3 hitDirection)
    {
        Debug.Log("[PlayerHealth] FallAndDie coroutine started");

        Vector3 playerForward = transform.forward;
        float dotProduct = Vector3.Dot(playerForward, hitDirection.normalized);
        float targetRotationX = dotProduct > 0 ? 90f : -90f;

        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(targetRotationX, transform.eulerAngles.y, transform.eulerAngles.z);

        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fallDuration;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        transform.rotation = targetRotation;

        isDead = true;
        Debug.Log("[PlayerHealth] Player died - calling GameOverManager");

        if (gameOverManager != null)
        {
            Debug.Log("[PlayerHealth] Calling ShowGameOver()");
            gameOverManager.ShowGameOver();
        }
        else
        {
            Debug.LogError("[PlayerHealth] ⚠️ GameOverManager not assigned in Inspector!");
        }
    }

    public void DisablePlayerControls()
    {
        Debug.Log("[PlayerHealth] Disabling player controls");

        AudioSource[] playerAudios = GetComponentsInChildren<AudioSource>();
        foreach (AudioSource audio in playerAudios)
        {
            if (audio != null && audio.isPlaying)
            {
                audio.Stop();
            }
        }

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != null && script != this && script.GetType() != typeof(PlayerHealth))
            {
                script.enabled = false;
            }
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.constraints = RigidbodyConstraints.None;
            rb.constraints |= RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ
                            | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        }

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
    }

    public void StopAllMice()
    {
        MouseMovement[] allMice = FindObjectsByType<MouseMovement>(FindObjectsSortMode.None);
        Debug.Log($"[PlayerHealth] Stopping {allMice.Length} mice");

        foreach (MouseMovement mouse in allMice)
        {
            mouse.StopMoving();

            if (mouse.audioSource != null && mouse.audioSource.isPlaying)
            {
                mouse.audioSource.Stop();
            }

            if (mouse.squeakSource != null && mouse.squeakSource.isPlaying)
            {
                mouse.squeakSource.Stop();
            }
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"Player Health: {currentHealth}/{maxHealth}";
        }
    }

    public bool IsDead() => isDead;
}
