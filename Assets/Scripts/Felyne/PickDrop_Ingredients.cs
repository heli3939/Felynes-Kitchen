using UnityEngine;


public class PickDrop_Ingredients : MonoBehaviour
{
    public Transform holdPoint;
    private GameObject heldItem;
    private GameObject nearbyItem;

    public GameObject GetHeldItem()
    {
        return heldItem;
    }

    public GameObject GetNearbyItem()
    {
        return nearbyItem;
    }

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

        UpdateNearbyItem();

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
            if (hit.collider.gameObject == gameObject ||
                hit.collider.CompareTag("Item") ||
                hit.collider.CompareTag("incorrect") ||
                hit.collider.CompareTag("Strawberry") ||
                hit.collider.CompareTag("Cream"))
            {
                continue;
            }

            DoorInteraction door = hit.collider.GetComponentInParent<DoorInteraction>();
            if (door != null && !door.IsOpen())
            {
                return false;
            }

            if (!hit.collider.CompareTag("airwalls") && !hit.collider.CompareTag("Player"))
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

        heldItem.transform.SetParent(holdPoint);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;

        ItemCollisionDetector detector = heldItem.GetComponent<ItemCollisionDetector>();
        if (detector == null)
        {
            detector = heldItem.AddComponent<ItemCollisionDetector>();
        }
        detector.pickDropScript = this;

        Debug.Log("Picked up: " + heldItem.name);
    }

    void DropItem()
    {
        ItemCollisionDetector detector = heldItem.GetComponent<ItemCollisionDetector>();
        if (detector != null)
        {
            Destroy(detector);
        }

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
        {
            col.isTrigger = false;
        }

        Vector3 dropStart = holdPoint.transform.position;
        Vector3 finalDropPosition = dropStart;

        if (Physics.Raycast(dropStart, Vector3.down, out RaycastHit hit, 5f))
        {
            float itemHeight = col.bounds.extents.y;
            finalDropPosition = hit.point + Vector3.up * itemHeight;
        }

        heldItem.transform.position = finalDropPosition;

        Debug.Log("Dropped: " + heldItem.name);

        heldItem = null;
        nearbyItem = null;
    }

    void UpdateNearbyItem()
    {
        nearbyItem = null;
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.3f);
        float closestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            if ((col.CompareTag("Item") ||
                col.CompareTag("incorrect") ||
                col.CompareTag("Strawberry") ||
                col.CompareTag("Cream")) &&
                col.gameObject != heldItem)
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

        // if (nearbyItem != null)
        // {
        //     Rigidbody rb = nearbyItem.GetComponent<Rigidbody>();
        //     if (rb != null) rb.isKinematic = true;
        // }
    }

    void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Item") ||
            other.CompareTag("incorrect") ||
            other.CompareTag("Strawberry") ||
            other.CompareTag("Cream")) &&
            other.gameObject != heldItem)
        {
            UpdateNearbyItem();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if ((other.CompareTag("Item") ||
            other.CompareTag("incorrect") ||
            other.CompareTag("Strawberry") ||
            other.CompareTag("Cream")) &&
            other.gameObject != heldItem)
        {
            UpdateNearbyItem();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Item") ||
            other.CompareTag("incorrect") ||
            other.CompareTag("Strawberry") ||
            other.CompareTag("Cream")))
        {
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

    void CheckItemCollision()
    {
        if (heldItem == null) return;

        Collider itemCollider = heldItem.GetComponent<Collider>();
        if (itemCollider == null) return;

        Bounds bounds = itemCollider.bounds;
        Vector3 center = bounds.center;
        Vector3 halfExtents = bounds.extents * 0.8f;

        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, heldItem.transform.rotation);

        foreach (Collider col in hitColliders)
        {
            if (col.gameObject == heldItem ||
                col.gameObject == gameObject ||
                col.CompareTag("Item") ||
                col.CompareTag("incorrect") ||
                col.CompareTag("Strawberry") ||
                col.CompareTag("Cream") ||
                col.CompareTag("airwalls") ||
                col.CompareTag("Player"))
            {
                continue;
            }

            Debug.Log($"Item collided with: {col.gameObject.name}");
            // DropItemBackward();
            return;
        }
    }

    //     public void DropItemBackward()
    //     {
    //         if (heldItem == null) return;

    //         ItemCollisionDetector detector = heldItem.GetComponent<ItemCollisionDetector>();
    //         if (detector != null)
    //         {
    //             Destroy(detector);
    //         }

    //         heldItem.transform.SetParent(null);
    //         Rigidbody rb = heldItem.GetComponent<Rigidbody>();
    //         Collider col = heldItem.GetComponent<Collider>();

    //         if (rb != null)
    //         {
    //             rb.isKinematic = false;
    //             rb.useGravity = true;
    //             rb.linearVelocity = Vector3.zero;
    //             rb.angularVelocity = Vector3.zero;
    //         }

    //         if (col != null)
    //         {
    //             col.isTrigger = false;
    //         }

    //         Vector3 backwardOffset = -transform.forward * 0.5f;
    //         Vector3 dropStart = transform.position + Vector3.up * 0.8f + backwardOffset;
    //         Vector3 finalDropPosition = dropStart;

    //         if (Physics.Raycast(dropStart, Vector3.down, out RaycastHit hit, 5f))
    //         {
    //             finalDropPosition = hit.point + Vector3.up * 0.1f;
    //         }

    //         heldItem.transform.position = finalDropPosition;

    //         if (rb != null)
    //         {
    //             rb.AddForce(-transform.forward * 0.5f, ForceMode.Impulse);
    //         }

    //         Debug.Log("Dropped backward: " + heldItem.name);

    //         heldItem = null;
    //     }
    // }

    public class ItemCollisionDetector : MonoBehaviour
    {
        public PickDrop_Ingredients pickDropScript;
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
                !other.CompareTag("incorrect") &&
                !other.CompareTag("Strawberry") &&
                !other.CompareTag("Cream") &&
                !other.CompareTag("airwalls") &&
                pickDropScript != null)
            {
                Vector3 itemTop = transform.position + Vector3.up * GetComponent<Collider>().bounds.extents.y;
                Vector3 closestPoint = other.ClosestPoint(itemTop);

                if (closestPoint.y > transform.position.y)
                {
                    Vector3 directionToCollision = (closestPoint - transform.position).normalized;
                    float upwardDot = Vector3.Dot(directionToCollision, Vector3.up);

                    if (upwardDot > 0.9f)
                    {
                        Debug.Log($"Item hit from above by: {other.gameObject.name}");
                        // pickDropScript.DropItemBackward();
                    }
                }
            }
        }
    }
}