using UnityEngine;
using UnityEngine.UI; // for Selectable/Button

public class Checklist : MonoBehaviour
{
    [Header("Target to show/hide")]
    public GameObject checklist;
    [Tooltip("Applied once at init only.")]
    public bool startVisible = false;

    [Header("Game flow")]
    [Tooltip("Require gameStarted before first open.")]
    public bool gameStarted = false;
    [Tooltip("When checklist is visible, pause the game (Time.timeScale=0).")]
    public bool pauseGameWhileOpen = true;

    [Header("Pause integration (optional)")]
    public MonoBehaviour pauseManager;      // if you have your own PauseManager with bool IsPaused {get;}
    public string pausePropertyName = "IsPaused";

    [Header("UI behaviour")]
    [Tooltip("Bring checklist above siblings when showing.")]
    public bool bringToFrontOnShow = true;
    [Tooltip("Disable (grey) the pause button while checklist is open (do NOT hide).")]
    public Selectable pauseButton;          // drag your PauseButton here (Button/Selectable)

    bool _initialized;

    [Header("Checklist items parent")]
    public Transform listRoot; // assign ChecklistPanel or a subfolder with ticks

    public void MarkDone(string itemName)
    {
        if (listRoot == null)
        {
            Debug.LogWarning("[Checklist] listRoot not set!");
            return;
        }

        foreach (Transform t in listRoot)
        {
            var item = t.GetComponent<ChecklistItem>();
            if (item != null && item.itemName == itemName)
            {
                item.SetDone();
                Debug.Log($"[Checklist] ✔ Marked {itemName}");
                return;
            }
        }

        Debug.LogWarning($"[Checklist] ❌ No checklist item named '{itemName}' found!");
    }

    void Awake() => InitOnce();
    void OnEnable() => InitOnce(); // safe; guarded

    void InitOnce()
    {
        if (_initialized) return;
        _initialized = true;

        if (checklist != null)
            checklist.SetActive(startVisible);

        // reflect initial state on pause button + timescale
        ApplyPauseEffects(startVisible);
    }

    // If this is a world-space sprite with a Collider
    void OnMouseDown()
    {
        // IMPORTANT: allow toggling even while paused so you can close it to resume
        if (!gameStarted) return;
        Toggle();
    }

    // Wire this to a UI Button OnClick if it's a UI button
    public void Toggle()
    {
        if (!gameStarted || checklist == null) return;

        bool next = !checklist.activeSelf;   // open if closed, close if open
        ApplyVisibility(next);
    }

    public void Show()
    {
        if (!gameStarted || checklist == null) return;
        ApplyVisibility(true);
    }

    public void Hide()
    {
        if (checklist == null) return;
        ApplyVisibility(false);
    }

    public void SetGameStarted(bool started) => gameStarted = started;

    // ---------------- core ----------------
    void ApplyVisibility(bool visible)
    {
        if (visible) ActivateParents(checklist);

        checklist.SetActive(visible);

        if (visible && bringToFrontOnShow)
            checklist.transform.SetAsLastSibling();

        ApplyPauseEffects(visible);
    }

    void ApplyPauseEffects(bool checklistVisible)
    {
        // Grey out (but keep visible) the Pause button
        if (pauseButton != null)
            pauseButton.interactable = !checklistVisible;

        // Pause / Resume game
        if (pauseGameWhileOpen)
        {
            if (checklistVisible)
                PauseGame();
            else
                ResumeGame();
        }
    }

    void PauseGame()
    {
        // If you have your own pause system, you can call it here.
        // Otherwise default to Time.timeScale = 0.
        Time.timeScale = 0f;
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    // ---------------- helpers ----------------
    bool IsPausedExternal()
    {
        if (pauseManager != null)
        {
            var t = pauseManager.GetType();
            var prop = t.GetProperty(pausePropertyName);
            if (prop != null && prop.PropertyType == typeof(bool))
                return (bool)prop.GetValue(pauseManager);
        }
        return Time.timeScale == 0f;
    }

    void ActivateParents(GameObject go)
    {
        var t = go.transform.parent;
        while (t != null)
        {
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
            t = t.parent;
        }
    }
}
