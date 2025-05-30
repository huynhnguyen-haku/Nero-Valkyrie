using System.Collections.Generic;
using UnityEngine;

public enum BossWeaponType { Flamethrower, Hammer }

public class Enemy_Boss : Enemy
{
    #region Fields and Properties

    [Header("Abilities")]
    public float minAbilityDistance;
    public float abilityCooldown;
    private float lastTimeAbility;

    [Header("Attack")]
    [SerializeField] private int meleeAttackDamage;
    [SerializeField] private Transform[] damagePoints;
    [SerializeField] private float attackCheckRadius;
    [SerializeField] private GameObject meleeAttackFX;

    [Header("Boss Detail")]
    public BossWeaponType weaponType;
    public float attackRange;
    public float actionCooldown = 10;
    public int attackAnimationCount;

    [Header("Flamethrower")]
    public int flameDamage;
    public float flameDamageCooldown;
    public float flamethrowDuration;
    public ParticleSystem flamethrower;
    public bool flamethrowerActive { get; private set; }

    [Header("Hammer")]
    public int hammerActiveDamage;
    public GameObject activationPrefab;
    [SerializeField] private float hammerCheckRadius;

    [Header("Jump Attack")]
    public int jumpAttackDamage;
    public float impactRadius = 2.5f;
    public float impactPower = 10;
    public Transform impactPoint;
    public ParticleSystem jumpAttackVFX;

    [Space]
    public float travelTimeToTarget = 1;
    public float jumpAttackCooldown = 10;
    [SerializeField] private float upwardsMulti = 10;
    private float lastTimeJump;
    public float minJumpDistanceRequired;

    [Header("Minimap Icon")]
    private GameObject minimapIcon;

    [Space]
    [SerializeField] private LayerMask whatToIgnore;

    public IdleState_Boss idleState { get; private set; }
    public MoveState_Boss moveState { get; private set; }
    public AttackState_Boss attackState { get; private set; }
    public JumpAttackState_Boss jumpAttackState { get; private set; }
    public AbilityState_Boss abilityState { get; private set; }
    public DeadState_Boss deadState { get; private set; }
    public Enemy_BossVisual bossVisual { get; private set; }
    public Enemy_BossSFX bossSFX { get; private set; }
    #endregion

    #region Unity Methods

    protected override void Awake()
    {
        base.Awake();
        bossVisual = GetComponent<Enemy_BossVisual>();
        bossSFX = GetComponent<Enemy_BossSFX>();

        idleState = new IdleState_Boss(this, stateMachine, "Idle");
        moveState = new MoveState_Boss(this, stateMachine, "Move");
        attackState = new AttackState_Boss(this, stateMachine, "Attack");
        jumpAttackState = new JumpAttackState_Boss(this, stateMachine, "JumpAttack");
        abilityState = new AbilityState_Boss(this, stateMachine, "Ability");
        deadState = new DeadState_Boss(this, stateMachine, "Idle");

        // Setup minimap icon if available
        if (minimapIcon == null)
        {
            var minimapSprite = GetComponentInChildren<MinimapSprite>(true);
            if (minimapSprite != null)
                minimapIcon = minimapSprite.gameObject;
        }
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
        stateMachine.currentState.Update();

        if (ShouldEnterBattleMode())
            EnterBattleMode();

        MeleeAttackCheck(damagePoints, attackCheckRadius, meleeAttackFX, meleeAttackDamage);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Visualize important boss ranges in the editor
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, minJumpDistanceRequired);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, impactRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minAbilityDistance);

        Gizmos.color = Color.yellow;
        if (damagePoints.Length > 0)
        {
            foreach (var damagePoint in damagePoints)
                Gizmos.DrawWireSphere(damagePoint.position, attackCheckRadius);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(damagePoints[0].position, hammerCheckRadius);
        }
    }
    #endregion

    #region Battle Mode Methods

    public override void EnterBattleMode()
    {
        if (inBattleMode)
            return;

        base.EnterBattleMode();
        stateMachine.ChangeState(moveState);
    }

    // True if player is within melee attack range
    public bool PlayerInAttackRange()
    {
        return Vector3.Distance(transform.position, player.position) < attackRange;
    }

    // True if player is in clear sight (for jump attacks/chase)
    public bool IsPlayerInClearSight()
    {
        Vector3 myPosition = transform.position + new Vector3(0, 1.5f, 0);
        Vector3 playerPosition = player.position + Vector3.up;
        Vector3 directionToPlayer = (playerPosition - myPosition).normalized;

        if (Physics.Raycast(myPosition, directionToPlayer, out RaycastHit hit, 100, ~whatToIgnore))
            return hit.transform.root == player.root;

        return false;
    }
    #endregion

    #region Attack Methods

    public void ActivateFlamethrower(bool activate)
    {
        flamethrowerActive = activate;

        if (!activate)
        {
            flamethrower.Stop();
            anim.SetTrigger("StopFlamethrower");
            return;
        }

        var mainModule = flamethrower.main;
        var extraModule = flamethrower.transform.GetChild(0).GetComponent<ParticleSystem>().main;

        mainModule.duration = flamethrowDuration;
        extraModule.duration = flamethrowDuration;

        flamethrower.Clear();
        flamethrower.Play();
        bossSFX.flameSFX.Play();
    }

    public void ActivateHammer()
    {
        GameObject newActivation = ObjectPool.instance.GetObject(activationPrefab, impactPoint);
        ObjectPool.instance.ReturnObject(newActivation, 1);

        MassDamage(damagePoints[0].position, hammerCheckRadius, hammerActiveDamage);
    }

    // True if boss can perform a jump attack
    public bool CanDoJumpAttack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < minJumpDistanceRequired)
            return false;

        if (Time.time > lastTimeJump + jumpAttackCooldown && IsPlayerInClearSight())
            return true;

        return false;
    }

    // Execute jump attack impact
    public void JumpImpact()
    {
        Transform impactPoint = this.impactPoint ?? transform;

        MassDamage(impactPoint.position, impactRadius, jumpAttackDamage);
        bossSFX.impactSFX.Play();
        jumpAttackVFX.Play();
    }

    // Deal damage to all entities within impact radius
    private void MassDamage(Vector3 impactPoint, float impactRadius, int damage)
    {
        HashSet<GameObject> uniqueEntities = new HashSet<GameObject>();
        Collider[] colliders = Physics.OverlapSphere(impactPoint, impactRadius, ~whatIsAlly);

        foreach (Collider hit in colliders)
        {
            I_Damagable damagable = hit.GetComponent<I_Damagable>();
            if (damagable != null)
            {
                GameObject rootEntity = hit.transform.root.gameObject;
                if (!uniqueEntities.Add(rootEntity))
                    continue;

                damagable.TakeDamage(damage);
            }
            ApplyPhysicalForce(impactPoint, impactRadius, hit);
        }
    }

    // Apply explosion force to entities in impact radius
    private void ApplyPhysicalForce(Vector3 impactPoint, float impactRadius, Collider hit)
    {
        Rigidbody rb = hit.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddExplosionForce(impactPower, impactPoint, impactRadius, upwardsMulti, ForceMode.Impulse);
    }

    // True if boss can use ability (e.g. flamethrower)
    public bool CanDoAbility()
    {
        bool playerWithinDistance = Vector3.Distance(transform.position, player.position) < minAbilityDistance;

        if (!playerWithinDistance)
            return false;

        if (Time.time > lastTimeAbility + abilityCooldown)
            return true;

        return false;
    }

    public void SetAbilityOnCooldown() => lastTimeAbility = Time.time;
    public void SetJumpAttackOnCooldown() => lastTimeJump = Time.time;
    #endregion

    #region Damage Methods

    public override void Die()
    {
        base.Die();

        if (!HealthController.muteDeathSound)
            bossSFX.deadSFX.Play();

        if (stateMachine.currentState != deadState)
            stateMachine.ChangeState(deadState);

        if (minimapIcon != null)
            minimapIcon.SetActive(false);

        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Enemy"));
    }

    // Set layer for this object and all children
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, newLayer);
    }
    #endregion
}
