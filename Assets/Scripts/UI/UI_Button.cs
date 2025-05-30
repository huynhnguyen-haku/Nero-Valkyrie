using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("Mouse Hover Settings")]
    public float scaleSpeed = 1;
    public float scaleRate;

    private Vector3 defaultScale;
    private Vector3 targetScale;

    private Image buttonImage;
    private TextMeshProUGUI buttonText;

    [Header("Audio")]
    [SerializeField] private AudioSource pointerEnterSFX;
    [SerializeField] private AudioSource pointerDownSFX;

    #region Unity Methods

    public virtual void Start()
    {
        defaultScale = transform.localScale;
        targetScale = defaultScale;
        buttonImage = GetComponentInChildren<Image>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public virtual void Update()
    {
        // Smoothly scale button for hover effect
        if (Mathf.Abs(transform.lossyScale.x - targetScale.x) > 0.01f)
        {
            float scaleValue = Mathf.Lerp(transform.localScale.x, targetScale.x, Time.unscaledDeltaTime * scaleSpeed);
            transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
        }
    }

    #endregion

    #region Pointer Events

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = defaultScale * scaleRate;
        pointerEnterSFX?.Play();
        if (buttonImage != null) buttonImage.color = Color.yellow;
        if (buttonText != null) buttonText.color = Color.yellow;
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        ResetToDefaultAppearance();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        pointerDownSFX?.Play();
        ResetToDefaultAppearance();
    }

    // Restore button to default appearance
    private void ResetToDefaultAppearance()
    {
        targetScale = defaultScale;
        if (buttonImage != null) buttonImage.color = Color.white;
        if (buttonText != null) buttonText.color = Color.white;
    }

    // Assign audio sources for button SFX
    public void AssignAudioSource()
    {
        pointerEnterSFX = GameObject.Find("UI_PointerEnter").GetComponent<AudioSource>();
        pointerDownSFX = GameObject.Find("UI_PointerDown").GetComponent<AudioSource>();
    }

    #endregion
}

