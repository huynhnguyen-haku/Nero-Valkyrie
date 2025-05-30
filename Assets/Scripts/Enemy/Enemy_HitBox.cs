using UnityEngine;

public class Enemy_HitBox : HitBox
{
    private Enemy enemy;

    protected override void Awake()
    {
        base.Awake();
        enemy = GetComponentInParent<Enemy>();
    }

    // Apply damage multiplier based on the hitbox
    // This is useful for different hitboxes having different damage multipliers
    public override void TakeDamage(int damage)
    {
        int newDamage = Mathf.RoundToInt(damage * damageMultiplier);
        enemy.GetHit(newDamage);
    }
}
