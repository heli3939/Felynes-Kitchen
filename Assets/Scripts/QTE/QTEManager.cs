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

    [Header("Oven QTE Check")]
    public OvenQTEManager ovenQTEManager;

    [Header("UI Hint")]
    public HintUI hintUI;
    
    [Header("Cake Decoration")]
    public GameObject cakeCream;
    public GameObject cakeFinal;
    private bool hasCream = false;
    private bool hasFinal = false;

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

    private string currentItemTag = "";

    public void StartQTE()
    {
        if (holdPoint == null || holdPoint.childCount == 0 ||
            (holdPoint.childCount > 0 && (holdPoint.GetChild(0).tag == "CookingPot" || holdPoint.GetChild(0).tag == "Cake")))
        {
            Debug.LogWarning("[QTEManager] QTE cannot start — player is not holding any item or holding pot/cake.");
            return;
        }

        currentItemTag = holdPoint.GetChild(0).tag;
        Debug.Log("[QTEManager] Current QTE item tag: " + currentItemTag);

        bool ovenCompleted = (ovenQTEManager != null && ovenQTEManager.HasCompletedOvenQTE());

        if (!ovenCompleted)
        {
            if (currentItemTag == "Cream")
            {
                Debug.LogWarning("[QTEManager] Cream cannot be used before baking!");

                if (hintUI != null)
                {
                    hintUI.ShowHint("Cream is for decoration after baking!");
                }

                if (holdPoint.childCount > 0)
                {
                    Destroy(holdPoint.GetChild(0).gameObject);
                    Debug.Log("[QTEManager] Cream destroyed - wrong timing!");
                }

                return;
            }

            if (currentItemTag == "Strawberry")
            {
                Debug.LogWarning("[QTEManager] ⚠️ Using Strawberry before baking! Score will be permanently locked!");
                ScoreSystem.Instance.TriggerPermanentZero();
            }
        }
        else
        {
            GameObject cake = GameObject.FindGameObjectWithTag("Cake");
            if (cake != null)
            {
                Vector3 cakeTablePosition = new Vector3(-2.632f, 6321066f, 3.007f);
                float distanceFromTable = Vector3.Distance(cake.transform.position, cakeTablePosition);

                if (distanceFromTable > 1.0f)
                {
                    Debug.LogWarning("[QTEManager] Cake is not on the table yet!");
                    if (hintUI != null)
                    {
                        hintUI.ShowHint("Take the cake out of the oven first! (Press R)");
                    }
                    return;
                }
            }
            else
            {
                if (hintUI != null)
                {
                    hintUI.ShowHint("Take the cake out of the oven first!");
                }
                return;
            }

            if (currentItemTag != "Cream" && currentItemTag != "Strawberry")
            {
                Debug.LogWarning("[QTEManager] This item is not for decoration!");

                if (hintUI != null)
                {
                    hintUI.ShowHint("This is not for decoration!");
                }

                if (holdPoint.childCount > 0)
                {
                    Destroy(holdPoint.GetChild(0).gameObject);
                    Debug.Log("[QTEManager] Wrong decoration item destroyed!");
                }

                return;
            }

            if (currentItemTag == "Strawberry" && !hasCream)
            {
                Debug.LogWarning("[QTEManager] Must add cream before strawberry!");

                if (hintUI != null)
                {
                    hintUI.ShowHint("Put cream first!");
                }

                return;
            }

            if (currentItemTag == "Strawberry" && hasFinal)
            {
                Debug.LogWarning("[QTEManager] Cake decoration already complete!");

                if (hintUI != null)
                {
                    hintUI.ShowHint("Cake is already finished!");
                }

                return;
            }
        }

        if (HasIncorrectInChildren(holdPoint))
        {
            ScoreSystem.Instance.TriggerPermanentZero();
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
            currentItemTag = "";
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

    private bool HasIncorrectInChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag("incorrect"))
            {
                return true;
            }
        }
        return false;
    }

    private void HandleFinished(QTEResult[] results)
    {
        if (qteHandled) return;

        bool incorrectFound = false;

        if (holdPoint != null && holdPoint.childCount > 0)
        {
            incorrectFound = HasIncorrectInChildren(holdPoint);
            Debug.Log($"[DEBUG] incorrectFound: {incorrectFound}");
        }
        else
        {
            Debug.Log("[DEBUG] No child under holdPoint!");
        }

        if (incorrectFound)
        {
            ScoreSystem.Instance.TriggerPermanentZero();
            Debug.Log("[QTEManager] ❌ Incorrect ingredient found — Score reset & permanently locked!");
        }
        else
        {
            foreach (var r in results)
            {
                ScoreSystem.Instance.AddScore(r);
            }
        }

        Debug.Log("[QTEManager] ✅ QTE ended. Final score: " + ScoreSystem.Instance.score);

        if (holdPoint != null && holdPoint.childCount > 0)
        {
            for (int i = holdPoint.childCount - 1; i >= 0; i--)
            {
                Destroy(holdPoint.GetChild(i).gameObject);
            }
        }

        bool ovenCompleted = (ovenQTEManager != null && ovenQTEManager.HasCompletedOvenQTE());
        Debug.Log($"[QTEManager] Oven completed? {ovenCompleted}");

        if (ovenCompleted)
        {
            Debug.Log($"[QTEManager] Processing decoration - currentItemTag: {currentItemTag}");
            if (currentItemTag == "Cream" && !hasCream)
            {
                hasCream = true;
                UpdateCakeVisual("cream");
                Debug.Log("[QTEManager] ✅ Cream added to cake!");
            }
            else if (currentItemTag == "Strawberry" && hasCream && !hasFinal)
            {
                hasFinal = true;
                UpdateCakeVisual("final");
                Debug.Log("[QTEManager] ✅ Strawberry added! Cake is complete!");
            }
        }
        else
        {
            Debug.Log("[QTEManager] This is a regular ingredient, increasing liquid level...");
            StartCoroutine(DelayedIncreaseLiquidLevel(0.5f));
        }

        qteHandled = true;
    }

    private void UpdateCakeVisual(string stage)
    {
        GameObject currentCake = GameObject.FindGameObjectWithTag("Cake");
        
        if (currentCake == null)
        {
            Debug.LogWarning("[QTEManager] Cannot find cake!");
            return;
        }

        Vector3 cakePos = currentCake.transform.position;
        Quaternion cakeRot = currentCake.transform.rotation;

        if (stage == "cream" && cakeCream != null)
        {
            Destroy(currentCake);

            cakeCream.SetActive(true);
            cakeCream.transform.position = cakePos;
            cakeCream.transform.rotation = cakeRot;
            
            Debug.Log("[QTEManager] Cake visual updated to cream stage");
        }
        else if (stage == "final" && cakeFinal != null)
        {
            Destroy(currentCake);

            cakeFinal.SetActive(true);
            cakeFinal.transform.position = cakePos;
            cakeFinal.transform.rotation = cakeRot;
            
            Debug.Log("[QTEManager] Cake visual updated to final stage");
            
            if (hintUI != null)
            {
                hintUI.ShowHint("Cake is complete!");
            }
        }
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