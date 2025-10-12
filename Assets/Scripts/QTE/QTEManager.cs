using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QTEManager : MonoBehaviour
{
    [Header("Cameras")]
    public GameObject tpCamera;   
    public GameObject fpCamera;   

    [Header("QTE UI")]
    public GameObject qteCanvas;
    public QTEUIController qteController;

    private GraphicRaycaster qteRaycaster;
    private bool qteRunning = false;

    [Header("Player Control Scripts")]
    public MonoBehaviour[] playerControlScripts;

    [Header("Pickup Check")]
    public Transform holdPoint;

    [Header("Liquid Control")]
    public Animator potLiquidAnimator;  
    private int currentFillLevel = 0;   
    private int maxFillLevel = 5;

    public int CurrentFillLevel => currentFillLevel;

    private void Awake()
    {
        if (qteCanvas != null)
            qteRaycaster = qteCanvas.GetComponent<GraphicRaycaster>();

        if (qteController != null)
            qteController.OnQTEFinished += HandleFinished;
    }

    private void OnDestroy()
    {
        if (qteController != null)
            qteController.OnQTEFinished -= HandleFinished;
    }


    public void StartQTE()
    {
        if (holdPoint == null || holdPoint.childCount == 0 || (holdPoint.childCount > 0 && holdPoint.GetChild(0).tag == "CookingPot"))
        {
            Debug.LogWarning("[QTEManager] QTE cannot start — player is not holding any item.");
            return;
        }

        if (qteRunning)
        {
            Debug.LogWarning("[QTEManager] QTE already running, ignoring duplicate Q press.");
            return;
        }

        qteRunning = true;
        SetPlayerControls(false);

        if (tpCamera != null) tpCamera.SetActive(false);

        if (fpCamera != null)
        {
            fpCamera.SetActive(true);
            var cam = fpCamera.GetComponent<Camera>();
            if (cam != null)
                cam.enabled = true;
        }

        if (qteCanvas != null)
        {
            qteCanvas.SetActive(true);

            if (qteController != null && !qteController.gameObject.activeSelf)
                qteController.gameObject.SetActive(true);

            if (qteRaycaster != null)
                qteRaycaster.enabled = true;

            var canvas = qteCanvas.GetComponent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                var fpCam = fpCamera != null ? fpCamera.GetComponent<Camera>() : null;
                canvas.worldCamera = fpCam;
            }
        }

        if (qteController != null)
        {
            qteController.StartQTE(); 
        }

        Debug.Log("[QTEManager] QTE started, switched to FP Camera ✅");
    }

    public void EndQTE()
    {
        qteRunning = false;

        if (qteController != null)
            qteController.StopQTE();

        if (qteRaycaster != null)
            qteRaycaster.enabled = false;

        if (qteCanvas != null)
        {
            var canvas = qteCanvas.GetComponent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                var tpCam = tpCamera != null ? tpCamera.GetComponent<Camera>() : null;
                canvas.worldCamera = tpCam;
            }

            qteCanvas.SetActive(false);
        }

        var camSwitcher = FindFirstObjectByType<CameraSwitcher>();
        if (camSwitcher != null)
        {
            camSwitcher.StartCoroutine(camSwitcher.SwitchBackToThirdPersonDelayed(3f));
        }

        if (holdPoint != null && holdPoint.childCount > 0)
        {
            Transform child = holdPoint.GetChild(0);
            Debug.Log($"[QTEManager] Destroying ingredient: {child.name}");
            Destroy(child.gameObject);
        }

        SetPlayerControls(true);

        qteHandled = false;
    }

    private bool qteHandled = false; 

    private void HandleFinished(QTEResult[] results)
    {
        if (qteHandled) return; 

        foreach (var r in results)
        {
            ScoreSystem.Instance.AddScore(r);
        }

        if (holdPoint != null && holdPoint.childCount > 0)
        {
            Destroy(holdPoint.GetChild(0).gameObject);
        }

        StartCoroutine(DelayedIncreaseLiquidLevel(0.5f));

        qteHandled = true; 
    }

    private void IncreaseLiquidLevel()
    {
        if (potLiquidAnimator == null) return;

        potLiquidAnimator.speed = 0.2f;

        if (currentFillLevel < maxFillLevel)
        {
            currentFillLevel++;
            potLiquidAnimator.SetInteger("FillLevel", currentFillLevel);
            Debug.Log($"[QTEManager] Liquid increased to stage {currentFillLevel}");
        }
        else
        {
            Debug.Log("[QTEManager] Pot is already full. QTE completed but liquid did not increase.");
        }

    }

    private IEnumerator DelayedIncreaseLiquidLevel(float delay)
    {
        yield return new WaitForSeconds(delay);
        IncreaseLiquidLevel();
    }

    public void SetPlayerControls(bool enabled)
    {
        foreach (var script in playerControlScripts)
        {
            if (script != null)
                script.enabled = enabled;
        }
        Debug.Log($"[QTEManager] Player control set to {(enabled ? "ENABLED" : "DISABLED")}");
    }
}

