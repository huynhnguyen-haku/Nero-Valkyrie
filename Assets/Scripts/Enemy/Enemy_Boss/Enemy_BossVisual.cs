using UnityEngine;

public class Enemy_BossVisual : MonoBehaviour
{
    private Enemy_Boss enemy;

    [Header("Jump Attack Visual")]
    [SerializeField] private float landingOffset = 1;
    [SerializeField] private ParticleSystem landingZoneFX;
    [SerializeField] private GameObject[] weaponTrails;

    [Header("Flamethrower Battery")]
    [SerializeField] private GameObject[] batteries;
    [SerializeField] private float initialBatteryCharge = 0.2f;

    [Space]
    private float dischargeSpeed;
    private float rechargeSpeed;
    private bool isRecharging;

    #region Unity Methods

    private void Awake()
    {
        enemy = GetComponent<Enemy_Boss>();

        landingZoneFX.transform.parent = null;
        landingZoneFX.Stop();

        ResetBatteries();
    }

    private void Update()
    {
        UpdateBatteriesScale();
    }
    #endregion

    #region Visual Effects Methods

    // Enable or disable weapon trail effects
    public void EnableWeaponTrail(bool active)
    {
        if (weaponTrails.Length <= 0)
            return;

        foreach (var trail in weaponTrails)
        {
            if (trail != null)
                trail.SetActive(active);
        }
    }

    // Place and play the landing zone effect for jump attack
    public void PlaceLandingZone(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        Vector3 offset = direction.normalized * landingOffset;

        landingZoneFX.transform.position = target + offset;
        landingZoneFX.Clear();

        var mainModule = landingZoneFX.main;
        mainModule.startLifetime = enemy.travelTimeToTarget * 2;

        landingZoneFX.Play();
    }
    #endregion

    #region Battery Management

    // Update battery scale based on charge/discharge state
    private void UpdateBatteriesScale()
    {
        if (batteries.Length <= 0)
            return;

        foreach (GameObject battery in batteries)
        {
            if (battery.activeSelf)
            {
                float scaleChange = (isRecharging ? rechargeSpeed : -dischargeSpeed) * Time.deltaTime;
                float newScaleY = Mathf.Clamp(battery.transform.localScale.y + scaleChange, 0, initialBatteryCharge);

                battery.transform.localScale = new Vector3(0.15f, newScaleY, 0.15f);

                if (battery.transform.localScale.y <= 0)
                    battery.SetActive(false); // Deactivate when empty
            }
        }
    }

    // Reset batteries to initial state and start recharging
    public void ResetBatteries()
    {
        isRecharging = true;
        rechargeSpeed = initialBatteryCharge / enemy.abilityCooldown;
        dischargeSpeed = initialBatteryCharge / (enemy.flamethrowDuration * 0.75f);

        foreach (GameObject battery in batteries)
            battery.SetActive(true);
    }

    // Start discharging batteries (used when flamethrower is active)
    public void DischargeBatteries()
    {
        isRecharging = false;
    }
    #endregion
}
