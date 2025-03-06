Shader "Sprites/glowballSha4"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HDR]_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
			alphaMag("Alpha Mag", Range(0, 1)) = 1
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
				 Ref 3
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

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
				};

				fixed4 _Color, _ColorB;

				sampler2D _MainTex;
				float4 _MainTex_ST;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);

					OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;
				float _DissolveExpo;
				float _radThres, alphaMag;

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 c = tex2D(_MainTex, (1 + 0.08 * (1 - alphaMag)) * (IN.texcoord - float2(0.5, 0.5)) + float2(0.5, 0.5)) * IN.color * alphaMag;
					float len = length(IN.texcoord - float2(0.5, 0.5));
					c *=
						//0.4 * (1 - pow(clamp(smoothstep(0.15, 0.5, len), 0, 1), 1))
						0.2 * (1 - pow(clamp(smoothstep(0.3, 0.5, len), 0, 1), 1))
						+
						0.2 * (1 - clamp(smoothstep(0.15, 0.3, len), 0, 1))
						+
						0.2 * (1 - clamp(smoothstep(0.05, 0.15, len), 0, 1))
					;

					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}