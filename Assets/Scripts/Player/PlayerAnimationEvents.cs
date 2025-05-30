using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private Player_WeaponVisuals weaponVisualController;
    private Player_WeaponController weaponController;

    private void Start()
    {
        weaponVisualController = GetComponentInParent<Player_WeaponVisuals>();
        weaponController = GetComponentInParent<Player_WeaponController>();
    }

    #region Animation Events

    // Called at the end of reload animation to actually refill bullets and update UI
    public void ReloadIsOver()
    {
        weaponVisualController.MaximizeRigWeight();
        weaponController.CurrentWeapon().RefillBullets();
        weaponController.SetWeaponReady(true);
        weaponController.UpdateWeaponUI();
    }

    // Called to restore rig and left hand IK weights after animation
    public void ReturnRig()
    {
        weaponVisualController.MaximizeRigWeight();
        weaponVisualController.MaximizeLeftHandWeight();
    }

    // Called at the end of weapon equip animation to allow firing
    public void WeaponEquippingIsOver()
    {
        weaponController.SetWeaponReady(true);
    }

    // Called to activate the current weapon model (e.g. after equip)
    public void SwitchOnWeapon() => weaponVisualController.SwitchOnCurrentWeaponModel();

    #endregion
}
