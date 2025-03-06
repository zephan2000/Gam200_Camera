Shader "Unlit/sampleButtonShader"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}


        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0

        _TintA("Tint A", Color) = (1, 1, 1, 1)
        _TintB("Tint B", Color) = (1, 1, 1, 1)
        _IsPressed("Is Pressed", Range(0, 1)) = 1
        _AlphaMod("Alpha Mod", Range(0, 1)) = 1
        _WarningColor("Warning Color", Color) = (1, 1, 1, 1)
        _WarningProgress("Warning Progress", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

            Stencil
            {
                Ref[_Stencil]
                Comp[_StencilComp]
                Pass[_StencilOp]
                ReadMask[_StencilReadMask]
                WriteMask[_StencilWriteMask]
            }
            Cull Off
            Lighting Off
            ZWrite Off
            ZTest[unity_GUIZTestMode]
            //Blend SrcAlpha OneMinusSrcAlpha

        Blend One OneMinusSrcAlpha
            ColorMask[_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

                #pragma target 2.0

                #include "UnityCG.cginc"
                #include "UnityUI.cginc"

                #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
                #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _TintA, _TintB;
            float _IsPressed, _AlphaMod;

            fixed4 _WarningColor;
            float _WarningProgress;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float _IsPressed2 = 0.75 + _IsPressed * 0.25;
                i.uv.x -= step(i.uv.x, 0.12 + 0.015) * 0.12 * (1 - _IsPressed2);
                clip(i.uv.x);
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * (step(i.uv.x, 0.12) * (_TintA * _IsPressed + _TintB * (1 - _IsPressed)) + step(0.12, i.uv.x) * _TintB);

                col.rgb *= fixed3(1,1,1) * (1 - _WarningProgress) + _WarningProgress * _WarningColor;

            col.a *= 1 - clamp(smoothstep(0.45, 1.0, i.uv.x), 0, 1);

            clip(abs(i.uv.x - 0.12) - 0.015);
            float2 theuv = float2(6 * (i.uv.x - 0.5) + 2.4, i.uv.y);
            clip(length(theuv - float2(0.12, 0.5)) - 0.15 * _IsPressed);

            clip(step(length(float2(0.5, 0.5) - i.uv),  0.68) - 0.01);

            col.a *= _AlphaMod;

            col.rgb *= col.a;
                return col;
            }
            ENDCG
        }
    }
}
