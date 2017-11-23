// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Ocias/Pixel Art Particle" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	AlphaTest Greater .01
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD2;
				#endif
			};
			
			float4 _MainTex_ST;

			inline float4 ViewSpacePixelSnap (float4 pos) {

				float2 halfScreenRes = _ScreenParams.xy * 0.5f;

				// // View space Pixel Snapping
				float2 pixelPos = floor(pos * halfScreenRes + 1 / halfScreenRes) / halfScreenRes; // put back in that half pixel offset when you're done
				pos.xy = pixelPos;
				
				// Odd resolution handling
				float2 odd = _ScreenParams.xy % 2;
				pos.x += odd.x * 0.5 / halfScreenRes.x;
				pos.y += odd.y * 0.5 / halfScreenRes.y;

				return pos;
			}

			v2f vert (appdata_t v)
			{
				v2f o;

				// Offset position based on distance from camera to nearest pixel
				float zoom = 1/(unity_CameraProjection[1][1]);
				float ppu = _ScreenParams.y / zoom / 2;
				float3 snappedCameraPosition = floor(_WorldSpaceCameraPos * ppu + 1 / ppu) / ppu;
				float3 cameraSubpixelOffset = snappedCameraPosition - _WorldSpaceCameraPos;
				v.vertex.x -= cameraSubpixelOffset.x;
				v.vertex.y -= cameraSubpixelOffset.y;

				o.vertex = UnityObjectToClipPos(v.vertex);
				


				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);

				o.vertex = ViewSpacePixelSnap(o.vertex);

				return o;
			}

			sampler2D_float _CameraDepthTexture;
			float _InvFade;
			
			fixed4 frag (v2f i) : SV_Target
			{
				#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
				float partZ = i.projPos.z;
				float fade = saturate (_InvFade * (sceneZ-partZ));
				i.color.a *= fade;
				#endif
				
				fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG 
		}
	}	
}
}
