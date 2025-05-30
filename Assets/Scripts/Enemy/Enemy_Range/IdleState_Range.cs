using UnityEngine;

public class IdleState_Range : EnemyState
{
    private Enemy_Range enemy; 

    public IdleState_Range(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Range)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();
        enemy.visual.EnableIK(true, false); // Enable IK for idle pose
        stateTimer = enemy.idleTime;        // Set idle duration
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // Switch to move state when idle timer runs out
        if (stateTimer < 0)
            stateMachine.ChangeState(enemy.moveState);
    }
    #endregion
}
