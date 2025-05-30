using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AttackState_Melee : EnemyState
{
    private Enemy_Melee enemy;

    private Vector3 attackDirection;      // Direction to move during attack
    private float attackMoveSpeed;        // Speed during attack
    private const float MAX_ATTACK_DISTANCE = 50f; // Max distance for charge attack

    public AttackState_Melee(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Melee)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();

        enemy.UpdateAttackData();
        enemy.visual.EnableWeaponModel(true);
        enemy.visual.EnableWeaponTrail(true);

        attackMoveSpeed = enemy.attackData.moveSpeed;
        enemy.anim.SetFloat("AttackAnimationSpeed", enemy.attackData.animationSpeed);
        enemy.anim.SetFloat("AttackIndex", enemy.attackData.attackIndex);

        // Randomize slash animation
        enemy.anim.SetFloat("SlashAttackIndex", Random.Range(0, 6));

        enemy.agent.isStopped = true;
        enemy.agent.velocity = Vector3.zero;

        attackDirection = enemy.transform.position + (enemy.transform.forward * MAX_ATTACK_DISTANCE);
    }

    public override void Update()
    {
        base.Update();

        // Update direction if manual rotation is active
        if (enemy.ManualRotationActive())
        {
            enemy.FaceTarget(enemy.player.position);
            attackDirection = enemy.transform.position + (enemy.transform.forward * MAX_ATTACK_DISTANCE);
        }

        // Move enemy if manual movement is active
        if (enemy.ManualMovementActive())
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, attackDirection, attackMoveSpeed * Time.deltaTime);

        // Switch to recovery or chase state after attack animation
        if (triggerCalled)
        {
            if (enemy.PlayerInAttackRange())
                stateMachine.ChangeState(enemy.recoveryState); // Next action: throw axe, attack, or chase
            else
                stateMachine.ChangeState(enemy.chaseState);    // Chase player if not in range
        }
    }

    public override void Exit()
    {
        base.Exit();
        SetupNextAttack(); // Prepare next attack data
        enemy.visual.EnableWeaponTrail(false);
    }
    #endregion

    #region Attack Setup Methods

    private void SetupNextAttack()
    {
        int recoveryIndex = PlayerClose() ? 1 : 0;
        enemy.anim.SetFloat("RecoveryIndex", recoveryIndex);
        enemy.attackData = UpdatedAttackData();
    }

    // True if player is within 1 unit
    private bool PlayerClose() => Vector3.Distance(enemy.transform.position, enemy.player.position) <= 1;

    // Select new attack data, exclude charge if player is close
    private AttackData_EnemyMelee UpdatedAttackData()
    {
        List<AttackData_EnemyMelee> validAttacks = new List<AttackData_EnemyMelee>(enemy.attackList);
        if (PlayerClose())
            validAttacks.RemoveAll(parameter => parameter.attackType == AttackType_Melee.Charge);

        int random = Random.Range(0, validAttacks.Count);
        return validAttacks[random];
    }
    #endregion
}
