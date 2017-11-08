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
	public float searchRadius = 6f;
    
	public int Health {
		get { return health; }
		set { health = value; healthBar.SetHealth(health, startingHealth); }
	}
	public RoguelikeAgent preassignedTarget;


    protected Rigidbody2D rb;
	protected Animator animator;
	protected Vector2 movementInput; //cached input coming from the Brain
	protected SpriteRenderer graphicsSpriteRenderer;

    [Header("Debug stuff")]
	[SerializeField]
	private RoguelikeAgent targetAgent;

	private bool hasToSearchForTarget;
    private int health;
	private float damageCooldown = 1f; //invincibility cooldown after a hit
	private float searchTargetInterval = 4f;
	private float lastSearchTime = -10f;
	private float lastHitTime; //used to verify cooldowns
	private int doAttackHash, isWalkingHash;
	private Color originalColour;
	private bool canAttack = true; //put to false when attacking, restored to true after the attackCooldown
    private bool hasBeenHit = false;
	private Vector2 startLocalPosition;
    private Vector2 rbLocalPosition;
    private Vector2 movementFactor;
	private bool isHealing;
	private Coroutine healCoroutine;
	private float movementTowardsTarget;
	private float distanceFromTargetSqr;
	private float thresholdDistanceFromTargetSqr;
	private HealthBar healthBar;
	private Transform parentTransform;
	private RoguelikeAcademy academy;


    public override void InitializeAgent()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		graphicsSpriteRenderer = transform.Find("Graphics").GetComponent<SpriteRenderer>();
		healthBar = transform.GetComponentInChildren<HealthBar>();
		doAttackHash = Animator.StringToHash("DoAttack");
		isWalkingHash = Animator.StringToHash("IsWalking");
		startLocalPosition = transform.localPosition;
		originalColour = graphicsSpriteRenderer.color;
		academy = FindObjectOfType<RoguelikeAcademy>();
		parentTransform = transform.parent;
		if(preassignedTarget != null)
		{
			targetAgent = preassignedTarget;
		}
		else
		{
			hasToSearchForTarget = true; //targetAgent will be looked for in the Update
		}
		
		AgentReset(); //will reset some key variables
	}

	public override List<float> CollectState()
	{
		List<float> state = new List<float>();
		//Agent data
		state.Add(rbLocalPosition.x * .1f);
		state.Add(rbLocalPosition.y * .1f);
		state.Add(Health * .1f);
		state.Add((canAttack) ? 1f : 0f); //can this Agent attack? (due to attack cooldown)
		state.Add((isHealing) ? 1f : 0f);

		//Enemy data
		if(targetAgent != null)
		{
			state.Add(1f); //does this Agent have an enemy?
			state.Add(targetAgent.rbLocalPosition.x * .1f);
			state.Add(targetAgent.rbLocalPosition.y * .1f);
			state.Add(movementTowardsTarget);
			state.Add(distanceFromTargetSqr * .001f);
		}
		else
		{
			//enemy data is set to zero
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
		//reset inputs
		bool attack = false;
		movementInput = Vector2.zero;

		if(brain.brainParameters.actionSpaceType == StateType.discrete)
		{
			if(act[0] == 0f)
			{
				//do nothing
			}
			if(act[0] == 1f)
			{
				movementInput.x = -1f;
			}
			if(act[0] == 2f)
			{
				movementInput.x = 1f;
			}
			if(act[0] == 3f)
			{
				movementInput.y = 1f;
			}
			if(act[0] == 4f)
			{
				movementInput.y = -1f;
			}
			if(act[0] == 5f)
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
		movementFactor = new Vector2(movementInput.x, movementInput.y) * Time.fixedDeltaTime * 2f;
		rb.position += (Vector2)movementFactor;
		Vector2 parentPos = (parentTransform != null) ? (Vector2)parentTransform.position : Vector2.zero; //calculating parent offset for obtaining local RB coordinates below
		rbLocalPosition = (Vector2)rb.position - parentPos + movementFactor;

		bool isInDanger = Health < startingHealth * .5f;

		//DISTANCE CHECK
		if (targetAgent != null)
		{
			distanceFromTargetSqr = (targetAgent.transform.localPosition - transform.localPosition).sqrMagnitude;
			movementTowardsTarget = Vector2.Dot(movementInput.normalized, (targetAgent.rbLocalPosition-rbLocalPosition).normalized); //-1f if moving away, 1f if moving closer
			
			if (!isInDanger)
			{
				//pursue
				if(distanceFromTargetSqr < thresholdDistanceFromTargetSqr)
				{
					reward += .1f / (distanceFromTargetSqr + .01f);// * movementTowardsTarget;
					thresholdDistanceFromTargetSqr = distanceFromTargetSqr;
				}
			}
			else
			{
				//retreat
				if(distanceFromTargetSqr > thresholdDistanceFromTargetSqr)
				{
					reward += .2f;
					thresholdDistanceFromTargetSqr = distanceFromTargetSqr;
				}
			}
		}
		
		//ATTACK
		if(attack)
		{
			if(canAttack)
			{
				StartCoroutine(DoAttack());
			}
			else
			{
				reward -= .1f; //penalty for trying to attack when it can't
			}

			/* //stop healing, if it was
			if(isHealing)
			{
				StopCoroutine(healCoroutine);
				reward -= .5f;
				isHealing = false;
			} */
		}
		/*else
		{
			//if not attacking, can start healing
			if(!isHealing
				&& Health < startingHealth)
			{
				if(brain.brainType != BrainType.External)
				{
					healCoroutine = StartCoroutine(Heal());
				}
			}
		}*/
	}

	private IEnumerator Heal()
	{
		isHealing = true;
		thresholdDistanceFromTargetSqr = 0f;
		yield return new WaitForSeconds(2f);
		
		while(isHealing
			&& Health < startingHealth)
		{	
			//heal
			Health++;
			yield return new WaitForSeconds(2f);
		}

		thresholdDistanceFromTargetSqr = Mathf.Infinity;
		isHealing = false;
	}

    private IEnumerator DoAttack()
    {
		canAttack = false;
        animator.SetTrigger(doAttackHash);

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
				reward += 1f;
				if(!academy.isInference)
				{
					done = true;
				}
			}
			else
			{
				reward += .5f;
			}
		}

	}

	//Returns if the Agent is dead or not, to reward the attacker
    public bool ReceiveDamage(int attackDamage)
    {
        Health -= attackDamage;
		UIManager.Instance.ShowDamageText(attackDamage, this.transform.position);

		reward -= 1f;
		if(Health <= 0)
		{
			Die();
			return true;
		}
		else
		{
			StartCoroutine(HitFlicker());
			return false;
		}
    }

	private void Die()
	{
		//During training
		done = true;

		if(brain.brainType == BrainType.Internal)
		{
			//During actual gameplay
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
		Health = startingHealth;
		if(brain.brainType == BrainType.External
			|| academy.isInference)
		{
			//fixed position, only for the trainee - or during gameplay
			transform.localPosition = startLocalPosition;
		}
		else
		{
			//randomised position for players, heuristic (the opponents during training) and during inference
			float offset = academy.startDistance;
			transform.localPosition = UnityEngine.Random.insideUnitCircle.normalized * offset;
		}
		if(targetAgent != null)
		{
			distanceFromTargetSqr = (targetAgent.transform.localPosition - transform.localPosition).sqrMagnitude;
		}
		rbLocalPosition = transform.localPosition;
		thresholdDistanceFromTargetSqr = Mathf.Infinity;
	}

	public override void AgentOnDone()
	{
		
	}

	private void Update()
	{
		animator.SetBool(isWalkingHash, movementFactor != Vector2.zero);

		if(hasToSearchForTarget)
		{
			//search for a potential target
			float currentTime = Time.time;
			if(currentTime > lastSearchTime + searchTargetInterval)
			{
				if(targetAgent == null)
				{
					//search for a new target (might be null anyway, because of distance)
					targetAgent = SearchForTarget();
				}
				else
				{
					//check if it's too far
					if(distanceFromTargetSqr > searchRadius * searchRadius)
					{
						//target lost
						targetAgent = null;
					}
				}
				lastSearchTime = currentTime;
			}
		}
	}

	protected virtual RoguelikeAgent SearchForTarget()
	{
		//this is implemented in inheriting classes
		return null;
	}
}
