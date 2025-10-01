using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [Header("Score Settings")]
    public int score = 0;
    public int perfectScore = 10;
    public int goodScore = 5;

    [Header("Ending Thresholds")]
    public int heThreshold = 60;  // 60 for Happy Ending
    public int beThreshold = 30;  // <= 30 for Bad Ending

    public static ScoreSystem Instance { get; private set; }

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
        }
    }

    public void AddScore(QTEResult result)
    {
        switch (result)
        {
            case QTEResult.Perfect:
                score += perfectScore;
                break;
            case QTEResult.Good:
                score += goodScore;
                break;
            case QTEResult.Miss:
                // No score for Miss
                break;
        }

        Debug.Log($"Current score: {score}");
    }

    public string GetEndingType()
    {
        if (score >= heThreshold)
            return "HE";
        else if (score <= beThreshold)
            return "BE";
        else
            return "NEUTRAL"; // Optional
    }
}

