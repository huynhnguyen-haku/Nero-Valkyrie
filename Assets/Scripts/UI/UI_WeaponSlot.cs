using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_WeaponSlot : MonoBehaviour
{
    public Image weaponIcon;
    public TextMeshProUGUI ammoText;

    private void Awake()
    {
        weaponIcon = GetComponentInChildren<Image>();
        ammoText = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update weapon icon and ammo display
    public void UpdateWeaponSlot(Weapon myWeapon, bool activeWeapon)
    {
        if (myWeapon == null)
        {
            weaponIcon.color = Color.clear;
            ammoText.text = "";
            return;
        }

        // Set icon color based on active state
        Color newColor = activeWeapon ? Color.white : new Color(1, 1, 1, 0.35f);
        weaponIcon.color = newColor;
        weaponIcon.sprite = myWeapon.weaponData.weaponIcon;

        ammoText.text = myWeapon.bulletsInMagazine + "/" + myWeapon.TotalReserveAmmo;
        ammoText.color = Color.white;
    }
}

