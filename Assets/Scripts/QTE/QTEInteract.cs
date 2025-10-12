using UnityEngine;

public class QTEInteract : MonoBehaviour
{
    public QTEManager qteManager;
    
    [Header("Pickup Check")]
    public Transform holdPoint;
    
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
            }
            
            Debug.Log("✅ Start ingredient QTE！");
            qteManager.StartQTE();
        }
        else if (!isInRange && Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Far away from pot, cannot start QTE！");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CookingPot")) 
        {
            isInRange = true;
            Debug.Log("in the range");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CookingPot"))
        {
            isInRange = false;
            Debug.Log("move away from the range");
        }
    }
}