using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    [SerializeField] private Transform ragdollParent;

    private Collider[] ragdollColliders;
    private Rigidbody[] ragdollRigidbodies;
    private Animator animator;

    #region Unity Methods

    private void Awake()
    {
        // Cache all colliders, rigidbodies, and animator
        ragdollColliders = GetComponentsInChildren<Collider>();
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();

        // Disable ragdoll by default
        RagdollActive(false);
    }

    #endregion

    #region Ragdoll Logic

    // Enable or disable ragdoll physics and animator
    public void RagdollActive(bool active)
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
            rb.isKinematic = !active;

        if (animator != null)
            animator.enabled = !active;
    }

    // Enable or disable all ragdoll colliders
    public void ColliderActive(bool active)
    {
        foreach (Collider col in ragdollColliders)
            col.enabled = active;
    }

    #endregion
}
