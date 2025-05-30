using UnityEngine;

public class IdleState_Melee : EnemyState
{
    private Enemy_Melee enemy;

    public IdleState_Melee(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Melee)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();
        stateTimer = enemyBase.idleTime; // Set idle duration
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        // Switch to move state when idle timer runs out
        if (stateTimer <= 0)
            stateMachine.ChangeState(enemy.moveState);
    }
    #endregion
}
