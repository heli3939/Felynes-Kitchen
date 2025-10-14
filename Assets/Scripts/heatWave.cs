using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class HeatWaveController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The RandomFlame component to sync with")]
    public RandomFlame flameSource;
    
    [Header("Fade In/Out")]
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 0.8f;
    
    [Header("Distortion Settings")]
    [SerializeField] private float maxDistortionStrength = 0.025f;
    [SerializeField] private float distortionSpeed = 1.5f;
    [SerializeField] private float noiseScale = 3.0f;
    
    [Header("Visual")]
    [SerializeField] private Color tintColor = new Color(1f, 0.8f, 0.6f, 0.1f);
    
    private MeshRenderer meshRenderer;
    private Material instancedMaterial;
    private bool isActive;
    private float fadeT;
    
    // Shader property IDs
    private static readonly int DistortionStrengthProp = Shader.PropertyToID("_DistortionStrength");
    private static readonly int DistortionSpeedProp = Shader.PropertyToID("_DistortionSpeed");
    private static readonly int NoiseScaleProp = Shader.PropertyToID("_NoiseScale");
    private static readonly int TintColorProp = Shader.PropertyToID("_TintColor");
    
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        
        // Create material instance
        if (meshRenderer.material != null)
        {
            instancedMaterial = meshRenderer.material;
            
            // Set initial properties
            instancedMaterial.SetFloat(DistortionSpeedProp, distortionSpeed);
            instancedMaterial.SetFloat(NoiseScaleProp, noiseScale);
            instancedMaterial.SetColor(TintColorProp, tintColor);
        }
        
        // Auto-find flame source if not assigned
        if (flameSource == null)
        {
            flameSource = GetComponentInParent<RandomFlame>();
            if (flameSource == null)
            {
                Debug.LogWarning($"HeatWaveController on {gameObject.name}: No RandomFlame found!");
            }
        }
        
        // Start hidden
        meshRenderer.enabled = false;
        fadeT = 0f;
    }
    
    private void Update()
    {
        if (flameSource == null || instancedMaterial == null) return;
        
        // Check if flame is on by accessing the private isOn field through reflection
        // OR we can check if the collider is enabled (which indicates flame is active)
        bool shouldBeActive = flameSource.gameObject.activeSelf && 
                            flameSource.enabled &&
                            IsFlameActive();
        
        // Update active state
        if (shouldBeActive && !isActive)
        {
            isActive = true;
            meshRenderer.enabled = true;
        }
        else if (!shouldBeActive && isActive)
        {
            isActive = false;
        }
        
        // Fade in/out
        if (isActive && fadeT < 1f)
        {
            fadeT += Time.deltaTime / fadeInTime;
            fadeT = Mathf.Min(fadeT, 1f);
        }
        else if (!isActive && fadeT > 0f)
        {
            fadeT -= Time.deltaTime / fadeOutTime;
            fadeT = Mathf.Max(fadeT, 0f);
            
            if (fadeT <= 0f)
            {
                meshRenderer.enabled = false;
            }
        }
        
        // Apply fade to distortion strength (exponential ease for subtlety)
        float easedFade = Mathf.Pow(fadeT, 2f);
        float currentStrength = maxDistortionStrength * easedFade;
        instancedMaterial.SetFloat(DistortionStrengthProp, currentStrength);
    }
    
    // Helper method to detect if flame is active
    // Checks if the particle system is playing and emitting
    private bool IsFlameActive()
    {
        var ps = flameSource.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            return ps.isPlaying && ps.emission.enabled;
        }
        return false;
    }
    
    private void OnValidate()
    {
        // Update properties in editor
        if (Application.isPlaying && instancedMaterial != null)
        {
            instancedMaterial.SetFloat(DistortionSpeedProp, distortionSpeed);
            instancedMaterial.SetFloat(NoiseScaleProp, noiseScale);
            instancedMaterial.SetColor(TintColorProp, tintColor);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up instanced material
        if (instancedMaterial != null)
        {
            Destroy(instancedMaterial);
        }
    }
}