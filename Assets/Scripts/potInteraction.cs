using UnityEngine;
using System.Collections;

public class PotPickDrop : MonoBehaviour
{
    [Header("Player Hold")]
    public Transform holdPoint;
    private bool isHeld = false;
    
    [Header("Pickup Detection")]
    private GameObject nearbyPlayer;
    public float pickupRange = 0.5f;

    [Header("Oven Interaction")]
    public DoorInteraction ovenDoor;
    public float ovenInteractDistance = 2f;
    public Vector3 ovenTargetPosition = new Vector3(4.22f, .116f, 1.173f);

    [Header("UI Hint")]
    public HintUI hintUI;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Rigidbody rb;
    private Collider col;

    private PotCollisionDetector collisionDetector;

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isHeld && nearbyPlayer != null)
            {
                Pickup();
            }
            else if (isHeld)
            {
                if (ovenDoor != null && ovenDoor.IsOpen())
                {
                    float distanceToOven = Vector3.Distance(transform.position, ovenDoor.GetDetectionPosition());
                    if (distanceToOven <= ovenInteractDistance)
                    {
                        PlaceInOven();
                        return;
                    }
                }

                Drop();
            }
            else if (!isHeld && nearbyPlayer == null)
            {
                Debug.Log("Too far from pot to pick up!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isHeld)
        {
            nearbyPlayer = other.gameObject;
            if (rb != null) rb.isKinematic = true;
            Debug.Log("Player near pot, press R to pick up");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == nearbyPlayer)
        {
            nearbyPlayer = null;
            if (!isHeld && rb != null) rb.isKinematic = false;
            Debug.Log("Player left pot range");
        }
    }

    private void Pickup()
    {
        isHeld = true;
        nearbyPlayer = null;

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (col != null)
        {
            col.isTrigger = true;
        }

        collisionDetector = gameObject.AddComponent<PotCollisionDetector>();
        collisionDetector.potScript = this;

        Debug.Log("Picked up pot");
    }

    private void Drop()
    {
        isHeld = false;

        if (collisionDetector != null)
        {
            Destroy(collisionDetector);
            collisionDetector = null;
        }

        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (col != null)
            col.isTrigger = false;

        Vector3 forwardOffset = holdPoint.forward * 0.4f;
        Vector3 dropStart = holdPoint.position + Vector3.up * 0.5f + forwardOffset;
        Vector3 finalDropPosition = dropStart;

        if (Physics.Raycast(dropStart, Vector3.down, out RaycastHit hit, 5f))
        {
            finalDropPosition = hit.point + Vector3.up * 0.1f;
        }

        transform.position = finalDropPosition;

        if (rb != null)
        {
            rb.AddForce(holdPoint.forward * 1.0f, ForceMode.Impulse);
        }

        Debug.Log("Dropped pot");
    }

    private void PlaceInOven()
    {
        isHeld = false;

        if (collisionDetector != null)
        {
            Destroy(collisionDetector);
            collisionDetector = null;
        }

        transform.SetParent(null);
        transform.position = ovenTargetPosition;
        transform.rotation = Quaternion.identity;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (col != null)
            col.isTrigger = false;

        if (hintUI != null)
        {
            hintUI.ShowHint("Keep the door open and press Q for QTE!");
        }

        Debug.Log("Placed pot in oven at: " + ovenTargetPosition);
    }

    public void DropBackward()
    {
        if (!isHeld) return;

        isHeld = false;

        if (collisionDetector != null)
        {
            Destroy(collisionDetector);
            collisionDetector = null;
        }

        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (col != null)
            col.isTrigger = false;

        Vector3 backwardOffset = -holdPoint.forward * 0.5f;
        Vector3 dropStart = holdPoint.position + Vector3.up * 0.8f + backwardOffset;
        Vector3 finalPos = dropStart;

        if (Physics.Raycast(dropStart, Vector3.down, out RaycastHit hit, 5f))
        {
            finalPos = hit.point + Vector3.up * 0.1f;
        }

        transform.position = finalPos;

        if (rb != null)
        {
            rb.AddForce(-holdPoint.forward * 0.5f, ForceMode.Impulse);
        }

        Debug.Log("Pot dropped backward");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}

public class PotCollisionDetector : MonoBehaviour
{
    public PotPickDrop potScript;
    private float collisionCooldown = 0.8f;
    private float lastPickupTime;

    void Start()
    {
        lastPickupTime = Time.time;
    }

    void OnTriggerStay(Collider other)
    {
        if (Time.time - lastPickupTime < collisionCooldown)
        {
            return;
        }

        if (!other.CompareTag("Player") && 
            !other.CompareTag("Item") && 
            !other.CompareTag("airwalls") &&
            !other.CompareTag("CookingPot") &&
            potScript != null)
        {
            Vector3 itemTop = transform.position + Vector3.up * GetComponent<Collider>().bounds.extents.y;
            Vector3 closestPoint = other.ClosestPoint(itemTop);
            
            if (closestPoint.y > transform.position.y)
            {
                Vector3 directionToCollision = (closestPoint - transform.position).normalized;
                float upwardDot = Vector3.Dot(directionToCollision, Vector3.up);
                
                if (upwardDot > 0.9f)
                {
                    Debug.Log($"Pot hit from above by: {other.gameObject.name}");
                    potScript.DropBackward();
                }
            }
        }
    }
}