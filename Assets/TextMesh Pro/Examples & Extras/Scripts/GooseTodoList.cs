using UnityEngine;
using UnityEngine.UI; // for Selectable/Button

public class Checklist : MonoBehaviour
{
    [Header("Target to show/hide")]
    public GameObject checklist;

    [Tooltip("Initial state only; applied once.")]
    public bool startVisible = false;

    [Header("Gating")]
    public bool gameStarted = false;
    public bool blockWhenPaused = true;
    public MonoBehaviour pauseManager;  // must expose public bool IsPaused { get; }
    public string pausePropertyName = "IsPaused";

    [Header("UI behaviour")]
    [Tooltip("Bring the checklist above siblings when showing.")]
    public bool bringToFrontOnShow = true;

    [Tooltip("Disable the Pause button while the checklist is visible.")]
    public Selectable pauseButton;            // e.g., your PauseButton (Button/Selectable)
    [Tooltip("Alternatively (or in addition), hide/show this whole Pause button gameObject.")]


    bool _initialized;

    void Awake() => InitOnce();
    void OnEnable() => InitOnce(); // safe: guarded

    void InitOnce()
    {
        if (_initialized) return;
        _initialized = true;
        if (checklist != null) checklist.SetActive(startVisible);
        ApplyPauseButtonLock(startVisible); // lock/unlock pause based on initial state
    }

    void OnMouseDown()
    {
        if (!CanInteract()) return;
        Toggle(); // same button toggles open/close
    }

    // -------- Public API (can also wire from a UI Button OnClick) --------
    public void Toggle()
    {
        if (!CanInteract() || checklist == null) return;

        bool next = !checklist.activeSelf; // TRUE = open, FALSE = close
        ApplyVisibility(next);
        // Debug.Log($"[Checklist] Toggle -> activeSelf={next}");
    }

    public void Show()
    {
        if (!CanInteract() || checklist == null) return;
        ApplyVisibility(true);
    }

    public void Hide()
    {
        if (checklist == null) return;
        ApplyVisibility(false);
    }

    public void SetGameStarted(bool started) => gameStarted = started;

    // ---------------- Core ----------------
    void ApplyVisibility(bool visible)
    {
        // ensure parents are active so activeInHierarchy is true when showing
        if (visible) ActivateParents(checklist);

        checklist.SetActive(visible);

        if (visible && bringToFrontOnShow)
            checklist.transform.SetAsLastSibling(); // draw on top

        // lock/unlock Pause while checklist is visible
        ApplyPauseButtonLock(visible);
    }

    void ApplyPauseButtonLock(bool checklistVisible)
    {
        // 1) Disable the Pause button’s interactable, so it won’t click
        if (pauseButton != null)
            pauseButton.interactable = !checklistVisible;
    }

    // ---------------- Helpers ----------------
    bool CanInteract()
    {
        if (!gameStarted) return false;
        if (blockWhenPaused && IsPaused()) return false;
        return true;
    }

    bool IsPaused()
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
