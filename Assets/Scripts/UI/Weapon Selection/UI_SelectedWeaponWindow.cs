using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SelectedWeaponWindow : MonoBehaviour
{
    public Weapon_Data weaponData;

    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponInfo;

    #region Unity Methods

    private void Start()
    {
        weaponData = null;
        UpdateSlotInfo(null);
    }

    #endregion

    #region Slot Logic

    // Assign a weapon to this slot and update UI
    public void SetWeaponSlot(Weapon_Data newWeaponData)
    {
        weaponData = newWeaponData;
        UpdateSlotInfo(weaponData);
    }

    // Update slot visuals based on weapon data
    public void UpdateSlotInfo(Weapon_Data weapon_data)
    {
        if (weapon_data == null)
        {
            weaponIcon.color = Color.clear;
            weaponInfo.text = "No Weapon Selected";
            return;
        }

        weaponIcon.color = Color.white;
        weaponIcon.sprite = weapon_data.weaponIcon;
        weaponInfo.text = weapon_data.weaponInfo;
    }

    // Check if this slot is empty
    public bool IsEmpty()
    {
        return weaponData == null;
    }

    #endregion
}

