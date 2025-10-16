using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public Animator animator;
    public string openParameter = "isOpen";
    
    [Header("Custom Detection")]
    public Transform customDetectionPoint;
    
    [Header("Baking Animation")]
    public bool isOvenDoor = false;
    public Animator bakingAnimator;
    public string bakingParameter = "isBaking";
    public float bakingDuration = 5f;

    public GameObject cookingPot;
    public Vector3 ovenTargetPosition = new Vector3(4.22f, 0.116f, 1.173f);
    public float positionTolerance = 0.2f;
    
    private bool isOpen = false;
    private bool isBaking = false;
    private bool hasBaked = false;
    
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

    public void StartBaking()
    {
        Debug.Log($"[Door] StartBaking() called on {gameObject.name}");
        
        if (!isOvenDoor)
        {
            Debug.LogWarning("[Door] ❌ Not an oven door!");
            return;
        }
        Debug.Log("[Door] ✅ Is oven door");

        if (hasBaked)
        {
            Debug.LogWarning("[Door] ❌ Oven has already been used! Cannot bake again.");
            return;
        }
        
        if (isBaking)
        {
            Debug.Log("[Door] ❌ Already baking!");
            return;
        }
        Debug.Log("[Door] ✅ Not already baking");
        
        if (isOpen)
        {
            Debug.LogWarning($"[Door] ❌ Cannot start baking with door open! isOpen={isOpen}");
            return;
        }
        Debug.Log($"[Door] ✅ Door is closed (isOpen={isOpen})");
        
        if (cookingPot != null)
        {
            float distance = Vector3.Distance(cookingPot.transform.position, ovenTargetPosition);
            Debug.Log($"[Door] Pot distance from target: {distance:F3}");
            
            if (distance > positionTolerance)
            {
                Debug.LogWarning($"[Door] ❌ Pot is not in the oven! Distance: {distance:F2} > {positionTolerance}");
                return;
            }
            Debug.Log($"[Door] ✅ Pot is in correct position (distance: {distance:F3})");
        }
        else
        {
            Debug.LogWarning("[Door] ⚠️ Cooking pot reference is missing! Cannot verify position.");
        }

        isBaking = true;
        hasBaked = true;
        
        if (bakingAnimator != null)
        {
            Debug.Log($"[Door] ✅ Setting bakingAnimator.SetBool({bakingParameter}, true)");
            bakingAnimator.SetBool(bakingParameter, true);
            
            bool paramValue = bakingAnimator.GetBool(bakingParameter);
            Debug.Log($"[Door] Parameter '{bakingParameter}' value after set: {paramValue}");
            
            AnimatorStateInfo stateInfo = bakingAnimator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[Door] Current animator state: {stateInfo.shortNameHash}");
        }
        else
        {
            Debug.LogError("[Door] ❌ bakingAnimator is NULL!");
        }
        
        Debug.Log($"[Door] 🔥 Baking started! Will complete in {bakingDuration} seconds");
        Invoke(nameof(OnBakingComplete), bakingDuration);
    }
    
    private void OnBakingComplete()
    {
        isBaking = false;
        
        if (bakingAnimator != null)
        {
            bakingAnimator.SetBool(bakingParameter, false);
        }
        
        Debug.Log("[Door] Baking completed! Door can now be opened.");
    }
    
    public bool IsBaking()
    {
        return isBaking;
    }
    
    public bool IsOvenDoor()
    {
        return isOvenDoor;
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