using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoguelikeAcademy : Academy {

	[Header("Roguelike specific")]
	public float startDistance = .1f;
	public float increment = .05f;
	public float maxDistance = 4f;

	public override void AcademyReset()
	{
		startDistance += increment;
		if(startDistance >= maxDistance)
		{
			startDistance = maxDistance;
		}
	}

	public override void AcademyStep()
	{
		

	}

}
