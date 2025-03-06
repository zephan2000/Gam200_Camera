Shader "Unlit/bgEyeShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HDR]_BaseColor("Base Color", Color) = (0.1,0.1,0.1,1)
		[HDR]_Color("Tint", Color) = (1,1,1,1)
		[HDR]_EyeColor("Eye Color", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_BGTex("BG Texture", 2D) = "white" {}
		_IrisTex("Iris Texture", 2D) = "white" {}
		_eyeX("Eye X", Range(0, 1)) = 0.5
		_eyeY("Eye Y", Range(0, 1)) = 0.5
		_eyeRadius("Eye Radius", Range(0.00, 0.5)) = 0.2
		_pixelMultiplier("Pixel Multiplier", Float) = 1
		_alphaMultiplier("Alpha Multiplier", Range(0, 1)) = 1
		_InnerEyeMultiplier("Inner Eye Multiplier", Range(0.75, 1.5)) = 1
		_InnerEyeOffsetX("Inner Eye Offset X", Range(-1, 1)) = 0
		_InnerEyeOffsetY("Inner Eye Offset Y", Range(-1, 1)) = 0
		_TalkProgress("Talk Progress", Range(0, 1)) = 0
		_eyeCol("Eye Color Real", Color) = (1, 1, 1, 1)
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
					float2 texcoordBG  : TEXCOORD1;
					float2 texcoordIris  : TEXCOORD2;
				};

				fixed4 _Color;

				float _alphaMultiplier;

				sampler2D _MainTex;
				float4 _MainTex_ST;

				float _InnerEyeOffsetX, _InnerEyeOffsetY, _TalkProgress;

				sampler2D _BGTex;
				float4 _BGTex_ST;
				float4 _BaseColor, _EyeColor;

				sampler2D _IrisTex;
				float4 _IrisTex_ST;

				float _eyeX, _eyeY, _eyeRadius;

				float _pixelMultiplier;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);

					_BGTex_ST.xy *= _pixelMultiplier;

					OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
					OUT.texcoordBG = TRANSFORM_TEX(IN.texcoord, _BGTex);
					OUT.texcoordIris = TRANSFORM_TEX(IN.texcoord, _IrisTex);
					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;
				fixed4 _eyeCol;
				float _InnerEyeMultiplier;

				fixed4 frag(v2f IN) : SV_Target
				{
					//_TalkProgress = ( 2.8 * _Time.y) % 1;
					_eyeRadius *= (1 + 1.2 * (0.5 - abs(0.5 - _TalkProgress))); //0.3 * sin(_TalkProgress * 3.14159265));

					float2 modthres = float2(1 / _BGTex_ST.x, 1 / _BGTex_ST.y) / _pixelMultiplier;
					float2 moddedTexcoords = float2(IN.texcoord.x - IN.texcoord.x % modthres.x, IN.texcoord.y - IN.texcoord.y % modthres.y) + 0.5 * float2(modthres.x, modthres.y);
					float distanceCenter = length(IN.texcoord - float2(0.5, 0.5)); //0.0 - 0.5;
					float moddedDistanceCenter = length(moddedTexcoords - float2(0.5, 0.5));
					clip(0.5 - distanceCenter);

					float2 eyeCoordinates = float2(_eyeX, _eyeY);
					float2 eyebrowCoordinates = float2(0.5, 0.8) + float2(0.3 * (eyeCoordinates.x - 0.5), 0.1 * (eyeCoordinates.y - 0.5));
					float moddedDistanceCenterFromEye = length(moddedTexcoords - eyeCoordinates);
					float moddedDistanceCenterFromEyebrow = length((moddedTexcoords - eyebrowCoordinates) * float2(0.8, 1.5));

					float ConditionToBeEye = step(0.01, _eyeRadius) * clamp(smoothstep(moddedDistanceCenterFromEye - 0.02, moddedDistanceCenterFromEye + 0.02, _eyeRadius), 0, 1);
					float ConditionToBeEyeCenter = step(0.01, _eyeRadius) * clamp(smoothstep(moddedDistanceCenterFromEye - 0.01, moddedDistanceCenterFromEye + 0.01, 0.5 * _eyeRadius), 0, 1);

					float ConditionToBeEyeRing = step(0.01, _eyeRadius) 
						* clamp(smoothstep(moddedDistanceCenterFromEye - 0.02, moddedDistanceCenterFromEye + 0.02, 1.5 * _TalkProgress), 0, 1)
						* (1 - clamp(smoothstep(moddedDistanceCenterFromEye - 0.02, moddedDistanceCenterFromEye + 0.02, 0.8 * _TalkProgress), 0, 1))
						;
					
					//step(moddedDistanceCenterFromEye, _eyeRadius);
					float ConditionToBeEyebrow = clamp(smoothstep(moddedDistanceCenterFromEyebrow, moddedDistanceCenterFromEyebrow + 0.02, 0.12), 0, 1);

					fixed4 c = _BaseColor;
					c += (1 - ConditionToBeEyebrow) * (1 - ConditionToBeEye) * tex2D(_BGTex, IN.texcoordBG) * IN.color * step(0.1, tex2D(_BGTex, IN.texcoordBG).r)
						* (1 - pow(clamp(smoothstep(0.25, 0.45, moddedDistanceCenter), 0, 1), 1.5))
						* (1 - pow(clamp(smoothstep(_eyeRadius * 1.2, _eyeRadius * 2, moddedDistanceCenterFromEye), 0, 0.5), 1.5))
						* (1 + ConditionToBeEyeRing * 0.9 * (1 - pow(clamp(smoothstep(0.25, 0.45, moddedDistanceCenter), 0, 1), 1.5)))
						+ (1 - ConditionToBeEyebrow) * ConditionToBeEye * _EyeColor * (1 - pow(clamp(smoothstep(0.25, 0.45, moddedDistanceCenter), 0, 1), 1.5)) * IN.color.a
						+ ConditionToBeEyebrow * _EyeColor * (1 - pow(clamp(smoothstep(0.25, 0.45, moddedDistanceCenter), 0, 1), 1.5)) * IN.color.a //* step(moddedDistanceCenter, 0.45)
						+ ConditionToBeEyeCenter
						//+ (1 - ConditionToBeEyebrow) * ConditionToBeEyeRing * 0.2 * (1 - pow(clamp(smoothstep(0.25, 0.45, moddedDistanceCenter), 0, 1), 1.5))
						;

					float2 trueTexcoordIris = ((12 - 14 * _eyeRadius) / _InnerEyeMultiplier) * (IN.texcoordIris - eyeCoordinates) + eyeCoordinates - 0.1 * float2(_InnerEyeOffsetX, _InnerEyeOffsetY);
					fixed4 irisTex = tex2D(_IrisTex, trueTexcoordIris);


					float UnmoddedDistanceCenterFromEye = length(IN.texcoord - eyeCoordinates);
					float ConditionToBeEyeUnmodded = clamp(smoothstep(UnmoddedDistanceCenterFromEye - 0.02, UnmoddedDistanceCenterFromEye + 0.02, _eyeRadius), 0, 1);

					c.rgb *= (1 - ConditionToBeEye) + ConditionToBeEye * irisTex.rgb * irisTex.a * _alphaMultiplier * _alphaMultiplier
						* step(abs(trueTexcoordIris.x - 0.5), 0.5) * step(abs(trueTexcoordIris.y - 0.5), 0.5)
						* _eyeCol;
					// step(3.147 * 7, (_Time.y * 4 + 0.5 * 3.147) % (3.147 * 8)) * 3 * (abs(sin(_Time.y * 4)))

					//c.rgb += ConditionToBeEyeUnmodded * irisTex.rgb * irisTex.a
					//	* step(abs(trueTexcoordIris.x - 0.5), 0.5) * step(abs(trueTexcoordIris.y - 0.5), 0.5);

					c.a *= 0.7f;

				c.rgb *= c.a;

				c.rgb += 0.003 * step(c.r, 0.01) * fixed3(1, 1, 1);

				c.a = 1 * _alphaMultiplier;
				c.rgb *= _alphaMultiplier;
				return c;
			}
		ENDCG
		}
		}
}
