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

    [Header("Angle Offsets (deg)")]
    public float successCenterOffsetDeg = 0f;
    public float perfectCenterOffsetDeg = 0f;

    [Header("Timer UI")]
    public TMP_Text timerText;             
    public Color normalColor = Color.white; 
    public Color warningColor = Color.red;  
    public float warningThreshold = 3f;

    [Header("Timer Settings")]
    public float totalTime = 10f;
    private float remainingTime;       
    private bool timeRunning = false;  

    public Action<QTEResult[]> OnQTEFinished;

    private void OnEnable() { }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket))   // '[' anticlockwise -1°
        {
            successCenterOffsetDeg -= 1f;
            perfectCenterOffsetDeg -= 1f;
            Debug.Log($"Offset = {successCenterOffsetDeg:F1}°");
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))  // ']' clockwise +1°
        {
            successCenterOffsetDeg += 1f;
            perfectCenterOffsetDeg += 1f;
            Debug.Log($"Offset = {successCenterOffsetDeg:F1}°");
        }

        if (!isRunning) return;

        if (timeRunning)
        {
            remainingTime -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = $"Time: {remainingTime:F1}s";

                timerText.color = (remainingTime <= warningThreshold)
                    ? warningColor
                    : normalColor;
            }

            if (remainingTime <= 0f)
            {
                timeRunning = false;
                HandleTimeOut();
                return;
            }
        }
        float z = indicator.localEulerAngles.z;
        z = Mathf.Repeat(z - rotateSpeed * Time.deltaTime, 360f);
        indicator.localEulerAngles = new Vector3(0, 0, z);

        if (Input.GetKeyDown(confirmKey))
        {
            QTEResult result = EvaluateHit();
            results[currentCheck] = result;

            ShowResultText(result);
            Debug.Log($"The {currentCheck + 1} time's result: {result}");

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

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        SetupZones();
        RandomizeCircleGroup();
        indicator.localEulerAngles = Vector3.zero;

        // Start counting
        remainingTime = totalTime;
        timeRunning = true;

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = $"Time: {totalTime:F1}s";
            timerText.color = normalColor;
        }

        if (resultText != null)
            resultText.gameObject.SetActive(false);
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

        float successCenterRaw = successZoneImage.rectTransform.eulerAngles.z;
        float successCenter = Mathf.Repeat(successCenterRaw + successCenterOffsetDeg, 360f);
        float successHalf = successArcDegrees * 0.5f;

        float perfectCenterRaw = perfectZoneImage.rectTransform.eulerAngles.z;
        float perfectCenter = Mathf.Repeat(perfectCenterRaw + perfectCenterOffsetDeg, 360f);
        float perfectHalf = perfectArcDegrees * 0.5f;

        bool InAngleRange(float angle, float center, float halfRange)
        {
            float diff = Mathf.DeltaAngle(angle, center);
            return Mathf.Abs(diff) <= halfRange;
        }

        if (InAngleRange(indicatorAngle, perfectCenter, perfectHalf))
            return QTEResult.Perfect;

        if (InAngleRange(indicatorAngle, successCenter, successHalf))
            return QTEResult.Good;

        return QTEResult.Miss;
    }


    private void Finish()
    {
        if (timerText != null)
            timerText.gameObject.SetActive(false);

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

    private void HandleTimeOut()
    {
        for (int i = currentCheck; i < totalChecks; i++)
        {
            results[i] = QTEResult.Miss;
        }

        ShowResultText(QTEResult.Miss);

        Finish();
    }
}
