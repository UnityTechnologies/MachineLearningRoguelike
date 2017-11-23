// Ppppperfect pixel art rendering!!
// By Alexander Ocias
// https://ocias.com

using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class PixelArtCamera : MonoBehaviour {
	[HideInInspector] public Vector2Int screenResolution;
	[HideInInspector] public Vector2Int internalResolution;
	[HideInInspector] public Vector2Int upscaledResolution;
	//[SerializeField] bool windowboxing = false; //Maybe implement this later?
	[HideInInspector] public bool smooth = false;
	[HideInInspector] public bool forceSquarePixels = false;
	[HideInInspector] public Vector2Int pixels = new Vector2Int(1080/12, 1920/12);
	[HideInInspector] public float pixelsPerUnit = 100f;
	RenderTexture rt;

	float targetAspectRatio;
	float currentAspectRatio;
	
	[HideInInspector] public Vector2 finalBlitStretch = Vector2.one;

	public Camera mainCamera;
	public Canvas mainCanvas;

	// Use this for initialization
	void Start () {
		SetupRenderTexture();
	}

	void Reset () {
		// Try to connect everything automatically on first attach
		mainCamera = Camera.main;
		GameObject canvasObj = GameObject.Find("Canvas");
		if (canvasObj != null) {
			mainCanvas = canvasObj.GetComponent<Canvas>();
		}
	}

	public void SetupRenderTexture () {
		// Try to connect missing pieces
		if (mainCamera == null) {
			mainCamera = Camera.main;
		}
		if (mainCanvas != null) {
			GameObject canvasObj = GameObject.Find("Canvas");
			if (canvasObj != null) {
				mainCanvas = canvasObj.GetComponent<Canvas>();
			}
		}
		// prevent 0-size rendertextures, just in case
		if (pixels.x == 0 || pixels.y == 0) {
			return;
		}
		// Configure canvas properly to match camera
		if (mainCanvas != null) {
			mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
			mainCanvas.worldCamera = mainCamera;
			CanvasScaler scaler = mainCanvas.GetComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(pixels.x, pixels.y);
			scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
			scaler.referencePixelsPerUnit = pixelsPerUnit;
		}
		
		if (rt != null) {
			rt.Release();
		}
		
		screenResolution.x = Screen.width;
		screenResolution.y = Screen.height;

		targetAspectRatio = (float)pixels.x / (float)pixels.y;
		currentAspectRatio = (float)Screen.width / (float)Screen.height;
		
		internalResolution.x = pixels.x;
		internalResolution.y = pixels.y;
		
		// Figure out best pixel resolution for aspect ratio we're on
		if (currentAspectRatio != targetAspectRatio) {
			if (currentAspectRatio > targetAspectRatio) {
				// Wider screen
				internalResolution.x = (int)Mathf.Round((float)pixels.y * currentAspectRatio);
			} else {
				// Taller screen
				internalResolution.y = (int)Mathf.Round((float)pixels.x / currentAspectRatio);
			}
		}
		
		// Determine scale to keep pixels square
		finalBlitStretch = Vector2.one;
		if (forceSquarePixels) {
			float internalResAspect = (float)internalResolution.x / (float)internalResolution.y;
			if (currentAspectRatio != targetAspectRatio) {
				if (currentAspectRatio > targetAspectRatio) {
					// Wider screen
					finalBlitStretch.x = (currentAspectRatio / internalResAspect);
				} else {
					// Taller screen
					finalBlitStretch.y = (internalResAspect / currentAspectRatio);
				}
			}
		}

		// Make sure our camera projection fits our resolution
		mainCamera.orthographicSize = internalResolution.y / 2f / pixelsPerUnit;

		rt = new RenderTexture(internalResolution.x, internalResolution.y, 16, RenderTextureFormat.ARGB32);
		
		if (smooth) {
			rt.filterMode = FilterMode.Bilinear;
		} else {
			rt.filterMode = FilterMode.Point;
		}
		rt.Create();
	}

	void OnPreRender() {
		if ((float)Screen.width / (float)Screen.height != currentAspectRatio) {
			SetupRenderTexture();
		}
		if (mainCamera != null) {
			// Render to our small internal texture
			mainCamera.targetTexture = rt;
		}
    }
	void OnPostRender() {
		if (mainCamera == null) {
			return;
		}
		// null the targettexture so we can blit to the screen
		mainCamera.targetTexture = null;
		
		if (smooth) {
			Graphics.Blit(rt, null, finalBlitStretch, (finalBlitStretch - Vector2.one) / -2f);
		} else {
			// draw to buffer at least as big as the screen
			int scaleMultiple = Mathf.CeilToInt((float)Screen.width / (float)internalResolution.x);
			upscaledResolution.x = internalResolution.x * scaleMultiple;
			upscaledResolution.y = internalResolution.y * scaleMultiple;
			RenderTexture largeRt = RenderTexture.GetTemporary(upscaledResolution.x, upscaledResolution.y);
			Graphics.Blit(rt, largeRt);
			
			// Scale down to screen		
			Graphics.Blit(largeRt, null, finalBlitStretch, (finalBlitStretch - Vector2.one) / -2f);
			RenderTexture.ReleaseTemporary(largeRt);
		}
		
    }
}


