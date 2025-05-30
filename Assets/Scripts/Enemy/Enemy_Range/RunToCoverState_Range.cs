using UnityEngine;

public class RunToCoverState_Range : EnemyState
{
    private Enemy_Range enemy;
    private Vector3 destination;

    private float footstepTimer;
    private float footstepInterval;

    public float lastTimeTookCover { get; private set; }

    public RunToCoverState_Range(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Range)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();
        destination = enemy.currentCover.transform.position;

        enemy.visual.EnableIK(true, false);
        enemy.agent.isStopped = false;
        enemy.agent.speed = enemy.runSpeed;
        enemy.agent.SetDestination(destination);

        footstepInterval = CalculateFootstepInterval(enemy.agent.speed);
        footstepTimer = 0f;
        PlayFootstepSFX();
    }

    public override void Exit()
    {
        base.Exit();
        lastTimeTookCover = Time.time;
    }

    public override void Update()
    {
        base.Update();
        enemy.FaceTarget(enemy.agent.steeringTarget);

        // Switch to battle state when cover is reached
        if (Vector3.Distance(enemy.transform.position, destination) < 0.5f)
            stateMachine.ChangeState(enemy.battleState);

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
