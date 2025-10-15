Shader "Project/CelOutline"
{
    Properties{
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Width (object-space)", Range(0,0.05)) = 0.01
    }
    SubShader{
        Tags{ "RenderType"="Opaque" "Queue"="Geometry+1" }
        Cull Front  ZWrite On  ZTest LEqual

        Pass{
            Name "OUTLINE"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _OutlineColor; float _OutlineWidth;

            struct app{ float4 v:POSITION; float3 n:NORMAL; };
            struct v2{ float4 pos:SV_POSITION; };

            v2 vert(app a){
                v2 o; float3 n=normalize(a.n);
                float4 expanded = a.v + float4(n*_OutlineWidth,0);
                o.pos = UnityObjectToClipPos(expanded); return o;
            }
            fixed4 frag(v2 i):SV_Target{ return _OutlineColor; }
            ENDCG
        }
    }
}
