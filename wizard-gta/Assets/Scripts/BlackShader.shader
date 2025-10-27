Shader "Unlit/BlackShader"
{
   Properties
    {
        _MainTex("Overlay Texture", 2D) = "white" {}
        _MaskTex("Mask Texture", 2D) = "white" {}
        _Color("Overlay Color", Color) = (0,0,0,0.85)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _Color;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float mask = tex2D(_MaskTex, i.uv).r;
                return half4(_Color.rgb, _Color.a * (1 - mask));
            }
            ENDHLSL
        }
    }
    }
