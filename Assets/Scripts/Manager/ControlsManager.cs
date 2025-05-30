using UnityEngine;

public class ControlsManager : MonoBehaviour
{
    public static ControlsManager instance { get; private set; }
    public PlayerControls controls { get; private set; }
    private Player player;

    #region Unity Methods

    private void Awake()
    {
        instance = this;
        controls = new PlayerControls();
    }

    private void Start()
    {
        player = GameManager.instance.player;
        SwitchToCharacterControls();
    }

    #endregion

    #region Control Switching

    // Enable character controls and related UI
    public void SwitchToCharacterControls()
    {
        controls.Character.Enable();
        controls.UI.Disable();
        controls.Car.Disable();

        player.SetControlsEnabled(true);
        UI.instance.inGameUI.SwitchToCharacterUI();
    }

    // Enable UI controls and disable gameplay controls
    public void SwitchToUIControls()
    {
        controls.UI.Enable();
        controls.Character.Disable();
        controls.Car.Disable();

        player.SetControlsEnabled(false);
    }

    // Enable car controls and related UI
    public void SwitchToCarControls()
    {
        controls.Car.Enable();
        controls.Character.Disable();
        controls.UI.Disable();

        player.SetControlsEnabled(false);
        UI.instance.inGameUI.SwitchToCarUI();
    }

    #endregion
}
