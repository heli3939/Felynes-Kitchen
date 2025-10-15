using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public Animator animator;
    public string openParameter = "isOpen";
    [Header("Custom Detection")]
    public Transform customDetectionPoint;
    
    private bool isOpen = false;
    
    public void ToggleDoor()
    {
        isOpen = !isOpen;
        if (animator != null)
            animator.SetBool(openParameter, isOpen);
    }
    
    public bool IsOpen()
    {
        return isOpen;
    }

    public void CloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            if (animator != null)
                animator.SetBool(openParameter, isOpen);
        }
    }
    
    public Vector3 GetDetectionPosition()
    {
        if (customDetectionPoint != null)
        {
            return customDetectionPoint.position;
        }
        
        Collider doorCollider = GetComponent<Collider>();
        if (doorCollider != null)
        {
            return doorCollider.bounds.center;
        }
        
        return transform.position;
    }
}