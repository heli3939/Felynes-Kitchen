using UnityEngine;

public class OvenQTEInteract : MonoBehaviour
{
    public OvenQTEManager ovenQTEManager;
    public DoorInteraction ovenDoor;
    public float interactDistance = 2f;

    [Header("Debug")]
    public bool showDebug = true;

    private void Update()
    {
        if (ovenQTEManager == null || ovenDoor == null) return;

        Vector3 doorPos = ovenDoor.GetDetectionPosition();
        float distance = Vector3.Distance(transform.position, doorPos);
        bool doorOpen = ovenDoor.IsOpen();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (showDebug)
            {
                Debug.Log($"[OvenQTE] Dist={distance:F2} | DoorOpen={doorOpen} | InRange={distance <= interactDistance}");
            }

            if (distance <= interactDistance && doorOpen)
            {
                Debug.Log("✅ Starting Oven QTE!");
                ovenQTEManager.StartOvenQTE();
            }
            else
            {
                if (distance > interactDistance)
                    Debug.Log($"❌ Too far! {distance:F2} > {interactDistance}");
                if (!doorOpen)
                    Debug.Log("❌ Door closed!");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebug || ovenDoor == null) return;

        Vector3 doorPos = ovenDoor.GetDetectionPosition();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(doorPos, interactDistance);

        if (Application.isPlaying)
        {
            float distance = Vector3.Distance(transform.position, doorPos);
            Gizmos.color = distance <= interactDistance ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, doorPos);
        }
    }
}