using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_WeaponController : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsAlly;
    [Space]
    private Player player;
    private const float REFERENCE_BULLET_SPEED = 20;

    [SerializeField] private List<Weapon_Data> defaultWeaponData;
    [SerializeField] private Weapon currentWeapon;
    private bool weaponReady;
    private bool isShooting;

    [Header("Bullet Settings")]
    [SerializeField] private float bulletImpactForce;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;

    [SerializeField] private Transform weaponHolder;

    [Header("Inventory")]
    [SerializeField] private List<Weapon> weaponSlots;
    [SerializeField] private int maxSlots = 3;

    [SerializeField] private GameObject weaponPickupPrefab;

    [Header("Minigun")]
    private bool isSpinning;
    private bool isFireSFXPlaying;
    private Coroutine spinCoroutine;
    private Coroutine spinnerCoroutine;

    private void Start()
    {
        player = GetComponent<Player>();
        AssignInputEvents();
    }

    private void Update()
    {
        if (isShooting)
        {
            Shot();
        }
    }

    public void UpdateWeaponUI()
    {
        UI.instance.inGameUI.UpdateWeaponUI(weaponSlots, currentWeapon);
    }

    #region Slot Management

    // Set default weapons and equip the first one
    public void SetDefaultWeapon(List<Weapon_Data> newWeaponData)
    {
        defaultWeaponData = new List<Weapon_Data>(newWeaponData);
        weaponSlots.Clear();

        foreach (Weapon_Data weaponData in defaultWeaponData)
        {
            PickupWeapon(new Weapon(weaponData));
        }

        EquipWeapon(0);
    }

    // Equip weapon by index
    private void EquipWeapon(int i)
    {
        if (i >= weaponSlots.Count)
            return;

        if (currentWeapon == weaponSlots[i])
            return;

        SetWeaponReady(false);
        currentWeapon = weaponSlots[i];
        player.weaponVisuals.PlayWeaponEquipAnimation();
        UpdateWeaponUI();
    }

    // Add weapon to inventory or swap if full
    public void PickupWeapon(Weapon newWeapon)
    {
        // If weapon exists, add ammo
        if (WeaponInSlots(newWeapon.weaponType) != null)
        {
            WeaponInSlots(newWeapon.weaponType).TotalReserveAmmo += newWeapon.TotalReserveAmmo;
            UpdateWeaponUI();
            return;
        }

        // If inventory full, swap with current
        if (weaponSlots.Count >= maxSlots && newWeapon.weaponType != currentWeapon.weaponType)
        {
            int weaponIndex = weaponSlots.IndexOf(currentWeapon);

            player.weaponVisuals.SwitchOffWeaponModels();
            weaponSlots[weaponIndex] = newWeapon;

            CreateWeaponOnTheGround();
            EquipWeapon(weaponIndex);
            return;
        }

        weaponSlots.Add(newWeapon);
        player.weaponVisuals.SwitchOnBackupWeaponModels();
        UpdateWeaponUI();
    }

    // Drop current weapon and equip next
    private void DropWeapon()
    {
        if (HasOneWeapon())
            return;

        CreateWeaponOnTheGround();
        weaponSlots.Remove(currentWeapon);
        EquipWeapon(0);
    }

    // Instantiate dropped weapon in the world
    private void CreateWeaponOnTheGround()
    {
        GameObject droppedWeapon = ObjectPool.instance.GetObject(weaponPickupPrefab, transform);
        droppedWeapon.GetComponent<Weapon_PickUp>().SetUpPickupWeapon(currentWeapon, player.transform);
    }

    public void SetWeaponReady(bool ready)
    {
        weaponReady = ready;
    }

    public bool WeaponReady() => weaponReady;

    #endregion

    #region Shooting Mechanics

    // Handle burst fire logic
    private IEnumerator BurstFire()
    {
        for (int i = 1; i <= currentWeapon.bulletsPerShot; i++)
        {
            FireSingleBullet();
            yield return new WaitForSeconds(currentWeapon.burst_FireDelay);

            if (i >= currentWeapon.bulletsPerShot)
                SetWeaponReady(true);
        }
    }

    // Main shooting logic, including minigun and burst
    private void Shot()
    {
        if (player.health.playerIsDead)
            return;

        if (!WeaponReady())
            return;

        if (!currentWeapon.CanShot())
            return;

        if (currentWeapon.weaponType == WeaponType.Minigun)
        {
            if (!isSpinning)
            {
                spinCoroutine = StartCoroutine(SpinUpMinigun());
            }
            return;
        }

        player.weaponVisuals.PlayFireAnimation();

        if (currentWeapon.shotType == ShotType.Single)
            isShooting = false;

        if (currentWeapon.BurstActivated())
        {
            StartCoroutine(BurstFire());
            TriggerEnemyDodge();
            return;
        }
        FireSingleBullet();
        TriggerEnemyDodge();
    }

    // Minigun spin-up and fire loop
    private IEnumerator SpinUpMinigun()
    {
        isSpinning = true;

        var spinSFX = player.weaponVisuals.CurrentWeaponModel().spinSFX;
        if (spinSFX != null && !spinSFX.isPlaying)
            spinSFX.Play();

        var spinner = player.weaponVisuals.CurrentWeaponModel().minigunSpinner;
        if (spinner != null)
            spinnerCoroutine = StartCoroutine(SpinMinigunSpinner());

        yield return new WaitForSeconds(1.25f);

        if (!isFireSFXPlaying)
        {
            var fireSFX = player.weaponVisuals.CurrentWeaponModel().fireSFX;
            fireSFX.Play();
            isFireSFXPlaying = true;
        }

        while (isShooting && currentWeapon.weaponType == WeaponType.Minigun)
        {
            if (!currentWeapon.HaveEnoughBullet())
            {
                StopMinigunFire();
                yield break;
            }
            FireSingleBullet();
            yield return new WaitForSeconds(60f / currentWeapon.fireRate);
        }
        StopMinigunFire();
    }

    // Rotate minigun spinner visual
    private IEnumerator SpinMinigunSpinner()
    {
        var spinner = player.weaponVisuals.CurrentWeaponModel().minigunSpinner;

        while (isSpinning)
        {
            if (spinner != null)
                spinner.Rotate(0, 0, -1000f * Time.deltaTime);

            yield return null;
        }
    }

    // Stop minigun fire and play SFX
    private void StopMinigunFire()
    {
        isSpinning = false;

        if (spinnerCoroutine != null)
        {
            StopCoroutine(spinnerCoroutine);
            spinnerCoroutine = null;
        }

        if (isFireSFXPlaying)
        {
            var fireSFX = player.weaponVisuals.CurrentWeaponModel().fireSFX;
            fireSFX.Stop();
            isFireSFXPlaying = false;
        }

        var endSpinSFX = player.weaponVisuals.CurrentWeaponModel().endSpinSFX;
        if (endSpinSFX != null && !endSpinSFX.isPlaying)
            endSpinSFX.Play();
    }

    // Fire a single bullet, apply spread, and play SFX
    private void FireSingleBullet()
    {
        currentWeapon.bulletsInMagazine--;
        UpdateWeaponUI();

        if (currentWeapon.weaponType == WeaponType.Shotgun)
            player.weaponVisuals.CurrentWeaponModel().fireSFX.Play();
        else if (currentWeapon.weaponType != WeaponType.Minigun)
            player.weaponVisuals.CurrentWeaponModel().fireSFX.PlayOneShot(player.weaponVisuals.CurrentWeaponModel().fireSFX.clip);

        GameObject bullet = ObjectPool.instance.GetObject(bulletPrefab, GunPoint());
        Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();

        bullet.transform.rotation = Quaternion.LookRotation(GunPoint().forward);

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.BulletSetup(whatIsAlly, currentWeapon.bulletDamage, bulletImpactForce);

        Vector3 bulletsDirection = currentWeapon.ApplySpread(BulletDirection());

        rbBullet.mass = REFERENCE_BULLET_SPEED / bulletSpeed;
        rbBullet.linearVelocity = bulletsDirection * bulletSpeed;
    }

    // Play reload animation and SFX (actual reload in animation event)
    private void Reload()
    {
        SetWeaponReady(false);
        player.weaponVisuals.PlayReloadAnimation();
        player.weaponVisuals.CurrentWeaponModel().reloadSFX.Play();
    }
    #endregion

    #region Utility Methods

    // Get bullet direction from gun to aim
    public Vector3 BulletDirection()
    {
        Transform aim = player.aim.Aim();
        Vector3 direction = (aim.position - GunPoint().position).normalized;
        return direction;
    }

    public Transform GunPoint() => player.weaponVisuals.CurrentWeaponModel().gunPoint;
    public bool HasOneWeapon() => weaponSlots.Count <= 1;

    // Find weapon in slots by type
    public Weapon WeaponInSlots(WeaponType weaponType)
    {
        foreach (Weapon weapon in weaponSlots)
        {
            if (weapon.weaponType == weaponType)
                return weapon;
        }
        return null;
    }

    public Weapon CurrentWeapon() => currentWeapon;

    // Trigger enemy dodge if hit by raycast
    private void TriggerEnemyDodge()
    {
        Vector3 rayOrigin = GunPoint().position;
        Vector3 rayDirection = BulletDirection();

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, Mathf.Infinity))
        {
            Enemy_Melee enemy_melee = hit.collider.GetComponentInParent<Enemy_Melee>();
            if (enemy_melee != null)
            {
                enemy_melee.ActivateDodgeRoll();
            }
        }
    }
    #endregion

    #region Input Events

    // Assign input events for firing, equipping, reloading, etc.
    private void AssignInputEvents()
    {
        PlayerControls controls = player.controls;

        controls.Character.Fire.performed += context => isShooting = true;
        controls.Character.Fire.canceled += context => isShooting = false;

        controls.Character.EquipSlot1.performed += context => EquipWeapon(0);
        controls.Character.EquipSlot2.performed += context => EquipWeapon(1);
        controls.Character.EquipSlot3.performed += context => EquipWeapon(2);
        controls.Character.EquipSlot4.performed += context => EquipWeapon(3);
        controls.Character.EquipSlot5.performed += context => EquipWeapon(4);

        controls.Character.ToggleBurstMode.performed += context => currentWeapon.ToggleBurstMode();
        controls.Character.DropCurrentWeapon.performed += context => DropWeapon();
        controls.Character.Reload.performed += context =>
        {
            if (currentWeapon.CanReload() && WeaponReady())
            {
                Reload();
            }
        };
        controls.Character.Fire.canceled += context =>
        {
            isShooting = false;

            if (currentWeapon.weaponType == WeaponType.Minigun)
            {
                if (isSpinning)
                {
                    StopCoroutine(spinCoroutine);
                    isSpinning = false;
                    var spinSFX = player.weaponVisuals.CurrentWeaponModel().spinSFX;
                    spinSFX.Stop();

                    var endSpinSFX = player.weaponVisuals.CurrentWeaponModel().endSpinSFX;
                    endSpinSFX.Play();
                }

                if (isFireSFXPlaying)
                {
                    isFireSFXPlaying = false;
                    var fireSFX = player.weaponVisuals.CurrentWeaponModel().fireSFX;
                    fireSFX.Stop();
                }
            }
        };
    }

    #endregion
}
