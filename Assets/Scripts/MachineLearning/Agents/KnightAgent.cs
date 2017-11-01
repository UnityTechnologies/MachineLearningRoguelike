using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightAgent : RoguelikeAgent
{
	private int walkingHash;

	public override void InitializeAgent()
	{
		base.InitializeAgent();

		walkingHash = Animator.StringToHash("Walking");
	}

	public override void AgentStep(float[] act)
	{
		base.AgentStep(act);

		float vel = movementInput.sqrMagnitude;
		animator.SetBool(walkingHash, (vel>.1f)?true:false);
	}
	
}
