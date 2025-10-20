using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

public class GooseTodoList : MonoBehaviour
{
    [Header("Panel")]
    public RectTransform panel;             // the paper/clipboard container
    public Vector2 hiddenPos = new Vector2(-600f, 0f);
    public Vector2 shownPos = new Vector2(40f, 0f);
    public float slideTime = 0.35f;
    [Range(0f, 1f)] public float overshoot = 0.15f;

    [Tooltip("Optional toggle button in HUD to open/close manually")]
    public Button toggleButton;

    [Header("Auto Show/Hide")]
    public bool autoShowOnComplete = true;
    public bool autoHideAfterComplete = true;
    public float autoHideDelay = 2.0f;

    [Header("Entries")]
    public List<TaskEntry> entries = new List<TaskEntry>();

    [Header("SFX (optional)")]
    public AudioSource sfx;
    public AudioClip scribbleTick; // small pencil/marker check sound
    public AudioClip paperSlide;   // paper rustle when opening

    [Serializable]
    public class TaskEntry
    {
        public string Id;                // e.g., "Sugar", "Egg", "Flour"
#if TMP_PRESENT
        public TextMeshProUGUI label;    // use a scribbly TMP font for vibe
#else
        public Text label;               // fallback
#endif
        public Image checkbox;           // empty box graphic
        public Image tick;               // checkmark image (set active on complete)
        public GameObject strike;        // thin line over text (off by default)
        public Animator flair;           // optional Animator for a tiny pop or pulse
        [NonSerialized] public bool done;
    }

    bool _isShown;
    Coroutine _slideCo;
    Coroutine _autoHideCo;
    Dictionary<string, TaskEntry> _byId;

    void Awake()
    {
        _byId = new Dictionary<string, TaskEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in entries)
        {
            if (!string.IsNullOrWhiteSpace(e.Id) && !_byId.ContainsKey(e.Id))
                _byId.Add(e.Id.Trim(), e);

            // ensure initial visuals
            SetEntryState(e, completed: false, instant: true);
        }

        if (toggleButton) toggleButton.onClick.AddListener(Toggle);

        if (panel) panel.anchoredPosition = hiddenPos;
        _isShown = false;
    }

    public void Toggle()
    {
        if (_isShown) Hide();
        else Show();
    }

    public void Show(bool playSound = true)
    {
        Time.timeScale = 0f;
        if (_isShown) return;
        _isShown = true;
        if (_slideCo != null) StopCoroutine(_slideCo);
        _slideCo = StartCoroutine(Slide(panel, panel.anchoredPosition, shownPos, playSound));
    }

    public void Hide()
    {
        if (!_isShown) return;
        _isShown = false;
        if (_slideCo != null) StopCoroutine(_slideCo);
        _slideCo = StartCoroutine(Slide(panel, panel.anchoredPosition, hiddenPos, false));
        if (_autoHideCo != null) { StopCoroutine(_autoHideCo); _autoHideCo = null; }
        Time.timeScale = 1f;
    }

    IEnumerator Slide(RectTransform rt, Vector2 from, Vector2 to, bool playSound)
    {
        if (!rt) yield break;
        if (playSound && sfx && paperSlide) sfx.PlayOneShot(paperSlide);

        float t = 0f;
        float dur = Mathf.Max(0.01f, slideTime);

        // simple eased slide with a tiny overshoot
        Vector2 overshootPos = Vector2.LerpUnclamped(from, to, 1f + overshoot);

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float u = t / dur;
            // ease out cubic
            float e = 1f - Mathf.Pow(1f - u, 3f);
            // go towards overshoot then back to final
            Vector2 step = Vector2.LerpUnclamped(overshootPos, to, e);
            rt.anchoredPosition = step;
            yield return null;
        }
        rt.anchoredPosition = to;
    }

    void SetEntryState(TaskEntry e, bool completed, bool instant = false)
    {
        e.done = completed;
        if (e.tick) e.tick.enabled = completed;
        if (e.strike) e.strike.SetActive(completed);

        // optional: dim label when done
        if (e.label)
        {
            var col = e.label.color;
            col.a = completed ? 0.7f : 1f;
            e.label.color = col;
        }

        if (!instant && e.flair)
        {
            // trigger a small pop animation on complete
            e.flair.SetTrigger(completed ? "Complete" : "Reset");
        }
    }

    /// Call this to mark a task done (e.g., after QTE pour).
    public void CompleteTask(string id, bool showPanel = true)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        if (!_byId.TryGetValue(id.Trim(), out var e))
        {
            Debug.LogWarning($"[GooseTodoList] No task with id '{id}'.");
            return;
        }
        if (e.done) return;

        SetEntryState(e, completed: true, instant: false);

        if (sfx && scribbleTick) sfx.PlayOneShot(scribbleTick);

        if (showPanel && autoShowOnComplete)
        {
            Show(playSound: true);
            if (autoHideAfterComplete)
            {
                if (_autoHideCo != null) StopCoroutine(_autoHideCo);
                _autoHideCo = StartCoroutine(AutoHideAfterDelay());
            }
        }
    }

    IEnumerator AutoHideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(autoHideDelay);
        Hide();
    }

}
