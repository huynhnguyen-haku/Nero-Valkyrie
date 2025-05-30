using UnityEngine;

public enum AxelType { Front, Rear }

[RequireComponent(typeof(WheelCollider))]
public class Car_Wheel : MonoBehaviour
{
    public AxelType axleType;
    public WheelCollider cd { get; private set; }
    public GameObject model;
    public TrailRenderer trailRenderer;

    private float defaultSideStiffness;

    private void Awake()
    {
        cd = GetComponent<WheelCollider>();

        if (trailRenderer == null)
            trailRenderer = GetComponentInChildren<TrailRenderer>();

        trailRenderer.emitting = false;

        if (model == null)
            model = GetComponentInChildren<MeshRenderer>().gameObject;
    }

    #region Wheel Stiffness

    // Set and restore default sideways friction stiffness
    public void SetDefaltStiffness(float newValue)
    {
        defaultSideStiffness = newValue;
        RestoreDefaultStiffness();
    }

    public void RestoreDefaultStiffness()
    {
        WheelFrictionCurve sidewaysFriction = cd.sidewaysFriction;
        sidewaysFriction.stiffness = defaultSideStiffness;
        cd.sidewaysFriction = sidewaysFriction;
    }

    #endregion
}

