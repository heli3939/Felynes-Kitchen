Shader "Project/CelShading"
{
    Properties
    {
        _Color        ("Albedo Color", Color) = (1,1,1,1)
        _MainTex      ("Albedo (RGB)", 2D)    = "white" {}
        _Steps        ("Diffuse Steps (>=2, -1=ramp)", Range(-1,8)) = 3
        _SpecSteps    ("Spec Steps (>=1)", Range(1,6)) = 1
        _SpecStrength ("Spec Strength", Range(0,2)) = 0.7
        _RampTex      ("(Optional) Diffuse Ramp", 2D) = "gray" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width (0=off)", Range(0,0.05)) = 0.039
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300
        Cull Back
        ZWrite On
        ZTest LEqual

        // ---------- CEL SHADING ----------
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
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float3 nWS : TEXCOORD1;
                float3 vWS : TEXCOORD2;
                SHADOW_COORDS(3)
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
                OUT.pos = UnityObjectToClipPos(IN.vertex);
                OUT.uv  = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.nWS = normalize(UnityObjectToWorldNormal(IN.normal));
                float3 worldPos = mul(unity_ObjectToWorld, IN.vertex).xyz;
                OUT.vWS = _WorldSpaceCameraPos - worldPos;
                TRANSFER_SHADOW(OUT);
                return OUT;
            }

            fixed4 frag(V2F IN) : SV_Target
            {
                fixed4 albedo = tex2D(_MainTex, IN.uv) * _Color;
                float3 N = normalize(IN.nWS);
                float3 V = normalize(IN.vWS);
                float3 L = normalize(_WorldSpaceLightPos0.xyz);

                fixed shadow = SHADOW_ATTENUATION(IN);
                fixed diffBand = Quantize(dot(N, L), _Steps, _RampTex) * shadow;

                float3 H = normalize(L + V);
                fixed specBand = Quantize(pow(saturate(dot(N, H)), 32.0), _SpecSteps, _RampTex);

                fixed3 lit = _LightColor0.rgb * (albedo.rgb * diffBand + specBand * _SpecStrength);
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * albedo.rgb;

                return fixed4(ambient + lit, albedo.a);
            }
            ENDCG
        }

        // ---------- OUTLINE ----------
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode"="Always" }
            Cull Front
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex   vertOutline
            #pragma fragment fragOutline
            #include "UnityCG.cginc"

            float  _OutlineWidth;
            float4 _OutlineColor;

            struct AppData
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct V2F
            {
                float4 pos : SV_POSITION;
            };

            V2F vertOutline(AppData IN)
            {
                V2F OUT;
                float3 n = normalize(IN.normal);
                float4 expanded = IN.vertex + float4(n * _OutlineWidth, 0);
                OUT.pos = UnityObjectToClipPos(expanded);
                return OUT;
            }

            fixed4 fragOutline(V2F IN) : SV_Target
            {
                if (_OutlineWidth <= 0.0001) discard;
                return _OutlineColor;
            }
            ENDCG
        }
    }
}
