public class DeadState_Melee : EnemyState
{
    private Enemy_Melee enemy;

    public DeadState_Melee(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Melee)enemyBase;
    }

    #region State Lifecycle Methods
    public override void Enter()
    {
        base.Enter();
        stateTimer = 1.5f;

        // Stop all footstep sounds
        if (enemy.meleeSFX.walkSFX.isPlaying)
            enemy.meleeSFX.walkSFX.Stop();

        if (enemy.meleeSFX.runSFX.isPlaying)
            enemy.meleeSFX.runSFX.Stop();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
    #endregion
}

