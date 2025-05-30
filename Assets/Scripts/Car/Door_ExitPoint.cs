using UnityEngine;

public class Door_ExitPoint : MonoBehaviour
{
    public bool isBlocked { get; private set; } = false;

    private void OnTriggerEnter(Collider other)
    {
        isBlocked = true;
    }

    private void OnTriggerExit(Collider other)
    {
        isBlocked = false;
    }
}
