using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoguelikeAgent : Agent
{
	[Header("Roguelike specific")]
	public int speed = 100;
	public int startingHealth = 100;
	public float attackCooldown = 1f;
	public int attackDamage = 5;

	protected Rigidbody2D rb;
	protected Animator animator;
	protected Vector2 movementInput; //cached input coming from the Brain
	protected SpriteRenderer graphicsSpriteRenderer;
	public int health;

	//[HideInInspector]
	public Vector2 desiredLocalPosition;
	public Vector3 desiredPosition;

	private float damageCooldown = 1f; //invincibility cooldown after a hit
	private float lastHitTime; //used to verify cooldowns
	private int doAttackHash;
	private Collider2D damageCollider;
	private Color originalColour;
	private bool canAttack = true; //put to false when attacking, restored to true after the attackCooldown
    private bool hasBeenHit = false;
	private Vector2 startLocalPosition;
	private Rigidbody2D enemyAgentRb;
	private bool isHealing;
	private float movementTowardsEnemy;
	//private float prevDistanceFromTarget;

	public RoguelikeAgent enemyAgent;

	private Coroutine healCoroutine;

    public override void InitializeAgent()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		graphicsSpriteRenderer = transform.Find("Graphics").GetComponent<SpriteRenderer>();
		doAttackHash = Animator.StringToHash("DoAttack");
		startLocalPosition = transform.localPosition;
		//prevDistanceFromTarget = Mathf.Infinity;
		originalColour = graphicsSpriteRenderer.color;
		if(enemyAgent != null)
		{
			enemyAgentRb = enemyAgent.GetComponent<Rigidbody2D>();
		}
		AgentReset(); //will reset some key variables
	}

	public override List<float> CollectState()
	{
		List<float> state = new List<float>();
		//Agent data
		state.Add(desiredLocalPosition.x);
		state.Add(desiredLocalPosition.y);
		state.Add(health);
		state.Add((canAttack) ? 1f : 0f); //can this Agent attack? (due to attack cooldown)
		state.Add((isHealing) ? 1f : 0f);

		//Enemy data
		if(enemyAgent != null)
		{
			state.Add(1f); //does this Agent have an enemy?
			state.Add(enemyAgent.desiredLocalPosition.x);
			state.Add(enemyAgent.desiredLocalPosition.y);
			state.Add(movementTowardsEnemy);
		}
		else
		{
			//enemy data is set to zero
			state.Add(0f);
			state.Add(0f);
			state.Add(0f);
			state.Add(0f);
		}
		return state;
	}

	public override void AgentStep(float[] act)
	{
		//reset inputs
		bool attack = false;
		movementInput = Vector2.zero;

		if(brain.brainParameters.actionSpaceType == StateType.discrete)
		{
			if(act[0] == 0f)
			{
				movementInput.x = -1f;
			}
			if(act[0] == 1f)
			{
				movementInput.x = 1f;
			}
			if(act[0] == 2f)
			{
				movementInput.y = 1f;
			}
			if(act[0] == 3f)
			{
				movementInput.y = -1f;
			}
			if(act[0] == 4f)
			{
				attack = true;
			}
		}
		else
		{
			movementInput.x = Mathf.Clamp(act[0], -1f, 1f);
			movementInput.y = Mathf.Clamp(act[1], -1f, 1f);
			attack = act[2] > 0f;
		}

		//MOVEMENT
		Vector2 movementFactor = new Vector2(movementInput.x, movementInput.y) * Time.fixedDeltaTime * 2f;
		desiredPosition = desiredPosition + (Vector3)movementFactor;
		desiredLocalPosition = desiredLocalPosition + movementFactor;
		rb.MovePosition(desiredPosition);

		//DISTANCE CHECK
		//float currentDistanceFromTarget = (enemyAgent.transform.localPosition - transform.localPosition).sqrMagnitude;
		if (enemyAgent != null)
		{
			movementTowardsEnemy = Vector2.Dot(movementInput.normalized, (enemyAgent.desiredLocalPosition-desiredLocalPosition).normalized); //-1f if moving away, 1f if moving closer
			
			if (health >= startingHealth * .5f)
			{
				reward += .01f * movementTowardsEnemy;
			}
			else
			{
				reward -= .01f * movementTowardsEnemy;
			}
		}
		//prevDistanceFromTarget = currentDistanceFromTarget;
		
		//ATTACK
		if(attack)
		{
			if(canAttack)
			{
				StartCoroutine(DoAttack());
			}
			else
			{
				reward -= .01f; //penalty for trying to attack when it can't
			}

			/* //stop healing, if it was
			if(isHealing)
			{
				StopCoroutine(healCoroutine);
				reward -= .5f;
				isHealing = false;
			} */
		}
		else
		{
			//if not attacking, can start healing
			if(!isHealing
				&& health < startingHealth)
			{
				healCoroutine = StartCoroutine(Heal());
			}
		}


		//FINAL OPERATIONS
		//rb.velocity *= .5f; //fake drag
		//reward -= .05f; //base negative reward, to push the agent to act
	}

	private IEnumerator Heal()
	{
		isHealing = true;
		yield return new WaitForSeconds(2f);
		
		while(isHealing
			&& health < startingHealth)
		{	
			//heal and get a reward for it
			health++;
			reward += .05f;

			yield return new WaitForSeconds(2f);
		}

		isHealing = false;
	}

    private IEnumerator DoAttack()
    {
		canAttack = false;
        animator.SetTrigger(doAttackHash);
		//rb.AddForce(movementInput * speed * .5f, ForceMode2D.Impulse); //add a strong push

		yield return new WaitForSeconds(attackCooldown);

		canAttack = true;
    }

    public void DealDamage(RoguelikeAgent target)
	{
		bool isTargetDead = false;

		if(!target.hasBeenHit)
		{
			isTargetDead = target.ReceiveDamage(attackDamage);
			if(isTargetDead)
			{
				reward += 2f;
				done = true;
			}
			else
			{
				reward += 1f;
			}
		}

	}

	//Returns if the Agent is dead or not, to reward the attacker
    public bool ReceiveDamage(int attackDamage)
    {
        health -= attackDamage;
		if(health <= 0)
		{
			Die();
			reward -= 2f;
			return true;
		}
		else
		{
			StartCoroutine(HitFlicker());
			reward -= .5f;
			return false;
		}
    }

	private void Die()
	{
		if(brain.brainType == BrainType.External)
		{
			//During training
			done = true;
		}
		else
		{
			//During gameplay
			Destroy(gameObject);
		}

	}

    private IEnumerator HitFlicker()
    {
		lastHitTime = Time.time;
		hasBeenHit = true;

        while(Time.time < lastHitTime + damageCooldown)
		{
			yield return new WaitForSeconds(.1f);
			graphicsSpriteRenderer.color = Color.red;

			yield return new WaitForSeconds(.1f);
			graphicsSpriteRenderer.color = originalColour;
		}

		hasBeenHit = false;
    }

	public override void AgentReset()
	{
		rb.velocity = Vector3.zero;
		health = startingHealth;
		if(brain.brainType == BrainType.InputManager)
		{
			transform.localPosition = new Vector3(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f), 0f);
		}
		else
		{
			transform.localPosition = startLocalPosition;
		}
		desiredLocalPosition = transform.localPosition;
		desiredPosition = transform.position;
		//prevDistanceFromTarget = Mathf.Infinity;
	}

	public override void AgentOnDone()
	{
		
	}
}
