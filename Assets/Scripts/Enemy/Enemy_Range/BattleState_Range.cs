using UnityEngine;

public class BattleState_Range : EnemyState
{
    private Enemy_Range enemy;

    private float lastTimeShot;         // Last time a shot was fired
    private int bulletsShoot = 0;       // Bullets fired in current attack

    private int bulletsPerAttack;       // Bullets to fire per attack
    private float weaponCooldown;       // Cooldown between attacks

    private float coverCheckTimer;      // Timer for cover checks
    private bool firstTimeAttack = true;// True if this is the first attack

    public BattleState_Range(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Range)enemyBase;
        lastTimeShot = Time.realtimeSinceStartup;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();
        SetupFirstAttack();

        enemy.agent.isStopped = true;
        enemy.agent.velocity = Vector3.zero;

        enemy.visual.EnableIK(true, true);
        stateTimer = enemy.attackDelay;
    }

    public override void Update()
    {
        base.Update();

        if (enemy.IsSeeingPlayer())
            enemy.FaceTarget(enemy.aim.position);

        if (enemy.CanThrowGrenade())
            stateMachine.ChangeState(enemy.throwGrenadeState);

        if (HandleAdvancePlayer())
            stateMachine.ChangeState(enemy.advancePlayerState);

        ChangeCoverIfShould();

        if (stateTimer > 0)
            return;

        // Out of bullets for this attack
        if (WeaponOutOfBullets())
        {
            if (enemy.IsUnstoppable() && UnStoppableWalkReady())
            {
                enemy.advanceDuration = weaponCooldown;
                stateMachine.ChangeState(enemy.advancePlayerState);
            }

            // If weapon cooldown is over, reset weapon
            if (WeaponOnCooldown())
                AttempToResetWeapon();

            return;
        }

        // Can shoot and aiming at player
        if (CanShoot() && enemy.IsAimingOnPlayer())
            Shoot();
    }
    #endregion

    #region Weapon System

    // Setup values for the first attack (aggression range, bullet count, cooldown)
    private void SetupFirstAttack()
    {
        if (firstTimeAttack)
        {
            enemy.arrgresssionRange = enemy.advanceStoppingDistance + 2;
            firstTimeAttack = false;
            bulletsPerAttack = enemy.weaponData.GetRandomBulletPerAttack();
            weaponCooldown = enemy.weaponData.GetRandomWeaponCooldown();
        }
    }

    // Reset weapon for next attack (reset bullet count, get new attack/cooldown values)
    private void AttempToResetWeapon()
    {
        bulletsShoot = 0;
        bulletsPerAttack = enemy.weaponData.GetRandomBulletPerAttack();
        weaponCooldown = enemy.weaponData.GetRandomWeaponCooldown();
    }

    // Fire one bullet and update counters
    private void Shoot()
    {
        enemy.FireSingleBullet();
        lastTimeShot = Time.time;
        bulletsShoot++;
    }

    // True if cooldown after attack is over
    private bool WeaponOnCooldown()
        => Time.time > lastTimeShot + weaponCooldown;

    // True if all bullets for this attack have been fired
    private bool WeaponOutOfBullets()
        => bulletsShoot >= bulletsPerAttack;

    // True if enough time has passed since last shot
    private bool CanShoot()
    {
        float timeBetweenShots = 60f / enemy.weaponData.fireRate;
        return Time.time > lastTimeShot + timeBetweenShots;
    }
    #endregion

    #region Cover System

    // Change cover if conditions are met
    private void ChangeCoverIfShould()
    {
        if (enemy.coverPerk != CoverPerk.ChangeCover)
            return;

        // Check the cover every 0.5 seconds
        coverCheckTimer -= Time.deltaTime;
        if (coverCheckTimer < 0)
            coverCheckTimer = 0.5f;

        if (ReadyToChangeCover() && ReadyToLeaveCover())
        {
            if (enemy.CanGetCover())
                stateMachine.ChangeState(enemy.runToCoverState);
        }
    }

    // True if player is a threat and advance time is over
    private bool ReadyToChangeCover()
    {
        bool inDanger = IsPlayerInClearSight() || IsPlayerClose();
        bool advanceTimeIsOver = Time.time > enemy.advancePlayerState.lastTimeAdvanced + enemy.advanceDuration;
        return inDanger && advanceTimeIsOver;
    }

    // True if enough time has passed since last cover
    private bool ReadyToLeaveCover()
        => Time.time > enemy.coverTime + enemy.runToCoverState.lastTimeTookCover;

    // True if player is within safe distance
    private bool IsPlayerClose()
        => Vector3.Distance(enemy.transform.position, enemy.player.transform.position) < enemy.safeDistance;

    // True if player is in direct line of sight
    private bool IsPlayerInClearSight()
    {
        Vector3 directionToPlayer = enemy.player.transform.position - enemy.transform.position;
        if (Physics.Raycast(enemy.transform.position, directionToPlayer, out RaycastHit hit))
            return hit.transform.root == enemy.player.root;
        return false;
    }
    #endregion

    #region Advance Logic

    // True if enemy should advance toward player
    private bool HandleAdvancePlayer()
    {
        if (enemy.IsUnstoppable())
            return false;
        // Player is out of aggression range, and enemy can leave cover
        return !enemy.IsPlayerInAgrressionRage() && ReadyToLeaveCover();
    }

    // True if unstoppable walk is ready (distance and cooldown)
    public bool UnStoppableWalkReady()
    {
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
        bool outOfStoppingDistance = distanceToPlayer > enemy.advanceStoppingDistance;
        bool onCooldown = Time.time < enemy.weaponData.minWeaponCooldown + enemy.advancePlayerState.lastTimeAdvanced;
        return outOfStoppingDistance && !onCooldown;
    }
    #endregion
}
