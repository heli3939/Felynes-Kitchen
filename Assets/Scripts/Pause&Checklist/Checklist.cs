using UnityEngine;

public class Checklist : MonoBehaviour
{
    [Header("Hook these up in Inspector. Each is the row that should tick.")]
    public ChecklistItem flourItem;
    public ChecklistItem sugarItem;
    public ChecklistItem milkItem;
    public ChecklistItem eggItem;
    public ChecklistItem butterItem;

    [Header("Decoration rows")]
    public ChecklistItem creamItem;
    public ChecklistItem strawberryItem;

    // called by QTEManager after QTE finishes an ingredient
    public void MarkDone(string keyFromQTE)
    {
        ChecklistItem target = ResolveTarget(keyFromQTE);

        if (target != null)
        {
            target.SetDone();
            Debug.Log($"[Checklist] ✓ Marked {keyFromQTE}");
        }
        else
        {
            Debug.LogWarning($"[Checklist] ❌ Couldn't map '{keyFromQTE}' to any checklist slot");
        }
    }

    private ChecklistItem ResolveTarget(string k)
    {
        // normalize input
        string key = k.Trim();

        // map multiple possible names for same row
        switch (key)
        {
            // Flour sometimes comes back as mesh name or prefab name.
            // put ALL flour aliases here:
            case "Flour":
                return flourItem;

            // Sugar
            case "FreeSugarShaker":
                return sugarItem;

            // Milk
            case "milk":
                return milkItem;

            // Egg
            case "egg":
            case "egg (1)":
            case "egg (2)":
            case "egg (3)":
            case "egg (4)":
            case "egg (5)":
            case "egg (6)":
            case "egg (7)":
            case "egg (8)":
                return eggItem;

            // Butter
            case "butter":
                return butterItem;

            // Cream (decoration)
            case "bowl_cream":
                return creamItem;

            // Strawberry (decoration)
            case "Strawberry (4)":
            case "Strawberry (8)":
            case "Strawberry (5)":
            case "Strawberry (9)":
            case "Strawberry (2)":
            case "Strawberry (6)":
            case "Strawberry (10)":
            case "Strawberry (3)":
            case "Strawberry (7)":
            case "Strawberry (11)":
                return strawberryItem;
        }

        // didn't match anything
        return null;
    }

    // you already call these from QTE to flash the checklist
    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
