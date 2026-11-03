// Habillage « comics » posé sur les illustrations de fond : vignettage + trame de points (demi-teinte) qui grossit vers les bords.
Shader "HD/UI/Backdrop"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        _Vignette ("Vignette strength", Range(0,1)) = 0.55
        _Dots ("Dots per screen height", Float) = 54
        _DotStrength ("Dot strength", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            struct appdata_t { float4 vertex : POSITION; float4 color : COLOR; float2 texcoord : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float4 worldPosition : TEXCOORD1; float2 screenUv : TEXCOORD2; UNITY_VERTEX_OUTPUT_STEREO };

            fixed4 _Color;
            float4 _ClipRect;
            float _Vignette, _Dots, _DotStrength;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.screenUv = OUT.vertex.xy / OUT.vertex.w * 0.5 + 0.5;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.screenUv;
                float2 c = uv - 0.5;
                float edge = saturate(dot(c, c) * 3.2);          // 0 au centre, ~1 dans les coins
                float vignette = edge * _Vignette;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 grid = float2(uv.x * aspect, uv.y) * _Dots;
                // Trame en quinconce, inclinée à 45 degrés.
                float2 r = float2(grid.x + grid.y, grid.x - grid.y) * 0.7071;
                float2 g = frac(r) - 0.5;
                float radius = edge * 0.55;
                float dots = smoothstep(radius, radius - 0.12, length(g)) * edge * _DotStrength;
                float a = saturate(vignette + dots);
                fixed4 color = fixed4(IN.color.rgb, a * IN.color.a);
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                return color;
            }
            ENDCG
        }
    }
}
