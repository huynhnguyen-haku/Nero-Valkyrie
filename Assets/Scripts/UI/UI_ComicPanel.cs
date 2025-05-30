using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ComicPanel : MonoBehaviour, IPointerDownHandler
{
    private Image myImage;

    [SerializeField] private Image[] comicPanel;
    [SerializeField] private GameObject continueButton;

    private int imageIndex;
    private bool isComicFinished;

    #region Unity Methods

    private void Start()
    {
        myImage = GetComponent<Image>();
        ShowNextImage();
    }

    #endregion

    #region Comic Logic

    // Fade in each comic panel image in sequence
    private void ShowNextImage()
    {
        if (isComicFinished)
            return;

        StartCoroutine(ChangeImageAlpha(1, 2.5f, ShowNextImage));
    }

    // Fade image alpha over time, then show next
    private IEnumerator ChangeImageAlpha(float targetAlpha, float duration, System.Action onComplete)
    {
        float timeElapsed = 0f;
        Color currentColor = comicPanel[imageIndex].color;
        float startAlpha = currentColor.a;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / duration);
            comicPanel[imageIndex].color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        comicPanel[imageIndex].color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
        imageIndex++;

        if (imageIndex >= comicPanel.Length)
        {
            EndComicDisplay();
        }

        onComplete?.Invoke();
    }

    // End comic and show continue button
    private void EndComicDisplay()
    {
        StopAllCoroutines();
        isComicFinished = true;
        continueButton.SetActive(true);
        myImage.raycastTarget = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ShowNextImageOnClick();
    }

    // Instantly show next image on click
    private void ShowNextImageOnClick()
    {
        if (imageIndex >= comicPanel.Length)
        {
            EndComicDisplay();
            return;
        }

        comicPanel[imageIndex].color = Color.white;
        imageIndex++;

        if (imageIndex >= comicPanel.Length)
        {
            EndComicDisplay();
            return;
        }

        if (isComicFinished)
            return;

        ShowNextImage();
    }

    #endregion
}

