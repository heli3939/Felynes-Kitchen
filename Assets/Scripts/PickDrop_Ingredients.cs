using UnityEngine;

public class PickDrop_Ingredients : MonoBehaviour
{
    public Transform holdPoint; // Where to hold item
    private GameObject heldItem; // Reference to currently held item
    private GameObject nearbyItem; // Item we are close enough to pick
    private GameObject lastDroppedItem; // Track just-dropped item
    public GameObject GetHeldItem()
    {
        return heldItem;
    }

    void Start()
    {
        UpdateNearbyItem(); // Initialize detection
    }

    void Update()
    {
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

    void PickItem()
    {
        heldItem = nearbyItem;

        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        Collider col = heldItem.GetComponent<Collider>();

        // mute the physics and collider
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (col != null)
            col.enabled = false;

        heldItem.transform.SetParent(holdPoint);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;

        Debug.Log("Picked up: " + heldItem.name);
    }

    void DropItem()
    {
        heldItem.transform.SetParent(null);
        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        Collider col = heldItem.GetComponent<Collider>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (col != null)
            col.enabled = true;

        Vector3 forwardOffset = transform.forward * 0.4f;
        Vector3 dropStart = transform.position + Vector3.up * 1.0f + forwardOffset;
        Vector3 finalDropPosition = dropStart;
        if (Physics.Raycast(dropStart, Vector3.down, out RaycastHit hit, 5f))
            finalDropPosition = hit.point + Vector3.up * 0.1f;

        heldItem.transform.position = finalDropPosition;

        if (rb != null)
            rb.AddForce(transform.forward * 1.0f, ForceMode.Impulse);

        Debug.Log("Dropped: " + heldItem.name);

        lastDroppedItem = heldItem;
        heldItem = null;

        nearbyItem = null;

        StartCoroutine(DelayedUpdateNearbyItem(0.3f));
    }

    private System.Collections.IEnumerator DelayedUpdateNearbyItem(float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateNearbyItem();   // transfer the update function
    }


    private System.Collections.IEnumerator ClearLastDroppedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        lastDroppedItem = null;
    }

    void UpdateNearbyItem()
    {
        nearbyItem = null;
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.3f);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.4f);

        if (nearbyItem != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nearbyItem.transform.position);
        }
    }
}