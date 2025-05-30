using UnityEngine;

public class DeadState_Boss : EnemyState
{
    private Enemy_Boss enemy;

    public DeadState_Boss(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Boss)enemyBase;
    }

    #region State Lifecycle Methods
    public override void Enter()
    {
        base.Enter();

        // Disable flamethrower if active
        enemy.abilityState.DisableFlamethrower();
        stateTimer = 1.5f;

        // Stop all footstep sounds
        if (enemy.bossSFX.walkSFX.isPlaying)
            enemy.bossSFX.walkSFX.Stop();

        if (enemy.bossSFX.runSFX.isPlaying)
            enemy.bossSFX.runSFX.Stop();
    }

    public override void Update()
    {
        base.Update();
    }
    #endregion
}