using UnityEngine;

public class AdvancePlayerState_Range : EnemyState
{
    private Enemy_Range enemy;

    public float lastTimeAdvanced { get; private set; } // Last time this state was exited
    private Vector3 playerPosition;

    private float footstepTimer;
    private float footstepInterval;

    public AdvancePlayerState_Range(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Range)enemyBase;
    }

    #region State Lifecycle Methods
    public override void Enter()
    {
        base.Enter();
        enemy.visual.EnableIK(true, false);

        enemy.agent.isStopped = false;
        enemy.agent.speed = enemy.advanceSpeed;

        if (enemy.IsUnstoppable())
        {
            enemy.visual.EnableIK(true, false);
            stateTimer = enemy.advanceDuration;
        }

        footstepInterval = CalculateFootstepInterval(enemy.agent.speed);
        footstepTimer = 0f;
        PlayFootstepSFX();
    }

    public override void Update()
    {
        base.Update();
        playerPosition = enemy.player.transform.position;

        enemy.UpdateAimPosition();
        enemy.agent.SetDestination(enemy.player.transform.position);
        enemy.FaceTarget(enemy.agent.steeringTarget);

        // Change to the battle state if conditions are met
        if (CanEnterBattleState() && enemy.IsSeeingPlayer())
            stateMachine.ChangeState(enemy.battleState);

        HandleFootstepSFX();
    }

    public override void Exit()
    {
        base.Exit();
        lastTimeAdvanced = Time.time; // Mark when this state was exited
    }
    #endregion

    #region State Change Conditions
    // True if enemy should switch to battle state
    public bool CanEnterBattleState()
    {
        // Check if the player is within the enemy's aggression range
        bool closeEnoughToPlayer = Vector3.Distance(enemy.transform.position, playerPosition) < enemy.arrgresssionRange;

        if (enemy.IsUnstoppable())
            return closeEnoughToPlayer || stateTimer <= 0; // Also allow if advance duration is over
        else
            return closeEnoughToPlayer;
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

    private void PlayFootstepSFX()
    {
        if (enemy.rangeSFX.walkSFX.isPlaying)
            enemy.rangeSFX.walkSFX.Stop();

        if (!enemy.rangeSFX.runSFX.isPlaying)
            enemy.rangeSFX.runSFX.PlayOneShot(enemy.rangeSFX.runSFX.clip);
    }

    private float CalculateFootstepInterval(float speed)
    {
        return Mathf.Clamp(1f / speed, 0.3f, 0.5f);
    }
    #endregion
}
