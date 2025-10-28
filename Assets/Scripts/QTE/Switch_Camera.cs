using UnityEngine;
using System.Collections;

public class CameraSwitcher : MonoBehaviour
{
    public Camera thirdPersonCam; 
    public Camera firstPersonCam; 
    public KeyCode interactKey = KeyCode.Q; 
    public MonoBehaviour playerMovementScript;

    [Header("Pickup Check")]
    public Transform holdPoint;

    [Header("UI Hint for empty")]
    public HintUI hintUI;

    [Header("Cooking Pot Check")]
    public GameObject cookingPot;

    private bool isPlayerNearby = false;
    private bool isInFirstPerson = false;
    private Vector3 lastThirdPersonPosition; 
    private Quaternion lastThirdPersonRotation;

    void Start()
    {
        if (thirdPersonCam != null)
        {
            thirdPersonCam.gameObject.SetActive(true);
            thirdPersonCam.enabled = true;
            var follow = thirdPersonCam.GetComponent<CameraFollow>();
            if (follow != null) follow.enabled = true;
            lastThirdPersonPosition = thirdPersonCam.transform.position;
            lastThirdPersonRotation = thirdPersonCam.transform.rotation;
        }
        if (firstPersonCam != null)
        {
            firstPersonCam.gameObject.SetActive(false); 
            firstPersonCam.enabled = false;
        }
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(interactKey))
        {
            if (!isInFirstPerson)
                SwitchToFirstPerson();
            else
                SwitchBackToThirdPerson();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (isInFirstPerson)
                SwitchBackToThirdPerson();
        }
    }

    private void SwitchToFirstPerson()
    {
        if (holdPoint == null || holdPoint.childCount == 0)
        {
            if (cookingPot != null)
            {
                Vector3 targetPosition = new Vector3(4.22f, .116f, 1.173f);
                float distance = Vector3.Distance(cookingPot.transform.position, targetPosition);

                if (distance <= 0.2f)
                {
                    Debug.Log("[CameraSwitcher] Pot is already in position — skip hint.");
                    return;
                }
            }

            Debug.LogWarning("[CameraSwitcher] Cannot switch — not holding item.");
            return;
        }

        Transform heldItem = holdPoint.GetChild(0);
        if (heldItem.CompareTag("CookingPot"))
        {
            Debug.LogWarning("[CameraSwitcher] Cannot switch to ingredient camera while holding pot!");
            return;
        }

        if (heldItem.CompareTag("Cream"))
        {
            OvenQTEManager ovenQTEManager = FindFirstObjectByType<OvenQTEManager>();
            bool ovenCompleted = (ovenQTEManager != null && ovenQTEManager.HasCompletedOvenQTE());

            if (!ovenCompleted)
            {
                return;
            }
        }

        if (thirdPersonCam != null)
        {
            lastThirdPersonPosition = thirdPersonCam.transform.position;
            lastThirdPersonRotation = thirdPersonCam.transform.rotation;
            thirdPersonCam.enabled = false;
            thirdPersonCam.gameObject.SetActive(false);
            var follow = thirdPersonCam.GetComponent<CameraFollow>();
            if (follow != null) follow.enabled = false;
        }
        if (firstPersonCam != null)
        {
            firstPersonCam.gameObject.SetActive(true);
            firstPersonCam.enabled = true;
        }
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }
        isInFirstPerson = true;
    }

    private void SwitchBackToThirdPerson()
    {
        if (firstPersonCam != null)
        {
            firstPersonCam.enabled = false;
            firstPersonCam.gameObject.SetActive(false);
        }
        if (thirdPersonCam != null)
        {
            thirdPersonCam.gameObject.SetActive(true);
            thirdPersonCam.enabled = true;
            var follow = thirdPersonCam.GetComponent<CameraFollow>();
            if (follow != null)
            {
                follow.enabled = true;
                thirdPersonCam.transform.position = lastThirdPersonPosition;
                thirdPersonCam.transform.rotation = lastThirdPersonRotation;
            }
        }
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true; 
        }
        isInFirstPerson = false;

        var qteManager = FindFirstObjectByType<QTEManager>();
        if (qteManager != null)
        {
            qteManager.SetPlayerControls(true);  
            Debug.Log("[CameraSwitcher] ✅ Player control restored after camera switch");
        }
    }

    public IEnumerator SwitchBackToThirdPersonDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        SwitchBackToThirdPerson();
    }
}