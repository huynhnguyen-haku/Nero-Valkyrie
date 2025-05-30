using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_WeaponSelectionButton : UI_Button
{
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Weapon_Data weaponData;

    private UI_WeaponSelection weaponSelectionUI;
    private UI_SelectedWeaponWindow emptySlot;

    private void OnValidate()
    {
        gameObject.name = "Button - Select Weapon: " + weaponData.weaponName;
    }

    public override void Start()
    {
        base.Start();
        weaponSelectionUI = GetComponentInParent<UI_WeaponSelection>();

        // Load the unlock state of the weapon
        weaponData.LoadUnlockState();

        // Update the weapon icon
        weaponIcon.sprite = weaponData.weaponIcon;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        weaponIcon.color = Color.yellow;
        emptySlot = weaponSelectionUI.FindEmptySlot();
        emptySlot?.UpdateSlotInfo(weaponData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        weaponIcon.color = Color.white;
        emptySlot?.UpdateSlotInfo(null);
        emptySlot = null;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        weaponIcon.color = Color.white;

        if (!weaponData.isUnlocked)
        {
            // Open weapon purchase confirmation UI
            UI.instance.weaponConfirmation.SetupConfirmationUI(weaponData);
            UI.instance.SwitchTo(UI.instance.weaponConfirmation.gameObject);
            return;
        }

        // Check if weapon is already assigned to a slot
        UI_SelectedWeaponWindow assignedWeaponSlot = weaponSelectionUI.FindSlotWithWeaponOfType(weaponData);
        if (assignedWeaponSlot != null)
        {
            // If already assigned, remove from slot
            assignedWeaponSlot.SetWeaponSlot(null);
            return;
        }

        // Assign weapon to an empty slot if available
        emptySlot = weaponSelectionUI.FindEmptySlot();
        if (emptySlot != null)
        {
            emptySlot.SetWeaponSlot(weaponData);
        }
        else
        {
            // Show warning if no empty slot
            weaponSelectionUI.ShowWarningMessage("No empty slot...");
        }

        emptySlot = null;
    }
}

