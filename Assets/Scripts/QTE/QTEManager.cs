using UnityEngine;

public class QTEManager : MonoBehaviour
{
    [Header("Cameras")]
    public GameObject tpCamera;   
    public GameObject fpCamera;   

    [Header("QTE UI")]
    public GameObject qteCanvas;
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

    private void HandleFinished(QTEResult[] results)
    {
        foreach (var r in results)
        {
            ScoreSystem.Instance.AddScore(r);
        }
    }

    public void EndQTE()
    {

        if (qteController != null) qteController.StopQTE();

        if (qteCanvas != null) qteCanvas.SetActive(false);

    }
}
