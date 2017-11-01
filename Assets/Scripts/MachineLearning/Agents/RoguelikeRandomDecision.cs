using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoguelikeRandomDecision : MonoBehaviour, Decision {

	public float[] Decide (List<float> state, List<Camera> observation, float reward, bool done, float[] memory)
	{
		float[] act = new float[3];
		act[0] = Random.Range(-.5f, .5f);
		act[1] = Random.Range(-.5f, .5f);

		act[2] = (float)Random.Range(-40, 2);

		return act;
	}

	public float[] MakeMemory (List<float> state, List<Camera> observation, float reward, bool done, float[] memory)
	{
		return default(float[]);
		
	}
}
