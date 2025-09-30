using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QTEUIController : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform circleGroup;
    public RectTransform indicator;
    public Image successZoneImage;   
    public Image perfectZoneImage;   

    [Header("Config")]
    public int totalChecks = 3;
    public float successArcDegrees = 60f;
    public float perfectArcDegrees = 20f;
    public float rotateSpeed = 180f;
    public KeyCode confirmKey = KeyCode.Space;
    public bool autoStartOnEnable = true;

    [Header("Tuning")]
    public float toleranceDeg = 2f;     

    [Header("UI Text Display")]
    public TMP_Text resultText;   
    public float resultDisplayTime = 1.2f; 
    private Coroutine resultCoroutine;

    private bool isRunning = false;
    private int currentCheck = 0;
    private bool[] results;

    public Action<string[]> OnQTEFinished;  // "Perfect"/"Good"/"Fail" 

    private void OnEnable()
    {
        if (autoStartOnEnable) StartQTE();
    }

    private void OnDisable()
    {
        isRunning = false;
    }

    private void Update()
    {
        if (!isRunning) return;

        float z = indicator.localEulerAngles.z;
        z = Mathf.Repeat(z - rotateSpeed * Time.deltaTime, 360f);
        indicator.localEulerAngles = new Vector3(0f, 0f, z);

        if (Input.GetKeyDown(confirmKey))
        {
            string result = EvaluateHit();
            Debug.Log($"result: {result}");
            ShowResultText(result);

            results[currentCheck] = result == "Perfect";
            currentCheck++;

            if (currentCheck >= totalChecks)
            {
                Finish();
            }
            else
            {
                RandomizeZones();
            }
        }
    }

    public void StartQTE()
    {
        if (indicator == null || successZoneImage == null || perfectZoneImage == null)
        {
            Debug.LogError("[QTE] 缺少引用！");
            return;
        }

        results = new bool[totalChecks];
        currentCheck = 0;
        isRunning = true;
        indicator.localEulerAngles = Vector3.zero;

        // ✅ 启动时清空结果提示文字
        if (resultText != null)
        {
            resultText.text = "";
            resultText.alpha = 0f;
        }

        SetupZones();
        RandomizeZones();
    }

    public void StopQTE()
    {
        isRunning = false;
    }

    private void Finish()
    {
        isRunning = false;

        // 转换结果数组
        string[] labels = new string[results.Length];
        for (int i = 0; i < results.Length; i++)
        {
            labels[i] = results[i] ? "Perfect" : "Good";
        }

        // ✅ 等待最后一个提示显示完再关闭
        if (resultText != null && !string.IsNullOrEmpty(resultText.text))
        {
            StartCoroutine(DelayFinish(labels));
        }
        else
        {
            OnQTEFinished?.Invoke(labels);
        }
    }

    private IEnumerator DelayFinish(string[] labels)
    {
        // 等待文字显示时间 + 一点淡出时间
        yield return new WaitForSeconds(resultDisplayTime + 0.5f);
        OnQTEFinished?.Invoke(labels);
    }


    private void SetupZones()
    {
        successZoneImage.type = Image.Type.Filled;
        successZoneImage.fillMethod = Image.FillMethod.Radial360;
        successZoneImage.fillAmount = successArcDegrees / 360f;

        perfectZoneImage.type = Image.Type.Filled;
        perfectZoneImage.fillMethod = Image.FillMethod.Radial360;

        // ✅ 保持视觉上完整显示
        perfectZoneImage.fillAmount = 1f;
    }


    private void RandomizeZones()
    {
        // 1️⃣ 刷新整个圆圈
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        circleGroup.localEulerAngles = new Vector3(0, 0, randomAngle);

        // 2️⃣ 确保两者绘制参数一致
        perfectZoneImage.fillOrigin = successZoneImage.fillOrigin;
        perfectZoneImage.fillClockwise = successZoneImage.fillClockwise;

        // ✅ 3️⃣ 不再对 perfectZoneImage 自己旋转，只要跟随父级
        perfectZoneImage.rectTransform.localEulerAngles = Vector3.zero;
    }

    private string EvaluateHit()
    {
        float indicatorAngle = Mathf.Repeat(indicator.eulerAngles.z, 360f);
        float successCenter = GetRadialCenterAngle(successZoneImage, successArcDegrees);

        // ✅ 随机 perfect 偏移（判定用）
        float maxOffset = (successArcDegrees - perfectArcDegrees) * 0.5f;
        float perfectOffset = UnityEngine.Random.Range(-maxOffset, +maxOffset);
        float perfectCenter = successCenter + perfectOffset;

        float diffSuccess = Mathf.Abs(Mathf.DeltaAngle(indicatorAngle, successCenter));
        float diffPerfect = Mathf.Abs(Mathf.DeltaAngle(indicatorAngle, perfectCenter));

        bool inPerfect = diffPerfect <= ((perfectArcDegrees * 0.5f + toleranceDeg) * 1f);
        bool inSuccess = diffSuccess <= (successArcDegrees * 0.5f + toleranceDeg);

        if (inPerfect) return "Perfect";
        if (inSuccess) return "Good";
        return "Miss";
    }

    private void ShowResultText(string result)
    {
        if (resultText == null) return;

        if (resultCoroutine != null) StopCoroutine(resultCoroutine);
        resultCoroutine = StartCoroutine(DisplayResultCoroutine(result));
    }

    private IEnumerator DisplayResultCoroutine(string result)
    {
        // 设置文字与颜色
        resultText.text = result.ToUpper();
        if (result == "Perfect")
            resultText.color = Color.yellow;
        else if (result == "Good")
            resultText.color = Color.blue;
        else
            resultText.color = Color.grey;

        // 淡入并保持一段时间
        resultText.alpha = 1f; // 立即显示
        yield return new WaitForSeconds(resultDisplayTime);

        // 淡出
        float fadeTime = 0.5f;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            resultText.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            yield return null;
        }

        resultText.text = "";
    }
    // 计算某个 Radial 360 Image 的“中心角”（世界角度，0~360）
    float GetRadialCenterAngle(Image img, float arcDegrees)
    {
        // 基础角：物体的世界Z角度
        float baseZ = img.rectTransform.eulerAngles.z;

        // 按 FillOrigin 决定起始角相对 baseZ 的偏移
        // Unity: 0=Bottom, 1=Right, 2=Top, 3=Left
        float originOffset = 0f;
        switch (img.fillOrigin)
        {
            case 0: originOffset = 180f; break;  // Bottom：向下
            case 1: originOffset = -90f; break;  // Right：向右
            case 2: originOffset = 0f; break;    // Top：向上
            case 3: originOffset = 90f; break;   // Left：向左
        }

        // 起始角（世界角度）
        float start = Mathf.Repeat(baseZ + originOffset, 360f);

        float dir = img.fillClockwise ? -1f : 1f;
        float center = Mathf.Repeat(start - dir * (arcDegrees * 0.5f), 360f);

        return center;
    }
}
