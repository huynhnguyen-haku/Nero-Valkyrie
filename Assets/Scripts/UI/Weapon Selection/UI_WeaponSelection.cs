using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_WeaponSelection : MonoBehaviour
{
    [SerializeField] private GameObject nextUIElementToActivate;
    public UI_SelectedWeaponWindow[] selectedWeapon;

    [Header("Warning Info")]
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private float disappearingSpeed = 0.25f;

    private float currentWarningAlpha;
    private float targetWarningAlpha;

    #region Unity Methods

    private void Start()
    {
        selectedWeapon = GetComponentsInChildren<UI_SelectedWeaponWindow>();
    }

    private void Update()
    {
        if (currentWarningAlpha > targetWarningAlpha)
        {
            currentWarningAlpha -= Time.deltaTime * disappearingSpeed;
            warningText.color = new Color(1, 1, 1, currentWarningAlpha);
        }
    }

    #endregion

    #region Weapon Selection Logic

    // Get all selected weapon data
    public List<Weapon_Data> SelectedWeaponData()
    {
        List<Weapon_Data> selectedData = new List<Weapon_Data>();
        foreach (UI_SelectedWeaponWindow weapon in selectedWeapon)
        {
            if (weapon.weaponData != null)
            {
                selectedData.Add(weapon.weaponData);
            }
        }
        return selectedData;
    }

    // Confirm weapon selection and proceed if valid
    public void ConfirmWeaponSelection()
    {
        if (HasSelectedWeapon())
        {
            UI.instance.SwitchTo(nextUIElementToActivate);
            UI.instance.StartLevelGeneration();
        }
    }

    public void TryStartGame()
    {
        // Check if at least one weapon is selected before starting the game
        if (HasSelectedWeapon())      
            UI.instance.StartGame();

        // If no weapon is selected, show a warning message
        else
            ShowWarningMessage("Please select at least one weapon.");
        
    }


    // Check if at least one weapon is selected
    private bool HasSelectedWeapon() => SelectedWeaponData().Count > 0;

    // Find the first empty weapon slot
    public UI_SelectedWeaponWindow FindEmptySlot()
    {
        for (int i = 0; i < selectedWeapon.Length; i++)
        {
            if (selectedWeapon[i].IsEmpty())
            {
                return selectedWeapon[i];
            }
        }
        return null;
    }

    // Find the slot containing the specified weapon
    public UI_SelectedWeaponWindow FindSlotWithWeaponOfType(Weapon_Data weaponData)
    {
        for (int i = 0; i < selectedWeapon.Length; i++)
        {
            if (selectedWeapon[i].weaponData == weaponData)
            {
                return selectedWeapon[i];
            }
        }
        return null;
    }

    // Show a warning message to the user
    public void ShowWarningMessage(string message)
    {
        warningText.color = Color.white;
        warningText.text = message;
        currentWarningAlpha = warningText.color.a;
        targetWarningAlpha = 0;
    }

    #endregion
}

