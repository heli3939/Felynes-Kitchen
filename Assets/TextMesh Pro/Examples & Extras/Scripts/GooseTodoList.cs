using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

public class Checklist : MonoBehaviour
{
    [Header("Panel")]
    public RectTransform panel;
    public Vector2 hiddenPos = new Vector2(-600f, 0f);
    public Vector2 shownPos  = new Vector2(40f, 0f);
    public float slideTime   = 0.35f;
    [Range(0f, 1f)] public float overshoot = 0.15f;

    [Tooltip("Optional toggle button in HUD to open/close manually")]
    public Button toggleButton;

    [Header("Auto Show/Hide")]
    public bool autoShowOnComplete    = true;
    public bool autoHideAfterComplete = true;
    public float autoHideDelay        = 2.0f;

    [Header("Entries")]
    public List<TaskEntry> entries = new List<TaskEntry>();

    [Header("SFX (optional)")]
    public AudioSource sfx;
    public AudioClip scribbleTick;
    public AudioClip paperSlide;

    [Header("Integration")]
    public PauseMenu pauseMenu; // assign in Inspector

    public bool IsShown { get { return _isShown; } }

    [Serializable]
    public class TaskEntry
    {
        public string Id;
#if TMP_PRESENT
        public TextMeshProUGUI label;
#else
        public Text label;
#endif
        public Image checkbox;
        public Image tick;
        public GameObject strike;
        public Animator flair;
        [NonSerialized] public bool done;
    }

    private bool _isShown;
    private Coroutine _slideCo;
    private Coroutine _autoHideCo;
    private Dictionary<string, TaskEntry> _byId;
    private bool _gameStarted = false; // unlocked by OnGameStarted()

    private string TagPrefix { get { return "[Checklist:" + gameObject.name + "] "; } }

    void OnEnable()
    {
        Debug.Log(TagPrefix + "OnEnable");
    }

    void Awake()
    {
        Debug.Log(TagPrefix + "Awake");

        _byId = new Dictionary<string, TaskEntry>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (e != null && !string.IsNullOrEmpty(e.Id) && !_byId.ContainsKey(e.Id))
                _byId.Add(e.Id.Trim(), e);
            SetEntryState(e, false, true);
        }

        // Try to auto-find PauseMenu if not assigned
        if (pauseMenu == null)
        {
            pauseMenu = FindObjectOfType<PauseMenu>();
            Debug.Log(TagPrefix + "pauseMenu auto-find: " + (pauseMenu != null ? pauseMenu.name : "<null>"));
        }

        // Wire button (or try to auto-find)
        if (toggleButton == null)
        {
            toggleButton = GetComponentInChildren<Button>(true);
            Debug.Log(TagPrefix + "toggleButton auto-find: " + (toggleButton != null ? toggleButton.name : "<null>"));
        }
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(Toggle);
            // visible but disabled until start
            toggleButton.gameObject.SetActive(true);
            toggleButton.interactable = false;
        }
        else
        {
            Debug.LogWarning(TagPrefix + "toggleButton NOT assigned and not found.");
        }

        if (panel != null)
        {
            panel.gameObject.SetActive(true);
            panel.anchoredPosition = hiddenPos;
        }
        else
        {
            Debug.LogWarning(TagPrefix + "panel NOT assigned.");
        }

        _isShown = false;

        Debug.Log(TagPrefix + "Awake done. gameStarted=" + _gameStarted +
                  ", isShown=" + _isShown +
                  ", toggle=" + (toggleButton != null ? toggleButton.name : "<null>") +
                  ", panel=" + (panel != null ? panel.name : "<null>") +
                  ", pauseMenu=" + (pauseMenu != null ? pauseMenu.name : "<null>"));
    }

    void Start()
    {
        Debug.Log(TagPrefix + "Start");
    }

    void Update()
    {
        // TEMP: keyboard fallback to prove UI logic works even if button wiring is wrong
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log(TagPrefix + "Key C pressed -> Toggle()");
            Toggle();
        }
    }

    public void Toggle()
    {
        Debug.Log(TagPrefix + "Toggle clicked. gameStarted=" + _gameStarted +
                  ", isPaused=" + ((pauseMenu != null) ? pauseMenu.IsPaused.ToString() : "<no PM>") +
                  ", isShown=" + _isShown);

        if (!_gameStarted)
        {
            Debug.Log(TagPrefix + "BLOCK: game not started. Call PauseMenu.OnGameStarted() which should call checklist.OnGameStarted().");
            return;
        }
        if (pauseMenu != null && pauseMenu.IsPaused)
        {
            Debug.Log(TagPrefix + "BLOCK: pause menu is open.");
            return;
        }

        if (_isShown) Hide();
        else Show();
    }

    public void Show(bool playSound = true)
    {
        Debug.Log(TagPrefix + "Show()");
        if (!_gameStarted) { Debug.Log(TagPrefix + "BLOCK Show: not started"); return; }
        if (pauseMenu != null && pauseMenu.IsPaused) { Debug.Log(TagPrefix + "BLOCK Show: paused"); return; }

        if (pauseMenu != null)
        {
            Debug.Log(TagPrefix + "Notify PauseMenu.OnChecklistOpened()");
            pauseMenu.OnChecklistOpened();
        }

        if (panel != null)
        {
            if (!panel.gameObject.activeSelf)
            {
                Debug.Log(TagPrefix + "Activating panel GameObject");
                panel.gameObject.SetActive(true);
            }
            panel.SetAsLastSibling();

            var cg = panel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
                Debug.Log(TagPrefix + "CanvasGroup forced visible");
            }
        }
        else
        {
            Debug.LogWarning(TagPrefix + "Show called but panel is null");
        }

        Debug.Log(TagPrefix + "Time.timeScale -> 0");
        Time.timeScale = 0f;

        if (_isShown) { Debug.Log(TagPrefix + "already shown"); return; }
        _isShown = true;

        if (_slideCo != null) StopCoroutine(_slideCo);
        Debug.Log(TagPrefix + "Start slide to shownPos " + shownPos);
        _slideCo = StartCoroutine(Slide(panel, panel.anchoredPosition, shownPos, playSound));
    }

    public void Hide()
    {
        Debug.Log(TagPrefix + "Hide()");
        if (!_isShown) { Debug.Log(TagPrefix + "already hidden"); return; }
        _isShown = false;

        if (_slideCo != null) StopCoroutine(_slideCo);
        Debug.Log(TagPrefix + "Start slide to hiddenPos " + hiddenPos);
        _slideCo = StartCoroutine(Slide(panel, panel.anchoredPosition, hiddenPos, false));

        if (_autoHideCo != null) { StopCoroutine(_autoHideCo); _autoHideCo = null; }

        Debug.Log(TagPrefix + "Time.timeScale -> 1");
        Time.timeScale = 1f;

        if (pauseMenu != null)
        {
            Debug.Log(TagPrefix + "Notify PauseMenu.OnChecklistClosed()");
            pauseMenu.OnChecklistClosed();
        }
    }

    IEnumerator Slide(RectTransform rt, Vector2 from, Vector2 to, bool playSound)
    {
        if (rt == null) yield break;

        if (playSound && sfx != null && paperSlide != null)
            sfx.PlayOneShot(paperSlide);

        float t = 0f;
        float dur = Mathf.Max(0.01f, slideTime);
        Vector2 overshootPos = Vector2.LerpUnclamped(from, to, 1f + overshoot);

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float u = t / dur;
            float e = 1f - Mathf.Pow(1f - u, 3f);
            rt.anchoredPosition = Vector2.LerpUnclamped(overshootPos, to, e);
            yield return null;
        }
        rt.anchoredPosition = to;
        Debug.Log(TagPrefix + "Slide finished. anchoredPosition=" + rt.anchoredPosition);
    }

    void SetEntryState(TaskEntry e, bool completed, bool instant)
    {
        if (e == null) return;

        e.done = completed;
        if (e.tick != null)   e.tick.enabled = completed;
        if (e.strike != null) e.strike.SetActive(completed);

        if (e.label != null)
        {
            var col = e.label.color;
            col.a = completed ? 0.7f : 1f;
            e.label.color = col;
        }

        if (!instant && e.flair != null)
            e.flair.SetTrigger(completed ? "Complete" : "Reset");
    }

    public void CompleteTask(string id, bool showPanel = true)
    {
        if (string.IsNullOrEmpty(id)) return;

        TaskEntry e;
        if (!_byId.TryGetValue(id.Trim(), out e))
        {
            Debug.LogWarning(TagPrefix + "CompleteTask: no task with id '" + id + "'");
            return;
        }
        if (e.done) return;

        SetEntryState(e, true, false);

        if (sfx != null && scribbleTick != null)
            sfx.PlayOneShot(scribbleTick);

        bool mayShow = showPanel && autoShowOnComplete && _gameStarted && (pauseMenu == null || !pauseMenu.IsPaused);
        Debug.Log(TagPrefix + "CompleteTask -> mayShow=" + mayShow);
        if (mayShow)
        {
            Show(true);
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

    // call from PauseMenu when dialogue ends
    public void OnGameStarted()
    {
        _gameStarted = true;
        if (toggleButton != null)
        {
            toggleButton.gameObject.SetActive(true);
            toggleButton.interactable = true;
        }
        Debug.Log(TagPrefix + "OnGameStarted -> unlocked. toggle active=" +
                  (toggleButton != null && toggleButton.gameObject.activeInHierarchy) +
                  ", interactable=" + (toggleButton != null && toggleButton.interactable));
    }
}
