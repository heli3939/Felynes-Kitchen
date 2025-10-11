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
                StartCoroutine(SwitchBackToThirdPersonDelayed(1f));
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
            Debug.LogWarning("[CameraSwitcher] Cannot switch to FP camera — player is not holding any item.");
            if (hintUI != null)
                hintUI.ShowHint("You need to pick up an ingredient first!");
            return;
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
            firstPersonCam.gameObject.SetActive(false); // prohibited
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