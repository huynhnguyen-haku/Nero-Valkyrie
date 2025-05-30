using System;
using UnityEngine;

public class MissionObject_Key : MonoBehaviour
{
    private GameObject player;
    public static event Action OnKeyPickedUp;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != player) return;

        OnKeyPickedUp?.Invoke();
        Destroy(gameObject);
    }
}
