using UnityEngine;

public enum SnapPointType
{
    Enter,
    Exit,
}

public class SnapPoint : MonoBehaviour
{
    public SnapPointType pointType;

    #region Unity Methods

    private void Start()
    {
        // Hide collider and mesh for snap points in the scene
        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

        if (boxCollider != null)
            boxCollider.enabled = false;

        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }

    private void OnValidate()
    {
        // Update GameObject name for clarity in the editor
        gameObject.name = "SnapPoint - " + pointType.ToString();
    }

    #endregion
}
