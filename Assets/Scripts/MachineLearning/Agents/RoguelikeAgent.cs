	using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoguelikeAgent : Agent
{
	[Header("Roguelike specific")]
	public int speed = 100;
	public int startingHealth = 100;
	public int startingMana = 50;
	public float attackCooldown = 1f;
	public int attackDamage = 5;

	protected Rigidbody2D rb;
	protected Animator animator;
	protected Vector2 movementInput; //cached input coming from the Brain

	private float damageCooldown = 1f; //invincibility cooldown after a hit
	private int doAttackHash;
	private Collider2D damageCollider;

	protected virtual void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		doAttackHash = Animator.StringToHash("DoAttack");
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
		//movment vector is valorized
		movementInput.x = act[0];
		movementInput.y = act[1];

		rb.AddForce(movementInput * speed, ForceMode2D.Force);
	}

	protected virtual void DealAttack()
	{
		animator.SetTrigger(doAttackHash);
	}

	protected virtual void FixedUpdate()
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
