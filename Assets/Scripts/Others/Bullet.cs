using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject bulletImpactFX;

    private int bulletDamage;
    private float impactForce;
    private BoxCollider boxCollider;
    private Rigidbody rb;

    [SerializeField] private TrailRenderer bulletTrail;
    [SerializeField] private float bulletLifeTime;
    private float currentLifeTime;

    private LayerMask allyLayerMask;
    private bool canCollide; // Prevents multiple collisions per bullet

    #region Unity Methods

    protected void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        bulletTrail = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        currentLifeTime = bulletLifeTime;
    }

    protected virtual void Update()
    {
        currentLifeTime -= Time.deltaTime;
        if (currentLifeTime <= 0)
        {
            ReturnBulletToPool();
        }
    }

    #endregion

    #region Bullet Setup

    // Initialize bullet parameters and reset state
    public void BulletSetup(LayerMask allyLayerMask, int bulletDamage, float impactForce = 100)
    {
        this.allyLayerMask = allyLayerMask;
        this.impactForce = impactForce;
        this.bulletDamage = bulletDamage;
        boxCollider.enabled = true;
        bulletTrail.Clear();
        canCollide = true;
    }

    #endregion

    #region Collision Logic

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!canCollide) return;

        canCollide = false;

        // Ignore collision with allies if friendly fire is off
        if (!FriendlyFire())
        {
            if ((allyLayerMask.value & (1 << collision.gameObject.layer)) > 0)
            {
                ReturnBulletToPool(10);
                return;
            }
        }

        CreateImpactFX();
        ReturnBulletToPool();

        // Deal damage if possible
        I_Damagable damagable = collision.gameObject.GetComponentInChildren<I_Damagable>();
        damagable?.TakeDamage(bulletDamage);

        ApplyBulletImpact(collision);
    }

    // Apply force to enemy ragdoll if hit
    private void ApplyBulletImpact(Collision collision)
    {
        Enemy enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            Vector3 force = rb.linearVelocity.normalized * impactForce;
            Rigidbody hitRigibBody = collision.collider.attachedRigidbody;
            enemy.BulletImpact(force, collision.contacts[0].point, hitRigibBody);
        }
    }

    #endregion

    #region Pool & FX

    protected void ReturnBulletToPool(float delay = 0)
    {
        ObjectPool.instance.ReturnObject(gameObject, delay);
    }

    protected void CreateImpactFX()
    {
        GameObject newImpactFX = ObjectPool.instance.GetObject(bulletImpactFX, transform);
        ObjectPool.instance.ReturnObject(newImpactFX, 1);
    }

    #endregion

    #region Utility

    public bool FriendlyFire()
    {
        return GameManager.instance.friendlyFire;
    }

    #endregion
}

