Shader "Sprites/PlayerBallShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_Progress("Progress", Range(0, 1)) = 0
		_ProgressX("Progress X", Range(0, 1)) = 0
		_ModX("Mod X", Float) = 0.5
		_ModY("Mod Y", Float) = 0.5
		_RespawnProgress("Respawn Progress", Range(0, 1)) = 1
		_RespawnColor("Respawn Tint", Color) = (1,1,1,1)
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
				float _FlipRight;


				sampler2D _MainTex;
				float4 _MainTex_ST;

				float _Progress, _ModX, _ModY, _ProgressX;
				float _RespawnProgress, _VerticalStretch, _IdleProgress, _RunningTimer;

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
				fixed4 _RespawnColor;

				fixed4 frag(v2f IN) : SV_Target
				{
					float mag = 0.12; //0.06
					float val = step(0.5, _FlipRight) * (1 + (1 - _FlipRight) * mag)
					+ (1 - step(0.5, _FlipRight)) * (0 - _FlipRight * mag)
					;
					fixed4 c = tex2D(_MainTex, float2( ///RIGHT TO LEFT -> 1 to 1.03 step -0.03 to 0
						(1 - IN.texcoord.x) * (1 - val) + IN.texcoord.x * val
						, IN.texcoord.y)) * IN.color;
					float smoothcol = clamp(smoothstep(0.5, 1, _RespawnProgress), 0, 1);
					c.rgb = c.rgb * smoothcol + (1 - smoothcol) * _RespawnColor;
					c.a *= clamp(smoothstep(0, 0.5, _RespawnProgress), 0, 1);

					//c.rgb *= 1 + step(0.3, c.r);
					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}