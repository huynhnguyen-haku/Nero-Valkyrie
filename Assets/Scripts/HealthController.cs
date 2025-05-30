using UnityEngine;

public class HealthController : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;

    public bool isDead;
    public static bool muteDeathSound = false; // Global flag to mute death sounds
    [SerializeField] private GameObject lowHealthEffect;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;

        // Check if lowHealthEffect is null before trying to disable it
        // Because some enemies don't have this effect
        if (lowHealthEffect != null)
            lowHealthEffect.SetActive(false);
    }

    public virtual void ReduceHealth(int damage)
    {
        currentHealth -= damage;
        UpdateHeathVFX();
    }

    public virtual void IncreaseHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        UpdateHeathVFX();
    }

    public void UpdateHealthUI()
    {
        UI.instance.inGameUI.UpdateHealthUI(currentHealth, maxHealth);
    }

    public void UpdateHeathVFX()
    {
        // Return early if lowHealthEffect is null
        if (lowHealthEffect == null)
            return;


        // Enable VFX if health is below 25%
        if (currentHealth < maxHealth * 0.25f)
        {
            if (!lowHealthEffect.activeSelf)
                lowHealthEffect.SetActive(true);
        }

        // Disable VFX if health is 25% or above
        // Used when player picks up medkit
        else
        {
            if (lowHealthEffect.activeSelf)
                lowHealthEffect.SetActive(false);
        }
    }

    // Return true if the enemy should die
    public bool EnemyShouldDie()
    {
        if (isDead)
            return false;

        if (currentHealth <= 0)
        {
            if (lowHealthEffect != null)
                lowHealthEffect.SetActive(false);

            isDead = true;
            return true;
        }

        return false;
    }

    // Return true if the player should die
    public bool PlayerShouldDie()
    {
        // Disable the low health effect if the player is dead
        if (currentHealth <= 0 && lowHealthEffect != null)
            lowHealthEffect.SetActive(false);
        
        return currentHealth <= 0;
    }

    public virtual void SetHealthToZero()
    {
        currentHealth = 0;
        isDead = true;

        if (TryGetComponent(out Enemy enemy))
        {
            enemy.Die();
        }
    }
}
