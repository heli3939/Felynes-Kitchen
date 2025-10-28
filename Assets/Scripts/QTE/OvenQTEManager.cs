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
    public GameObject cookingPot;
    public Vector3 ovenTargetPosition = new Vector3(4.22f, 0.116f, 1.173f);
    public float positionTolerance = 0.2f;

    [Header("Camera Switcher")]
    public OvenCameraSwitcher ovenCameraSwitcher;

    [Header("UI Hint")]
    public HintUI hintUI;

    [Header("Cake System")]
    public GameObject potLiquid;
    public Color cakeColor = new Color(1f, 0.8f, 0.6f);
    private bool cakeColorChanged = false;

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

        if (cookingPot != null)
        {
            float distance = Vector3.Distance(cookingPot.transform.position, ovenTargetPosition);
            if (distance > positionTolerance)
            {
                Debug.LogWarning($"[OvenQTE] Pot is not in the oven! Distance: {distance:F2}");
                if (hintUI != null)
                    hintUI.ShowHint("Put the pot in the oven first!");
                return;
            }
        }
        else
        {
            Debug.LogWarning("[OvenQTE] Cooking pot reference is missing!");
            if (hintUI != null)
                hintUI.ShowHint("Pot not found!");
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
            hintUI.ShowHint("Move away from the door to start baking!");
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
        
        Debug.Log("[OvenQTE] QTE completed! Move away to close door and start baking.");
        
        qteHandled = true;
    }
    public void ChangeLiquidToCake()
    {
        if (potLiquid != null && !cakeColorChanged)
        {
            Renderer liquidRenderer = potLiquid.GetComponent<Renderer>();
            if (liquidRenderer != null)
            {
                liquidRenderer.material.color = cakeColor;
                potLiquid.tag = "Cake";
                cakeColorChanged = true;
                Debug.Log("[OvenQTE] 🍰 Liquid changed to cake color during baking!");
            }
        }
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

    public bool IsCakeReady()
    {
        return cakeColorChanged;
    }

    public bool IsQTERunning()
    {
        return qteRunning;
    }
}