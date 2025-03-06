Shader "Sprites/deathballShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[HDR]_ColorB("Tint B", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
			_radThres("Radius Threshold", Range(0, 0.5)) = 0.45
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

				fixed4 _Color, _ColorB;

				sampler2D _MainTex;
				float4 _MainTex_ST;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);

					OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
					OUT.color = IN.color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;
				float _DissolveExpo;
				float _radThres;

				fixed4 frag(v2f IN) : SV_Target
				{
					float len = length(IN.texcoord - float2(0.5, 0.5));
					fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color * (_Color * step(len, _radThres) + _ColorB * (1 - step(len, _radThres)));
					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}