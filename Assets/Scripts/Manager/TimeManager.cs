using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    [SerializeField] private float resumeRate = 3;
    [SerializeField] private float pauseRate = 7;

    private float timeAdjustRate;
    private float targetTimeScale;

    #region Unity Methods

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // Debug: Q for slow motion
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SlowMotionFor(1f);
        }

        // Smoothly adjust time scale toward target
        if (Mathf.Abs(Time.timeScale - targetTimeScale) > 0.05f)
        {
            float adjustRate = Time.unscaledDeltaTime * timeAdjustRate;
            Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, adjustRate);
        }
        else
        {
            Time.timeScale = targetTimeScale;
        }
    }

    #endregion

    #region Time Control

    // Pause game time and player movement
    public void PauseTime()
    {
        timeAdjustRate = pauseRate;
        targetTimeScale = 0;
        GameManager.instance.player.movement.SetPaused(true);
    }

    // Resume game time and player movement
    public void ResumeTime()
    {
        timeAdjustRate = resumeRate;
        targetTimeScale = 1;
        GameManager.instance.player.movement.SetPaused(false);
    }

    // Trigger slow motion for a duration
    public void SlowMotionFor(float duration)
    {
        StartCoroutine(SlowTime(duration));
    }

    private IEnumerator SlowTime(float duration)
    {
        targetTimeScale = 0.5f;
        Time.timeScale = targetTimeScale;
        yield return new WaitForSecondsRealtime(duration);
        ResumeTime();
    }

    #endregion
}
