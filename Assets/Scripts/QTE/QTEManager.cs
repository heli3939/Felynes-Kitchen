using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Burst.CompilerServices;

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
    public QTEInteract qteInteract;

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
    public GameObject pot;
    public float decorationDistanceThreshold = 2.5f;  // Distance player must be from cake to decorate
    private bool hasCream = false;
    private bool hasFinal = false;
    private bool isGameEnding = false; // Flag to prevent EndQTE during game ending

    private Vector3 originalPosition;
    public int CurrentFillLevel => currentFillLevel;

    public Checklist checklist;

    private void Awake()
    {
        originalPosition = pot.transform.position;
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

    // >>> helper: turn "Egg (Clone)" into "Egg"
    private string ResolveIngredientKey(Transform t)
    {
        if (t == null) return "";

        string rawName = t.name;
        int parenIndex = rawName.IndexOf('(');
        if (parenIndex >= 0)
            rawName = rawName.Substring(0, parenIndex);

        string cleaned = rawName.Trim();

        if (!string.IsNullOrEmpty(cleaned))
            return cleaned;

        // fallback: use tag if somehow empty
        return t.tag;
    }

    public void StartQTE()
    {
        // >>> safety: make sure we actually have something in hand
        if (holdPoint == null || holdPoint.childCount == 0)
        {
            Debug.LogWarning("[QTEManager] QTE cannot start — nothing in holdPoint.");
            if (hintUI != null)
                    {
                        hintUI.ShowHint("You should pick an ingredient first to start QTE.");
                    }
            return;
        }

        currentItemTag = holdPoint.GetChild(0).tag;

        if (pot.transform.position == originalPosition)
        {
            Debug.Log("");
            if (holdPoint == null || holdPoint.childCount == 0 ||
            (holdPoint.childCount > 0 && (holdPoint.GetChild(0).tag == "CookingPot" || holdPoint.GetChild(0).tag == "Cake")))
            {
                Debug.LogWarning("[QTEManager] QTE cannot start — player is not holding any item or holding pot/cake.");
                if (hintUI != null)
                    {
                        hintUI.ShowHint("You should pick an ingredient first to start QTE.");
                    }
                return;
            }
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

                    return;
                }

                if (currentItemTag == "Strawberry")
                {
                    Debug.LogWarning("[QTEManager] ⚠️ Using Strawberry before baking! Score will be permanently locked!");
                    ScoreSystem.Instance.TriggerPermanentZero();
                }
            }
        }
        else if (pot.transform.position != originalPosition && !ovenQTEManager.HasCompletedOvenQTE() && qteInteract.inRange())
        {
            if (hintUI != null)
            {
                hintUI.ShowHint("Put pot back for QTE.");
            }
            return;
        }
        else
        {
            GameObject cake = GameObject.FindGameObjectWithTag("Cake");
            if (cake != null)
            {
                Vector3 originalCakePosition = new Vector3(7.519f, 0.6071066f, 1.157f);
                float distanceFromOriginal = Vector3.Distance(cake.transform.position, originalCakePosition);
                float cakePositionThreshold = 0.5f;

                if (distanceFromOriginal > cakePositionThreshold)
                {
                    Debug.LogWarning("[QTEManager] Cake is not on the table yet!" + cake.transform.position);
                    return;
                }

                // Check if player is close enough to decorate the cake
                if (holdPoint != null)
                {
                    float playerToCakeDistance = Vector3.Distance(holdPoint.position, cake.transform.position);

                    if (playerToCakeDistance > decorationDistanceThreshold)
                    {
                        Debug.LogWarning($"[QTEManager] Too far from cake to decorate! Distance: {playerToCakeDistance:F2}m (need < {decorationDistanceThreshold}m)");

                        if (hintUI != null)
                        {
                            hintUI.ShowHint("Get closer to the cake to decorate.");
                        }

                        return;
                    }

                    Debug.Log($"[QTEManager] ✅ Player close enough to cake: {playerToCakeDistance:F2}m");
                }
            }
            else
            {
                if (ovenQTEManager.HasCompletedOvenQTE())
                {
                    if (hintUI != null)
                    {
                        hintUI.ShowHint("Take the cake out of the oven first!");
                    }
                }
                return;
            }

            if (currentItemTag != "Cream" && currentItemTag != "Strawberry")
            {
                Debug.LogWarning("[QTEManager] This item is not for decoration" + "a" + currentItemTag + ".");

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
        // If game is ending (strawberry decoration complete), skip normal EndQTE process
        if (isGameEnding)
        {
            Debug.Log("[QTEManager] EndQTE skipped - game ending in progress");
            return;
        }

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
            camSwitcher.StartCoroutine(camSwitcher.SwitchBackToThirdPersonDelayed(1f));
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
        bool ovenCompleted = (ovenQTEManager != null && ovenQTEManager.HasCompletedOvenQTE());
        Debug.Log($"[QTEManager] Oven completed? {ovenCompleted}");

        // ✅ Score logic as before
        foreach (var r in results)
            ScoreSystem.Instance.AddScore(r);

        if (!ovenCompleted)
        {
            // ✅ Ingredient handling
            if (holdPoint != null && holdPoint.childCount > 0)
            {
                Transform child = holdPoint.GetChild(0);

                // Only tick if it's an ingredient
                if (child.CompareTag("Item"))
                {
                    // >>> NEW: use prefab-ish base name as checklist key
                    string itemName = ResolveIngredientKey(child);

                    checklist.MarkDone(itemName);
                    Debug.Log($"[QTE] ✅ Checklist ticked for ingredient key: '{itemName}'");
                    StartCoroutine(ShowChecklistBriefly(1.5f));
                }
                else
                {
                    Debug.Log($"[QTE] ⚠ Object '{child.name}' ignored (tag != 'Item').");
                }

                if (!child.CompareTag("Cream"))
                {
                    Destroy(child.gameObject);
                    Debug.Log($"[QTE] 🗑 Destroyed poured ingredient: '{child.name}'");
                }
            }
        }

        if (ovenCompleted)
        {
            Debug.Log($"[QTEManager] Processing decoration - currentItemTag: {currentItemTag}");
            Debug.Log($"[QTEManager] hasCream: {hasCream}, hasFinal: {hasFinal}");

            // >>> figure out key for decoration items too
            Transform decoChild = null;
            if (holdPoint != null && holdPoint.childCount > 0)
                decoChild = holdPoint.GetChild(0);

            string decoKey = decoChild != null ? ResolveIngredientKey(decoChild) : currentItemTag;

            if (currentItemTag == "Cream" && !hasCream)
            {
                hasCream = true;
                UpdateCakeVisual("cream");

                checklist.MarkDone(decoKey);
                Debug.Log("[QTEManager] ✅ Cream added to cake!");
                StartCoroutine(ShowChecklistBriefly(1.5f));

            }
            else if (currentItemTag == "Strawberry" && hasCream && !hasFinal)
            {
                Debug.Log("[QTEManager] 🍓 All conditions met for strawberry! Updating to final stage...");
                hasFinal = true;
                isGameEnding = true; // Set flag to prevent EndQTE
                qteHandled = true;   // Mark as handled before returning
                UpdateCakeVisual("final");
                return; // Return early to skip normal EndQTE process
            }
            else if (currentItemTag == "Strawberry")
            {
                Debug.LogWarning($"[QTEManager] ⚠️ Strawberry conditions not met! hasCream={hasCream}, hasFinal={hasFinal}");
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
        Debug.Log($"[QTEManager] UpdateCakeVisual called with stage: {stage}");

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
        else if (stage == "final")
        {
            if (cakeFinal == null)
            {
                Debug.LogError("[QTEManager] ❌ cakeFinal is NULL! Cannot update to final stage!");
                return;
            }

            Debug.Log("[QTEManager] Destroying current cake and activating cakeFinal...");
            Destroy(currentCake);
            checklist.MarkDone("Strawberry (3)");
            Debug.Log("[QTEManager] ✅ Strawberry added! Cake is complete!");
            StartCoroutine(ShowChecklistBriefly(1.5f));

            cakeFinal.SetActive(true);
            cakeFinal.transform.position = cakePos;
            cakeFinal.transform.rotation = cakeRot;

            Debug.Log("[QTEManager] Cake visual updated to final stage");
            Debug.Log("[QTEManager] 🚀 Starting TriggerGameEnding coroutine...");

            // Trigger game ending after final cake model is activated
            StartCoroutine(TriggerGameEnding(2f));
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

    private IEnumerator ShowChecklistBriefly(float delay)
    {

        checklist.Toggle();
        yield return new WaitForSecondsRealtime(delay);
        checklist.Hide();
    }

    private IEnumerator TriggerGameEnding(float delay)
    {
        Debug.Log("[QTEManager] 🎮 TriggerGameEnding started!");

        yield return new WaitForSeconds(delay);

        Debug.Log("[QTEManager] 📷 Cleaning up QTE UI...");

        // Stop and clean up QTE first
        qteRunning = false;

        if (qteController != null)
            qteController.StopQTE();

        if (qteRaycaster != null)
            qteRaycaster.enabled = false;

        if (qteCanvas != null)
        {
            qteCanvas.SetActive(false);
        }

        // Destroy the strawberry item
        if (holdPoint != null && holdPoint.childCount > 0)
        {
            Transform child = holdPoint.GetChild(0);
            Debug.Log($"[QTEManager] Destroying ingredient: {child.name}");
            Destroy(child.gameObject);
        }

        Debug.Log("[QTEManager] 📷 Switching camera back...");

        // Switch back to third person camera
        if (fpCamera != null)
        {
            fpCamera.SetActive(false);
        }

        if (tpCamera != null)
        {
            tpCamera.SetActive(true);
        }

        // IMMEDIATELY freeze the game (don't wait)
        Debug.Log("[QTEManager] ⏸️ Freezing game NOW...");

        Time.timeScale = 0f;

        // Disable all player controls
        SetPlayerControls(false);

        Debug.Log("[QTEManager] 🎯 Getting ending type...");

        // Determine ending type based on score
        string endingType = ScoreSystem.Instance.GetEndingType();

        Debug.Log($"[QTEManager] 🎯 Ending type: {endingType}, Score: {ScoreSystem.Instance.score}");

        if (hintUI != null)
        {
            Debug.Log("[QTEManager] 💬 Showing hint...");

            if (endingType == "BE")
            {
                Debug.Log("[QTEManager] 😿 Bad Ending displayed");
                SceneManager.LoadScene("BadEnding");

            }
            else if (endingType == "HE")
            {
                Debug.Log("[QTEManager] 🎉 Happy Ending displayed");
                SceneManager.LoadScene("HappyEnding");
            }
        }
        else
        {
            Debug.LogWarning("[QTEManager] ⚠️ HintUI is null!");
        }

        Debug.Log($"[QTEManager] 🎮 Game Ended! Final Score: {ScoreSystem.Instance.score} | Ending: {endingType}");
    }
}
