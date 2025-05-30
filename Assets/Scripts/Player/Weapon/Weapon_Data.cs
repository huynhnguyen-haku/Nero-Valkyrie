using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Data", menuName = "ScriptableObjects/Weapon Data", order = 1)]
public class Weapon_Data : ScriptableObject
{
    public string weaponName;
    public WeaponType weaponType;

    [Header("Bullet Info")]
    public int bulletDamage;

    [Header("Weapon Details")]
    public ShotType shotType;
    public int bulletsPerShot = 1;
    public float fireRate;

    [Header("Magazine Details")]
    public int bulletsInMagazine;
    public int magazineCapacity;
    public int TotalReserveAmmo;

    [Header("Weapon Settings")]
    [Range(1, 6)]
    public float reloadSpeed = 1;
    [Range(1, 3)]
    public float equipSpeed = 1;
    [Range(4, 8)]
    public float cameraDistance = 6;


    [Header("Spread")]
    public float minSpread;
    public float maxSpread;
    public float spreadIncreaseRate = 0.15f;

    [Header("Burst Fire")]
    public bool burstAvailable;
    public bool burstActive;
    public int burst_BulletsPerShot;
    public int burst_FireRate;
    public float burst_FireDelay = 0.1f;

    [Header("Weapon Icon")]
    public Sprite weaponIcon;
    public string weaponInfo;

    [Header("Unlock System")]
    public bool isUnlocked = false; 
    public int price = 100;

    public void SaveUnlockState()
    {
        if (WeaponUnlockManager.instance != null)
        {
            WeaponUnlockManager.instance.SaveWeaponState(this);
        }
    }

    public void LoadUnlockState()
    {
        if (WeaponUnlockManager.instance != null)
        {
            WeaponUnlockManager.instance.LoadWeaponState(this);
        }
    }

}
