using System;
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
	protected SpriteRenderer graphicsSpriteRenderer;
	public int health;
	protected int mana;

	private float damageCooldown = 1f; //invincibility cooldown after a hit
	private float lastHitTime; //used to verify cooldowns
	private int doAttackHash;
	private Collider2D damageCollider;
	private bool canAttack = true; //put to false when attacking, restored to true after the attackCooldown
    private bool hasBeenHit = false;
	private Vector2 startPosition;
	private Vector3 prevPosition;
	private Rigidbody2D enemyAgentRb;
	private bool isHealing;
	private float enemyDistance, closestEnemyDistance;

	public RoguelikeAgent enemyAgent;

	private Coroutine healCoroutine;

    protected virtual void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		graphicsSpriteRenderer = transform.Find("Graphics").GetComponent<SpriteRenderer>();
		doAttackHash = Animator.StringToHash("DoAttack");
		startPosition = transform.position;
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
		state.Add(transform.position.x);
		state.Add(transform.position.y);
		state.Add(health);
		state.Add((canAttack) ? 1f : 0f); //can this Agent attack? (due to attack cooldown)
		state.Add(rb.velocity.x);
		state.Add(rb.velocity.y);

		//Enemy data
		if(enemyAgent != null)
		{
			state.Add(1f); //does this Agent have an enemy?
			state.Add(enemyAgent.transform.position.x);
			state.Add(enemyAgent.transform.position.y);
			state.Add(enemyAgentRb.velocity.x);
			state.Add(enemyAgentRb.velocity.y);
			state.Add(enemyDistance); //squared distance from the enemy
		}
		else
		{
			//enemy data is set to zero
			state.Add(0f);
			state.Add(0f);
			state.Add(0f);
			state.Add(0f);
			state.Add(0f);
			state.Add(0f);
		}
		return state;
	}

	public override void AgentStep(float[] act)
	{
		//MOVEMENT
		movementInput.x = Mathf.Clamp(act[0], -1f, 1f);
		movementInput.y = Mathf.Clamp(act[1], -1f, 1f);

		rb.AddForce(movementInput * speed, ForceMode2D.Force);
		

		//DISTANCE CHECK
		if(enemyAgent != null)
		{
			float newEnemyDistance = Vector3.SqrMagnitude(enemyAgent.transform.position - this.transform.position);
			//is the Agent aggressive or is it fleeing?
			if(health > startingHealth * .5f)
			{
				//aggressive behaviour
				if(newEnemyDistance < enemyDistance) //moving closer
				{
					if(newEnemyDistance < closestEnemyDistance) //to avoid exploitation (back and forth)
					{
						reward += .1f; //reward the hunt
					}
					else
					{
						reward -= .05f;
					}
				}
			}
			else
			{
				//(is its health below half of the original?)
				if(newEnemyDistance > enemyDistance) //getting further
				{
					reward += .1f; //reward the escape
				}
			}
			enemyDistance = newEnemyDistance; //cached for CollectState
			closestEnemyDistance = enemyDistance;
		}


		//ATTACK
		float attack = act[2];
		if(attack > 0f)
		{
			if(canAttack)
			{
				StartCoroutine(DoAttack());
			}
			else
			{
				reward -= .001f; //penalty for trying to attack when it can't
			}

			//stop healing, if it was
			if(isHealing)
			{
				StopCoroutine(healCoroutine);
				isHealing = false;
			}
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
		rb.velocity *= .5f; //fake drag
		//reward -= .1f; //base negative reward, to push the agent to act
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

		closestEnemyDistance = Mathf.Infinity; //this will allow again a reward for getting closer
		isHealing = false;
	}

    private IEnumerator DoAttack()
    {
		canAttack = false;
        animator.SetTrigger(doAttackHash);
		rb.AddForce(movementInput * speed * .5f, ForceMode2D.Impulse); //add a strong push

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
			}
			else
			{
				reward += .8f;
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
			graphicsSpriteRenderer.color = Color.white;
		}

		hasBeenHit = false;
    }

    protected virtual void FixedUpdate()
	{
		
	}

	public override void AgentReset()
	{
		rb.velocity = Vector3.zero;
		health = startingHealth;
		mana = startingMana;
		transform.position = startPosition;
		closestEnemyDistance = Mathf.Infinity;
	}

	public override void AgentOnDone()
	{
		
	}
}
