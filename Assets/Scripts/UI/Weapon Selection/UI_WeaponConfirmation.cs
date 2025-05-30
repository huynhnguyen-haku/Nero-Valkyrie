using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_WeaponConfirmation : MonoBehaviour
{
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI weaponPriceText;
    [SerializeField] private TextMeshProUGUI confirmText;
    [SerializeField] private TextMeshProUGUI playerMoney;

    private Weapon_Data currentWeaponData;
    private GameManager gameManager;

    #region Unity Methods

    private void Start()
    {
        gameManager = GameManager.instance;
        playerMoney.text = $"Money: {gameManager.playerMoney} golds";
    }

    #endregion

    #region Confirmation Logic

    // Setup confirmation UI with weapon info
    public void SetupConfirmationUI(Weapon_Data weaponData)
    {
        currentWeaponData = weaponData;
        weaponIcon.sprite = weaponData.weaponIcon;
        weaponNameText.text = weaponData.weaponName;
        weaponPriceText.text = $"Price: {weaponData.price} golds";
        confirmText.text = "Do you want to buy this weapon?";
        playerMoney.text = $"Money: {GameManager.instance.playerMoney} golds";
    }

    // Confirm weapon purchase
    public void ConfirmPurchase()
    {
        if (GameManager.instance.playerMoney >= currentWeaponData.price)
        {
            // Deduct money and unlock weapon
            // Save the unlock state in the PlayerPrefs
            GameManager.instance.AddMoney(-currentWeaponData.price);
            currentWeaponData.isUnlocked = true;
            currentWeaponData.SaveUnlockState();

            Debug.Log($"Weapon {currentWeaponData.weaponName} unlocked!");
            confirmText.text = "Weapon unlocked!";
            playerMoney.text = $"Money: {GameManager.instance.playerMoney} golds";
            UI.instance.SwitchTo(UI.instance.weaponSelection.gameObject);
        }
        else
        {
            confirmText.text = "Not enough money!";
        }
    }

    // Return to weapon selection UI
    public void ReturnToWeaponSelection()
    {
        UI.instance.SwitchTo(UI.instance.weaponSelection.gameObject);
    }

    #endregion
}

