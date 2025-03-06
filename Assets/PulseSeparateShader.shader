Shader "Unlit/PulseSeparateShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HDR]_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_Speed("Speed", Float) = 1
		_alphaMultiplier("Alpha Multiplier", Range(0, 1)) = 1
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

				fixed4 _Color;

				float _Speed, _alphaMultiplier;

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

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

					//left to right
					float dist = length(IN.texcoord.xy - float2(0.5, 0.5));
					float loopingCoord = abs(dist - 0.5 * _Speed * _Time.y) % 0.8;
					c.a *= (0.02 * step(0.5, c.r) * IN.color
						* clamp(smoothstep(0.35, 0.425, loopingCoord), 0, 1)
						* (1 - clamp(smoothstep(0.425, 0.5, loopingCoord), 0, 1))
						//* (clamp(smoothstep(0.2, 0.26, dist), 0, 1))
						* (1 - clamp(smoothstep(0.6, 0.8, (0.5 * _Speed * _Time.y) % 0.8), 0, 1))
						* (clamp(smoothstep(0.4, 0.5, (0.5 * _Speed * _Time.y) % 0.8), 0, 1))
						);

				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
		}
}
