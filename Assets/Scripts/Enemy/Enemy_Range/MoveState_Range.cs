using UnityEngine;

public class MoveState_Range : EnemyState
{
    private Enemy_Range enemy;
    private Vector3 destination;

    private float footstepTimer;
    private float footstepInterval;

    public MoveState_Range(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Range)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();
        enemy.visual.EnableIK(true, false);
        enemy.agent.speed = enemy.walkSpeed;
        destination = enemy.GetPatrolDestination();
        enemy.agent.SetDestination(destination);

        footstepInterval = CalculateFootstepInterval(enemy.agent.speed);
        footstepTimer = 0f;
        PlayFootstepSFX();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        enemy.FaceTarget(enemy.agent.steeringTarget);

        // Switch to idle state when destination is reached
        if (enemy.agent.remainingDistance <= enemy.agent.stoppingDistance + 0.05f)
            stateMachine.ChangeState(enemy.idleState);

        HandleFootstepSFX();
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
        if (enemy.rangeSFX.runSFX.isPlaying)
            enemy.rangeSFX.runSFX.Stop();

        if (!enemy.rangeSFX.walkSFX.isPlaying)
            enemy.rangeSFX.walkSFX.PlayOneShot(enemy.rangeSFX.walkSFX.clip);
    }

    // Interval between footstep sounds based on speed
    private float CalculateFootstepInterval(float speed)
    {
        return Mathf.Clamp(1f / speed, 0.4f, 0.7f);
    }
    #endregion
}
