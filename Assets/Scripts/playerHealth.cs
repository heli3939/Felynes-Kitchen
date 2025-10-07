using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private bool isDead = false;

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Player died!");
        StopAllMice();

        GameOverManager gameOverManager = FindFirstObjectByType<GameOverManager>();
        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver();
        }

        DisablePlayerControls();
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
}