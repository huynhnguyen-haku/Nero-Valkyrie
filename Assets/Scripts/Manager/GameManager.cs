using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player player;
    public Car_Controller currentCar;

    public int playerMoney;

    [Header("Settings")]
    public bool friendlyFire;
    public bool quickStart; // Remove this in the final build

    // Game state for BGM management
    private enum GameState { MainMenu, InGame, GameOver, MissionComplete }
    private GameState currentGameState = GameState.MainMenu;

    #region Unity Methods

    private void Awake()
    {
        instance = this;
        player = FindObjectOfType<Player>();
        LoadPlayerMoney();
    }

    private void Start()
    {
        UpdateGameState(GameState.MainMenu);
    }

    #endregion

    #region Player Money

    // Add money and save to PlayerPrefs
    public void AddMoney(int amount)
    {
        playerMoney += amount;
        Debug.Log($"Player received {amount} golds. Total money: {playerMoney}");
        SavePlayerMoney();
    }

    private void SavePlayerMoney()
    {
        PlayerPrefs.SetInt("PlayerMoney", playerMoney);
        PlayerPrefs.Save();
    }

    private void LoadPlayerMoney()
    {
        playerMoney = PlayerPrefs.GetInt("PlayerMoney", 0);
        Debug.Log($"Loaded player money: {playerMoney}");
    }

    #endregion

    #region Game State Logic

    public void GameStart()
    {
        SetDefaultWeapon();
        Cursor.visible = false;
        UpdateGameState(GameState.InGame);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Cursor.visible = true;

        PathfindingIndicator pathfindingIndicator = FindObjectOfType<PathfindingIndicator>();
        if (pathfindingIndicator != null)
        {
            pathfindingIndicator.Reset();
        }
        else
        {
            Debug.LogWarning("GameManager: PathfindingIndicator not found during RestartScene!");
        }
        UpdateGameState(GameState.MainMenu);
    }

    public void GameOver()
    {
        TimeManager.instance.SlowMotionFor(2);
        UI.instance.ShowGameOverUI();
        //CameraManager.instance.ChangeCameraDistance(5);
        Cursor.visible = true;
        UpdateGameState(GameState.GameOver);
    }

    public void CompleteGame()
    {
        UI.instance.DisplayVictoryScreenUI();
        ControlsManager.instance.controls.Character.Disable();
        player.health.currentHealth += 999;
        Cursor.visible = true;
        UpdateGameState(GameState.MissionComplete);
    }

    // Set default weapon selection for player
    private void SetDefaultWeapon()
    {
        List<Weapon_Data> newList = UI.instance.weaponSelection.SelectedWeaponData();
        player.weapon.SetDefaultWeapon(newList);
    }

    // Update game state and play corresponding BGM
    private void UpdateGameState(GameState newState)
    {
        if (currentGameState == newState)
            return;

        currentGameState = newState;

        switch (currentGameState)
        {
            case GameState.MainMenu:
                AudioManager.instance.PlayBGM(0);
                break;

            case GameState.InGame:
                AudioManager.instance.PlayBGM(1);
                break;

            case GameState.GameOver:
                AudioManager.instance.PlayBGM(2);
                break;

            case GameState.MissionComplete:
                AudioManager.instance.PlayBGM(3);
                break;
        }

        Debug.Log($"GameManager: Updated state to {currentGameState}");
    }

    #endregion
}
