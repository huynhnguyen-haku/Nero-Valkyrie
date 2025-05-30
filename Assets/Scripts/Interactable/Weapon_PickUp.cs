using UnityEngine;

public class Weapon_PickUp : Interactable
{
    [SerializeField] private Weapon_Data weaponData;
    [SerializeField] private Weapon weapon;
    [SerializeField] private BackupWeaponModel[] weaponModels;
    private bool oldWeapon;

    private void Start()
    {
        if (!oldWeapon)
        {
            weapon = new Weapon(weaponData);
        }
        SetupGameObject();
    }

    #region Setup Logic

    // Set up this pickup as an existing weapon (e.g. dropped by player)
    public void SetUpPickupWeapon(Weapon weapon, Transform transform)
    {
        oldWeapon = true;
        this.weapon = weapon;
        weaponData = weapon.weaponData;
        this.transform.position = transform.position;
    }

    [ContextMenu("Update Weapon Model")]
    public void SetupGameObject()
    {
        gameObject.name = "Pickup_Weapon - " + weaponData.weaponName.ToString();
        SetupWeaponModel();
    }

    // Activate only the correct weapon model for this pickup
    private void SetupWeaponModel()
    {
        foreach (BackupWeaponModel model in weaponModels)
        {
            model.gameObject.SetActive(false);
            if (model.weaponType == weaponData.weaponType)
            {
                model.gameObject.SetActive(true);
                UpdateMeshAndMaterial(model.GetComponentInChildren<MeshRenderer>());
            }
        }
    }

    #endregion

    #region Interaction Logic

    // Give weapon to player and return to pool
    public override void Interact()
    {
        Debug.Log("Picked up weapon: " + weaponData.weaponName);
        weaponController.PickupWeapon(weapon);
        ObjectPool.instance.ReturnObject(gameObject);
    }

    #endregion
}
