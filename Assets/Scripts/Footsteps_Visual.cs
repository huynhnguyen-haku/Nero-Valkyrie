using UnityEngine;

public class Footsteps_Visual : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsGround;

    [SerializeField] private TrailRenderer leftFoot;
    [SerializeField] private TrailRenderer rightFoot;

    [Range(0.001f, 0.3f)]
    [SerializeField] private float checkRadius = 0.05f;

    [Range(-0.15f, 0.15f)]
    [SerializeField] private float rayDistance = -0.05f;

    private void Update()
    {
        CheckFootstep(leftFoot);
        CheckFootstep(rightFoot);
    }

    private void CheckFootstep(TrailRenderer foot)
    {
        Vector3 checkPosition = foot.transform.position + Vector3.down * rayDistance;

        bool isGrounded = Physics.CheckSphere(checkPosition, checkRadius, whatIsGround);

        foot.emitting = isGrounded;
    }

    private void OnDrawGizmos()
    {
        DrawFootGizmos(leftFoot.transform);
        DrawFootGizmos(rightFoot.transform);
    }

    private void DrawFootGizmos(Transform foot)
    {
        if (foot == null)
        {
            return;
        }

        Gizmos.color = Color.blue;
        Vector3 checkPosition = foot.transform.position + Vector3.down * rayDistance;

        Gizmos.DrawWireSphere(checkPosition, checkRadius);
    }
}
