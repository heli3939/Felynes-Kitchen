using UnityEngine;
using UnityEngine.UI;

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

        // Close firstperson camera
        if (fpCamera != null)
            fpCamera.SetActive(false);

        if (tpCamera != null)
        {
            tpCamera.SetActive(true);
            var cam = tpCamera.GetComponent<Camera>();
            if (cam != null)
            {
                cam.enabled = true;
                cam.tag = "MainCamera"; 
            }
        }

        // restore the player's movement
        SetPlayerControls(true);

        Debug.Log("[QTEManager] QTE ended, switched back to TP Camera ✅");
    }

    private void HandleFinished(QTEResult[] results)
    {
        foreach (var r in results)
        {
            ScoreSystem.Instance.AddScore(r);
        }

        Debug.Log($"[QTEManager] QTE finished! Total Score: {ScoreSystem.Instance.score}");
    }

    private void SetPlayerControls(bool enabled)
    {
        foreach (var script in playerControlScripts)
        {
            if (script != null)
                script.enabled = enabled;
        }
        Debug.Log($"[QTEManager] Player control set to {(enabled ? "ENABLED" : "DISABLED")}");
    }
}

