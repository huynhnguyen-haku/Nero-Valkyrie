using System;
using UnityEngine;

public enum DriveType { FrontWheelDrive, RearWheelDrive, AllWheelDrive }

[RequireComponent(typeof(Rigidbody))]
public class Car_Controller : MonoBehaviour
{
    public Car_SFX carSounds { get; private set; }
    public Rigidbody rb { get; private set; }
    private PlayerControls controls;

    public bool carActive { get; private set; }
    [SerializeField] private LayerMask whatIsGround;

    private float moveInput;
    private float turnInput;

    // Actual speed of rigidbody
    public float actualSpeedMPS { get; private set; } // Mile per second
    public float actualSpeedKPH { get; private set; } // Kilometer per hour

    [Range(30, 60)][SerializeField] private float turnSensitivity = 30;

    [Header("Car Settings")]
    [SerializeField] private DriveType driveType;
    [SerializeField] private Transform centerOfMass;

    [Range(350, 1000)]
    [SerializeField] private float carMass;

    [Range(20, 80)]
    [SerializeField] private float wheelsMass;

    [Range(0.5f, 2)]
    [SerializeField] private float frontWheelTraction = 1;

    [Range(0.5f, 2)]
    [SerializeField] private float rearWheelTraction = 1;

    [Header("Engine Settings")]
    private float currentMotorInputFactor;

    [Range(20, 150)]
    [SerializeField] private float maxSpeedKPH = 80;

    [Range(0.5f, 10)]
    [SerializeField] private float accelerationRate = 2;

    [Range(1500, 5000)]
    [SerializeField] private float motorForce = 1500f;

    [Header("Brake Settings")]
    public bool isBraking { get; private set; }

    [Range(0, 10)]
    public float frontBrakeSensitivity = 5;

    [Range(0, 10)]
    public float backBrakeSensitivity = 5;

    [Range(4000, 6000)]
    public float brakeForce = 5000;

    [Header("Drift Settings")]
    [Range(0, 1)]
    [SerializeField] private float frontDriftFactor = 0.5f;

    [Range(0, 1)]
    [SerializeField] private float rearDriftFactor = 0.5f;

    [SerializeField] private float driftDuration = 1;
    private float driftTimer;
    public bool isDrifting { get; private set; }
    private bool canEmitTrails = true;

    [Header("Drift Effects")]
    [SerializeField] private ParticleSystem RLWParticleSystem;
    [SerializeField] private ParticleSystem RRWParticleSystem;

    private Car_Wheel[] wheels;

    #region Unity Methods

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        wheels = GetComponentsInChildren<Car_Wheel>();
        carSounds = GetComponent<Car_SFX>();

        controls = ControlsManager.instance.controls;
        if (controls == null)
        {
            Debug.LogError("Car_Controller: ControlsManager.instance.controls is null!");
        }

        ActivateCar(false);
        AssignInputEvents();
        SetupDefaultValues();
    }

    private void FixedUpdate()
    {
        if (!carActive)
        {
            DecelerateCar();
            return;
        }

        HandleWheelAnimation();
        ApplyTrailOnTheGround();
        HandleDriving();
        HandleSteering();
        HandleBraking();
        HandleSpeedLimit();

        if (isDrifting)
            HandleDrift();
        else
            StopDrift();
    }

    public void DecelerateCar()
    {
        StopDrift();
        isDrifting = false;

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 7f);
        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 7f);

        HandleWheelAnimation();
    }

    private void Update()
    {
        // Calculate car speed
        actualSpeedMPS = rb.linearVelocity.magnitude;
        actualSpeedKPH = actualSpeedMPS * 3.6f;

        if (!carActive)
            return;

        // Update UI speed text
        UI.instance.inGameUI.UpdateSpeedText(Mathf.RoundToInt(actualSpeedKPH) + " km/h");

        if (carActive)
        {
            driftTimer -= Time.deltaTime;
            if (driftTimer < 0)
            {
                isDrifting = false;
            }
        }
    }

    #endregion

    #region Setup Methods

    private void SetupDefaultValues()
    {
        rb.centerOfMass = centerOfMass.localPosition;
        rb.mass = carMass;

        foreach (var wheel in wheels)
        {
            if (wheel.cd != null)
            {
                wheel.cd.mass = wheelsMass;

                if (wheel.axleType == AxelType.Front)
                    wheel.SetDefaltStiffness(frontWheelTraction);

                if (wheel.axleType == AxelType.Rear)
                    wheel.SetDefaltStiffness(rearWheelTraction);
            }
            else
            {
                Debug.LogWarning($"Car_Controller: WheelCollider (cd) is null for one of the wheels in GameObject {wheel.gameObject.name}", wheel.gameObject);
            }
        }
    }

    private void AssignInputEvents()
    {
        if (controls == null) return;

        controls.Car.Movement.performed += ctx =>
        {
            Vector2 input = ctx.ReadValue<Vector2>();
            moveInput = input.y;
            turnInput = input.x;
        };

        controls.Car.Movement.canceled += ctx =>
        {
            moveInput = 0;
            turnInput = 0;
        };

        controls.Car.Brake.performed += ctx =>
        {
            isBraking = true;
            isDrifting = true;
            driftTimer = driftDuration;
        };

        controls.Car.Brake.canceled += ctx => isBraking = false;

        var carInteraction = GetComponent<Car_Interaction>();
        if (carInteraction != null)
            controls.Car.EnterExit.performed += ctx => carInteraction.ExitCar();

        else
            Debug.LogWarning("Car_Controller: Car_Interaction component not found. Enter/Exit control will not work.");


        controls.Car.TogglePauseUI.performed += ctx => { if (UI.instance != null) UI.instance.TogglePauseUI(); };
        controls.Car.ToggleMinimap.performed += ctx =>
        {
            if (UI.instance != null && UI.instance.inGameUI != null && UI.instance.inGameUI.minimap != null)
            {
                bool isMinimapActive = UI.instance.inGameUI.minimap.activeSelf;
                UI.instance.ToggleMinimap(!isMinimapActive);
            }
        };
    }

    #endregion

    #region Driving Methods

    private void HandleDriving()
    {
        currentMotorInputFactor = moveInput * accelerationRate * Time.deltaTime;
        float motorTorqueValue = motorForce * currentMotorInputFactor;

        foreach (var wheel in wheels)
        {
            if (wheel.cd == null) continue;

            if (driveType == DriveType.FrontWheelDrive)
            {
                if (wheel.axleType == AxelType.Front)
                    wheel.cd.motorTorque = motorTorqueValue;
            }
            else if (driveType == DriveType.RearWheelDrive)
            {
                if (wheel.axleType == AxelType.Rear)
                    wheel.cd.motorTorque = motorTorqueValue;
            }
            else if (driveType == DriveType.AllWheelDrive)
                wheel.cd.motorTorque = motorTorqueValue;
        }
    }

    private void HandleSpeedLimit()
    {
        // Conver maxSpeedKPH (km/h) to m/s to compare it to rb.linearVelocity.magnitude
        // 1 km/h = 1000m / 3600s = 1 / 3.6 m/s
        float maxSpeedMPS = maxSpeedKPH / 3.6f;
        if (rb.linearVelocity.magnitude > maxSpeedMPS)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeedMPS;
    }

    private void HandleSteering()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.cd == null) continue;
            if (wheel.axleType == AxelType.Front)
            {
                float targetSteeringAngle = turnInput * turnSensitivity;
                wheel.cd.steerAngle = Mathf.Lerp(wheel.cd.steerAngle, targetSteeringAngle, 0.5f);
            }
        }
    }

    private void HandleBraking()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.cd == null) continue;

            bool backBrakes = wheel.axleType == AxelType.Rear;
            float brakeSensitivity = backBrakes ? backBrakeSensitivity : frontBrakeSensitivity;

            float actualBrakeForce = brakeForce * brakeSensitivity * Time.fixedDeltaTime;
            float currentBrakeTorque = isBraking ? actualBrakeForce : 0;
            wheel.cd.brakeTorque = currentBrakeTorque;
        }
    }

    private void ApplyTrailOnTheGround()
    {
        if (!canEmitTrails)
            return;

        foreach (var wheel in wheels)
        {
            if (wheel.cd == null) continue;

            if (isDrifting || isBraking)
            {
                if (wheel.cd.GetGroundHit(out WheelHit hit) &&
                    (whatIsGround == (whatIsGround | (1 << hit.collider.gameObject.layer))))
                {
                    if (wheel.trailRenderer != null)
                        wheel.trailRenderer.emitting = true;
                }
                else
                {
                    if (wheel.trailRenderer != null)
                        wheel.trailRenderer.emitting = false;
                }
            }
            else
            {
                if (wheel.trailRenderer != null)
                    wheel.trailRenderer.emitting = false;
            }
        }
    }

    #endregion

    #region Drift Methods

    private void HandleDrift()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.cd == null) continue;

            bool backWheel = wheel.axleType == AxelType.Rear;
            float driftFactor = backWheel ? rearDriftFactor : frontDriftFactor;

            WheelFrictionCurve sidewaysFriction = wheel.cd.sidewaysFriction;
            sidewaysFriction.stiffness *= (1f - driftFactor);
            wheel.cd.sidewaysFriction = sidewaysFriction;
        }

        DriftCarPS(true);
        if (carSounds != null) carSounds.HandleTireSqueal(true);
    }

    private void StopDrift()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.cd == null) continue;
            wheel.RestoreDefaultStiffness();
        }

        DriftCarPS(false);
        if (carSounds != null) carSounds.HandleTireSqueal(false);
        foreach (var wheel in wheels)
        {
            if (wheel.trailRenderer != null)
                wheel.trailRenderer.emitting = false;
        }
    }

    private void DriftCarPS(bool play)
    {
        if (play)
        {
            RLWParticleSystem?.Play();
            RRWParticleSystem?.Play();
        }
        else
        {
            RLWParticleSystem?.Stop();
            RRWParticleSystem?.Stop();
        }
    }

    #endregion

    #region Animation Methods

    private void HandleWheelAnimation()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.cd == null) continue;
            wheel.cd.GetWorldPose(out Vector3 position, out Quaternion rotation);

            if (wheel.model != null)
            {
                wheel.model.transform.position = position;
                wheel.model.transform.rotation = rotation;
            }
        }
    }

    #endregion

    #region Public Methods

    // Used when player enters or exits the car
    public void ActivateCar(bool active)
    {
        carActive = active;
        if (carSounds != null)
        {
            carSounds.ActivateCarSFX(active);
        }

        if (!active)
        {
            isBraking = false;
            isDrifting = false;
        }
    }
    
    // Used when the car's health = 0
    public void BreakCar()
    {
        canEmitTrails = false;

        foreach (var wheel in wheels)
        {
            if (wheel.trailRenderer != null)
                wheel.trailRenderer.emitting = false;
        }

        if (carSounds != null)
            carSounds.StopEngineIdleOnly();

        motorForce = 0;
        rb.linearDamping = 1;

    }

    #endregion
}