Shader "Unlit/MenuTestGlitchShader"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _DisgustingTex("Disgusting Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0

        _FillProgress("Fill Progress", Range(0, 1)) = 1
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
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask[_ColorMask]

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
                #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

                struct appdata_t
                {
                    float4 vertex   : POSITION;
                    float4 color    : COLOR;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord  : TEXCOORD0;
                    float4 worldPosition : TEXCOORD1;
                    float2 texcoordD  : TEXCOORD2;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                fixed4 _Color, _BorderColor, _InnerColor, _OutlineColor, _UnfillColor, _TopSmoothColor, _IFrameFillColor, _FlashFillColor;
                fixed4 _TextureSampleAdd;
                float4 _ClipRect;
                float4 _MainTex_ST;
                float _FillProgress;

                sampler2D _DisgustingTex;
                float4 _DisgustingTex_ST;

                v2f vert(appdata_t v)
                {
                    v2f OUT;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                    OUT.worldPosition = v.vertex;
                    OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                    OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    OUT.texcoordD = TRANSFORM_TEX(v.texcoord, _DisgustingTex);

                    OUT.color = v.color;
                    return OUT;
                }

                fixed4 frag(v2f IN) : SV_Target
                {
                    float thres = 0.2;
                    _DisgustingTex_ST.w += _Time.y;
                    half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                    float2 modthres = float2(1 , 1 ) / 100;
                    float2 pixelTexcoords = float2(IN.texcoordD.x - IN.texcoordD.x % modthres.x, IN.texcoordD.y - IN.texcoordD.y % modthres.y) + 0.5 * float2(modthres.x, modthres.y);
                    half4 unspeakable = tex2D(_DisgustingTex, pixelTexcoords);

                    _FillProgress = _FillProgress * (1 + thres) - thres;

                    clip(/*step(_FillProgress, 0.001) + */ step(_FillProgress, IN.texcoord.y - thres) + unspeakable.r - (1 - (IN.texcoord.y - _FillProgress) / thres));

                    color.a *= step(_FillProgress, IN.texcoord.y);

                    #ifdef UNITY_UI_CLIP_RECT
                    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                    #endif

                    #ifdef UNITY_UI_ALPHACLIP
                    clip(color.a - 0.001);
                    #endif

                    return color;
                }
            ENDCG
            }
        }
}
