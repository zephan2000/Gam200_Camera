Shader "Unlit/bg2OLDShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HDR]_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_BGTex("BG Texture", 2D) = "white" {}
		_DistortTex("Distort Texture", 2D) = "white" {}
		_Distort2Tex("Distort2 Texture", 2D) = "white" {}
		_ProgressLaugh("Progress Laugh", Range(0, 1)) = 0
		_EyeRelativeX("Eye Relative X", Range(0, 1)) = 0.5
		_EyeRelativeY("Eye Relative Y", Range(0, 1)) = 0.5
		_Speed("Speed", Float) = 1
		[HDR] _MagicColor("Magic Color", Color) = (1, 1, 1, 1)
			_UsesEyeHighlight("Uses Eye Highlight", Range(0, 1)) = 1
			pixelDivi ("Pixel Divi", Float)= 120
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
					float2 texcoordD  : TEXCOORD1;
					float2 texcoordD2  : TEXCOORD2;
					float2 texcoordBG  : TEXCOORD3;
				};

				fixed4 _Color;

				float _Speed;

				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _DistortTex;
				float4 _DistortTex_ST;

				sampler2D _Distort2Tex;
				float4 _Distort2Tex_ST;

				sampler2D _BGTex;
				float4 _BGTex_ST;

				fixed4 _MagicColor;

				float _ProgressLaugh, _EyeRelativeX, _EyeRelativeY, _UsesEyeHighlight;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);

					OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
					OUT.texcoordD = TRANSFORM_TEX(IN.texcoord, _DistortTex);
					OUT.texcoordD2 = TRANSFORM_TEX(IN.texcoord, _Distort2Tex);
					OUT.texcoordBG = TRANSFORM_TEX(IN.texcoord, _BGTex);
					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;
				float pixelDivi;

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 rawColor = tex2D(_DistortTex, IN.texcoordD);
					fixed3 c = (1 - rawColor.rgb);
					fixed4 truecolor = fixed4(c.r, c.g, c.b, rawColor.a);

					//left to right
					float dist = length(IN.texcoord.xy - float2(_EyeRelativeX, _EyeRelativeY))
						+ 0.15 * sin(atan2(IN.texcoord.y, IN.texcoord.x));
					float loopingCoord = abs(dist - 0.5 * _Speed * _Time.y) % 1.5;
					float eyeHighlightCondition = step(0.5, _UsesEyeHighlight);

					float loopstart = 0.1;
					float loopend = 0.5;

					truecolor.rgb *= (0.02
						+ 0.02 * step(0.5, c.r) * IN.color
						* pow(clamp(smoothstep(loopstart, (loopstart + loopend) / 2, loopingCoord), 0, 1), 1)
						* pow(1 - clamp(smoothstep((loopstart + loopend) / 2, loopend, loopingCoord), 0, 1), 1)
						+ 0.015 * step(0.5, c.r) * IN.color
						* (1 - clamp(smoothstep(0.1, 0.18, dist), 0, 1))
						);

				truecolor.rgb *= truecolor.a;
				return truecolor;
			}
		ENDCG
		}
		}
}
