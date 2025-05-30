using UnityEngine;

public class MinimapSprite : MonoBehaviour
{
    #region Unity Methods

    private void Start()
    {
        // Set initial rotation for minimap icon
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private void LateUpdate()
    {
        // Keep minimap icon facing up regardless of parent rotation
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    #endregion
}
