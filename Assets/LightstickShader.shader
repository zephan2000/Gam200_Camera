Shader "Sprites/LightstickShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HDR] _Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_Progress("Progress", Range(0, 1)) = 1
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

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _Progress;

				v2f vert(appdata_t IN)
				{
					v2f OUT;

					float2 test = IN.texcoord - float2(0.5, 0.5);

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
					float toprad = length(float2(IN.texcoord.x, (IN.texcoord.y - 0.9) / 0.2 + 0.9) - float2(0.5, 0.9));
					clip(1 - step(0.9, IN.texcoord.y) * step(0.5, toprad) - 0.01);
					float botrad = length(float2(IN.texcoord.x, (IN.texcoord.y - 0.1) / 0.2 + 0.1) - float2(0.5, 0.1));
					clip(1 - step(IN.texcoord.y, 0.1) * step(0.5, botrad) - 0.01);

					float radius = length(IN.texcoord - float2(0.5, 0.5)) / 0.5;
					//float squareBorderValue = clamp(smoothstep(0.25, 0.3, abs(IN.texcoord.x - 0.5)), 0, 1)
					//	+ clamp(smoothstep(0.4, 0.45, abs(IN.texcoord.y - 0.5)), 0, 1);
					fixed4 c = tex2D(_MainTex, IN.texcoord) * _Color; //* clamp(radius * radius, 1, 2);
					c.a *= 1 - smoothstep(0, 0.5, abs(IN.texcoord.x - 0.5));
					c.a *= 1 - clamp(smoothstep(0.4, 0.5, abs(IN.texcoord.y - 0.5)), 0, 1);
					c.a *= _Progress;
					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}