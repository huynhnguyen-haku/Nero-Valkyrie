using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class Car_HealthController : MonoBehaviour, I_Damagable
{
    private Car_Controller carController;

    public int maxHealth;
    public int currentHealth;

    public bool carBroken;

    [Header("Explosion Setting")]
    [SerializeField] private int explosionDamage = 350;
    [SerializeField] private float explosionRadius = 5;
    [Space]
    [SerializeField] private ParticleSystem fireFX;
    [SerializeField] private ParticleSystem explosionFX;
    [SerializeField] private Transform explosionPoint;
    [Space]
    [SerializeField] private float explosionDelay = 3;
    [SerializeField] private float explosionForce = 7;
    [SerializeField] private float explosionUpwardModifier = 2;

    private void Start()
    {
        carController = GetComponent<Car_Controller>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (fireFX.gameObject.activeSelf)
        {
            fireFX.transform.rotation = Quaternion.identity;
        }
    }

    #region UI

    public void UpdateCarHealthUI()
    {
        if (GameManager.instance.currentCar == carController)
        {
            UI.instance.inGameUI.UpdateCarHealthUI(currentHealth, maxHealth);
        }
    }

    #endregion

    #region Health Logic

    // Reduce car health and handle breaking
    private void ReduceHealth(int damage)
    {
        if (carBroken)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            BreakTheCar();
        }
    }

    // Set car to broken state and start explosion sequence
    private void BreakTheCar()
    {
        carBroken = true;
        carController.BreakCar();

        fireFX.gameObject.SetActive(true);
        StartCoroutine(ExplosionCar(explosionDelay));
    }

    public void TakeDamage(int damage)
    {
        ReduceHealth(damage);
        if (GameManager.instance.currentCar == carController)
        {
            UpdateCarHealthUI();
        }
    }

    #endregion

    #region Explosion Logic

    // Start explosion after delay
    private IEnumerator ExplosionCar(float delay)
    {
        yield return new WaitForSeconds(delay);

        explosionFX.gameObject.SetActive(true);
        carController.rb.AddExplosionForce(explosionForce, explosionPoint.position, explosionRadius, explosionUpwardModifier, ForceMode.Impulse);

        Explode();
    }

    // Damage all entities in explosion radius and apply force
    private void Explode()
    {
        HashSet<GameObject> uniqueEntities = new HashSet<GameObject>();
        Collider[] colliders = Physics.OverlapSphere(explosionPoint.position, explosionRadius);

        foreach (Collider hit in colliders)
        {
            I_Damagable damagable = hit.GetComponent<I_Damagable>();
            if (damagable != null)
            {
                GameObject rootEntity = hit.transform.root.gameObject;
                if (!uniqueEntities.Add(rootEntity))
                    continue; // Skip if already hit

                damagable.TakeDamage(explosionDamage);

                hit.GetComponentInChildren<Rigidbody>()?.
                    AddExplosionForce(explosionForce, explosionPoint.position, explosionRadius, explosionUpwardModifier, ForceMode.VelocityChange);
            }
        }

        // Damage player if in car and within explosion radius
        Transform playerTransform = GameManager.instance.player.transform;
        float distanceToExplosion = Vector3.Distance(playerTransform.position, explosionPoint.position);

        if (distanceToExplosion <= explosionRadius)
        {
            if (GameManager.instance.player.movement.isInCar)
            {
                Player_Health playerHealth = GameManager.instance.player.health;
                if (playerHealth != null)
                {
                    playerHealth.ReduceHealth(explosionDamage);
                }
            }
        }
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(explosionPoint.position, explosionRadius);
    }
}

