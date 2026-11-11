Shader "HeroesDawn/Character3D"
{
    // Personnage 3D sans éclairage (la scène des activités n'a pas de lumière) avec un masque de zones :
    // R = vêtements (teinte A), G = équipement sombre (teinte B). La peau et le reste ne sont pas teintés.
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        _MaskMap("Masque des zones (R vêtements, G équipement)", 2D) = "black" {}
        [MainColor] _BaseColor("Teinte des vêtements", Color) = (1,1,1,1)
        _TintB("Teinte de l'équipement", Color) = (1,1,1,1)
        _FlashColor("Éclat (RGB, alpha = force)", Color) = (1,1,1,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_MaskMap); SAMPLER(sampler_MaskMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _TintB;
                half4 _FlashColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half4 mask = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, input.uv);
                half3 tintA = lerp(half3(1, 1, 1), _BaseColor.rgb, mask.r);
                half3 tintB = lerp(half3(1, 1, 1), _TintB.rgb, mask.g);
                half3 color = baseTex.rgb * tintA * tintB;
                color = lerp(color, _FlashColor.rgb, _FlashColor.a);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
