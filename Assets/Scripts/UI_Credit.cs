using UnityEngine;

public class UI_Credit : MonoBehaviour
{
    [SerializeField] private RectTransform creditText;

    private float scrollSpeed = 150f;
    private bool isScrolling = true;
    private Vector2 startPosition; // Initial position of creditText

    #region Unity Methods

    private void Awake()
    {
        // Cache the initial position of the credit text
        startPosition = creditText.anchoredPosition;
    }

    private void OnEnable()
    {
        // Reset position and start scrolling when UI is enabled
        creditText.anchoredPosition = startPosition;
        isScrolling = true;
    }

    private void OnDisable()
    {
        // Stop scrolling when UI is disabled
        isScrolling = false;
    }

    private void Update()
    {
        // Scroll the credit text upward if active
        if (isScrolling)
        {
            creditText.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
        }
    }

    #endregion
}
