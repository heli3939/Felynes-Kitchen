using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [Header("Score Settings")]
    public int score = 0;
    public int perfectScore = 10;
    public int goodScore = 5;

    [Header("Ending Thresholds")]
    public int heThreshold = 192;  // for Happy Ending
    public int beThreshold = 192;  // for Bad Ending

    public static ScoreSystem Instance { get; private set; }

    private static bool permanentlyZero = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // remain till the end
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void AddScore(QTEResult result)
    {
        if (permanentlyZero)
        {
            score = 0;
            Debug.Log("[ScoreSystem] Score locked at 0 due to incorrect ingredient.");
            return;
        }

        switch (result)
        {
            case QTEResult.Perfect:
                score += perfectScore;
                break;
            case QTEResult.Good:
                score += goodScore;
                break;
            case QTEResult.Miss:
                break;
        }

        Debug.Log($"[ScoreSystem] Current score: {score}");
    }

    public void TriggerPermanentZero()
    {
        permanentlyZero = true;
        score = 0;
        Debug.Log("[ScoreSystem] Incorrect ingredient detected! Score is now permanently locked at 0.");
    }

    public void ResetGameScore()
    {
        score = 0;
        permanentlyZero = false;
        Debug.Log("[ScoreSystem] Game score and permanent lock reset.");
    }

    public string GetEndingType()
    {
        if (score >= heThreshold)
            return "HE";
        else if (score < beThreshold)
            return "BE";
        else
            return "NEUTRAL"; // Optional
    }
}

