using UnityEngine;

public class AbilityState_Melee : EnemyState
{
    private Enemy_Melee enemy;

    private Vector3 movementDirection; // Direction to move during ability
    private float moveSpeed;           // Speed during ability
    private const float MAX_MOVEMENT_DISTANCE = 20f; // Max distance for ability move

    public AbilityState_Melee(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Melee)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();

        // Enable weapon model and set up movement for ability
        enemy.visual.EnableWeaponModel(true);
        moveSpeed = enemy.walkSpeed;
        movementDirection = enemy.transform.position + (enemy.transform.forward * MAX_MOVEMENT_DISTANCE);
    }

    public override void Update()
    {
        base.Update();

        // Update direction if manual rotation is active
        if (enemy.ManualRotationActive())
        {
            enemy.FaceTarget(enemy.player.position);
            movementDirection = enemy.transform.position + (enemy.transform.forward * MAX_MOVEMENT_DISTANCE);
        }

        // Move enemy if manual movement is active
        if (enemy.ManualMovementActive())
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, movementDirection, enemy.walkSpeed * Time.deltaTime);

        // Switch to recovery state when ability is complete
        if (triggerCalled)
            stateMachine.ChangeState(enemy.recoveryState);
    }

    public override void Exit()
    {
        base.Exit();

        // Reset speed and animation parameters
        enemy.walkSpeed = moveSpeed;
        enemy.anim.SetFloat("RecoveryIndex", 0);
    }
    #endregion

    #region Ability Trigger Logic

    public override void AbilityTrigger()
    {
        base.AbilityTrigger();
        enemy.ThrowAxe();
    }
    #endregion
}
