Shader "Unlit/bg2Shader"
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
					float2 modthres = float2(1 / _DistortTex_ST.x, 1 / _DistortTex_ST.y) / pixelDivi;
					float2 moddedTexcoords = float2(IN.texcoordD.x - IN.texcoordD.x % modthres.x, IN.texcoordD.y - IN.texcoordD.y % modthres.y) + 0.5 * float2(modthres.x, modthres.y);
					
					//clip(step(length(IN.texcoordD - moddedTexcoords), 1 / (6 * pixelDivi)) - 0.01);

					fixed4 rawColor = tex2D(_DistortTex, IN.texcoordD);//moddedTexcoords);
					fixed4 rawColor2 = tex2D(_DistortTex, moddedTexcoords);
					fixed3 c = rawColor.rgb;//(1 - rawColor.rgb);
					fixed3 c2 = rawColor2.rgb;//(1 - rawColor.rgb);
					fixed4 truecolor = fixed4(c.r, c.g, c.b, rawColor.a);
					fixed4 truecolor2 = fixed4(c2.r, c2.g, c2.b, rawColor2.a);

					//left to right
					//float dist = length(IN.texcoord.xy - float2(_EyeRelativeX, _EyeRelativeY))
					//	+ 0.15 * sin(atan2(IN.texcoord.y, IN.texcoord.x));
					float dist = length(moddedTexcoords - float2(_EyeRelativeX, _EyeRelativeY));
						//+ 0.15 * sin(atan2(moddedTexcoords.y, moddedTexcoords.x));
					float loopingCoord = abs(dist - 0.5 * _Speed * _Time.y) % 1.5;
					float eyeHighlightCondition = step(0.5, _UsesEyeHighlight);

					float2 moddedTexRadius = moddedTexcoords - float2(0.5, 0.5);
					float funnyradian = atan2(moddedTexRadius.y, moddedTexRadius.x) / (3.1419);
					float loopstart = 0.15 + 0.1 * tex2D(_Distort2Tex, float2(0.5 + 0.5 * funnyradian, 0.1));
					float loopend = 0.65 + 0.1 * tex2D(_Distort2Tex, float2(0.5 + 0.5 * funnyradian, 0.9));

					float magValue = clamp(abs(loopingCoord - (loopstart + loopend) / 2) / (loopend - loopstart), 0, 1);

					//with placeholder (120 pixel)
					//float WithinCircleByteCondition = step(loopstart, loopingCoord) * step(loopingCoord, loopend) * step(0.1, truecolor.r) * step(0.1, truecolor2.r) * step(length(IN.texcoordD - moddedTexcoords), 1 / ((3 + 40 * pow(magValue, 2)) * pixelDivi));
					//without placeholder
					float WithinCircleByteCondition = step(loopstart, loopingCoord) * step(loopingCoord, loopend) * step(0.1, truecolor.r) * step(0.1, truecolor2.r) * step(length(IN.texcoordD - moddedTexcoords), 1 / ((3 + 40 * pow(magValue, 2)) * pixelDivi));
					
					//for eye version
					float EyeCond = step(length(IN.texcoord.xy - float2(0.5, 0.5)), 0.1);
					
					truecolor.rgb += 0 * EyeCond + (1 - EyeCond) * ((1 - WithinCircleByteCondition) * fixed3(0, 0, 0) //truecolor.rgb
						+ WithinCircleByteCondition * _MagicColor * (0.2 + 0.8 * step(truecolor.b, 0.1)));

					truecolor.rgb *= 0.5 * EyeCond + (1 - EyeCond) * (0.02
						+ 0.02 * WithinCircleByteCondition * step(0.5, c.r) * IN.color
						* pow(clamp(smoothstep(loopstart, (loopstart + loopend) / 2, loopingCoord), 0, 1), 1)
						* pow(1 - clamp(smoothstep((loopstart + loopend) / 2, loopend, loopingCoord), 0, 1), 1)
						//* clamp(smoothstep(0.1, 0.3, loopingCoord), 0, 1)
						//* (1 - clamp(smoothstep(0.3, 0.5, loopingCoord), 0, 1))
						+ eyeHighlightCondition * WithinCircleByteCondition * 0.015 * step(0.5, c.r) * IN.color
						* (1 - clamp(smoothstep(0.1, 0.18, dist), 0, 1))
						);

					float testcond2 = (1 - clamp(smoothstep(0.05, 0.1, length(IN.texcoord.xy - float2(0.5, 0.5))), 0, 1));
					truecolor.rgb = truecolor.rgb * (1 - EyeCond) + EyeCond * ( truecolor.rgb * testcond2);

				truecolor.rgb *= truecolor.a;
				return truecolor;
			}
		ENDCG
		}
		}
}
