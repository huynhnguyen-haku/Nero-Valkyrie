using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour
{
    [SerializeField] private GameObject characterUI;
    [SerializeField] private GameObject carUI;

    [Header("Minimap")]
    public GameObject minimap;

    [Header("Health Bar")]
    [SerializeField] private Image healthBar;

    [Header("Weapon Slots")]
    [SerializeField] private UI_WeaponSlot[] weaponSlots_UI;

    [Header("Mission")]
    [SerializeField] private GameObject missionUIParent;
    [SerializeField] private GameObject missionGuide;
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private TextMeshProUGUI missionDetai;

    [Header("Car")]
    [SerializeField] private Image carHealthBar;
    [SerializeField] private TextMeshProUGUI carSpeedText;

    private bool MissionUIActive = true;

    #region Unity Methods

    private void Awake()
    {
        weaponSlots_UI = GetComponentsInChildren<UI_WeaponSlot>();
    }

    #endregion

    #region UI Logic

    // Switch to character UI (hide car UI)
    public void SwitchToCharacterUI()
    {
        characterUI.SetActive(true);
        carUI.SetActive(false);
    }

    // Switch to car UI (hide character UI)
    public void SwitchToCarUI()
    {
        characterUI.SetActive(false);
        carUI.SetActive(true);
    }

    // Toggle mission UI and guide
    public void ToggleMissionUI()
    {
        MissionUIActive = !MissionUIActive;
        missionUIParent.SetActive(MissionUIActive);
        missionGuide.SetActive(!MissionUIActive);
    }

    // Update mission text and details
    public void UpdateMissionUI(string mission, string missionDetails = "")
    {
        missionText.text = mission;
        missionDetai.text = missionDetails;
    }

    // Update weapon slots UI
    public void UpdateWeaponUI(List<Weapon> weaponSlots, Weapon currentWeapon)
    {
        for (int i = 0; i < weaponSlots_UI.Length; i++)
        {
            if (i < weaponSlots.Count)
            {
                bool isActiveWeapon = weaponSlots[i] == currentWeapon;
                weaponSlots_UI[i].UpdateWeaponSlot(weaponSlots[i], isActiveWeapon);
            }
            else
            {
                weaponSlots_UI[i].UpdateWeaponSlot(null, false);
            }
        }
    }

    // Update player health bar
    public void UpdateHealthUI(float currenHealth, float maxHealth)
    {
        healthBar.fillAmount = currenHealth / maxHealth;
    }

    // Update car health bar
    public void UpdateCarHealthUI(float currentCarHealth, float maxCarHealth)
    {
        carHealthBar.fillAmount = currentCarHealth / maxCarHealth;
    }

    // Update car speed text
    public void UpdateSpeedText(string text)
    {
        carSpeedText.text = text;
    }

    // Show/hide minimap
    public void ToggleMinimap(bool isActive)
    {
        if (minimap != null)
        {
            minimap.SetActive(isActive);
        }
    }

    #endregion
}

