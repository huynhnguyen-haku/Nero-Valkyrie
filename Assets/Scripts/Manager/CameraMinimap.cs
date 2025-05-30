using UnityEngine;

public class CameraMinimap : MonoBehaviour
{
    public GameObject Player;

    private void LateUpdate()
    {
        transform.position = new Vector3(Player.transform.position.x, 10, Player.transform.position.z);
    }
}
