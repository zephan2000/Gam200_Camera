Shader "Sprites/respawnShad"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HDR]_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_NoiseTex("Noise Tex", 2D) = "white" {}
		_Progress("Progress", Range(0, 1)) = 0
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
					float2 texcoordN : TEXCOORD1;
				};

				fixed4 _Color;

				float _Progress;
				sampler2D _MainTex, _NoiseTex;
				float4 _MainTex_ST, _NoiseTex_ST;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);

					OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
					OUT.texcoordN = TRANSFORM_TEX(IN.texcoord, _NoiseTex);
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
					float len = length(IN.texcoord - float2(0.5, 0.5));
					float val = tex2D(_NoiseTex, 
						//IN.texcoordN
						(1 - _Progress * (1 - clamp(smoothstep(0.4 * _NoiseTex_ST.x / 2, _NoiseTex_ST.x / 2, len), 0, 1))) * (IN.texcoordN - float2(_NoiseTex_ST.x / 2, _NoiseTex_ST.y / 2)) + float2(_NoiseTex_ST.x / 2, _NoiseTex_ST.y / 2)
					).r;
					clip(val - 0.1);
					fixed4 c =  fixed4(IN.color.rgb, tex2D(_MainTex, 
						IN.texcoord
					).a);
					c.a *= 1 - clamp(smoothstep(0.3, 0.8, val + abs(sin(_Progress * 3.14159 + 3.14159 * 0.5))), 0, 1);
					c.a *= 1 - clamp(smoothstep(0.1, 0.5, len), 0, 1);
					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}