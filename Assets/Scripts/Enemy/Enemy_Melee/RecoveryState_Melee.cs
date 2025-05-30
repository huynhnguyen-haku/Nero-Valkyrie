using UnityEngine;

public class RecoveryState_Melee : EnemyState
{
    private Enemy_Melee enemy;

    public RecoveryState_Melee(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Melee)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();
        enemy.agent.isStopped = true;
    }

    public override void Update()
    {
        base.Update();
        enemy.FaceTarget(enemy.player.transform.position);

        // Switch to next state based on conditions when animation trigger is called
        if (triggerCalled)
        {
            if (enemy.CanThrowAxe())
                stateMachine.ChangeState(enemy.abilityState);      // Axe throw if available

            else if (enemy.PlayerInAttackRange())
                stateMachine.ChangeState(enemy.attackState);       // Attack if in range

            else
                stateMachine.ChangeState(enemy.chaseState);        // Otherwise chase player
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    #endregion
}
