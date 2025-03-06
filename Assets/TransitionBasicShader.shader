Shader "Unlit/TransitionBasicShader"
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

            fixed4 frag(v2f IN) : SV_Target
            {
                // sample the texture
                fixed4 col = fixed4(0, 0, 0, 1 - _Progress);
                col.rgb *= col.a;

                return col;
            }
            ENDCG
        }
    }
}
