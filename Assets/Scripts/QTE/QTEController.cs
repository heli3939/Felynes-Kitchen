using System;
using UnityEngine;
using UnityEngine.UI;

public class QTEUIController : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform circleGroup;     
    public RectTransform indicator;      
    public Image successZoneImage;        

    [Header("Config")]
    public int totalChecks = 3;           // check 3 times for once
    public float successArcDegrees = 60f; 
    public float rotateSpeed = 180f;      
    public KeyCode confirmKey = KeyCode.Space;
    public bool autoStartOnEnable = true; 

    [Header("State (read-only)")]
    [SerializeField] private bool isRunning = false;
    [SerializeField] private int currentCheck = 0;

    public Action<bool[]> OnQTEFinished;  

    private bool[] results;

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
            bool hit = EvaluateHit();
            results[currentCheck] = hit;

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

        successZoneImage.type = Image.Type.Filled;
        successZoneImage.fillMethod = Image.FillMethod.Radial360;
        successZoneImage.fillAmount = Mathf.Clamp01(successArcDegrees / 360f);


        results = new bool[totalChecks];
        currentCheck = 0;
        isRunning = true;

        indicator.localEulerAngles = Vector3.zero;

        RandomizeCircleGroup();
    }

    public void StopQTE() 
    {
        isRunning = false;
    }

    private void Finish()
    {
        isRunning = false;
        OnQTEFinished?.Invoke(results);
    }

    private void RandomizeCircleGroup()
    {
        float a = UnityEngine.Random.Range(0f, 360f);
        circleGroup.localEulerAngles = new Vector3(0f, 0f, a);
    }

    private bool EvaluateHit()
    {

        float indicatorWorld = Mathf.Repeat(indicator.eulerAngles.z, 360f);
        float zoneCenterWorld = Mathf.Repeat(successZoneImage.rectTransform.eulerAngles.z, 360f);

        float half = successArcDegrees * 0.5f;
        float diff = Mathf.Abs(Mathf.DeltaAngle(indicatorWorld, zoneCenterWorld));
        return diff <= half;
    }
}
