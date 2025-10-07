using UnityEngine;

public class PickDrop_Ingredients : MonoBehaviour
{
    public Transform holdPoint;
    private GameObject heldItem;
    private GameObject nearbyItem;
    private GameObject lastDroppedItem;
    

    void Start()
    {
        UpdateNearbyItem();
    }

    void Update()
    {
        if (heldItem != null)
        {
            CheckItemCollision();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldItem == null && nearbyItem != null)
            {
                PickItem();
            }
            else if (heldItem != null)
            {
                DropItem();
            }
        }
    }

    bool IsItemAccessible(Vector3 itemPosition)
    {
        Vector3 startPosition = transform.position + Vector3.up * 0.5f;
        Vector3 directionToItem = (itemPosition - startPosition).normalized;
        float distance = Vector3.Distance(startPosition, itemPosition);
        
        RaycastHit[] hits = Physics.RaycastAll(startPosition, directionToItem, distance);
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject || hit.collider.CompareTag("Item"))
            {
                continue;
            }
            
            DoorInteraction door = hit.collider.GetComponentInParent<DoorInteraction>();
            if (door != null && !door.IsOpen())
            {
                return false;
            }
            
            if (!hit.collider.CompareTag("airwalls") && 
                !hit.collider.CompareTag("Player"))
            {
                if (hit.collider.GetComponent<DoorInteraction>() != null)
                {
                    DoorInteraction directDoor = hit.collider.GetComponent<DoorInteraction>();
                    if (!directDoor.IsOpen())
                    {
                        return false;
                    }
                }
            }
        }
        
        return true;
    }

    void CheckItemCollision()
    {
        Collider itemCollider = heldItem.GetComponent<Collider>();
        if (itemCollider == null) return;

        Bounds bounds = itemCollider.bounds;
        Vector3 itemCenter = bounds.center;
        Vector3 itemSize = bounds.size;

        Vector3[] directions = new Vector3[]
        {
        Vector3.up,
        Vector3.down,
        transform.forward,
        -transform.forward,
        transform.right,
        -transform.right
        };

        float rayDistance = Mathf.Max(itemSize.x, itemSize.y, itemSize.z) * 0.6f;

        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(itemCenter, dir, out RaycastHit hit, rayDistance))
            {
                if (hit.collider.gameObject != heldItem &&
                    hit.collider.gameObject != gameObject &&
                    !hit.collider.CompareTag("Item") &&
                    !hit.collider.CompareTag("airwalls") &&
                    !hit.collider.CompareTag("Player"))
                {
                    Debug.Log($"detect: {hit.collider.gameObject.name}, drop backwards");
                    DropItemBackward();
                    return;
                }
            }
        }
    }

    void DropItemBackward()
    {
        heldItem.transform.SetParent(null);
        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Vector3 backwardOffset = -transform.forward * 0.5f;
        Vector3 dropStart = transform.position + Vector3.up * 0.8f + backwardOffset;
        Vector3 finalDropPosition = dropStart;

        if (Physics.Raycast(dropStart, Vector3.down, out RaycastHit hit, 5f))
        {
            finalDropPosition = hit.point + Vector3.up * 0.1f;
        }

        heldItem.transform.position = finalDropPosition;

        if (rb != null)
        {
            rb.AddForce(-transform.forward * 0.5f, ForceMode.Impulse);
        }

        lastDroppedItem = heldItem;
        heldItem = null;

        StartCoroutine(DelayedUpdateNearbyItem(0.2f));
    }

    void PickItem()
    {
        heldItem = nearbyItem;
        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        heldItem.transform.SetParent(holdPoint);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;
        Debug.Log("Picked up: " + heldItem.name);
    }

    void DropItem()
    {
        heldItem.transform.SetParent(null);
        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;         
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Vector3 forwardOffset = transform.forward * 0.4f;
        Vector3 dropStart = transform.position + Vector3.up * 1.0f + forwardOffset;
        
        Vector3 finalDropPosition = dropStart;

        if (Physics.Raycast(dropStart, Vector3.down, out RaycastHit hit, 5f))
        {
            finalDropPosition = hit.point + Vector3.up * 0.1f; 

        }

        heldItem.transform.position = finalDropPosition;

        if (rb != null)
        {
            rb.AddForce(transform.forward * 1.0f, ForceMode.Impulse);
        }

        Debug.Log("Dropped: " + heldItem.name + " at position: " + heldItem.transform.position);
        lastDroppedItem = heldItem;
        heldItem = null;

        StartCoroutine(DelayedUpdateNearbyItem(0.2f));
    }

    private System.Collections.IEnumerator DelayedUpdateNearbyItem(float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateNearbyItem();
    }

    void UpdateNearbyItem()
    {
        nearbyItem = null;
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.4f);
        float closestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Item") && col.gameObject != heldItem && col.gameObject != lastDroppedItem)
            {
                if (!IsItemAccessible(col.transform.position))
                {
                    continue;
                }
                
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearbyItem = col.gameObject;
                }
            }
        }

        if (nearbyItem != null)
        {
            Rigidbody rb = nearbyItem.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            Debug.Log("New nearby item found: " + nearbyItem.name);
        }

        lastDroppedItem = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item") && nearbyItem == null)
        {
            if (!IsItemAccessible(other.transform.position))
            {
                Debug.Log($" {other.name} block by door, can't pick");
                return;
            }
            
            nearbyItem = other.gameObject;
            Rigidbody rb = nearbyItem.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            Debug.Log("Item nearby: " + other.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == nearbyItem)
        {
            nearbyItem = null;
            Debug.Log("Item left range");
            UpdateNearbyItem();
        }
    }
}