Shader "Unlit/WindNewShader"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        _WindTex("Wind Texture", 2D) = "white" {}
        _WindThreshold("Wind Threshold", Range(0, 1)) = 0.4
        _DistortTex("Distort Texture", 2D) = "white" {}
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

            Stencil
             {
                 Ref 2
                 Comp NotEqual
             }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord  : TEXCOORD0;
                float2 texcoordW  : TEXCOORD1;
                float2 texcoordD  : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _WindTex;
            float4 _WindTex_ST;
            sampler2D _DistortTex;
            float4 _DistortTex_ST;

            float _WindThreshold;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);

                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.texcoordW = TRANSFORM_TEX(IN.texcoord, _WindTex);
                OUT.texcoordD = TRANSFORM_TEX(IN.texcoord, _DistortTex);
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _AlphaTex;
            float _AlphaSplitEnabled;

            fixed4 frag (v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                //PART A: BODY
                float thresholdA = 0.4;
                float thresholdB = 0.95;
                c.a *= 0.25 * (1 - clamp(smoothstep(thresholdA - 0.2, thresholdA + 0.05, IN.texcoord.y + 0.5 * pow(abs(0.5 - IN.texcoord.x), 2.5)), 0, 1))
                    + 0.15 * (1 - clamp(smoothstep(thresholdB - 0.2, thresholdB + 0.05, IN.texcoord.y + 0.5 * pow(abs(0.5 - IN.texcoord.x), 3)), 0, 1))
                    ;
                c.a *= (1 - pow(clamp(smoothstep(0.2, 0.5, abs(0.5 - IN.texcoord.x)), 0, 1), 1.4));
                c.a *= (clamp(smoothstep(0, 0.01, IN.texcoord.y), 0, 1));

                //PART B: Glitchy Effects!
                fixed4 contentC = tex2D(_MainTex, IN.texcoord) * IN.color;
                float distortion = tex2D(_DistortTex, IN.texcoordD + float2(_Time.y * 0.3, 0)); //0 - 1
                distortion -= 0.5; //-0.5 - 0.5
                distortion *= 0.5; //-0.25 + 0.25
                float2 windCoord = IN.texcoordW - float2(0, _Time.y);
                float windValue = tex2D(_WindTex, windCoord).r + distortion;
                //contentC.a *= step(_WindThreshold, windValue);
                float killValue = step(_WindThreshold, windValue);
                contentC.a *= killValue * smoothstep(_WindThreshold, 1.0, windValue);
                float theval = 0.15 * (1 - pow(clamp(smoothstep(0.0, 0.8, IN.texcoordW.y), 0, 1), 2));

                float absval = abs(0.5 - (IN.texcoordW.x - float2(0, _Time.y)));
                float otherval = clamp(smoothstep(0.43, 0.5, absval), 0, 1);
                contentC.a += killValue * otherval; //adds edges
                otherval = clamp(smoothstep(0.225, 0.25, absval) * (1 - smoothstep(0.25, 0.275, absval)), 0, 1);
                contentC.a += 0.5 * killValue * otherval;

                //PART C: Glitchy Center
                otherval = clamp(smoothstep(0, 0.0, absval) * (1 - smoothstep(0.005, 0.02, absval)), 0, 1);
                contentC.a += killValue * otherval;

                //REMOVE THIS IF WE GET A LOOPING TEXTURE
                contentC.a *= (1 - clamp(smoothstep(0.4, 0.49, abs(0.5 - ((abs(IN.texcoord.y - 0.49 * _Time.y) * 2.04 - 0.02) % 1))), 0, 1))
                    ;

                contentC.a *= (1 - pow(clamp(smoothstep(0.25, 0.5, abs(0.5 - IN.texcoord.x)), 0, 1), 1.4));
                contentC.a *= (1 - clamp(smoothstep(thresholdB - 0.25, thresholdB - 0.05, IN.texcoord.y + 0.5 * pow(abs(0.5 - IN.texcoord.x), 2)), 0, 1))
                    ;
                c.a += contentC.a;

                c.a = clamp(c.a, 0, 1);

                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
