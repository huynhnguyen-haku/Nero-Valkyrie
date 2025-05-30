using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI instance;

    public UI_InGame inGameUI { get; private set; }
    public UI_WeaponSelection weaponSelection { get; private set; }
    public UI_GameOver gameOverUI { get; private set; }
    public UI_Settings settingsUI { get; private set; }
    public UI_WeaponConfirmation weaponConfirmation { get; private set; }

    public GameObject victoryScreenUI;
    public GameObject pauseUI;

    [SerializeField] private GameObject[] UIElements;

    [Header("Fade Image")]
    [SerializeField] private Image fadeImage;

    #region Unity Methods

    private void Awake()
    {
        // Cache references to all UI panels
        instance = this;
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        weaponSelection = GetComponentInChildren<UI_WeaponSelection>(true);
        gameOverUI = GetComponentInChildren<UI_GameOver>(true);
        settingsUI = GetComponentInChildren<UI_Settings>(true);
        weaponConfirmation = GetComponentInChildren<UI_WeaponConfirmation>(true);
    }

    private void Start()
    {
        AssignInputUI();
        StartCoroutine(ChangeImageAlpha(0, 1.5f, null));
        settingsUI.LoadSettingsValues();

        // Quick start for testing/recording
        if (GameManager.instance.quickStart)
        {
            StartGame();
        }
    }

    #endregion

    #region UI Navigation

    // Switch to a specific UI panel
    public void SwitchTo(GameObject uiElementToActivate)
    {
        foreach (GameObject go in UIElements)
            go.SetActive(false);

        uiElementToActivate.SetActive(true);
    }

    // Start the game and show in-game UI
    public void StartGame()
    {
        StartCoroutine(StartGameSequence());
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        TogglePauseUI();
        StartCoroutine(ChangeImageAlpha(1, 1f, GameManager.instance.RestartScene));
    }

    // Show game over UI with a message
    public void ShowGameOverUI(string message = "Game Over")
    {
        SwitchTo(gameOverUI.gameObject);
        gameOverUI.ShowGameOverMessage(message);
    }

    public void StartLevelGeneration() => LevelGenerator.instance.InitializeGeneration();

    // Toggle pause menu and handle input/cursor state
    public void TogglePauseUI()
    {
        if (pauseUI.activeSelf)
        {
            SwitchTo(inGameUI.gameObject);
            Cursor.visible = false;
            if (GameManager.instance.player.movement.isInCar)
                ControlsManager.instance.SwitchToCarControls();
            else
                ControlsManager.instance.SwitchToCharacterControls();

            TimeManager.instance.ResumeTime();

            // Refresh interactables
            Player_Interaction playerInteraction = GameManager.instance.player.GetComponent<Player_Interaction>();
            if (playerInteraction != null)
            {
                playerInteraction.GetInteractables().Clear();
                playerInteraction.UpdateClosestInteracble();
            }
            return;
        }

        if (!inGameUI.gameObject.activeSelf)
        {
            Debug.Log("Pause Menu is only available when In-Game UI is active.");
            return;
        }

        Cursor.visible = true;
        SwitchTo(pauseUI);
        ControlsManager.instance.SwitchToUIControls();
        TimeManager.instance.PauseTime();
    }

    public void ToggleMinimap(bool isActive)
    {
        inGameUI.ToggleMinimap(isActive);
    }

    #endregion

    #region UI Effects & Helpers

    // Assign input events for UI controls
    private void AssignInputUI()
    {
        PlayerControls controls = GameManager.instance.player.controls;
        controls.UI.TogglePauseUI.performed += ctx => TogglePauseUI();
    }

    // Show victory screen with fade effect
    public void DisplayVictoryScreenUI()
    {
        StartCoroutine(ChangeImageAlpha(1, 1.5f, SwitchToVictoryScreenUI));
    }

    private void SwitchToVictoryScreenUI()
    {
        SwitchTo(victoryScreenUI);
        Color color = fadeImage.color;
        color.a = 0;
        fadeImage.color = color;
    }

    // Fade in/out effect for UI transitions
    private IEnumerator StartGameSequence()
    {
        bool quickStart = GameManager.instance.quickStart;

        if (!quickStart)
        {
            fadeImage.color = Color.black;
            StartCoroutine(ChangeImageAlpha(1, 1, null));
            yield return new WaitForSeconds(1f);
        }

        yield return null;
        SwitchTo(inGameUI.gameObject);
        GameManager.instance.GameStart();

        if (quickStart)
            StartCoroutine(ChangeImageAlpha(0, 0.1f, null));
        else
            StartCoroutine(ChangeImageAlpha(0, 1f, null));
    }

    private IEnumerator ChangeImageAlpha(float targetAlpha, float duration, System.Action onComplete)
    {
        float timeElapsed = 0f;
        Color currentColor = fadeImage.color;
        float startAlpha = currentColor.a;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / duration);
            fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
        onComplete?.Invoke();
    }

    // Assign audio sources to all UI buttons in the scene
    [ContextMenu("Assign Audio Listeners to Button")]
    public void AssignAudioListenersToButton()
    {
        UI_Button[] buttons = FindObjectsOfType<UI_Button>(true);
        foreach (var button in buttons)
        {
            button.AssignAudioSource();
        }
    }

    #endregion
}

