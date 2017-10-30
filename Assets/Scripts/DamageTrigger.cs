using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTrigger : MonoBehaviour {

	private RoguelikeAgent rlAgent;
	
	private void Awake()
	{
		rlAgent = GetComponentInParent<RoguelikeAgent>();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		Transform parent = other.transform.parent;
		if(parent.CompareTag("Agent"))
		{
			rlAgent.DealDamage(parent.GetComponent<RoguelikeAgent>());
		}
	}
}
