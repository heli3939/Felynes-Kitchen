using UnityEngine;

public class PickDrop_Ingredients : MonoBehaviour
{
    public Transform holdPoint; // Where to hold item
    private GameObject heldItem; // Reference to currently held item
    private GameObject nearbyItem; // Item we are close enough to pick
    private GameObject lastDroppedItem; // Track just-dropped item

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

        // Calculate the Drop point in front of the character
        Vector3 forwardOffset = transform.forward * 0.4f; 
        Vector3 dropStart = transform.position + Vector3.up * 1.0f + forwardOffset;

        Vector3 finalDropPosition = dropStart;

        if (Physics.Raycast(dropStart, Vector3.down, out RaycastHit hit, 5f))
        {
            finalDropPosition = hit.point + Vector3.up * 0.1f; 
            Debug.Log("Raycast hit ground at: " + hit.point);
        }
        else
        {
            Debug.Log("Raycast did not hit ground, using default drop position");
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
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.4f); // Reduced to 0.4 units
        float closestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Item") && col.gameObject != heldItem && col.gameObject != lastDroppedItem)
            {
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

        lastDroppedItem = null; // Reset
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item") && nearbyItem == null)
        {
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