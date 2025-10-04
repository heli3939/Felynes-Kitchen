using UnityEngine;

public class PickDrop_Ingredients : MonoBehaviour
{
    public Transform holdPoint; // Where to hold item
    private GameObject heldItem;    // Reference to currently held item
    private GameObject nearbyItem;  // Item we are close enough to pick

    void Update()
    {
        // Pick
        if (Input.GetKeyDown(KeyCode.E) && nearbyItem != null && heldItem == null)
        {
            PickItem();
        }

        // Drop
        if (Input.GetKeyDown(KeyCode.Z) && heldItem != null)
        {
            DropItem();
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
        if (rb != null) rb.isKinematic = false;

        Debug.Log("Dropped: " + heldItem.name);
        heldItem = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item")) 
        {
            nearbyItem = other.gameObject;
            Debug.Log("Item nearby: " + other.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == nearbyItem)
        {
            nearbyItem = null;
            Debug.Log("Item left range");
        }
    }
}
