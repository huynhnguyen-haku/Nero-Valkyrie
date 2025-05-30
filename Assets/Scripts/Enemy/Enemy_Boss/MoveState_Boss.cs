using UnityEngine;

public class MoveState_Boss : EnemyState
{
    private Enemy_Boss enemy;

    private Vector3 destination;
    private float actionTimer;
    private float timeBeforeSpeedUp = 5f;
    private bool SpeedUpActive;

    private float footstepTimer;
    private float footstepInterval;

    public MoveState_Boss(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        this.enemy = (Enemy_Boss)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();
        SpeedReset();
        enemy.agent.isStopped = false;

        destination = enemy.GetPatrolDestination();
        enemy.agent.SetDestination(destination);
        actionTimer = enemy.actionCooldown;

        footstepInterval = CalculateFootstepInterval(enemy.agent.speed);
        footstepTimer = 0f;
    }

    public override void Update()
    {
        base.Update();
        actionTimer -= Time.deltaTime;
        enemy.FaceTarget(enemy.agent.steeringTarget);

        if (enemy.inBattleMode)
        {
            // In battle mode: chase player and perform actions
            if (ShouldSpeedUp())
                SpeedUp();

            Vector3 playerPosition = enemy.player.position;
            enemy.agent.SetDestination(playerPosition);

            if (actionTimer < 0)
                PerfomRandomAction(); // Try jump attack or special ability

            else if (enemy.PlayerInAttackRange())
                stateMachine.ChangeState(enemy.attackState); // Normal attack if in range
        }
        else
        {
            // Patrol mode: switch to idle when close to destination
            if (Vector3.Distance(enemy.transform.position, destination) < 0.25f)
                stateMachine.ChangeState(enemy.idleState);
        }

        HandleFootstepSFX();
    }
    #endregion

    #region Footstep Sound Effects

    private void HandleFootstepSFX()
    {
        footstepTimer += Time.deltaTime;
        if (footstepTimer >= footstepInterval)
        {
            footstepTimer = 0f;
            PlayFootstepSFX();
        }
    }

    // Play walk or run SFX based on speed state
    private void PlayFootstepSFX()
    {
        if (SpeedUpActive)
        {
            enemy.bossSFX.walkSFX.Stop();
            enemy.bossSFX.runSFX.PlayOneShot(enemy.bossSFX.runSFX.clip);
        }
        else
        {
            enemy.bossSFX.runSFX.Stop();
            enemy.bossSFX.walkSFX.PlayOneShot(enemy.bossSFX.walkSFX.clip);
        }
    }

    private float CalculateFootstepInterval(float speed)
        => Mathf.Clamp(1f / speed, 0.3f, 0.5f);
    #endregion

    #region Speed Control Methods

    private void SpeedReset()
    {
        SpeedUpActive = false;
        enemy.anim.SetFloat("MoveIndex", 0); // Walk animation
        enemy.agent.speed = enemy.walkSpeed;
        footstepInterval = CalculateFootstepInterval(enemy.walkSpeed);
    }

    private void SpeedUp()
    {
        SpeedUpActive = true;
        enemy.agent.speed = enemy.runSpeed;
        enemy.anim.SetFloat("MoveIndex", 1); // Run animation
        footstepInterval = CalculateFootstepInterval(enemy.runSpeed);
    }

    // True if enough time has passed since last attack to speed up
    private bool ShouldSpeedUp()
    {
        if (SpeedUpActive)
            return false;

        if (Time.time > enemy.attackState.lastTimeAttack + timeBeforeSpeedUp)
            return true;

        return false;
    }
    #endregion

    #region Action Logic

    // Decide whether to use special ability or jump attack
    private void PerfomRandomAction()
    {
        actionTimer = enemy.actionCooldown;

        if (Random.Range(0, 2) == 0)
            ActiveSpecialAbility(); // 50% chance to use special ability
        else
        {
            if (enemy.CanDoJumpAttack())
                stateMachine.ChangeState(enemy.jumpAttackState); // Prioritize jump attack
            else
            {
                switch (enemy.weaponType)
                {
                    case BossWeaponType.Hammer:
                        ActiveSpecialAbility();  // Use hammer ability
                        break;

                    case BossWeaponType.Flamethrower:
                        ActiveSpecialAbility(); // Use flamethrower ability
                        break;
                }
            }
        }
    }

    // Switch to ability state if possible
    private void ActiveSpecialAbility()
    {
        if (enemy.CanDoAbility())
            stateMachine.ChangeState(enemy.abilityState);
    }
    #endregion
}
