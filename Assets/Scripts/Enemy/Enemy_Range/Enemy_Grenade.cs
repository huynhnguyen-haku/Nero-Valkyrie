using System.Collections.Generic;
using UnityEngine;

public class Enemy_Grenade : MonoBehaviour
{
    [SerializeField] private GameObject explosionFX;
    [SerializeField] private float impactRadius;
    [SerializeField] private float upwardsMulti = 1;

    private Rigidbody rb;
    private float timer;
    private float impactPower;
    private int grenadeDamage;

    private LayerMask allyLayerMask;
    private bool canExplode;

    #region Unity Methods
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0 && canExplode)
        {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision) { }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
    #endregion

    #region Explosion Logic
    private void Explode()
    {
        canExplode = false; // Prevent further explosions, make sure that it can only explode once
        CreateExplosionFX();

        HashSet<GameObject> uniqueEntities = new HashSet<GameObject>(); // To store unique entities hit by the explosion
        Collider[] colliders = Physics.OverlapSphere(transform.position, impactRadius);

        foreach (Collider hit in colliders)
        {
            I_Damagable damagable = hit.GetComponent<I_Damagable>();
            if (damagable != null)
            {
                if (IsTargetValid(hit) == false)
                    continue;

                GameObject rootEntity = hit.transform.root.gameObject;
                if (uniqueEntities.Add(rootEntity) == false)
                    continue; // Skip if the entity has already been hit

                damagable.TakeDamage(grenadeDamage);
            }
            ApplyPhysicalForce(hit);
        }
        ObjectPool.instance.ReturnObject(gameObject); // Return the grenade to the pool
    }

    private void ApplyPhysicalForce(Collider hit)
    {
        Rigidbody rb = hit.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddExplosionForce(impactPower, transform.position, impactRadius, upwardsMulti, ForceMode.Impulse);
    }

    private void CreateExplosionFX()
    {
        GameObject newFX = ObjectPool.instance.GetObject(explosionFX, transform);
        ObjectPool.instance.ReturnObject(gameObject);
        ObjectPool.instance.ReturnObject(newFX, 2);
    }
    #endregion

    #region Setup Methods
    public void SetupGrenade(LayerMask allyLayerMask, Vector3 target, float timeToTarget, float countdown, float impactPower, int grenadeDamage)
    {
        this.allyLayerMask = allyLayerMask;
        this.grenadeDamage = grenadeDamage;
        this.impactPower = impactPower;

        timer = countdown + timeToTarget;
        canExplode = true;

        rb.linearVelocity = CalculateLaunchVelocity(target, timeToTarget);
    }

    // If friendly fire is enabled, then other enemies can also take damage (is valid target)
    // If disabled, then only the player and vehicles can take damage
    private bool IsTargetValid(Collider collider)
    {
        if (GameManager.instance.friendlyFire)
            return true;

        if ((allyLayerMask & (1 << collider.gameObject.layer)) > 0)
            return false;

        return true;
    }

    // Calculate the launch velocity to reach the target (make the grenade fly in an parabolic arc)
    private Vector3 CalculateLaunchVelocity(Vector3 target, float timeToTarget)
    {
        Vector3 direction = target - transform.position; // Vector from current position to target
        Vector3 directionXZ = new Vector3(direction.x, 0, direction.z); // Horizontal (XZ) component only
        Vector3 velocityXZ = directionXZ / timeToTarget; // Required horizontal velocity

        // Required vertical velocity, accounting for gravity
        float velocityY = (direction.y - (Physics.gravity.y * Mathf.Pow(timeToTarget, 2)) / 2) / timeToTarget;

        Vector3 launchVelocity = velocityXZ + Vector3.up * velocityY; // Combine horizontal and vertical

        return launchVelocity;
    }

    #endregion
}