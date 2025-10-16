using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    [Header("movement settings")]
    public float moveSpeed = 2f;
    public float leftBoundary = -9.27f;
    public float rightBoundary = 4.489f;
    public bool startFromLeft = true;
    
    [Header("variation")]
    public bool useSineCurve = true;
    public float amplitude = 1f;
    public float frequency = 1f;
    public float phaseOffset = 0f;

    [Header("Footstep Sound Settings")]
    public AudioSource audioSource;       
    public Transform player;              
    public float maxHearingDistance = 15f; 
    public float minVolume = 0.05f;

    [Header("Squeak Sound Settings")]
    public AudioSource squeakSource;           
    public AudioClip[] squeakClips;            
    public float minSqueakInterval = 1f;      
    public float maxSqueakInterval = 3f;       
    public float squeakDistanceThreshold = 5f;

    [Tooltip("Maximum mouse sound volume")]
    [Range(0f, 1f)]
    public float maxSqueakVolume = 0.4f;

    private float squeakTimer;

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

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.loop = true;
            audioSource.Play();
        }

        ResetSqueakTimer();
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

        if (player != null && audioSource != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            float t = Mathf.Clamp01(1f - distance / maxHearingDistance);
            audioSource.volume = Mathf.Lerp(minVolume, 1f, t); 
        }

        HandleSqueak();
    }

    private void HandleSqueak()
    {
        if (player == null || squeakSource == null || squeakClips.Length == 0) return;

        squeakTimer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, player.position);

        if (squeakTimer <= 0f && distance <= squeakDistanceThreshold)
        {
            float distanceFactor = Mathf.Clamp01(1f - (distance / squeakDistanceThreshold));
            squeakSource.volume = distanceFactor * maxSqueakVolume;

            AudioClip clip = squeakClips[Random.Range(0, squeakClips.Length)];
            squeakSource.PlayOneShot(clip);

            ResetSqueakTimer();
        }
    }

    private void ResetSqueakTimer()
    {
        squeakTimer = Random.Range(minSqueakInterval, maxSqueakInterval);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector3 hitDirection = (other.transform.position - transform.position).normalized;
                playerHealth.Die(hitDirection);
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
                Vector3 hitDirection = (collision.transform.position - transform.position).normalized;
                playerHealth.Die(hitDirection);
            }
        }
    }

    public void StopMoving()
    {
        isActive = false;
    }
}