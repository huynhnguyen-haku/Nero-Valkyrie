using UnityEngine;

public class Car_SFX : MonoBehaviour
{
    private Car_Controller carController;

    [SerializeField] private AudioSource engineStart;
    [SerializeField] private AudioSource engineIdle;
    [SerializeField] private AudioSource engineStop;
    [SerializeField] private AudioSource tireSqueal;

    private float defaultEngineIdleVolume;
    private float maxSpeed = 20;

    public float minPitch = 0.75f;
    public float maxPitch = 1.5f;

    private bool enableCarSFX;

    private void Start()
    {
        carController = GetComponent<Car_Controller>();
        defaultEngineIdleVolume = engineIdle.volume;
        Invoke(nameof(EnableCarSFX), 1f);
    }

    private void Update()
    {
        UpdateEngineSFX();
    }

    #region Engine SFX

    // Update engine idle pitch based on car speed
    private void UpdateEngineSFX()
    {
        float currentSpeed = Mathf.Abs(carController.actualSpeedKPH) / 3.6f;
        float pitch = Mathf.Lerp(minPitch, maxPitch, currentSpeed / maxSpeed);
        engineIdle.pitch = pitch;
    }

    // Start or stop engine SFX
    public void ActivateCarSFX(bool active)
    {
        if (!enableCarSFX)
            return;

        if (active)
        {
            engineStart.Play();
            AudioManager.instance.ControlSFX_FadeAndDelay(engineIdle, true, defaultEngineIdleVolume, 0.75f, 0.75f);
        }
        else
        {
            AudioManager.instance.ControlSFX_FadeAndDelay(engineIdle, false, 0f, 0.25f);
            engineStop.Play();
        }
    }

    // Play when car is broken down
    public void StopEngineIdleOnly()
    {
        if (!enableCarSFX)
            return;

        AudioManager.instance.ControlSFX_FadeAndDelay(engineIdle, false, 0f, 0.25f);
    }

    #endregion

    #region Tire SFX

    // Play or fade out tire squeal SFX when drifting
    public void HandleTireSqueal(bool isDrifting)
    {
        if (!enableCarSFX || tireSqueal == null)
            return;

        if (isDrifting)
        {
            if (!tireSqueal.isPlaying)
            {
                tireSqueal.volume = 0.2f;
                tireSqueal.Play();
            }
        }
        else
        {
            if (tireSqueal.isPlaying)
            {
                AudioManager.instance.ControlSFX_FadeAndDelay(tireSqueal, false, 0f, 0.25f, 0.25f);
            }
        }
    }

    private void EnableCarSFX()
    {
        enableCarSFX = true;
    }

    #endregion
}

