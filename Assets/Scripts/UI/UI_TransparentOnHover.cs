using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_TransparentOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Store original colors for all images and texts in this UI
    private Dictionary<Image, Color> originalImageColors = new Dictionary<Image, Color>();
    private Dictionary<TextMeshProUGUI, Color> originalTextColors = new Dictionary<TextMeshProUGUI, Color>();

    private bool hasWeaponSlots;
    private Player_WeaponController playerWeaponController;

    private void Start()
    {
        // Check if this UI contains weapon slots
        hasWeaponSlots = GetComponentInChildren<UI_WeaponSlot>();
        if (hasWeaponSlots)
        {
            playerWeaponController = FindObjectOfType<Player_WeaponController>();
        }

        foreach (var image in GetComponentsInChildren<Image>(true))
        {
            originalImageColors[image] = image.color;
        }

        foreach (var text in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            originalTextColors[text] = text.color;
        }
    }

    // Make all images and texts semi-transparent on hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (var image in originalImageColors.Keys)
        {
            var color = image.color;
            color.a = 0.15f;
            image.color = color;
        }

        foreach (var text in originalTextColors.Keys)
        {
            var color = text.color;
            color.a = 0.15f;
            text.color = color;
        }
    }

    // Restore original colors and update weapon UI if needed
    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (var image in originalImageColors.Keys)
        {
            image.color = originalImageColors[image];
        }

        foreach (var text in originalTextColors.Keys)
        {
            text.color = originalTextColors[text];
        }

        // Refresh weapon UI if this panel contains weapon slots
        playerWeaponController?.UpdateWeaponUI();
    }
}

