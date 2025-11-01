Shader "Custom/HeatWaveDistortion"
{
    Properties
    {
        _DistortionStrength ("Distortion Strength", Range(0, 0.1)) = 0.02
        _DistortionSpeed ("Distortion Speed", Range(0, 5)) = 1.0
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 2.0
        _RiseSpeed ("Rise Speed", Range(0, 5)) = 0.5
        _FadeTop ("Fade Top", Range(0, 1)) = 0.8
        _FadeBottom ("Fade Bottom", Range(0, 1)) = 0.2
        _TintColor ("Tint Color", Color) = (1, 0.8, 0.6, 0.1)
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        
        // Grab the screen behind the object into _GrabTexture
        GrabPass { "_GrabTexture" }
        
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            // Properties
            sampler2D _GrabTexture;
            float _DistortionStrength;
            float _DistortionSpeed;
            float _NoiseScale;
            float _RiseSpeed;
            float _FadeTop;
            float _FadeBottom;
            float4 _TintColor;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 grabPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };
            
            // Simple 2D noise function
            float noise(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // Smooth noise with interpolation
            float smoothNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f); // Smoothstep
                
                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            // Fractal Brownian Motion for more organic noise
            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for(int i = 0; i < 4; i++)
                {
                    value += amplitude * smoothNoise(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                
                return value;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Calculate vertical fade (stronger at bottom, fades at top)
                float verticalFade = smoothstep(_FadeTop, _FadeBottom, i.uv.y);
                
                // Animated noise coordinates
                float2 noiseCoord1 = i.worldPos.xz * _NoiseScale;
                noiseCoord1.y += _Time.y * _RiseSpeed;
                noiseCoord1.x += _Time.y * _DistortionSpeed * 0.3;
                
                float2 noiseCoord2 = i.worldPos.xz * _NoiseScale * 1.7;
                noiseCoord2.y += _Time.y * _RiseSpeed * 1.3;
                noiseCoord2.x -= _Time.y * _DistortionSpeed * 0.2;
                
                // Generate layered noise for distortion
                float noise1 = fbm(noiseCoord1);
                float noise2 = fbm(noiseCoord2);
                
                // Combine noises and convert to distortion offset
                float2 distortion = float2(noise1 - 0.5, noise2 - 0.5) * 2.0;
                
                // Add wavering motion (side-to-side)
                distortion.x += sin(_Time.y * _DistortionSpeed + i.worldPos.z) * 0.5;
                
                // Scale distortion by strength and vertical fade
                distortion *= _DistortionStrength * verticalFade;
                
                // Apply distortion to grab coordinates
                float2 grabUV = i.grabPos.xy / i.grabPos.w;
                grabUV += distortion;
                
                // Sample the distorted background
                fixed4 bgcolor = tex2D(_GrabTexture, grabUV);
                
                // Add subtle warm tint
                fixed4 finalColor = bgcolor;
                finalColor.rgb = lerp(bgcolor.rgb, _TintColor.rgb, _TintColor.a * verticalFade * 0.5);
                
                // Set alpha based on vertical fade
                finalColor.a = verticalFade * 0.8;
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    Fallback "Transparent/Diffuse"
}