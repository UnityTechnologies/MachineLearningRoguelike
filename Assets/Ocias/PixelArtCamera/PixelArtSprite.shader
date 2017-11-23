// Version 1.0
// By Alexander Ocias
// https://ocias.com

Shader "Ocias/Pixel Art Sprite"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
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
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;
			float _PixelsPerUnit;

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

			// inline float4 WorldSpacePixelSnap (float4 pos) {

			// 	//float ppu = _PixelsPerUnit;
			// 	float zoom = 1/(unity_CameraProjection[1][1]);
			// 	float ppu = _ScreenParams.y / zoom / 2;

			// 	pos = mul(unity_ObjectToWorld, pos);
				
			// 	// World space Pixel Snapping
			// 	pos = floor(pos * ppu + 1 / ppu) / ppu;
			// 	// Adjust to pixel relative to camera position
			// 	float3 snappedCameraPosition = floor(_WorldSpaceCameraPos * ppu + 1 / ppu) / ppu;
			// 	float3 cameraSubpixelOffset = snappedCameraPosition - _WorldSpaceCameraPos;
			// 	pos.x -= cameraSubpixelOffset.x;
			// 	pos.y -= cameraSubpixelOffset.y;
			// 	// Odd resolution handling
			// 	float2 odd = _ScreenParams.xy % 2;
			// 	pos.x += odd.x * 0.5 / ppu;
			// 	pos.y += odd.y * 0.5 / ppu;

			// 	pos = mul(unity_WorldToObject, pos);

			// 	return pos;
			// }

			

			v2f vert(appdata_t IN)
			{
				v2f OUT;

				//IN.vertex = WorldSpacePixelSnap(IN.vertex);

				// Offset position based on distance from camera to nearest pixel
				float zoom = 1/(unity_CameraProjection[1][1]);
				float ppu = _ScreenParams.y / zoom / 2;
				float3 snappedCameraPosition = floor(_WorldSpaceCameraPos * ppu + 1 / ppu) / ppu;
				float3 cameraSubpixelOffset = snappedCameraPosition - _WorldSpaceCameraPos;
				IN.vertex.x -= cameraSubpixelOffset.x;
				IN.vertex.y -= cameraSubpixelOffset.y;

				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.x -= (float)1 / _ScreenParams.x;
				OUT.vertex.y += (float)1 / _ScreenParams.y;
				#endif
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;

				OUT.vertex = ViewSpacePixelSnap(OUT.vertex);

				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}