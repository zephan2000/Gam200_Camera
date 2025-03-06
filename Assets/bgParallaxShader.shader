Shader "Unlit/bgParallaxShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HDR]_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_BGTex("BG Texture", 2D) = "white" {}
		_ParallaxXRate("Parallax X Rate", Float) = 1
		_ParallaxYRate("Parallax Y Rate", Float) = 1
		_XCoord("X Coord Sampling", Float) = 0
		_YCoord("Y Coord Sampling", Float) = 0
		_TrafficSpeed("Traffic Speed", Float) = 1
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
				};

				fixed4 _Color;

				float _ParallaxXRate, _ParallaxYRate;

				float _XCoord, _YCoord;

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _TrafficSpeed;

				sampler2D _BGTex;
				float4 _BGTex_ST;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);

					_BGTex_ST.z += _XCoord * _ParallaxXRate + _TrafficSpeed * _Time.y;
					_BGTex_ST.w += _YCoord * _ParallaxYRate;

					OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
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
					fixed4 c = tex2D(_BGTex, IN.texcoordBG) * IN.color;
					c.rgb *= c.a;
					return c;
			}
		ENDCG
		}
		}
}
