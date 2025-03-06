Shader "Unlit/WindGlitchShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_WindTex("Wind Texture", 2D) = "white" {}
		_WindThreshold("Wind Threshold", Range(0, 1)) = 0.4
		_DistortTex("Distort Texture", 2D) = "white" {}
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
				float2 texcoordW  : TEXCOORD1;
				float2 texcoordD  : TEXCOORD2;
			};

			fixed4 _Color;

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _WindTex;
			float4 _WindTex_ST;
			sampler2D _DistortTex;
			float4 _DistortTex_ST;

			float _WindThreshold;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);

				OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
				OUT.texcoordW = TRANSFORM_TEX(IN.texcoord, _WindTex);
				OUT.texcoordD = TRANSFORM_TEX(IN.texcoord, _DistortTex);
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

				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

			//Glitch Effect I discovered!
			float distortion = tex2D(_DistortTex, IN.texcoordD + float2(_Time.y * 0.3, 0)) ; //0 - 1
			distortion -= 0.5; //-0.5 - 0.5
			distortion *= 0.5; //-0.25 + 0.25
			float windValue = tex2D(_WindTex, IN.texcoordW - float2(0, _Time.y)).r + distortion;
			clip(windValue - _WindThreshold);
			c.a *= smoothstep(_WindThreshold, 1.0, windValue);
			float theval = 0.15 * (1 - pow(clamp(smoothstep(0.0, 0.8, IN.texcoordW.y), 0, 1), 2));

			float absval = abs(0.5 - (IN.texcoordW.x - float2(0, _Time.y)));
			float otherval = clamp(smoothstep(0.4, 0.5, absval), 0, 1);
			c.a += otherval;
			otherval = clamp(smoothstep(0.225, 0.25, absval) * (1 - smoothstep(0.25, 0.275, absval)), 0, 1);
			c.a += otherval;

			c.rgb *= c.a;
			return c;
		}
	ENDCG
	}
	}
}
