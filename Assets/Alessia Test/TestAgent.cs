using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAgent : Agent
{
	public GameObject target;
	private Vector2 startPosition;
	private float prevDistanceFromTarget;

	public override void InitializeAgent()
	{
		startPosition = transform.localPosition;
		prevDistanceFromTarget = Mathf.Infinity;
	}

	public override List<float> CollectState()
	{
		List<float> state = new List<float>();
		state.Add(transform.localPosition.x);
		state.Add(transform.localPosition.y);
		state.Add(target.transform.localPosition.x);
		state.Add(target.transform.localPosition.y);
		state.Add(target.transform.localPosition.x - transform.localPosition.x);
		state.Add(target.transform.localPosition.y - transform.localPosition.y);
		return state;
	}

	public override void AgentStep(float[] act)
	{
		float actionX = Mathf.Clamp(act[0], -1f, 1f);
		float actionY = Mathf.Clamp(act[1], -1f, 1f);

		GetComponent<Rigidbody2D>().MovePosition((Vector2)transform.position + new Vector2(actionX, actionY) * Time.fixedDeltaTime);

		float currentDistanceFromTarget = (target.transform.localPosition - transform.localPosition).magnitude;
		
		reward = (currentDistanceFromTarget < prevDistanceFromTarget) ? 0.1f / currentDistanceFromTarget : -0.1f / currentDistanceFromTarget;
		prevDistanceFromTarget = currentDistanceFromTarget;

		if (currentDistanceFromTarget <= .5f)
		{
			reward = 1f;
			done = true;
		}

	}

	public override void AgentReset()
	{
		transform.localPosition = startPosition;
		target.transform.localPosition = new Vector3(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f), 0f);
		prevDistanceFromTarget = Mathf.Infinity;
	}

	public override void AgentOnDone()
	{
		
	}
}
