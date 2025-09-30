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

    private void HandleFinished(string[] results)
    {
        bool allPerfect = true;
        bool hasGood = false;

        foreach (var r in results)
        {
            if (r == "Good") hasGood = true;
            if (r != "Perfect") allPerfect = false;
        }

        if (allPerfect)
            Debug.Log("🌟 All Perfect！");
        else if (hasGood)
            Debug.Log("✅ Partial Good ");
        else
            Debug.Log("❌ All fail ");

        EndQTE(); 
    }

    public void EndQTE()
    {

        if (qteController != null) qteController.StopQTE();

        if (qteCanvas != null) qteCanvas.SetActive(false);

    }
}
