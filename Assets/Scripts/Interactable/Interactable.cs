using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private Material highlightMaterial;
    [SerializeField] protected Material defaultMaterial;
    [SerializeField] protected MeshRenderer meshRenderer;

    protected Player_WeaponController weaponController;

    private void Start()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }
        defaultMaterial = meshRenderer.sharedMaterial;
    }

    #region Mesh & Highlight

    // Update mesh renderer and default material reference
    protected void UpdateMeshAndMaterial(MeshRenderer newMesh)
    {
        meshRenderer = newMesh;
        defaultMaterial = newMesh.sharedMaterial;
    }

    // Highlight or unhighlight the object
    public virtual void Highlight(bool active)
    {
        meshRenderer.material = active ? highlightMaterial : defaultMaterial;
    }

    #endregion

    #region Interaction

    public virtual void Interact()
    {
        Debug.Log("Interacting with " + name);
    }

    #endregion

    #region Trigger Logic

    // Register with player interaction system on enter
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (weaponController == null)
        {
            weaponController = other.GetComponent<Player_WeaponController>();
        }

        Player_Interaction playerInteraction = other.GetComponent<Player_Interaction>();
        if (playerInteraction == null)
            return;

        playerInteraction.GetInteractables().Add(this);
        playerInteraction.UpdateClosestInteracble();
    }

    // Unregister from player interaction system on exit
    protected virtual void OnTriggerExit(Collider other)
    {
        Player_Interaction playerInteraction = other.GetComponent<Player_Interaction>();
        if (playerInteraction == null)
            return;

        playerInteraction.GetInteractables().Remove(this);
        playerInteraction.UpdateClosestInteracble();
    }

    #endregion
}
