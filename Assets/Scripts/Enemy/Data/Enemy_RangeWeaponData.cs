using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Enemy Data/Range Weapon Data", order = 1)]

public class Enemy_RangeWeaponData : ScriptableObject
{
    [Header("Weapon Details")]
    public Enemy_RangeWeaponType weaponType;
    public float fireRate = 120;

    public int minBulletPerAttack = 1;
    public int maxBulletPerAttack = 1;

    public int minWeaponCooldown = 2;
    public int maxWeaponCooldown = 3;

    [Header("Bullet Details")]
    public int bulletDamage;
    [Space]
    public float bulletSpeed = 20;
    public float weaponSpread = 0.1f;

    // Set random number of bullets per attack
    public int GetRandomBulletPerAttack()
    {
        return Random.Range(minBulletPerAttack, maxBulletPerAttack);
    }

    // Set random reload time
    public float GetRandomWeaponCooldown()
    {
        return Random.Range(minWeaponCooldown, maxWeaponCooldown);
    }

    // Apply weapon spread to the bullet direction
    public Vector3 ApplyWeaponSpread(Vector3 originalDirection)
    {
        float randomizedValue = UnityEngine.Random.Range(-weaponSpread, weaponSpread);
        Quaternion spreadRotation = Quaternion.Euler(randomizedValue, randomizedValue, randomizedValue);

        return spreadRotation * originalDirection;
    }
}
