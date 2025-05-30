using UnityEngine;

public class AttackState_Boss : EnemyState
{
    private Enemy_Boss enemy;

    public float lastTimeAttack { get; private set; } // Last time the boss attacked

    public AttackState_Boss(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Boss)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();
        enemy.bossVisual.EnableWeaponTrail(true);

        // Randomize attack animation
        enemy.anim.SetFloat("AttackIndex", Random.Range(0, enemy.attackAnimationCount));
        enemy.agent.isStopped = true;
        stateTimer = 1f;
    }

    public override void Update()
    {
        base.Update();
        // Face player while attacking
        if (stateTimer > 0)
            enemy.FaceTarget(enemy.player.position, 20);

        // Switch state after attack animation
        if (triggerCalled)
        {
            if (enemy.PlayerInAttackRange())
                stateMachine.ChangeState(enemy.idleState); // Attack again if player is in range
            else
                stateMachine.ChangeState(enemy.moveState); // Chase player if not in range
        }
    }

    public override void Exit()
    {
        base.Exit();
        lastTimeAttack = Time.time; // Store last attack time for speed-up logic
        enemy.bossVisual.EnableWeaponTrail(false);
    }
    #endregion
}
