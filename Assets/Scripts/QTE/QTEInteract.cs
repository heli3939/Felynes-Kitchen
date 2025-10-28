using UnityEngine;

public class QTEInteract : MonoBehaviour
{
    public QTEManager qteManager;
    
    [Header("Pickup Check")]
    public Transform holdPoint;
    
    [Header("Oven QTE Check")]
    public OvenQTEManager ovenQTEManager;
    
    private bool isInRange = false;

    private void Update()
    {
        if (isInRange && Input.GetKeyDown(KeyCode.Q))
        {
            if (holdPoint != null && holdPoint.childCount > 0)
            {
                Transform heldItem = holdPoint.GetChild(0);
                if (heldItem.CompareTag("CookingPot"))
                {
                    Debug.Log("❌ Cannot start ingredient QTE while holding the pot!");
                    return;
                }
                else if (heldItem.CompareTag("Cake"))
                {
                    Debug.Log("❌ Cannot start ingredient QTE while holding the cake!");
                    return;
                }
            }
            
            Debug.Log("✅ Start ingredient QTE！");
            qteManager.StartQTE();
        }
        else if (!isInRange && Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Too far away, cannot start QTE！");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        bool ovenCompleted = (ovenQTEManager != null && ovenQTEManager.HasCompletedOvenQTE());

        if (ovenCompleted)
        {
            if (other.CompareTag("Cake"))
            {
                isInRange = true;
                Debug.Log("✅ In range (Cake detected - decoration mode)");
            }
        }
        else
        {
            if (other.CompareTag("CookingPot"))
            {
                isInRange = true;
                Debug.Log("✅ In range (CookingPot detected - ingredient mode)");
            }
        }
    }
    
    public bool inRange()
    {
        return isInRange;
    }

    private void OnTriggerExit(Collider other)
    {
        bool ovenCompleted = (ovenQTEManager != null && ovenQTEManager.HasCompletedOvenQTE());
        
        if (ovenCompleted)
        {
            if (other.CompareTag("Cake"))
            {
                isInRange = false;
                Debug.Log("❌ Left range (Cake)");
            }
        }
        else
        {
            if (other.CompareTag("CookingPot"))
            {
                isInRange = false;
                Debug.Log("❌ Left range (CookingPot)");
            }
        }
    }
}