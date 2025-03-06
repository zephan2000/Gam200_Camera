Shader "Unlit/arrowShader"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _TintA("Tint A", Color) = (1, 1, 1, 1)
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
                    float2 uv : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed4 _TintA;
                float _Bounce;

                float _UTime;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    i.uv.y *= 2.4;
                i.uv.y -= 0.75;
                i.uv.y += 0.75 * pow(abs(cos(1.5 * _Bounce * _UTime * 3.141)), 1);

                clip(i.uv.y);

                    fixed4 col = tex2D(_MainTex, i.uv) * _TintA;

                    col.rgb *= col.a;
                    return col;
                }
                ENDCG
            }
        }
}
