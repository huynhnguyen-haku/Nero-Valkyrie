using UnityEngine;

public class Car_DamageZone : MonoBehaviour
{
    private Car_Controller carController;
    [SerializeField] private float minSpeedToDamage = 4f;
    [SerializeField] private int carDamage;
    [SerializeField] private float impactForce = 150;
    [SerializeField] private float upwardsMulti = 3;

    private void Awake()
    {
        carController = GetComponentInParent<Car_Controller>();
    }

    #region Collision Logic

    // Deal damage and apply force if car is moving fast enough
    private void OnTriggerEnter(Collider other)
    {
        if (Mathf.Abs(carController.actualSpeedKPH) < minSpeedToDamage)
            return;

        I_Damagable damagable = other.GetComponent<I_Damagable>();
        if (damagable == null)
            return;

        damagable.TakeDamage(carDamage);

        // Apply force to rigidbody if present
        Rigidbody rigidbody = other.GetComponent<Rigidbody>();
        if (rigidbody != null)
            ApplyForce(rigidbody);
    }

    // Apply explosion force to the hit rigidbody
    private void ApplyForce(Rigidbody rigidbody)
    {
        if (rigidbody == null)
            return;

        rigidbody.isKinematic = false;
        rigidbody.AddExplosionForce(impactForce, transform.position, 3, upwardsMulti, ForceMode.Impulse);
    }

    #endregion
}
