using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AttackData_EnemyMelee
{
    public int attackDamage;
    public string attackName;
    public float attackRange;
    public float moveSpeed;
    public float attackIndex;
    [Range(1, 2)]
    public float animationSpeed;
    public AttackType_Melee attackType;
}

public enum AttackType_Melee { Close, Charge }
public enum EnemyMelee_Type { Regular, Shield, Dodge, AxeThrow }

public class Enemy_Melee : Enemy
{
    public Enemy_MeleeSFX meleeSFX { get; private set; }

    #region States
    public IdleState_Melee idleState { get; private set; }
    public MoveState_Melee moveState { get; private set; }
    public RecoveryState_Melee recoveryState { get; private set; }
    public ChaseState_Melee chaseState { get; private set; }
    public AttackState_Melee attackState { get; private set; }
    public DeadState_Melee deadState { get; private set; }
    public AbilityState_Melee abilityState { get; private set; }
    #endregion

    #region Fields
    [Header("Enemy Setting")]
    public EnemyMelee_Type meleeType;
    public Enemy_MeleeWeaponType weaponType;

    [Header("Shield")]
    public int shieldDurability;
    public Transform shieldTransform;

    [Header("Dodge")]
    public float dodgeCooldown;
    private float lastDodgeTime;

    [Header("Attack Data")]
    public AttackData_EnemyMelee attackData;
    public List<AttackData_EnemyMelee> attackList;
    private Enemy_WeaponModel currentWeapon;
    private bool isAttackReady;

    [Header("Special Attack")]
    public int axeDamage;
    public GameObject axePrefab;
    public float axeFlySpeed;
    public float axeAimTimer;
    public float axeThrowCooldown;
    private float lastAxeThrowTime;
    public Transform axeStartPoint;

    [Header("Minimap Icon")]
    private GameObject minimapIcon;

    [Space]
    [SerializeField] private GameObject meleeAttackFX;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();

        meleeSFX = GetComponent<Enemy_MeleeSFX>();

        idleState = new IdleState_Melee(this, stateMachine, "Idle");
        moveState = new MoveState_Melee(this, stateMachine, "Move");
        recoveryState = new RecoveryState_Melee(this, stateMachine, "Recovery");
        chaseState = new ChaseState_Melee(this, stateMachine, "Chase");
        attackState = new AttackState_Melee(this, stateMachine, "Attack");
        deadState = new DeadState_Melee(this, stateMachine, "Idle"); // Uses ragdoll on death
        abilityState = new AbilityState_Melee(this, stateMachine, "AxeThrow");

        // Add minimap icon if available
        if (minimapIcon == null)
        {
            var minimapSprite = GetComponentInChildren<MinimapSprite>(true);
            if (minimapSprite != null)
                minimapIcon = minimapSprite.gameObject;
        }

        lastDodgeTime = Time.realtimeSinceStartup;
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);

        InitializePerk();
        visual.SetupVisual();

        RandomizeFirstAttack();
        UpdateAttackData();
    }

    // Randomize the first attack to avoid repetition
    private void RandomizeFirstAttack()
    {
        if (attackList.Count > 0)
            attackData = attackList[Random.Range(0, attackList.Count)];
    }

    protected override void Update()
    {
        base.Update();
        stateMachine.currentState.Update();

        MeleeAttackCheck(currentWeapon.damagePoints, currentWeapon.attackCheckRadius, meleeAttackFX, attackData.attackDamage);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackData.attackRange);
    }
    #endregion

    #region State Methods
    public override void EnterBattleMode()
    {
        if (inBattleMode)
            return;

        base.EnterBattleMode();
        UpdateAttackData();
        stateMachine.ChangeState(recoveryState);
    }

    public override void AbilityTrigger()
    {
        base.AbilityTrigger();
        walkSpeed = walkSpeed * 0.5f;
        visual.EnableWeaponModel(false);
    }

    public override void Die()
    {
        base.Die();

        if (!HealthController.muteDeathSound)
            meleeSFX.deadSFX.Play();

        if (stateMachine.currentState != deadState)
            stateMachine.ChangeState(deadState);

        if (minimapIcon != null)
            minimapIcon.SetActive(false);
    }

    // Update the current weapon and attack data
    public void UpdateAttackData()
    {
        currentWeapon = visual.currentWeaponModel.GetComponent<Enemy_WeaponModel>();
        if (currentWeapon.weaponData != null)
        {
            attackList = new List<AttackData_EnemyMelee>(currentWeapon.weaponData.attackData);
            turnSpeed = currentWeapon.weaponData.turnSpeed;
        }
    }

    // Throw an axe at the player
    public void ThrowAxe()
    {
        GameObject newAxe = ObjectPool.instance.GetObject(axePrefab, axeStartPoint);
        newAxe.GetComponent<Enemy_Axe>().AxeSetup(axeFlySpeed, player, axeAimTimer, axeDamage);
    }

    // True if the enemy can throw an axe
    public bool CanThrowAxe()
    {
        if (meleeType != EnemyMelee_Type.AxeThrow)
            return false;

        if (Time.time > lastAxeThrowTime + axeThrowCooldown)
        {
            lastAxeThrowTime = Time.time;
            return true;
        }
        return false;
    }

    // True if the player is within attack range
    public bool PlayerInAttackRange()
    {
        return Vector3.Distance(transform.position, player.position) < attackData.attackRange;
    }
    #endregion

    #region Ability Methods
    // Activate a dodge roll if conditions are met
    public void ActivateDodgeRoll()
    {
        // Only dodge roll if the player is outside of attack range
        if (Vector3.Distance(transform.position, player.position) < attackData.attackRange)
            return;

        // Only dodge roll if the enemy is a dodge type
        if (meleeType != EnemyMelee_Type.Dodge)
            return;

        // Only dodge roll during chase state
        if (stateMachine.currentState != chaseState)
            return;

        if (Time.time > lastDodgeTime + dodgeCooldown)
        {
            lastDodgeTime = Time.time;
            anim.SetTrigger("Dodge");
        }
    }
    #endregion

    #region Initialization Methods
    // Initialize perks based on enemy type
    protected override void InitializePerk()
    {
        if (meleeType == EnemyMelee_Type.Shield)
        {
            anim.SetFloat("ChaseIndex", 1);
            shieldTransform.gameObject.SetActive(true);
            weaponType = Enemy_MeleeWeaponType.OneHand;
        }
        else if (meleeType == EnemyMelee_Type.Dodge)
            weaponType = Enemy_MeleeWeaponType.Unarmed;
        else if (meleeType == EnemyMelee_Type.AxeThrow)
            weaponType = Enemy_MeleeWeaponType.Throw;
    }
    #endregion
}
