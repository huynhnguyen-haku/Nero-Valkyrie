using UnityEngine;

public class ChaseState_Melee : EnemyState
{
    private Enemy_Melee enemy;

    private float lastTimeUpdateDestination; // Last time destination was updated
    private float footstepTimer;
    private float footstepInterval;

    public ChaseState_Melee(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Melee)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();
        enemy.agent.speed = enemy.runSpeed;
        enemy.agent.isStopped = false;

        footstepInterval = CalculateFootstepInterval(enemy.agent.speed);
        footstepTimer = 0f;
        PlayFootstepSFX();
    }

    public override void Update()
    {
        base.Update();
        enemy.FaceTarget(enemy.agent.steeringTarget);

        // Switch to attack state if player is in range
        if (enemy.PlayerInAttackRange())
            stateMachine.ChangeState(enemy.attackState);

        // Update destination to player's position if allowed
        if (CanUpdateDestination())
            enemy.agent.SetDestination(enemy.player.transform.position);

        HandleFootstepSFX();
    }

    public override void Exit()
    {
        base.Exit();
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
        if (enemy.meleeSFX.walkSFX.isPlaying)
            enemy.meleeSFX.walkSFX.Stop();

        if (!enemy.meleeSFX.runSFX.isPlaying)
            enemy.meleeSFX.runSFX.PlayOneShot(enemy.meleeSFX.runSFX.clip);
    }

    private float CalculateFootstepInterval(float speed)
    {
        return Mathf.Clamp(1f / speed, 0.1f, 0.1f);
    }
    #endregion

    #region Destination Logic

    // True if enough time has passed to update destination
    private bool CanUpdateDestination()
    {
        if (Time.time > lastTimeUpdateDestination + 0.25f)
        {
            lastTimeUpdateDestination = Time.time;
            return true;
        }
        return false;
    }
    #endregion
}
