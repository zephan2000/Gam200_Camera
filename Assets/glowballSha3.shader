Shader "Sprites/glowballSha3"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HDR]_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
			alphaMag("Alpha Mag", Range(0, 1)) = 1
		_Progress("Progress", Range(0, 1)) = 0
		_ProgressX("Progress X", Range(0, 1)) = 0
		_ModX("Mod X", Float) = 0.5
		_ModY("Mod Y", Float) = 0.5
		_FlipRight("Flip Right", Range(0, 1)) = 1
		_VerticalStretch("Vertical Stretch", Range(0, 1)) = 0
		_IdleProgress("Idle Progress", Range(0, 1)) = 0
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

			//Stencil
			// {
			//	 Ref 2
			//	 Comp NotEqual
			// }

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

				float _FlipRight;
				float _RespawnProgress, _VerticalStretch, _IdleProgress;
				float _Progress, _ModX, _ModY, _ProgressX;
				float _RunningTimer;

				v2f vert(appdata_t IN)
				{
					v2f OUT;

					float2 test2 = IN.texcoord - float2(0.5, _VerticalStretch);
					IN.vertex.xy += test2 * float2(0, _Progress * _ModY + _IdleProgress * 0.06 * cos(pow((0.65 * _RunningTimer) % 1.0, 0.9) * 3.1415 * 2 + 3.1415));

					float2 test3 = IN.texcoord - float2(0 + 1 - (1 - _FlipRight), 0.5);
					IN.vertex.xy += test3 * float2(_ProgressX * _ModX, 0);

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
					float mag = 0.12; //0.06
					float val = step(0.5, _FlipRight) * (1 + (1 - _FlipRight) * mag)
					+ (1 - step(0.5, _FlipRight)) * (0 - _FlipRight * mag)
					;

					IN.texcoord = float2( ///RIGHT TO LEFT -> 1 to 1.03 step -0.03 to 0
						(1 - IN.texcoord.x) * (1 - val) + IN.texcoord.x * val
						, IN.texcoord.y);

					fixed4 c =  fixed4(IN.color.rgb, tex2D(_MainTex, IN.texcoord).a);
					//clip(c.a - 0.999);
					c *= alphaMag;
					float2 smallTexcoords = 1.25 * (IN.texcoord - float2(0.5, 0.5)) + float2(0.5, 0.5);
					float len = length(float2(1, 1.5) * (float2(0.5, 0.44) - IN.texcoord));
					float cond = step(tex2D(_MainTex, smallTexcoords).a, 0.2);
					float condLen = 1 - clamp(smoothstep(0.25, 0.6, len), 0, 1);
					c.a *= (1 - cond);// + cond * 0.1 * condLen;
					c.rgb *= 4;
					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}