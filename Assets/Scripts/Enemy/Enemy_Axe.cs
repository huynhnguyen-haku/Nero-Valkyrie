using UnityEngine;
using System.Collections.Generic;

public class Enemy_Axe : MonoBehaviour
{
    [SerializeField] private GameObject impactFx;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform axeVisual;

    private Vector3 direction;
    private Transform player;
    private float flySpeed;
    private float rotationSpeed;
    private float timer = 1;
    private float currentLifeTime = 10;
    private int damage;

    // Track unique entities hit by this axe
    private HashSet<GameObject> uniqueEntities = new HashSet<GameObject>();

    // Initialize axe parameters and target
    public void AxeSetup(float flySpeed, Transform player, float timer, int damage)
    {
        rotationSpeed = 1600;
        this.damage = damage;
        this.flySpeed = flySpeed;
        this.player = player;
        this.timer = timer;

        uniqueEntities.Clear(); // Reset hit tracking for this throw
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        currentLifeTime -= Time.deltaTime;

        UpdateRotation();

        if (timer > 0)
            UpdateDirection();

        if (currentLifeTime <= 0)
            ReturnAxeToPool();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = direction.normalized * flySpeed;
    }

    private void UpdateRotation()
    {
        axeVisual.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
        transform.forward = rb.linearVelocity;
    }

    private void UpdateDirection()
    {
        direction = player.position + Vector3.up - transform.position;
    }

    private void ReturnAxeToPool()
    {
        ObjectPool.instance.ReturnObject(gameObject);
    }

    // Only damage each entity once per axe throw
    private void OnCollisionEnter(Collision collision)
    {
        GameObject rootEntity = collision.transform.root.gameObject;

        if (!uniqueEntities.Contains(rootEntity))
        {
            I_Damagable damagable = rootEntity.GetComponentInChildren<I_Damagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(damage);
                uniqueEntities.Add(rootEntity);
            }
        }

        GameObject newFx = ObjectPool.instance.GetObject(impactFx, transform);

        ObjectPool.instance.ReturnObject(gameObject);
        ObjectPool.instance.ReturnObject(newFx, 1f);
    }
}
