Shader "Sprites/LaserShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HDR]_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_LaserColor("Laser Color", Color) = (1, 1, 1, 1)
		_LaserEndColor("Laser End Color", Color) = (1, 1, 1, 1)
			_LaserSpeed ("Laser Speed", Float) = 2
			_DistortTex("Distort Lightning Texture", 2D) = "white" {}
		_inter("Inter", Range(0, 0.5)) = 0.14
			_NEOValue("NEO Value", Range(0, 1)) = 0
		[HDR]_NEOColor("NEO Tint", Color) = (1,1,1,1)
			_alphaMag("Alpha Mag", Range(0, 1)) = 1
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

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
					float2 texcoordScaled  : TEXCOORD1;
					float2 texcoordD : TEXCOORD2;
				};

				fixed4 _Color, _LaserColor, _LaserEndColor;


				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _DistortTex;
				float4 _DistortTex_ST;

				fixed4 _NEOColor;
				float _NEOValue;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);

					OUT.texcoord = IN.texcoord;
					OUT.texcoordScaled = TRANSFORM_TEX(IN.texcoord, _MainTex);
					OUT.texcoordD = TRANSFORM_TEX(IN.texcoord, _DistortTex);
					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;
				float _DissolveExpo;
				float _LaserSpeed;
				float _inter;
				float _alphaMag;

				fixed4 frag(v2f IN) : SV_Target
				{
					float tanvalu = _Time.y * 1.23 * 3.14159 * 2;
					_inter += 0.02 * clamp(tan(tanvalu), -1, 1) * (1 - pow(abs(3.14159 - tanvalu % (2 * 3.14159))/ 3.14159, 3));
					float _Timey = _Time.y * _LaserSpeed;
					float TIMEY = _Timey - _Timey % _inter;

					fixed4 c =
					tex2D(_MainTex, float2(IN.texcoordScaled.x, IN.texcoordScaled.y - TIMEY ) * float2(1, 4)) * 0.2; //white streaks
					//* (1 - clamp(smoothstep(0.475, 0.5, abs(IN.texcoord.y - 0.5)), 0, 1)) * 0.2;
					c.rgb *= c.a;


					float toPointBlank = clamp(
						//smoothstep(0.4, 0.5, 
						//abs(IN.texcoord.y - 0.5))

						smoothstep(0.35, 0.5, length(
							(IN.texcoord - float2(0.5, 0.5)) * float2(1.2, 1)
						))
						
						, 0, 1);


					fixed4 trueLaserColor = fixed4(_LaserColor.rgb * (1 - _NEOValue) + _NEOColor.rgb * _NEOValue, _LaserColor.a);

					//DISTANCE THRESHOLD OF LASER IN TEXTURE
					//c += //(_LaserColor * (1 - toPointBlank) + _LaserEndColor * toPointBlank)
					//	_LaserColor
					//	* (1 - clamp(smoothstep(0.125, 0.3, abs(IN.texcoord.x - 0.5)), 0, 1))
					//;
					c += trueLaserColor
						* (1 - clamp(smoothstep(0.03, 0.07, abs(IN.texcoord.x - 0.5)), 0, 1))
						;

					//LIGHTNING DISTORTION
					clip(c.a - 0.001);
					float absval = abs(0.5 - IN.texcoord.x
						+ 0.2 * 
						(
							-step(0.5, IN.texcoord.x) * tan(3.1419 * (-3 * TIMEY + IN.texcoord.y + 0.4))
							+ step(IN.texcoord.x, 0.5) * tan(3.1419 * (-3 * TIMEY + IN.texcoord.y ))
						
							
						)
						* tex2D(_DistortTex, (IN.texcoordD - float2(0, TIMEY)) * float2(1 , 2))
					
					);
					c += trueLaserColor * 2 * clamp(smoothstep(0.10, 0.15, absval) * (1 - smoothstep(0.15, 0.2, absval)), 0, 1)
						
						;

					//c.rgb = (c.rgb * (1 - toPointBlank) + _LaserEndColor.rgb * toPointBlank);
					c.r = clamp(c.r, 0, 1);
					c.g = clamp(c.g, 0, 1);
					c.b = clamp(c.b, 0, 1);
					c *= IN.color;

					//c.a *= (1 - clamp(
					//	smoothstep(0.35, 0.5, length(
					//		(IN.texcoord - float2(0.5, 0.5)) * float2(0.5, 1)
					//	))
					//	, 0, 1));

					//float theCond = step(0.4, (IN.texcoord.y - 0.5));
					//float theCond2 = step(0.4, (0.5 - IN.texcoord.y));
					//c.a *= (1 - theCond) * (1 - theCond2)
					//	+ theCond *
					//		(1 - clamp(
					//		smoothstep(0.1, 0.125, length(
					//			(IN.texcoord - float2(0.5, 0.9)) * float2(0.45, 1)
					//		))
					//		, 0, 1))
					//	+ theCond2 *
					//	(1 - clamp(
					//		smoothstep(0.1, 0.125, length(
					//			(IN.texcoord - float2(0.5, 0.1)) * float2(0.45, 1)
					//		))
					//		, 0, 1))
					//	;



					//c.a *= (1 - clamp(
					//	smoothstep(0.475, 0.5, abs(
					//		(IN.texcoord.y - 0.5)
					//	))
					//	, 0, 1));

					c.a *= _alphaMag;

					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}