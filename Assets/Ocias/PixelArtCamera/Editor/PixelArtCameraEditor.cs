using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(PixelArtCamera))]
public class PixelArtCameraEditor : Editor {
	SerializedProperty pixels;
	SerializedProperty pixelsPerUnit;
	SerializedProperty smooth;
	SerializedProperty forceSquarePixels;

	SerializedProperty screenResolution;
	SerializedProperty upscaledResolution;
	SerializedProperty internalResolution;
	SerializedProperty finalBlitStretch;

	SerializedProperty mainCamera;
	SerializedProperty mainCanvas;
	
	void OnEnable () {
		pixels = serializedObject.FindProperty("pixels");		
		pixelsPerUnit = serializedObject.FindProperty("pixelsPerUnit");
		smooth = serializedObject.FindProperty("smooth");
		forceSquarePixels = serializedObject.FindProperty("forceSquarePixels");
		screenResolution = serializedObject.FindProperty("screenResolution");
		upscaledResolution = serializedObject.FindProperty("upscaledResolution");
		internalResolution = serializedObject.FindProperty("internalResolution");
		finalBlitStretch = serializedObject.FindProperty("finalBlitStretch");
		mainCamera = serializedObject.FindProperty("mainCamera");
		mainCanvas = serializedObject.FindProperty("mainCanvas");
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		//GUILayout.Label ("Smooth");
		DrawDefaultInspector ();
		pixels.vector2IntValue = EditorGUILayout.Vector2IntField("Target Pixel Dimensions", pixels.vector2IntValue);
		pixelsPerUnit.floatValue = EditorGUILayout.FloatField("Pixels Per Unit", pixelsPerUnit.floatValue);
		smooth.boolValue = EditorGUILayout.Toggle("Smooth", smooth.boolValue);
		forceSquarePixels.boolValue = EditorGUILayout.Toggle("Force Square Pixels", forceSquarePixels.boolValue);
		EditorGUILayout.LabelField("Screen: " + screenResolution.vector2IntValue.x + "×" + screenResolution.vector2IntValue.y);
		EditorGUILayout.LabelField("Pixel Resolution: " + internalResolution.vector2IntValue.x + "×" + internalResolution.vector2IntValue.y);
		EditorGUILayout.LabelField("Upscaled Resolution: " + upscaledResolution.vector2IntValue.x + "×" + upscaledResolution.vector2IntValue.y);
		Vector2 pixelSize = Vector2.zero;
		pixelSize.x = (float)screenResolution.vector2IntValue.x / (float)internalResolution.vector2IntValue.x / finalBlitStretch.vector2Value.x;
		pixelSize.y = (float)screenResolution.vector2IntValue.y / (float)internalResolution.vector2IntValue.y / finalBlitStretch.vector2Value.y;
		EditorGUILayout.LabelField("Pixel Scale: " + pixelSize.x + "×" + pixelSize.y);

		serializedObject.ApplyModifiedProperties ();
	}
}
