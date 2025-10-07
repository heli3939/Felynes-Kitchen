using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    private bool isDead = false;
    private bool isFalling = false;
    
    [Header("fall settings")]
    public float fallDuration = 1f;
    
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

    void DisablePlayerControls()
    {
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeRotationY;
            rb.constraints = RigidbodyConstraints.FreezeRotationZ;
            rb.constraints = RigidbodyConstraints.FreezePositionY;
            rb.constraints = RigidbodyConstraints.FreezePositionZ;
        }
        
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }
    }
    
    void StopAllMice()
    {
        MouseMovement[] allMice = FindObjectsByType<MouseMovement>(FindObjectsSortMode.None);
        foreach (MouseMovement mouse in allMice)
        {
            mouse.StopMoving();
        }
    }
    
    public bool IsDead()
    {
        return isDead;
    }
}