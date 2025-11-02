Shader "Project/CelShading"
{
    Properties
    {
        _Color        ("Albedo Color", Color) = (1,1,1,1)
        _MainTex      ("Albedo (RGB)", 2D)    = "white" {}
        _Steps        ("Diffuse Steps (>=2, -1=ramp)", Range(-1,8)) = 2
        _SpecSteps    ("Spec Steps (>=1)", Range(1,6)) = 1
        _SpecStrength ("Spec Strength", Range(0,2)) = 0.7
        _RampTex      ("(Optional) Diffuse Ramp", 2D) = "gray" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300
        Cull Back
        ZWrite On
        ZTest LEqual

        Pass
        {
            Name "FORWARD_BASE"
            Tags{ "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            sampler2D _MainTex; float4 _MainTex_ST;
            sampler2D _RampTex;
            fixed4 _Color;
            float _Steps, _SpecSteps, _SpecStrength;

            struct AppData
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct V2F
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                float3 nWS   : TEXCOORD1;
                float3 vWS   : TEXCOORD2;
                float3 wPos  : TEXCOORD3;  
                SHADOW_COORDS(4)
            };

            inline fixed Quantize(float x, float bands, sampler2D rampTex)
            {
                x = saturate(x);
                if (bands < 0.0) return tex2D(rampTex, float2(x, 0.5)).r;
                bands = max(bands, 2.0);
                return saturate(floor(x * bands) / (bands - 1.0));
            }

            V2F vert(AppData IN)
            {
                V2F OUT;
                OUT.pos  = UnityObjectToClipPos(IN.vertex);
                OUT.uv   = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.nWS  = normalize(UnityObjectToWorldNormal(IN.normal));
                OUT.wPos = mul(unity_ObjectToWorld, IN.vertex).xyz;
                OUT.vWS  = _WorldSpaceCameraPos - OUT.wPos;
                TRANSFER_SHADOW(OUT);
                return OUT;
            }

            fixed4 frag(V2F IN) : SV_Target
            {
                fixed4 albedo = tex2D(_MainTex, IN.uv) * _Color;
                float3 N = normalize(IN.nWS);
                float3 V = normalize(IN.vWS);

                float3 L = normalize(UnityWorldSpaceLightDir(IN.wPos));

                fixed shadow   = SHADOW_ATTENUATION(IN);
                fixed diffBand = Quantize(dot(N, L), _Steps, _RampTex) * shadow;

                float3 H       = normalize(L + V);
                fixed specBand = Quantize(pow(saturate(dot(N, H)), 32.0), _SpecSteps, _RampTex);

                fixed3 lit     = _LightColor0.rgb * (albedo.rgb * diffBand + specBand * _SpecStrength);
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * albedo.rgb;

                return fixed4(ambient + lit, albedo.a);
            }
            ENDCG
        }

        Pass
        {
            Tags { "LightMode"="ForwardAdd" }
            Blend One One
            ZWrite Off
            Cull Back

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float  _Steps;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f {
                float4 pos       : SV_POSITION;
                float3 worldPos  : TEXCOORD0;
                float3 worldNorm : TEXCOORD1;
                float2 uv        : TEXCOORD2;
                SHADOW_COORDS(3)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos       = UnityObjectToClipPos(v.vertex);
                o.worldPos  = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNorm = UnityObjectToWorldNormal(v.normal);
                o.uv        = TRANSFORM_TEX(v.uv, _MainTex);
                TRANSFER_SHADOW(o);
                return o;
            }

            half ToonStep(half ndl, half steps)
            {
                steps = max(2.0h, steps);
                return (floor(ndl * steps + 1e-4h) / (steps - 1.0h)) > 0.5h ? 1.0h : 0.0h;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);

                float3 L = normalize(UnityWorldSpaceLightDir(i.worldPos));

                float3 N   = normalize(i.worldNorm);
                half   ndl = saturate(dot(N, L));
                half   band= (_Steps >= 2.0h) ? ToonStep(ndl, (half)_Steps) : (ndl > 0.5h ? 1.0h : 0.0h);

                fixed3 albedo = tex2D(_MainTex, i.uv).rgb * _Color.rgb;
                fixed3 lit    = albedo * _LightColor0.rgb * band * atten; 

                return fixed4(lit, 0);
            }
            ENDCG
        }
    }
}
