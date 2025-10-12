using UnityEngine;
using System.Collections;

public class OvenCameraSwitcher : MonoBehaviour
{
    public Camera thirdPersonCam; 
    public Camera ovenCam;
    public MonoBehaviour playerMovementScript;

    [Header("Pickup Check")]
    public Transform holdPoint;

    private Vector3 lastThirdPersonPosition; 
    private Quaternion lastThirdPersonRotation;

    void Start()
    {
        if (thirdPersonCam != null)
        {
            lastThirdPersonPosition = thirdPersonCam.transform.position;
            lastThirdPersonRotation = thirdPersonCam.transform.rotation;
        }
        
        if (ovenCam != null)
        {
            ovenCam.gameObject.SetActive(false); 
            ovenCam.enabled = false;
        }
    }

    public void SwitchToOvenView()
    {
        if (thirdPersonCam != null)
        {
            lastThirdPersonPosition = thirdPersonCam.transform.position;
            lastThirdPersonRotation = thirdPersonCam.transform.rotation;
            thirdPersonCam.enabled = false;
            thirdPersonCam.gameObject.SetActive(false);
            var follow = thirdPersonCam.GetComponent<CameraFollow>();
            if (follow != null) follow.enabled = false;
        }
        
        if (ovenCam != null)
        {
            ovenCam.gameObject.SetActive(true);
            ovenCam.enabled = true;
        }
        
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false; 
        }
        
        Debug.Log("[OvenCameraSwitcher] Switched to Oven Camera");
    }

    public void SwitchBackToThirdPerson()
    {
        if (ovenCam != null)
        {
            ovenCam.enabled = false;
            ovenCam.gameObject.SetActive(false);
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
        
        var ovenQteManager = FindFirstObjectByType<OvenQTEManager>();
        if (ovenQteManager != null)
        {
            ovenQteManager.SetPlayerControls(true);  
        }
        
        Debug.Log("[OvenCameraSwitcher] Back to Third Person");
    }

    public IEnumerator SwitchBackToThirdPersonDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        SwitchBackToThirdPerson();
    }
}