using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public enum QTEResult
{
    Perfect,
    Good,
    Miss
}

public class QTEUIController : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform circleGroup;
    public RectTransform indicator;
    public Image successZoneImage;   
    public Image perfectZoneImage;   

    [Header("QTE Settings")]
    public int totalChecks = 3;
    public float successArcDegrees = 60f;
    public float perfectArcDegrees = 20f;   
    public float rotateSpeed = 180f;
    public KeyCode confirmKey = KeyCode.Space;

    [Header("Runtime State")]
    private bool isRunning = false;
    private int currentCheck = 0;
    private QTEResult[] results;

    [Header("Result Display")]
    public TMP_Text resultText;
    public float resultDisplayTime = 1f; 

    [Header("Fade Effect")]
    public CanvasGroup canvasGroup;
    public float fadeDuration = 1f; 

    public Action<QTEResult[]> OnQTEFinished;

    private void OnEnable()
    {
        StartQTE();

        // Mute the prompt
        if (resultText != null)
            resultText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isRunning) return;

        float z = indicator.localEulerAngles.z;
        z = Mathf.Repeat(z - rotateSpeed * Time.deltaTime, 360f);
        indicator.localEulerAngles = new Vector3(0, 0, z);

        if (Input.GetKeyDown(confirmKey))
        {
            QTEResult result = EvaluateHit();
            results[currentCheck] = result;
            ShowResultText(result);

            Debug.Log($" The {currentCheck + 1} result: {result}");

            currentCheck++;
            if (currentCheck >= totalChecks)
            {
                Finish();
            }
            else
            {
                RandomizeCircleGroup();
            }
        }
    }

    public void StartQTE()
    {
        isRunning = true;
        currentCheck = 0;
        results = new QTEResult[totalChecks];

        SetupZones();
        RandomizeCircleGroup();
        indicator.localEulerAngles = Vector3.zero;
    }

    public void StopQTE()
    {
        isRunning = false;
    }

    private void SetupZones()
    {
        if (successZoneImage != null && perfectZoneImage != null)
        {
            // Let the transform of perfectzone be the same as successzone
            perfectZoneImage.rectTransform.localEulerAngles =
                successZoneImage.rectTransform.localEulerAngles;
        }
    }


    private void RandomizeCircleGroup()
    {
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        circleGroup.localEulerAngles = new Vector3(0, 0, randomAngle);
    }

    private QTEResult EvaluateHit()
    {
        float indicatorAngle = Mathf.Repeat(indicator.eulerAngles.z, 360f);

        // SuccessZone range and angle
        float successCenter = Mathf.Repeat(successZoneImage.rectTransform.eulerAngles.z, 360f);
        float successHalfRange = successArcDegrees * 0.5f;

        // PerfectZone range and angle
        float perfectCenter = Mathf.Repeat(perfectZoneImage.rectTransform.eulerAngles.z, 360f);
        float perfectHalfRange = perfectArcDegrees * 0.5f;

        float diffSuccess = Mathf.Abs(Mathf.DeltaAngle(indicatorAngle, successCenter));
        float diffPerfect = Mathf.Abs(Mathf.DeltaAngle(indicatorAngle, perfectCenter));

        // Estimate if hit the target
        if (diffPerfect <= perfectHalfRange) return QTEResult.Perfect;
        else if (diffSuccess <= successHalfRange) return QTEResult.Good;
        else return QTEResult.Miss;
    }

    private void Finish()
    {
        isRunning = false;

        OnQTEFinished?.Invoke(results);

        StartCoroutine(FadeOutQTE(1f)); 
    }

    private void ShowResultText(QTEResult result)
    {
        if (resultText == null) return;

        switch (result)
        {
            case QTEResult.Perfect:
                resultText.text = "PERFECT!";
                resultText.color = Color.orange;
                break;

            case QTEResult.Good:
                resultText.text = "GOOD";
                resultText.color = Color.yellow;
                break;

            case QTEResult.Miss:
                resultText.text = "MISS";
                resultText.color = Color.grey;
                break;
        }

        resultText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideResultText)); 
        Invoke(nameof(HideResultText), resultDisplayTime);
    }

    private void HideResultText()
    {
        if (resultText != null)
            resultText.gameObject.SetActive(false);
    }

    private IEnumerator FadeOutQTE(float delay)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);

        FindFirstObjectByType<QTEManager>()?.EndQTE();

    }
}
