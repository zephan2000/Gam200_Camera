Shader "Unlit/blockshader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[HDR]_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0

		_DisintegrationProgress("Disintegration Progress", Range(0, 1)) = 0
		_DisintegrationNoiseTex("Disintegration Noise Texture", 2D) = "white" {}
		_DissolveColor("Dissolve Color", Color) = (0, 0, 0, 1)
		_DissolveExpo("Dissolve Expo", Float) = 0.8
		_DissolveExplodeScaleX("Dissolve Explode Scale X", Float) = 1.5
		_DissolveExplodeScaleY("Dissolve Explode Scale Y", Float) = 1.5
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

				fixed4 _DissolveColor;
				float _DisintegrationProgress;
				sampler2D _DisintegrationNoiseTex;
				float4 _DisintegrationNoiseTex_ST;

				float _DissolveExplodeScaleX, _DissolveExplodeScaleY;

				sampler2D _MainTex;
				float4 _MainTex_ST;

				v2f vert(appdata_t IN)
				{
					v2f OUT;

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

				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);



	#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, uv).r;
	#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

					return color;
				}
				fixed4 frag(v2f IN) : SV_Target
				{

					fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}