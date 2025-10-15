using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    private bool isDead = false;
    private bool isFalling = false;

    [Header("fall settings")]
    public float fallDuration = 1f;

    // Cache the player's main collider (capsule/character/etc.)
    [SerializeField] private Collider myCollider;

    void Awake()
    {
        if (myCollider == null)
            myCollider = GetComponent<Collider>() ?? GetComponentInChildren<Collider>();
    }

    // --- Trigger handling moved here ---
    private void OnTriggerEnter(Collider other) => TryBurn(other);
    private void OnTriggerStay(Collider other) => TryBurn(other);

    private void TryBurn(Collider other)
    {
        // Flame collider is on the flame object; detect its RandomFlame component
        RandomFlame flame = other.GetComponent<RandomFlame>() ?? other.GetComponentInParent<RandomFlame>();
        if (flame == null || myCollider == null) return;

        if (!isDead && !isFalling && flame.IsDamagingNow(myCollider))
        {
            Vector3 hitDir = flame.DamageDirection(transform.position);
            Die(hitDir);
        }
    }
    // -----------------------------------

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

        Debug.Log("GameOver");

        isDead = true;
        GameOverManager gameOverManager = FindFirstObjectByType<GameOverManager>();
        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver();
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

        // stop physics & lock axes
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;           // (fix: was linearVelocity)
            rb.angularVelocity = Vector3.zero;

            // combine constraints instead of overwriting
            rb.constraints = RigidbodyConstraints.None;
            rb.constraints |= RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ
                            | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        }

        // disable CharacterController if present
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
