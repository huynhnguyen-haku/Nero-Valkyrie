using UnityEngine;

public class Health_Pickup : Interactable
{
    [SerializeField] private int healthAmount = 50; // Amount of health to restore

    #region Interaction Logic

    // Restore health to player and return to pool
    public override void Interact()
    {
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            Player_Health playerHealth = player.GetComponent<Player_Health>();
            if (playerHealth != null)
            {
                RestoreHealth(playerHealth);
                playerHealth.UpdateHealthUI();
            }
        }
        ObjectPool.instance.ReturnObject(gameObject);
    }

    private void RestoreHealth(Player_Health playerHealth)
    {
        playerHealth.IncreaseHealth(healthAmount);
        Debug.Log($"[Health_Pickup] Player restored {healthAmount} health.");
    }

    #endregion
}
