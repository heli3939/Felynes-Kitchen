using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public float autoCloseDistance = 1.5f;
    public float maxOpenDistance = 0.7f;

    private DoorInteraction[] allDoors;
    private Collider catCollider;

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
                door.CloseDoor();
                Debug.Log("Auto closed door: " + door.name + "," + dist);
            }
        }
    }
}