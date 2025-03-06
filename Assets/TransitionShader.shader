Shader "Unlit/TransitionShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Progress("Progress", Range(0, 1)) = 0
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

            Cull Off
            Lighting Off
            ZWrite Off
            Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Progress;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f IN) : SV_Target
            {   
                float2 modthres = float2(1 / (1920.0f / 1080.0f), 1) / 120;
                float2 moddedTexcoords = float2(IN.texcoord.x - IN.texcoord.x % modthres.x, IN.texcoord.y - IN.texcoord.y % modthres.y) + 0.5 * float2(modthres.x, modthres.y);
                float thelen = length(float2(1920.0f / 1080.0f, 1) * (float2(0.5, 0.5) - moddedTexcoords));

                _Progress = _Progress * 1.5 - 0.5;

                float pixelVal = thelen - 1.05 * _Progress - 0.01 * step(0.001, _Progress);
                clip(pixelVal);

                // sample the texture
                fixed4 col = fixed4(0, 0, 0, 1) * (1 - step(pixelVal, 0.5)) + step(pixelVal, 0.5) * fixed4(0, 0, 0, 1);
                col.a *= clamp((pixelVal - pixelVal % 0.1 + 0.1) * 2, 0, 1);

                col.rgb *= col.a;

                return col;
            }
            ENDCG
        }
    }
}
