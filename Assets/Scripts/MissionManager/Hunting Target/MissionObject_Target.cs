using System;
using UnityEngine;

public class MissionObject_Target : MonoBehaviour
{
    public static event Action OnTargetKilled;

    // Call this when the target is killed
    public void InvokeOnTargetKilled()
    {
        Debug.Log($"Target killed: {gameObject.name}");
        OnTargetKilled?.Invoke();
    }
}
