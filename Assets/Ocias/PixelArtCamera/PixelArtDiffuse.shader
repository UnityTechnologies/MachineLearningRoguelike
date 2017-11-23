// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Ocias/Pixel Art Diffuse" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
#pragma surface surf Lambert vertex:vert noforwardadd

sampler2D _MainTex;

struct Input {
	float2 uv_MainTex;
};

inline float4 ViewSpacePixelSnap (float4 pos) {

	float2 halfScreenRes = _ScreenParams.xy * 0.5f;

	// // View space Pixel Snapping
	float2 pixelPos = floor(pos * halfScreenRes + 1 / halfScreenRes) / halfScreenRes; // put back in that half pixel offset when you're done
	pos.xy = pixelPos;
	
	// Odd resolution handling
	// float2 odd = _ScreenParams.xy % 2;
	// pos.x += odd.x * 0.5 / halfScreenRes.x;
	// pos.y += odd.y * 0.5 / halfScreenRes.y;

	return pos;
}

inline float4 WorldSpacePixelSnap (float4 pos) {

	//float ppu = _PixelsPerUnit;
	float zoom = 1/(unity_CameraProjection[1][1]);
	float ppu = _ScreenParams.y / zoom / 2;

	pos = mul(unity_ObjectToWorld, pos);
	
	// World space Pixel Snapping
	pos = floor(pos * ppu + 1 / ppu) / ppu;
	// Adjust to pixel relative to camera position
	float3 snappedCameraPosition = floor(_WorldSpaceCameraPos * ppu + 1 / ppu) / ppu;
	float3 cameraSubpixelOffset = snappedCameraPosition - _WorldSpaceCameraPos;
	pos.x -= cameraSubpixelOffset.x;
	pos.y -= cameraSubpixelOffset.y;
	// Odd resolution handling
	float2 odd = round(_ScreenParams.xy) % 2;
	pos.x += odd.x * 0.5 / ppu;
	pos.y += odd.y * 0.5 / ppu;

	pos = mul(unity_WorldToObject, pos);

	return pos;
}

void vert (inout appdata_full v) {
	// Offset position based on distance from camera to nearest pixel
	// float zoom = 1/(unity_CameraProjection[1][1]);
	// float ppu = _ScreenParams.y / zoom / 2;
	// float3 snappedCameraPosition = floor(_WorldSpaceCameraPos * ppu + 1 / ppu) / ppu;
	// float3 cameraSubpixelOffset = snappedCameraPosition - _WorldSpaceCameraPos;
	// v.vertex.x -= cameraSubpixelOffset.x;
	// v.vertex.y -= cameraSubpixelOffset.y;

	v.vertex = WorldSpacePixelSnap(v.vertex);
}

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
