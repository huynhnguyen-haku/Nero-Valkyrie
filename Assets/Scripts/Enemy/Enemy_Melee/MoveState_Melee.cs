using UnityEngine;

public class MoveState_Melee : EnemyState
{
    private Enemy_Melee enemy;

    private Vector3 destination;   // Patrol destination
    private float footstepTimer;
    private float footstepInterval;

    public MoveState_Melee(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Melee)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();
        enemy.agent.speed = enemy.walkSpeed;

        // Set patrol destination
        destination = enemy.GetPatrolDestination();
        enemy.agent.SetDestination(destination);

        footstepInterval = CalculateFootstepInterval(enemy.agent.speed);
        footstepTimer = 0f;
        PlayFootstepSFX();
    }

    public override void Update()
    {
        base.Update();
        enemy.FaceTarget(enemy.agent.steeringTarget);

        // Switch to idle state when destination is reached
        if (enemy.agent.remainingDistance <= enemy.agent.stoppingDistance + 0.05f)
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }
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
        if (enemy.meleeSFX.runSFX.isPlaying)
            enemy.meleeSFX.runSFX.Stop();

        if (!enemy.meleeSFX.walkSFX.isPlaying)
            enemy.meleeSFX.walkSFX.PlayOneShot(enemy.meleeSFX.walkSFX.clip);
    }

    private float CalculateFootstepInterval(float speed)
    {
        return Mathf.Clamp(1f / speed, 0.12f, 0.12f);
    }
    #endregion
}
