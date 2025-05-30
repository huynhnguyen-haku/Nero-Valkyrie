using System.Collections.Generic;
using UnityEngine;

public enum AmmoBoxType
{
    smallBox,
    bigBox
}

[System.Serializable]
public struct AmmoData
{
    public WeaponType weaponType;
    [Range(10, 250)] public int amount;
}

public class Ammo_PickUp : Interactable
{
    [SerializeField] private AmmoBoxType boxType;
    [SerializeField] private List<AmmoData> smallBoxAmmo;
    [SerializeField] private List<AmmoData> bigBoxAmmo;

    [SerializeField] private GameObject[] boxModel;

    private void Start()
    {
        SetupBoxModel();
    }

    #region Interaction Logic

    // Give ammo to all matching weapons in inventory
    public override void Interact()
    {
        List<AmmoData> currentAmmoList = smallBoxAmmo;

        if (boxType == AmmoBoxType.bigBox)
            currentAmmoList = bigBoxAmmo;

        foreach (AmmoData ammo in currentAmmoList)
        {
            Weapon weapon = weaponController.WeaponInSlots(ammo.weaponType);
            AddBullets(weapon, ammo.amount);
        }

        ObjectPool.instance.ReturnObject(gameObject);
    }

    // Add bullets to weapon if found
    private void AddBullets(Weapon weapon, int amount)
    {
        if (weapon == null)
            return;

        weapon.TotalReserveAmmo += amount;
        weaponController.UpdateWeaponUI();
    }

    // Activate the correct box model and update mesh/material
    private void SetupBoxModel()
    {
        for (int i = 0; i < boxModel.Length; i++)
        {
            boxModel[i].SetActive(false);

            if (i == (int)boxType)
            {
                boxModel[i].SetActive(true);
                UpdateMeshAndMaterial(boxModel[i].GetComponentInChildren<MeshRenderer>());
            }
        }
    }

    #endregion
}
