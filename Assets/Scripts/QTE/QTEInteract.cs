using UnityEngine;

public class QTEInteract : MonoBehaviour
{
    public QTEManager qteManager; 
    private bool isInRange = false;

    private void Update()
    {
        if (isInRange && Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Start QTE！");
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
