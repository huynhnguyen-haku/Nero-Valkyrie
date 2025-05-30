using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ToolTipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private GameObject tooltip;

    [Header("Audio")]
    [SerializeField] private AudioSource pointerEnterSFX;
    [SerializeField] private AudioSource pointerDownSFX;

    // Play SFX on pointer down
    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownSFX?.Play();
    }

    // Show tooltip and play SFX on hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter == gameObject)
        {
            pointerEnterSFX?.Play();
        }
        if (tooltip != null)
            tooltip.SetActive(true);
    }

    // Hide tooltip on exit
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
            tooltip.SetActive(false);
    }
}

