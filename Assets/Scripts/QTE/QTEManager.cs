using UnityEngine;

public class QTEManager : MonoBehaviour
{
    [Header("Cameras")]
    public GameObject tpCamera;  
    public GameObject fpCamera;   

    [Header("QTE UI")]
    public GameObject qteCanvas;        // QTE Canvas
    public QTEUIController qteController;

    private void Awake()
    {
        if (qteController != null)
            qteController.OnQTEFinished += HandleFinished;
    }

    private void OnDestroy()
    {
        if (qteController != null)
            qteController.OnQTEFinished -= HandleFinished;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartQTE();
        }
    }

    public void StartQTE()
    {
        if (tpCamera != null) tpCamera.SetActive(false);
        if (fpCamera != null) fpCamera.SetActive(true);

        if (qteCanvas != null) qteCanvas.SetActive(true);
    }

    private void HandleFinished(bool[] results)
    {
        // Statistical results
        bool allSuccess = true;
        foreach (var r in results) if (!r) { allSuccess = false; break; }

        EndQTE();
    }

    public void EndQTE()
    {
        // close QTE
        if (qteController != null) qteController.StopQTE();
        if (qteCanvas != null) qteCanvas.SetActive(false);

        // back to main camera
        if (fpCamera != null) fpCamera.SetActive(false);
        if (tpCamera != null) tpCamera.SetActive(true);
    }
}
