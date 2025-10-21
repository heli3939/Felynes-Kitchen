using UnityEngine;
using UnityEngine.UI;

public class ChecklistItem : MonoBehaviour
{
    [Tooltip("Must match the ingredient's name in the game (e.g., Milk, Egg).")]
    public string itemName;
    private Image tick;

    private bool done = false;

    void Awake()
    {
        tick = GetComponent<Image>();
        if (tick != null)
            tick.enabled = false; // start hidden
    }

    public void SetDone()
    {
        if (done) return;
        done = true;

        if (tick != null)
        {
            tick.enabled = true;
            Debug.Log($"[ChecklistItem] ✅ Ticked {itemName}");
        }
    }
}
