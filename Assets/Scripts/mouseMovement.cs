using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    [Header("movement setting")]
    public float moveSpeed = 2f;
    public float leftBoundary = -9.27f;
    public float rightBoundary = 4.489f;
    public bool startFromLeft = true;
    
    [Header("curve setting")]
    public bool useSineCurve = true;
    public float amplitude = 1f;
    public float frequency = 1f;
    public float phaseOffset = 0f;

    private float direction;
    private float initialY;
    private float initialZ;
    private bool isActive = true;
    private float currentX;

    void Start()
    {
        initialY = transform.position.y;
        initialZ = transform.position.z;

        if (startFromLeft)
        {
            currentX = leftBoundary;
            direction = 1f;
        }
        else
        {
            currentX = rightBoundary;
            direction = -1f;
        }
        
        transform.position = new Vector3(currentX, initialY, initialZ);
    }

    void Update()
    {
        if (!isActive) return;

        currentX += direction * moveSpeed * Time.deltaTime;

        if (currentX >= rightBoundary)
        {
            currentX = rightBoundary;
            direction = -1f;
        }
        else if (currentX <= leftBoundary)
        {
            currentX = leftBoundary;
            direction = 1f;
        }

        float zOffset;
        if (useSineCurve)
        {
            zOffset = Mathf.Sin(currentX * frequency + phaseOffset) * amplitude;
        }
        else
        {
            zOffset = Mathf.Cos(currentX * frequency + phaseOffset) * amplitude;
        }

        transform.position = new Vector3(currentX, initialY, initialZ + zOffset);

        if (direction > 0)
            transform.rotation = Quaternion.Euler(0, 90, 0);
        else
            transform.rotation = Quaternion.Euler(0, -90, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Die();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Die();
            }
        }
    }

    public void StopMoving()
    {
        isActive = false;
    }
}