using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// CoreBrain which decides actions using Player input.
public class CoreBrainInputManager : ScriptableObject, CoreBrain
{

    [System.Serializable]
    private struct ContinuousPlayerAction
    {
        public string axis;
        public int index;
    }

    [SerializeField]
    /// Contains the mapping from input to continuous actions
    private ContinuousPlayerAction[] continuousPlayerActions;
    [SerializeField]
    private int defaultAction = -1;

    /// Reference to the brain that uses this CoreBrainPlayer
    public Brain brain;

    /// Create the reference to the brain
    public void SetBrain(Brain b)
    {
        brain = b;
    }

    /// Nothing to implement
    public void InitializeCoreBrain()
    {

    }

    /// Uses the continuous inputs or dicrete inputs of the player to 
    /// decide action
    public void DecideAction()
    {
        float[] action = new float[brain.brainParameters.actionSize];
        foreach (ContinuousPlayerAction cha in continuousPlayerActions)
        {
			float axisValue = Input.GetAxis(cha.axis);
            
			action[cha.index] = axisValue;
        }
        Dictionary<int, float[]> actions = new Dictionary<int, float[]>();
        foreach (KeyValuePair<int, Agent> idAgent in brain.agents)
        {
            actions.Add(idAgent.Key, action);
        }
        brain.SendActions(actions);
    }

    /// Nothing to implement, the Player does not use the state to make 
    /// decisions
    public void SendState()
    {

    }

    /// Displays continuous or discrete input mapping in the inspector
    public void OnInspector()
    {
#if UNITY_EDITOR
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        SerializedObject serializedBrain = new SerializedObject(this);
        if (brain.brainParameters.actionSpaceType == StateType.continuous)
        {
            GUILayout.Label("Edit the continuous inputs for you actions", EditorStyles.boldLabel);
            SerializedProperty chas = serializedBrain.FindProperty("continuousPlayerActions");
            serializedBrain.Update();
            EditorGUILayout.PropertyField(chas, true);
            serializedBrain.ApplyModifiedProperties();
            if (continuousPlayerActions == null)
            {
                continuousPlayerActions = new ContinuousPlayerAction[0];
            }
            foreach (ContinuousPlayerAction cha in continuousPlayerActions)
            {
                if (cha.index >= brain.brainParameters.actionSize)
                {
                    EditorGUILayout.HelpBox(string.Format("Axis {0} is assigned to index {1} but the action size is only of size {2}"
                        , cha.axis.ToString(), cha.index.ToString(), brain.brainParameters.actionSize.ToString()), MessageType.Error);
                }
            }
		}
		else
		{
			EditorGUILayout.HelpBox("Select Continuous Actions from the Brain", MessageType.Error);
		}
#endif
    }
}
