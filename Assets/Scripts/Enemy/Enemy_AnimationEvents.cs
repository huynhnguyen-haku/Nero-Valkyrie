using UnityEngine;

public class Enemy_AnimationEvents : MonoBehaviour
{
    private Enemy enemy;
    private Enemy_Melee melee;
    private Enemy_Boss boss;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
        melee = GetComponentInParent<Enemy_Melee>();
        boss = GetComponentInParent<Enemy_Boss>();
    }

    // Called by animation event to trigger state logic
    public void AnimationTrigger()
    {
        enemy.AnimationTrigger();
    }

    // Enable/disable manual movement during animation
    public void StartManualMovement() => enemy.ActivateManualMovement(true);
    public void StopManualMovement() => enemy.ActivateManualMovement(false);

    // Enable/disable manual rotation during animation
    public void StartManualRotation() => enemy.ActivateManualRotation(true);
    public void StopManualRotation() => enemy.ActivateManualRotation(false);

    // Called by animation event to trigger ability logic
    public void AbilityEvent()
    {
        enemy.AbilityTrigger();
    }

    // Enable IK for aiming/holding weapon
    public void EnableIK() => enemy.visual.EnableIK(true, true, 1f);

    // Show weapon model and hide holding model
    public void EnableWeaponModel()
    {
        enemy.visual.EnableWeaponModel(true);
        enemy.visual.EnableHoldingWeaponModel(false);
    }

    // Boss jump attack impact event
    public void BossJumpImpact()
    {
        boss?.JumpImpact();
    }

    // Begin melee attack check and play SFX
    public void BeginMeleeAttackCheck()
    {
        enemy?.EnableMeleeAttack(true);

        // Play melee attack SFX for melee and boss
        enemy?.audioManager.PlaySFX(melee?.meleeSFX.slashSFX, true);
        enemy?.audioManager.PlaySFX(boss?.bossSFX.slashSFX, true);
    }

    // End melee attack check
    public void FinishMeleeAttackCheck()
    {
        enemy?.EnableMeleeAttack(false);
    }
}
