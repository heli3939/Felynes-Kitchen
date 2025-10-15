using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    private bool isDead = false;
    private bool isFalling = false;

    [Header("Fall Settings")]
    public float fallDuration = 1f;

    [SerializeField] private Collider myCollider;
    [SerializeField] private GameOverManager gameOverManager; 

    void Awake()
    {
        currentHealth = maxHealth;

        if (myCollider == null)
            myCollider = GetComponent<Collider>() ?? GetComponentInChildren<Collider>();
    }

    private void OnTriggerEnter(Collider other) => TryBurn(other);
    private void OnTriggerStay(Collider other) => TryBurn(other);

    private void TryBurn(Collider other)
    {
        RandomFlame flame = other.GetComponent<RandomFlame>() ?? other.GetComponentInParent<RandomFlame>();
        if (flame == null || myCollider == null) return;

        if (!isDead && !isFalling && flame.IsDamagingNow(myCollider))
        {
            TakeDamage(100, flame.DamageDirection(transform.position)); 
        }
    }

    public void TakeDamage(int amount, Vector3? hitDirection = null)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die(hitDirection ?? -transform.forward);
        }
    }

    public void Die(Vector3 hitDirection)
    {
        if (isDead || isFalling) return;

        isFalling = true;
        StopAllMice();
        DisablePlayerControls();

        StartCoroutine(FallAndDie(hitDirection));
    }

    IEnumerator FallAndDie(Vector3 hitDirection)
    {
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
        Debug.Log("Player died");

        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver();
        }
        else
        {
            Debug.LogWarning("⚠️ GameOverManager not assigned!");
        }
    }

    public void DisablePlayerControls()
    {
        // disable all other behaviours on the player
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this) script.enabled = false;
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
        foreach (MouseMovement mouse in allMice)
        {
            mouse.StopMoving();
        }
    }

    public bool IsDead() => isDead;
}
