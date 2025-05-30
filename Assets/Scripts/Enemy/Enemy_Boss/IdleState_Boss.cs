using UnityEngine;

public class IdleState_Boss : EnemyState
{
    private Enemy_Boss enemy;

    public IdleState_Boss(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Boss)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();
        stateTimer = enemy.idleTime;
    }

    public override void Update()
    {
        base.Update();

        // Switch to attack state if player is in range and boss is in battle mode
        if (enemy.inBattleMode && enemy.PlayerInAttackRange())
            stateMachine.ChangeState(enemy.attackState);

        // Switch to move state when idle timer runs out
        if (stateTimer < 0)
            stateMachine.ChangeState(enemy.moveState);
    }
    #endregion
}
