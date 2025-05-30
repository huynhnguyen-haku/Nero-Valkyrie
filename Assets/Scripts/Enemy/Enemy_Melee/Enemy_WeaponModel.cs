using UnityEngine;

public class Enemy_WeaponModel : MonoBehaviour
{
    public Enemy_MeleeWeaponType weaponType; // Type of weapon
    public AnimatorOverrideController overrideController; // Override controller for animations
    public Enemy_MeleeWeaponData weaponData; // Data associated with the weapon

    [SerializeField] private GameObject[] trailEffects; // Array of trail effect objects

    [Header("Damage Attributes")]
    public Transform[] damagePoints; // Points to check for damage
    public float attackCheckRadius; // Radius for attack damage checks

    #region Unity Methods
    [ContextMenu("Assign Damage Points")]
    private void GetDamagePoints()
    {
        damagePoints = new Transform[trailEffects.Length];
        for (int i = 0; i < trailEffects.Length; i++)
            damagePoints[i] = trailEffects[i].transform;
        
    }

    // Draw gizmos in the editor for debugging
    private void OnDrawGizmos()
    {
        if (damagePoints.Length > 0)
        {
            foreach (Transform point in damagePoints)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(point.position, attackCheckRadius);
            }
        }
    }
    #endregion

    #region Visual Effects Methods
    // Enable trail effects
    public void EnableTrailEffect(bool enable)
    {
        foreach (var effect in trailEffects)
            effect.SetActive(enable);
    }
    #endregion
}