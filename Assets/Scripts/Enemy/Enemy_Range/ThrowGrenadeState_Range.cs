using UnityEngine;

public class ThrowGrenadeState_Range : EnemyState
{
    private Enemy_Range enemy;
    public bool finishedThrowing { get; private set; } = true;

    public ThrowGrenadeState_Range(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Range)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();
        finishedThrowing = false;

        enemy.visual.EnableWeaponModel(false);
        enemy.visual.EnableIK(false, false);
        enemy.visual.EnableHoldingWeaponModel(true);
        enemy.visual.EnableGrenadeModel(true);
    }

    public override void Update()
    {
        base.Update();

        Vector3 playerPosition = enemy.player.position + Vector3.up;
        enemy.FaceTarget(playerPosition);
        enemy.aim.position = playerPosition;

        // Switch to battle state after throwing
        if (triggerCalled)
            stateMachine.ChangeState(enemy.battleState);
    }
    #endregion

    #region Ability Trigger Logic

    public override void AbilityTrigger()
    {
        base.AbilityTrigger();
        finishedThrowing = true;
        enemy.ThrowGrenade();
    }
    #endregion
}
