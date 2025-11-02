Shader "Project/ItemOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth ("Outline Width (fraction of size)", Range(0, 0.05)) = 0.015
        _ZOffset      ("Depth Offset", Range(0, 4)) = 1
        _Alpha        ("Alpha", Range(0,1)) = 1

        // set per-object from script (world-units). Hide it in Inspector.
        [HideInInspector] _Bounds ("Object Bounds (world units)", Float) = 1
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
            float  _Bounds;   // ≈ object “size” in world units

            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; };
            struct v2f { float4 pos:SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;

                // ---- inflate in WORLD space by a fraction of object size
                float3 wpos  = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 wnorm = normalize(UnityObjectToWorldNormal(v.normal));

                // outline width is proportional to object bounds (e.g., 0.5%–2%)
                float widWorld = _OutlineWidth * _Bounds;

                wpos += wnorm * widWorld;

                o.pos = UnityWorldToClipPos(wpos);
                return o;
            }

            fixed4 frag (v2f i):SV_Target { return fixed4(_OutlineColor.rgb, _Alpha); }
            ENDCG
        }
    }

    FallBack Off
}
