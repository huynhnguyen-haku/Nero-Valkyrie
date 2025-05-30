using UnityEngine;

public class AbilityState_Boss : EnemyState
{
    private Enemy_Boss enemy;

    public AbilityState_Boss(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = (Enemy_Boss)enemyBase;
    }

    #region State Lifecycle Methods

    public override void Enter()
    {
        base.Enter();

        enemy.agent.isStopped = true;
        enemy.agent.velocity = Vector3.zero;

        stateTimer = enemy.flamethrowDuration;
        enemy.bossVisual.EnableWeaponTrail(true);
    }

    public override void Update()
    {
        base.Update();

        // Face the player while using ability
        enemy.FaceTarget(enemy.player.position);

        if (ShouldDisableFlamethrower())
            DisableFlamethrower();

        // Switch to move state when animation trigger is called
        if (triggerCalled)
            stateMachine.ChangeState(enemy.moveState);
    }

    public override void Exit()
    {
        base.Exit();

        // Reset cooldowns and visual effects
        enemy.SetAbilityOnCooldown();
        enemy.bossVisual.ResetBatteries();
        enemy.bossVisual.EnableWeaponTrail(false);
    }
    #endregion

    #region Ability Trigger Logic

    public override void AbilityTrigger()
    {
        base.AbilityTrigger();

        // Flamethrower: activate, discharge batteries, expand damage area
        if (enemy.weaponType == BossWeaponType.Flamethrower)
        {
            enemy.ActivateFlamethrower(true);
            enemy.bossVisual.DischargeBatteries();
            enemy.bossVisual.EnableWeaponTrail(false);

            Flamethrower_DamageArea damageArea = enemy.flamethrower.GetComponentInChildren<Flamethrower_DamageArea>();
            if (damageArea != null)
                damageArea.StartExpandingCollider();
        }

        // Hammer: activate and enable weapon trail
        if (enemy.weaponType == BossWeaponType.Hammer)
        {
            enemy.ActivateHammer();
            enemy.bossVisual.EnableWeaponTrail(true);
        }
    }
    #endregion

    #region Flamethrower Control Methods

    // True if flamethrower should be disabled (timer expired)
    private bool ShouldDisableFlamethrower()
    {
        return stateTimer < 0;
    }

    // Disable flamethrower if active
    public void DisableFlamethrower()
    {
        if (enemy.weaponType != BossWeaponType.Flamethrower)
            return;

        if (!enemy.flamethrowerActive)
            return;

        enemy.ActivateFlamethrower(false);
    }
    #endregion
}
