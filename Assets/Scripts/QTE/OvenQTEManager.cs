using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OvenQTEManager : MonoBehaviour
{
    [Header("QTE UI")]
    public GameObject ovenQteCanvas;
    public QTEUIController ovenQteController;

    private GraphicRaycaster qteRaycaster;
    private bool qteRunning = false;
    private bool hasCompletedOvenQTE = false;

    [Header("Player Control Scripts")]
    public MonoBehaviour[] playerControlScripts;

    [Header("Oven Check")]
    public Transform holdPoint;
    public DoorInteraction ovenDoor;

    [Header("Camera Switcher")]
    public OvenCameraSwitcher ovenCameraSwitcher;

    [Header("UI Hint")]
    public HintUI hintUI;

    private void Awake()
    {
        if (ovenQteCanvas != null)
            qteRaycaster = ovenQteCanvas.GetComponent<GraphicRaycaster>();

        if (ovenQteController != null)
            ovenQteController.OnQTEFinished += HandleFinished;
    }

    private void OnDestroy()
    {
        if (ovenQteController != null)
            ovenQteController.OnQTEFinished -= HandleFinished;
    }

    public void StartOvenQTE()
    {
        if (hasCompletedOvenQTE)
        {
            Debug.LogWarning("[OvenQTE] Oven QTE already completed! Cannot start again.");
            if (hintUI != null)
                hintUI.ShowHint("Cake baked already.");
            return;
        }

        if (ovenDoor != null && !ovenDoor.IsOpen())
        {
            Debug.LogWarning("[OvenQTE] Oven door is closed!");
            if (hintUI != null)
                hintUI.ShowHint("Open the oven door first!");
            return;
        }

        if (holdPoint != null && holdPoint.childCount > 0)
        {
            Transform heldItem = holdPoint.GetChild(0);
            if (heldItem.CompareTag("CookingPot"))
            {
                Debug.LogWarning("[OvenQTE] Cannot start QTE while holding the pot! Put it in the oven first.");
                if (hintUI != null)
                    hintUI.ShowHint("Put the pot in the oven first!");
                return;
            }
        }

        if (qteRunning)
        {
            Debug.LogWarning("[OvenQTE] Already running!");
            return;
        }

        qteRunning = true;
        SetPlayerControls(false);

        if (ovenCameraSwitcher != null)
        {
            ovenCameraSwitcher.SwitchToOvenView();
        }

        if (ovenQteCanvas != null)
        {
            ovenQteCanvas.SetActive(true);

            if (ovenQteController != null && !ovenQteController.gameObject.activeSelf)
                ovenQteController.gameObject.SetActive(true);

            if (qteRaycaster != null)
                qteRaycaster.enabled = true;
        }

        if (ovenQteController != null)
        {
            ovenQteController.StartQTE(); 
        }

        Debug.Log("[OvenQTE] Started ✅");
    }

    public void EndOvenQTE()
    {
        qteRunning = false;

        if (ovenQteController != null)
            ovenQteController.StopQTE();

        if (qteRaycaster != null)
            qteRaycaster.enabled = false;

        if (ovenQteCanvas != null)
        {
            ovenQteCanvas.SetActive(false);
        }

        if (ovenCameraSwitcher != null)
        {
            ovenCameraSwitcher.StartCoroutine(ovenCameraSwitcher.SwitchBackToThirdPersonDelayed(1.5f));
        }
        
        if (hintUI != null)
        {
            hintUI.ShowHint("Moving away from oven to start baking.");
        }

        SetPlayerControls(true);
        qteHandled = false;
        
        Debug.Log("[OvenQTE] Ended ✅");
    }

    private bool qteHandled = false;

    private void HandleFinished(QTEResult[] results)
    {
        if (qteHandled) return;

        foreach (var r in results)
        {
            ScoreSystem.Instance.AddScore(r);
        }

        hasCompletedOvenQTE = true;
        Debug.Log("[OvenQTE] QTE completed! Oven QTE is now locked.");
        
        qteHandled = true;
    }

    public void SetPlayerControls(bool enabled)
    {
        foreach (var script in playerControlScripts)
        {
            if (script != null)
                script.enabled = enabled;
        }
        Debug.Log($"[OvenQTE] Player control {(enabled ? "ENABLED" : "DISABLED")}");
    }

    public bool HasCompletedOvenQTE()
    {
        return hasCompletedOvenQTE;
    }
}