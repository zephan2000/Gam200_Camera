Shader "Unlit/bgShader"
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _DistortTex;
			float4 _DistortTex_ST;

			sampler2D _Distort2Tex;
			float4 _Distort2Tex_ST;

			sampler2D _BGTex;
			float4 _BGTex_ST;

			float _ProgressLaugh;

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

			fixed4 frag(v2f IN) : SV_Target
			{

				//funny effect
				//IN.texcoordD.xy += tex2D(_Distort2Tex, IN.texcoordD2) * float2(-0.12 * _Time.y, -0.05 * _Time.y);

				fixed4 othercol = tex2D(_BGTex, IN.texcoordBG);
				othercol.rgb *= step(0.1, othercol.r);

				IN.texcoordD.xy += float2(-0.12 * _Time.y, -0.05 * _Time.y) + tex2D(_Distort2Tex, IN.texcoordD2).r;

				fixed4 c = tex2D(_DistortTex, IN.texcoordD) * IN.color;
			c.a *= tex2D(_DistortTex, IN.texcoordD).r;

			c.rgb *= 1
				- 0.2 * (1 - clamp(smoothstep(0.8, 0.9, c.a), 0, 1))
				- 0.3 * (1 - clamp(smoothstep(0.45, 0.65, c.a), 0, 1))
				- 0.49 * (1 - clamp(smoothstep(0.2, 0.35, c.a), 0, 1))
				;
			
			//replace that with (sin(_Time.y * 8) + 1)
			float thethe = c.g;
			//c.rgb *= 0;//step(0.25, abs(0.5 - IN.texcoord.x));

			//c.rgb += othercol.rgb * othercol.a * (step(0.98 - 0.98 * _ProgressLaugh, othercol.r)) 
			//	* step(0.4, tex2D(_Distort2Tex, IN.texcoordD2 - float2(0, 0.5 * _Time.x)))
			//	* (1 - step(0.5, thethe))
			//	;

			c.rgb *= IN.color * 0.01;

			c.rgb *= c.a;
			return c;
		}
	ENDCG
	}
	}
}
