using UnityEngine;
using UnityEngine.UI;

public class ChecklistItem : MonoBehaviour
{
    [Tooltip("Must match the ingredient's name key (e.g. 'egg', 'trash_Bag_1'). This is just for debug now.")]
    public string itemName;

    [Header("Drag the actual checkmark Image for THIS row here")]
    [SerializeField] private Image tick;

    private bool done = false;

    void Awake()
    {
        if (tick != null)
        {
            // hide at start
            tick.enabled = false;
            tick.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"[ChecklistItem] No tick Image assigned on {gameObject.name}");
        }
    }

    public void SetDone()
    {
        if (done) return;
        done = true;

        if (tick != null)
        {
            // FORCE THIS THING VISIBLE ON TOP, NO EXCUSES
            tick.gameObject.SetActive(true);
            tick.enabled = true;

            // make sure alpha isn't 0 / weird
            tick.color = new Color(0f, 1f, 0f, 1f); // neon green full alpha so you SEE it

            // make sure it's scaled sanely
            tick.rectTransform.localScale = Vector3.one;

            // render in front of siblings
            tick.rectTransform.SetAsLastSibling();

            Debug.Log($"[ChecklistItem] ✅ Ticked {itemName} | tick={tick.gameObject.name} | parent={tick.transform.parent.name}");
        }
        else
        {
            Debug.LogError($"[ChecklistItem] ❌ tick Image is NULL on {gameObject.name}");
        }
    }
}
