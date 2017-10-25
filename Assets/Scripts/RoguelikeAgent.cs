	using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoguelikeAgent : Agent
{
	[Header("Roguelike specific")]
	[Range(20,200)]
	public float speed = 100f;
	[Range(10,200)]
	public int startingHealth = 100;
	[Range(10,200)]
	public int startingMana = 50;

	private Rigidbody2D rb;
	private Animator animator;
	private int walkingHash;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		walkingHash = Animator.StringToHash("Walking");
	}

	public override List<float> CollectState()
	{
		List<float> state = new List<float>();
		state.Add(transform.position.x);
		state.Add(transform.position.y);
		return state;
	}

	public override void AgentStep(float[] act)
	{
		float horizontalMovement = act[0];
		float verticalMovement = act[1];
		Vector2 movementInput = new Vector2(horizontalMovement, verticalMovement);


		rb.AddForce(movementInput * speed);

		float vel = movementInput.sqrMagnitude;
		animator.SetBool(walkingHash, (vel>.1f)?true:false);
	}

	private void FixedUpdate()
	{
		rb.velocity *= .05f;
	}

	public override void AgentReset()
	{
		rb.velocity = Vector3.zero;
	}

	public override void AgentOnDone()
	{

	}
}
