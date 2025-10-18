Shader "Project/ItemOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.05)) = 0.015
        _ZOffset      ("Depth Offset", Range(0, 4)) = 1
        _Alpha        ("Alpha", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }
        LOD 100

        Pass
        {
            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            Offset [_ZOffset], [_ZOffset]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _OutlineColor;
            float  _OutlineWidth;
            float  _Alpha;

            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; };
            struct v2f { float4 pos:SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                float3 vpos  = mul(UNITY_MATRIX_MV, v.vertex).xyz;
                float3 vnorm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                vpos += vnorm * _OutlineWidth;                 // inflate backfaces
                o.pos = mul(UNITY_MATRIX_P, float4(vpos,1));
                return o;
            }

            fixed4 frag (v2f i):SV_Target { return fixed4(_OutlineColor.rgb, _Alpha); }
            ENDCG
        }
    }

    FallBack Off
}
