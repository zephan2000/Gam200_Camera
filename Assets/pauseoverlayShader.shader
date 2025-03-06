Shader "Unlit/pauseoverlayShader"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0

        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineMapValue("Outline Map Value", Range(0, 1)) = 0.5

        _FillProgress("Fill Progress", Range(0, 1)) = 1
        _UnfillColor("Unfill Color", Color) = (1, 1, 1, 1)

        _FlashFillProgress("Flash Fill Progress", Range(0, 1)) = 0
        _FlashFillClipThreshold("Flash Fill Clip Threshold", Range(0, 1)) = 1
        _FlashFillColor("Flash Fill Color", Color) = (1, 1, 1, 1)

        _IFrameFillProgress("IFrame Fill Progress", Range(0, 1)) = 1
        _IFrameFillColor("IFrame Fill Color", Color) = (1, 1, 1, 1)

        _TopSmoothColor("Top Smooth Color", Color) = (1, 1, 1, 1)
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
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                fixed4 _Color, _BorderColor, _InnerColor, _OutlineColor, _UnfillColor, _TopSmoothColor, _IFrameFillColor, _FlashFillColor;
                fixed4 _TextureSampleAdd;
                float4 _ClipRect;
                float4 _MainTex_ST;
                float _OutlineMapValue, _FillProgress;
                float _IFrameFillProgress, _FlashFillProgress, _FlashFillClipThreshold;

                float _UTime;

                v2f vert(appdata_t v)
                {
                    v2f OUT;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                    OUT.worldPosition = v.vertex;
                    OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                    OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                    //OUT.color = v.color * (step(_FillProgress, OUT.texcoord.y) * _UnfillColor + step(OUT.texcoord.y, _FillProgress) * _Color);
                    OUT.color = v.color;
                    return OUT;
                }

                fixed4 frag(v2f IN) : SV_Target
                {
                    _FlashFillProgress = _FlashFillProgress * 2;

                    fixed4 unfillTrue = step(_FlashFillClipThreshold, IN.texcoord.y) * _UnfillColor +
                    step(IN.texcoord.y, _FlashFillClipThreshold) *
                    (
                    step(_FlashFillProgress, 1.001) * (_UnfillColor * (1 - _FlashFillProgress) + _FlashFillColor * (_FlashFillProgress))
                    + step(1.001, _FlashFillProgress) * (_FlashFillColor * (2 - _FlashFillProgress) + _Color * (_FlashFillProgress - 1))
                    );

                    fixed4 colorValue = step(_FillProgress, IN.texcoord.y) * unfillTrue + step(IN.texcoord.y, _FillProgress) * _Color;
                    float colorLerpValue = clamp(smoothstep(0.25, 1.0, IN.texcoord.y), 0, 1) * clamp(step(IN.texcoord.y, _FillProgress), 0.1, 1.0);
                    IN.color *= colorValue * (1 - colorLerpValue) + _TopSmoothColor * colorLerpValue;

                    half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                    //float LEDRad = 0.02;
                    //float LEDGap = 0.00;
                    //float2 LEDUV = IN.texcoord % (LEDRad * 2);
                    //float LEDLength = length(LEDUV - float2(LEDRad, LEDRad));
                    //clip(step(LEDLength, LEDRad - LEDGap) - 0.0001);


                    float2 shrunkUV = 1.04 * (IN.texcoord - float2(0.5, 0.5)) + float2(0.5, 0.5);
                    float outlineCond = step(clamp(tex2D(_MainTex, shrunkUV).r , 0, 1) * tex2D(_MainTex, shrunkUV).a, 0.9); //0 means outline, 1 means fill
                    color.rgb = (1 - outlineCond) * color.rgb + outlineCond * (color.rgb * (1 - _OutlineMapValue) + _OutlineColor * _OutlineMapValue);

                    clip((1 - outlineCond) + step(0.01, _OutlineColor.a) - 0.01);

                    //Unused
                    color.rgb = outlineCond * color.rgb + (1 - outlineCond) * ((_IFrameFillColor.rgb * _IFrameFillColor.a + color.rgb * (1 - _IFrameFillColor.a)) * step(IN.texcoord.y, _IFrameFillProgress) + step(_IFrameFillProgress, IN.texcoord.y) * color.rgb);
                    //color.rgb += (1 - outlineCond) * _IFrameFillColor.rgb * _IFrameFillColor.a * step(IN.texcoord.y, _IFrameFillProgress);

                    //float lineInnerCondition = (1 - outlineCond) * step(fmod((IN.texcoord.y + 0.8 * _Time.y) % 1.0f, 0.033333 * 1.2), 0.01 * 1.2);
                    float lineInnerCondition = (1 - outlineCond) * step(fmod((IN.texcoord.y + 0.8 * _UTime) % 1.0f, 0.033333 * 0.6), 0.01 * 0.6);
                    color.rgb = color.rgb * (1 - lineInnerCondition) + lineInnerCondition * color.rgb * 0.75;



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
