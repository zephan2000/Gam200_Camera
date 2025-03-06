Shader "Sprites/SummoningRingShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HDR]_Color("Tint", Color) = (1,1,1,1)
		[HDR]_TechColor("Tint Tech", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
			alphaMag("Alpha Mag", Range(0, 1)) = 1
			_TechTex("Tech Texture", 2D) = "white" {}
		_spd("Speed Revolving", Float) = 0.7
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
			//	 Ref 3
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
					float2 texcoordT  : TEXCOORD1;
				};

				fixed4 _Color, _TechColor;

				sampler2D _MainTex, _TechTex;
				float4 _MainTex_ST, _TechTex_ST;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);

					OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
					OUT.texcoordT = TRANSFORM_TEX(IN.texcoord, _TechTex);
					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;
				float _DissolveExpo;
				float _radThres, alphaMag, _spd;

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color * alphaMag;
					//float len = length(IN.texcoord - float2(0.5, 0.5));
					float len = length((IN.texcoord - float2(0.5, 0.5)) * float2(1, 1));
					float thres = 0.425;
					c.a *= 1 - clamp(smoothstep(0, 0.5 - thres, abs(thres - len)), 0, 1);

					float ang = atan2(IN.texcoord.y - 0.5, IN.texcoord.x - 0.5) / 3.141;

					fixed4 bigshot = tex2D(_TechTex, 
						//IN.texcoordT
					float2(ang + _spd *  _Time.y, len - thres / (0.5 - thres)) * float2(_TechTex_ST.x, _TechTex_ST.y)
					) * _TechColor * alphaMag;


					c += clamp(smoothstep(0, 0.2, c.a), 0, 1) * bigshot * bigshot.r;

					c.a = clamp(c.a, 0, 1);
					c.a *= alphaMag;
					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}