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
        player.SetMovementEnabled(true);
        player.aim.allowCameraLook = true;
        player.weapon.enabled = true; // Cho phép bắn
        player.aim.allowZoom = true; // Cho phép zoom khi điều khiển nhân vật
        UI.instance.inGameUI.SwitchToCharacterUI();
    }

    public void SwitchToCarControls()
    {
        controls.Car.Enable();
        controls.Character.Enable();
        controls.UI.Disable();

        player.SetControlsEnabled(false);
        player.SetMovementEnabled(false);
        player.aim.allowCameraLook = true;
        player.weapon.enabled = false; // Không cho phép bắn khi ở trên xe
        player.aim.allowZoom = false; // Không cho phép zoom khi ở trên xe
        UI.instance.inGameUI.SwitchToCarUI();
    }

    public void SwitchToUIControls()
    {
        controls.UI.Enable();
        controls.Character.Disable();
        controls.Car.Disable();

        player.SetControlsEnabled(false);
        player.aim.allowCameraLook = false; // Không cho phép xoay camera khi ở UI
        player.weapon.enabled = false; // Không cho phép bắn khi ở UI
    }
    #endregion
}
