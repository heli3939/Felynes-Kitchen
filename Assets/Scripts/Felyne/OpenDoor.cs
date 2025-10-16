using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public float autoCloseDistance = 1.5f;
    public float maxOpenDistance = 0.7f;

    private DoorInteraction[] allDoors;
    private Collider catCollider;

    [Header("UI Hint")]
    public HintUI hintUI;

    [Header("Oven QTE Manager")]
    public OvenQTEManager ovenQTEManager;

    void Start()
    {
        catCollider = GetComponent<Collider>();
        allDoors = GameObject.FindObjectsByType<DoorInteraction>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DoorInteraction closestDoor = null;
            float minDist = Mathf.Infinity;

            foreach (var door in allDoors)
            {
                Vector3 doorPosition = door.GetDetectionPosition();
                float dist = Vector3.Distance(catCollider.bounds.center, doorPosition);

                if (dist < minDist && !door.IsOpen())
                {
                    minDist = dist;
                    closestDoor = door;
                }
            }

            if (closestDoor != null)
            {
                if (minDist <= maxOpenDistance)
                {
                    if (closestDoor.IsOvenDoor() && closestDoor.IsBaking())
                    {
                        Debug.Log("❌ Oven is baking! Cannot open the door.");
                        if (hintUI != null)
                        {
                            hintUI.ShowHint("Oven is baking! Please wait...");
                        }
                        return;
                    }
                    
                    closestDoor.ToggleDoor();
                    Debug.Log("Opened door: " + closestDoor.name + "," + minDist);
                }
                else
                {
                    Debug.Log("Too far to open door: " + closestDoor.name + " (" + minDist + " units away)");
                }
            }
        }

        foreach (var door in allDoors)
        {
            if (!door.IsOpen()) continue;

            Vector3 doorPosition = door.GetDetectionPosition();
            float dist = Vector3.Distance(catCollider.bounds.center, doorPosition);

            if (dist > autoCloseDistance)
            {
                Debug.Log($"[OpenDoor] Auto closing door: {door.name}");
                
                if (door.IsOvenDoor())
                {
                    Debug.Log("[OpenDoor] This is an oven door!");
                    
                    if (ovenQTEManager != null)
                    {
                        bool hasCompleted = ovenQTEManager.HasCompletedOvenQTE();
                        Debug.Log($"[OpenDoor] OvenQTE completed: {hasCompleted}");
                        
                        if (hasCompleted)
                        {
                            Debug.Log("[OpenDoor] ✅ Starting baking...");
                            door.CloseDoor();
                            door.StartBaking();
                        }
                        else
                        {
                            Debug.Log("[OpenDoor] ❌ OvenQTE not completed yet");
                            door.CloseDoor();
                        }
                    }
                    else
                    {
                        Debug.LogError("[OpenDoor] ❌ OvenQTEManager reference is missing! Drag it in Inspector.");
                        door.CloseDoor();
                    }
                }
                else
                {
                    door.CloseDoor();
                }
            }
        }
    }
}