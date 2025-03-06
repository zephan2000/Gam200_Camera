/*
Shader Classification: Jump Cloud Particle Shader

Simple Shader designed to produce an irregularly-shaped circular dust particle. I ended up not using most of the declared variables.

Programmed by: Jovan Low Zhuo Wen
*/

Shader "Particles/JumpCloud_Shader" {
    Properties{
        _TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
        _MainTex("Particle Texture", 2D) = "white" {}
        _InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _Progress("Progress", Range(0, 1)) = 0
        [HDR]_NewColor("New Color", Color) = (1, 1, 1, 1)
    }

        Category{
            Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
            Blend One OneMinusSrcAlpha
            ColorMask RGB
            Cull Off Lighting Off ZWrite Off

            SubShader {
                Pass {

                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma target 2.0
                    #pragma multi_compile_particles
                    #pragma multi_compile_fog

                    #include "UnityCG.cginc"

                    sampler2D _MainTex, _NoiseTex;
                    fixed4 _TintColor;

                    struct appdata_t {
                        float4 vertex : POSITION;
                        fixed4 color : COLOR;
                        float4 texcoord : TEXCOORD0;
                        fixed4 Custom1 : TEXCOORD1;
                        UNITY_VERTEX_INPUT_INSTANCE_ID
                    };

                    struct v2f {
                        float4 vertex : SV_POSITION;
                        fixed4 color : COLOR;
                        float2 texcoord : TEXCOORD0;
                        UNITY_FOG_COORDS(1)
                        #ifdef SOFTPARTICLES_ON
                        float4 projPos : TEXCOORD2;
                        #endif
                        float4 worldvert : TEXCOORD3;
                        fixed4 Custom1 : TEXCOORD4;
                        UNITY_VERTEX_OUTPUT_STEREO
                    };

                    float4 _MainTex_ST;
                    float4 _NoiseTex_ST;
                    float _Progress;

                    v2f vert(appdata_t v)
                    {
                        v2f o;
                        UNITY_SETUP_INSTANCE_ID(v);
                        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                        o.Custom1 = fixed4(v.texcoord.zw,v.Custom1.xy); //This is seriously how Unity's Particle Systems input vertex stream data...
                        o.worldvert = mul(unity_ObjectToWorld, v.vertex);
                        //v.vertex = lerp(v.vertex, mul(unity_WorldToObject, float3(0, 5, 0)), v.Custom1.z); //testing
                        o.vertex = UnityObjectToClipPos(v.vertex);
                        #ifdef SOFTPARTICLES_ON
                        o.projPos = ComputeScreenPos(o.vertex);
                        COMPUTE_EYEDEPTH(o.projPos.z);
                        #endif
                        o.color = v.color;
                        o.texcoord = TRANSFORM_TEX(v.texcoord.xy,_MainTex);
                        UNITY_TRANSFER_FOG(o,o.vertex);
                        return o;
                    }

                    UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
                    float _InvFade;

                    fixed4 _NewColor;

                    fixed3 mapRGB(float value, fixed3 min1, fixed3 max1, fixed3 min2, fixed3 max2) {
                        return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
                    }

                    fixed4 frag(v2f i) : SV_Target
                    {
                        float2 vec = i.texcoord - float2(0.5, 0.5);
                        float len = length(vec);
                        float angle = atan2(vec.y, vec.x);
                        float thres = 0.1 * tex2D(_NoiseTex, 0.5* (cos(0.5*angle) * 0.5 + 0.5)) + 0.4;
                        clip(thres - len);

                        #ifdef SOFTPARTICLES_ON
                        float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                        float partZ = i.projPos.z;
                        float fade = saturate(_InvFade * (sceneZ - partZ));
                        i.color *= fade;
                        #endif

                        float val = 1 - clamp(smoothstep(thres * 0.3, 0.95*thres, len), 0, 1);

                        fixed4 tex = tex2D(_MainTex, i.texcoord) * _NewColor; //+ 0.025 * clamp(smoothstep(0.3, 1, val), 0, 1);
                        fixed4 col;
                        col.rgb = _TintColor.rgb * i.color.rgb * 2.0f * tex.rgb;
                        col.a = (1 - tex.a) * (_TintColor.a * i.color.a * 2.0f);

                        col.rgb *= val;
                        col.a *= val;

                        float trueval = clamp(smoothstep(0, 0.2, tex2D(_NoiseTex, i.texcoord) - (1 - _Progress)) , 0, 1);
                        
                        float hasCustom1 = clamp(step(0.01, i.Custom1.r) + step(0.01, i.Custom1.g) + step(0.01, i.Custom1.b), 0, 1);
                        col.rgb *= (1 - hasCustom1) + hasCustom1 * i.Custom1.rgb * 1.2f; //As you know, we do not use booleans in shaders, so we take the liberty of using step as a substitute.

                        col.rgb = (1 - _Progress) * col.rgb;
                        col.a = (1 - _Progress) * col.rgb;

                        UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0));
                        return col;
                    }
                    ENDCG
                }
            }

        }
}