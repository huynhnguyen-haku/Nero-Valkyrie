using UnityEngine;

public class JumpAttackState_Boss : EnemyState
{
    private Enemy_Boss enemy;

    private Vector3 lastPlayerPosition;      // Player's position at jump start
    private float jumpAttackMovementSpeed;   // Speed for jump attack movement

    public JumpAttackState_Boss(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Boss)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();

        // Store player's position for targeting
        lastPlayerPosition = enemy.player.position;

        enemy.agent.isStopped = true;
        enemy.agent.velocity = Vector3.zero;

        // Show landing zone effect at target position
        enemy.bossVisual.PlaceLandingZone(lastPlayerPosition);
        enemy.bossVisual.EnableWeaponTrail(true);

        // Calculate movement speed for jump attack
        float distanceToPlayer = Vector3.Distance(lastPlayerPosition, enemy.transform.position);
        jumpAttackMovementSpeed = distanceToPlayer / enemy.travelTimeToTarget;
        enemy.FaceTarget(lastPlayerPosition, 1000);

        // For hammer boss, use NavMeshAgent to move toward player
        if (enemy.weaponType == BossWeaponType.Hammer)
        {
            enemy.agent.isStopped = false;
            enemy.agent.speed = enemy.runSpeed;
            enemy.agent.SetDestination(lastPlayerPosition);
        }
    }

    public override void Update()
    {
        base.Update();

        Vector3 myPosition = enemy.transform.position;
        enemy.agent.enabled = !enemy.ManualMovementActive();

        // Use manual movement if active, otherwise use NavMeshAgent
        if (enemy.ManualMovementActive())
        {
            enemy.agent.velocity = Vector3.zero;
            enemy.transform.position = Vector3.MoveTowards(myPosition, lastPlayerPosition, jumpAttackMovementSpeed * Time.deltaTime);
        }

        // Switch to move state when jump attack is complete
        if (triggerCalled)
            stateMachine.ChangeState(enemy.moveState);
    }

    public override void Exit()
    {
        base.Exit();

        // Set jump attack on cooldown and disable weapon trail
        enemy.SetJumpAttackOnCooldown();
        enemy.bossVisual.EnableWeaponTrail(false);
    }
    #endregion
}
