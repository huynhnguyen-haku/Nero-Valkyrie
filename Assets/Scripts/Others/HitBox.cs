using UnityEngine;

public class HitBox : MonoBehaviour, I_Damagable
{
    [SerializeField] protected float damageMultiplier = 1;

    protected virtual void Awake() { }

    public virtual void TakeDamage(int damage) { }
}
