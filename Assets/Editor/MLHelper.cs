using UnityEngine;
using System.Collections;
using UnityEditor;

[System.Serializable]
public class MLHelper : EditorWindow
{
	private GameObject brainObject;
	private Brain mlBrain;
	private bool tryToFindBrain = true;

	[MenuItem("Machine Learning/Open Helper")]
	public static void ShowEditorWindow()
	{
		EditorWindow window = GetWindow<MLHelper>("ML Helper");
		Vector2 minSize = new Vector2(50f, 150f);
		window.minSize = minSize;
	}

	private void OnGUI ()
	{
		GUIStyle style = new GUIStyle();
		style.padding = new RectOffset(10, 10, 20, 10);
		EditorGUILayout.BeginVertical(style);

		EditorGUILayout.LabelField("Brain to use");
		brainObject = (GameObject)EditorGUILayout.ObjectField(brainObject, typeof(GameObject), true);
		if(brainObject != null)
		{
			mlBrain = brainObject.GetComponent<Brain>();
		}
		else
		{
			bool findButton = GUILayout.Button("Find brain", GUILayout.Height(20f));
			if(tryToFindBrain || findButton)
			{
				Brain[] allBrains = GameObject.FindObjectsOfType<Brain>();
				foreach(Brain br in allBrains)
				{
					if(br.brainType == BrainType.External || br.brainType == BrainType.Internal)
					{
						//found
						brainObject = br.gameObject;
						break;
					}
				}

				tryToFindBrain = false;
			}
		}

		GUILayout.Space(20f);

		EditorGUILayout.LabelField("Training", EditorStyles.boldLabel);
		if(mlBrain == null)
		{
			EditorGUILayout.HelpBox("Select a GameObject that has a Brain!", MessageType.Error);
		}
		else
		{
			bool buildButton = GUILayout.Button("Build for Training", GUILayout.Height(40f));
			if(buildButton)
			{
				if(mlBrain != null)
				{
					mlBrain.brainType = BrainType.External;
					BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
					buildPlayerOptions.options = BuildOptions.ShowBuiltPlayer;
					buildPlayerOptions.scenes = new[] { "Assets/Scenes/Training.unity" };
					buildPlayerOptions.target = BuildTarget.StandaloneOSXUniversal;
					buildPlayerOptions.locationPathName = "python/ML-Roguelike";
					BuildPipeline.BuildPlayer(buildPlayerOptions);
				}
			}
		}

		GUILayout.Space(20f);

		EditorGUILayout.LabelField("Inference", EditorStyles.boldLabel);
		if(mlBrain == null)
		{
			EditorGUILayout.HelpBox("Select a GameObject that has a Brain!", MessageType.Error);
		}
		else
		{
			bool copyModel = GUILayout.Button("1: Copy model to project", GUILayout.Height(30f));
			if(copyModel)
			{
				FileUtil.ReplaceFile("python/models/ppo/ML-Roguelike.bytes", "Assets/ML Models/ML-Roguelike.bytes");
			}

			bool restoreInternalBrain = GUILayout.Button("2: Restore Internal Brain", GUILayout.Height(30f));
			if(restoreInternalBrain)
			{
				mlBrain.brainType = BrainType.Internal;
			}
			EditorGUILayout.LabelField("Don't forget to press Play :-)");
		}

		EditorGUILayout.EndVertical();
	}
}