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

    [Header("Player Control")]
    public MonoBehaviour[] playerControlScripts;

    private GraphicRaycaster qteRaycaster;

    private void Awake()
    {
        if (qteCanvas != null)
        {
            qteRaycaster = qteCanvas.GetComponent<GraphicRaycaster>();
        }

        if (qteController != null)
            qteController.OnQTEFinished += HandleFinished;
    }

    private void OnDestroy()
    {
        if (qteController != null)
            qteController.OnQTEFinished -= HandleFinished;
    }

    private void SetPlayerControls(bool enabled)
    {
        foreach (var script in playerControlScripts)
        {
            if (script != null)
                script.enabled = enabled;
        }
    }

    public void StartQTE()
    {
        SetPlayerControls(false);

        if (tpCamera != null) tpCamera.SetActive(false);
        if (fpCamera != null) fpCamera.SetActive(true);

        if (qteCanvas != null) qteCanvas.SetActive(true);

        if (qteRaycaster != null)
            qteRaycaster.enabled = true;

        if (qteController != null)
            qteController.StartQTE();
    }

    private void HandleFinished(QTEResult[] results)
    {
        foreach (var r in results)
        {
            ScoreSystem.Instance.AddScore(r);
        }
    }

    public void EndQTE()
    {
        if (qteController != null)
            qteController.StopQTE();

        if (qteRaycaster != null)
            qteRaycaster.enabled = false;

        if (qteCanvas != null)
            qteCanvas.SetActive(false);

        if (fpCamera != null)
            fpCamera.SetActive(false);

        if (tpCamera != null)
        {
            tpCamera.SetActive(true);

            var cam = tpCamera.GetComponent<Camera>();
            if (cam != null && !cam.enabled)
                cam.enabled = true;
        }
        SetPlayerControls(true);

        Debug.Log("[QTEManager] QTE ended, switched back to TP Camera");
    }

}
