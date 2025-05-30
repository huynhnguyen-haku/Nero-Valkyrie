using System;
using UnityEngine;

public class MissionObject_Car : MonoBehaviour
{
    public static event Action OnCarDelivery;

    // Invoke event when car is delivered
    // This method called back to the method in Mission_CarDelivery to complete the mission
    public void InvokeCarDelivery() => OnCarDelivery?.Invoke();
}
