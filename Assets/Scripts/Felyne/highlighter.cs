using UnityEngine;

public class InteractionHighlighter : MonoBehaviour
{
    [Header("References")]
    public PickDrop_Ingredients pickDropScript;
    public OpenDoor openDoorScript;
    
    [Header("Detection Settings")]
    public float doorOpenRange = 0.7f;
    public float doorDetectionRadius = 1.5f;
    public string doorTag = "Door";
    
    [Header("Outline Material")]
    public Material outlineMaterial;
    public Color itemOutlineColor = Color.yellow;
    public Color doorOutlineColor = Color.cyan;
    public float outlineWidth = 0.02f;
    public float outlineAlpha = 0.8f;
    
    private GameObject currentHighlightedObject;
    private Renderer currentOutlineRenderer;
    private Material[] originalMaterials;
    
    void Start()
    {
        if (pickDropScript == null)
            pickDropScript = GetComponent<PickDrop_Ingredients>();
        
        if (openDoorScript == null)
            openDoorScript = GetComponent<OpenDoor>();
    }
    
    void Update()
    {
        GameObject targetToHighlight = DetermineHighlightTarget();
        
        if (targetToHighlight != currentHighlightedObject)
        {
            ClearHighlight();
            ApplyHighlight(targetToHighlight);
        }
    }
    
    GameObject DetermineHighlightTarget()
    {
        if (pickDropScript != null)
        {
            GameObject nearbyItem = pickDropScript.GetNearbyItem();
            if (nearbyItem != null)
            {
                return nearbyItem;
            }
        }
        
        DoorInteraction nearestDoor = FindNearestOpenableDoor();
        if (nearestDoor != null)
        {
            return nearestDoor.gameObject;
        }
        
        return null;
    }
    
    DoorInteraction FindNearestOpenableDoor()
    {
        DoorInteraction[] allDoors = FindObjectsByType<DoorInteraction>(FindObjectsSortMode.None);
        DoorInteraction nearest = null;
        float minDist = Mathf.Infinity;
        
        foreach (var door in allDoors)
        {
            if (door.IsOpen()) continue;
            
            Vector3 doorPos = door.GetDetectionPosition();
            float dist = Vector3.Distance(transform.position, doorPos);
            
            if (dist <= doorOpenRange && dist < minDist)
            {
                minDist = dist;
                nearest = door;
            }
        }
        
        return nearest;
    }
    
    void ApplyHighlight(GameObject target)
    {
        if (target == null) return;
        
        currentHighlightedObject = target;
        currentOutlineRenderer = target.GetComponent<Renderer>();
        
        if (currentOutlineRenderer == null)
        {
            currentOutlineRenderer = target.GetComponentInChildren<Renderer>();
        }
        
        if (currentOutlineRenderer == null) return;
        
        originalMaterials = currentOutlineRenderer.materials;
        
        Material outlineInstance = new Material(outlineMaterial);
        
        bool isDoor = target.GetComponent<DoorInteraction>() != null;
        outlineInstance.SetColor("_OutlineColor", isDoor ? doorOutlineColor : itemOutlineColor);
        outlineInstance.SetFloat("_OutlineWidth", outlineWidth);
        outlineInstance.SetFloat("_Alpha", outlineAlpha);
        
        Bounds bounds = currentOutlineRenderer.bounds;
        float objectSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        outlineInstance.SetFloat("_Bounds", objectSize);
        
        Material[] newMaterials = new Material[originalMaterials.Length + 1];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            newMaterials[i] = originalMaterials[i];
        }
        newMaterials[originalMaterials.Length] = outlineInstance;
        
        currentOutlineRenderer.materials = newMaterials;
    }
    
    void ClearHighlight()
    {
        if (currentOutlineRenderer != null && originalMaterials != null)
        {
            currentOutlineRenderer.materials = originalMaterials;
            originalMaterials = null;
        }
        
        currentHighlightedObject = null;
        currentOutlineRenderer = null;
    }
    
    void OnDestroy()
    {
        ClearHighlight();
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, doorOpenRange);
        
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireSphere(transform.position, doorDetectionRadius);
    }
}