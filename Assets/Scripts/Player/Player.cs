using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform playerBody;
    public PlayerControls controls { get; private set; }
    public Player_AimController aim { get; private set; }
    public Player_Movement movement { get; private set; }
    public Player_WeaponController weapon { get; private set; }
    public Player_WeaponVisuals weaponVisuals { get; private set; }
    public Player_Interaction interaction { get; private set; }
    public Player_Health health { get; private set; }
    public Player_SFX sfx { get; private set; }
    public Ragdoll ragdoll { get; private set; }
    public Animator anim { get; private set; }

    public bool controlsEnabled { get; private set; }

    private void Awake()
    {
        aim = GetComponent<Player_AimController>();
        movement = GetComponent<Player_Movement>();
        weapon = GetComponent<Player_WeaponController>();
        weaponVisuals = GetComponentInChildren<Player_WeaponVisuals>();
        interaction = GetComponent<Player_Interaction>();
        health = GetComponent<Player_Health>();
        sfx = GetComponent<Player_SFX>();
        ragdoll = GetComponent<Ragdoll>();
        anim = GetComponentInChildren<Animator>();
        controls = ControlsManager.instance.controls;
    }

    private void OnEnable()
    {
        controls.Enable();

        // Register UI toggle actions to input events
        controls.Character.ToggleMissionUI.performed += ctx => UI.instance.inGameUI.ToggleMissionUI();
        controls.Character.TogglePauseUI.performed += ctx => UI.instance.TogglePauseUI();
        controls.Character.ToggleMinimap.performed += ctx =>
        {
            bool isMinimapActive = UI.instance.inGameUI.minimap.activeSelf;
            UI.instance.ToggleMinimap(!isMinimapActive);
        };
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    // Enable or disable player controls and related systems
    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
        ragdoll.ColliderActive(enabled);
        aim.EnableLaserAim(enabled);
        aim.EnableAimTarget(enabled);
    }

    public void SetMovementEnabled(bool enabled)
    {
        if (movement != null)
            movement.enabled = enabled;
    }

}
