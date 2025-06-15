using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Player_WeaponVisuals : MonoBehaviour
{
    private Player player;
    private Animator animator;

    [SerializeField] private WeaponModel[] weaponModels;
    [SerializeField] private BackupWeaponModel[] backupModels;

    [Header("Left Hand IK Settings")]
    [SerializeField] private Transform leftHandIK_Target;
    [SerializeField] private Transform leftHandIK_Hint;
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private float leftHandIK_WeightIncrease;
    private bool shouldIncrease_LeftHandIK_Weight;

    [Header("Rig Settings")]
    [SerializeField] private float rig_WeightIncrease;
    private bool shouldIncrease_Rig_Weight;
    private Rig rig;

    private void Start()
    {
        player = GetComponent<Player>();
        animator = GetComponentInChildren<Animator>();
        rig = GetComponentInChildren<Rig>();
        weaponModels = GetComponentsInChildren<WeaponModel>(true);
        backupModels = GetComponentsInChildren<BackupWeaponModel>(true);
    }

    private void Update()
    {
        UpdateRigWeight();
        UpdateLeftHandWeight();
    }

    #region Animation Triggers

    public void PlayFireAnimation()
    {
        animator.SetTrigger("Fire");
    }

    public void PlayReloadAnimation()
    {
        float reloadSpeed = player.weapon.CurrentWeapon().reloadSpeed / 2;
        animator.SetTrigger("Reload");
        animator.SetFloat("ReloadSpeed", reloadSpeed);
        ReduceRigWeight();
    }

    public void PlayWeaponEquipAnimation()
    {
        EquipType equipType = CurrentWeaponModel().equipAnimationType;
        float equipSpeed = player.weapon.CurrentWeapon().equipSpeed;

        leftHandIK.weight = 0;
        ReduceRigWeight();
        animator.SetTrigger("EquipWeapon");
        animator.SetFloat("EquipType", (float)equipType);
        animator.SetFloat("EquipSpeed", equipSpeed);
    }

    #endregion

    #region Weapon Model Switching

    // Show only the current weapon model and correct backup models
    public void SwitchOnCurrentWeaponModel()
    {
        int animationIndex = (int)CurrentWeaponModel().holdType;

        SwitchOffWeaponModels();
        SwitchOffBackupWeaponModels();

        if (!player.weapon.HasOneWeapon())
            SwitchOnBackupWeaponModels();

        SwitchAnimationLayer(animationIndex);
        CurrentWeaponModel().gameObject.SetActive(true);
        AttachLeftHand();
    }

    public void SwitchOffWeaponModels()
    {
        for (int i = 0; i < weaponModels.Length; i++)
        {
            weaponModels[i].gameObject.SetActive(false);
        }
    }

    public void SwitchOffBackupWeaponModels()
    {
        foreach (BackupWeaponModel backupModel in backupModels)
        {
            backupModel.gameObject.SetActive(false);
        }
    }

    // Show backup weapon models for all weapons not currently equipped
    public void SwitchOnBackupWeaponModels()
    {
        SwitchOffBackupWeaponModels();
        BackupWeaponModel lowhangWeapon = null;
        BackupWeaponModel backhangWeapon = null;
        BackupWeaponModel sidehangWeapon = null;

        foreach (BackupWeaponModel backupModel in backupModels)
        {
            if (backupModel.weaponType == player.weapon.CurrentWeapon().weaponType)
                continue;

            if (player.weapon.WeaponInSlots(backupModel.weaponType) != null)
            {
                if (backupModel.HangTypeIs(HangType.LowBackHang))
                    lowhangWeapon = backupModel;

                if (backupModel.HangTypeIs(HangType.BackHang))
                    backhangWeapon = backupModel;

                if (backupModel.HangTypeIs(HangType.SideHang))
                    sidehangWeapon = backupModel;
            }
        }

        // Activate backup models if found
        lowhangWeapon?.gameObject.SetActive(true);
        backhangWeapon?.gameObject.SetActive(true);
        sidehangWeapon?.gameObject.SetActive(true);
    }

    // Set animator layer for weapon holding type
    private void SwitchAnimationLayer(int layerIndex)
    {
        for (int i = 0; i < animator.layerCount; i++)
        {
            animator.SetLayerWeight(i, 0);
        }
        animator.SetLayerWeight(layerIndex, 1);
    }

    // Get the currently equipped weapon model
    public WeaponModel CurrentWeaponModel()
    {
        WeaponModel weaponModel = null;
        WeaponType weaponType = player.weapon.CurrentWeapon().weaponType;
        for (int i = 0; i < weaponModels.Length; i++)
        {
            if (weaponModels[i].weaponType == weaponType)
            {
                weaponModel = weaponModels[i];
            }
        }
        return weaponModel;
    }

    #endregion

    #region Animation Rigging Methods

    // Attach left hand IK to weapon hold point
    private void AttachLeftHand()
    {
        Transform targetTranform = CurrentWeaponModel().holdPoint;
        leftHandIK_Target.localPosition = targetTranform.localPosition;
        leftHandIK_Target.localRotation = targetTranform.localRotation;

        if (CurrentWeaponModel().leftElbowHintPoint != null && leftHandIK_Hint != null)
        {
            leftHandIK_Hint.localPosition = CurrentWeaponModel().leftElbowHintPoint.localPosition;
            leftHandIK_Hint.localRotation = CurrentWeaponModel().leftElbowHintPoint.localRotation;
        }
    }

    // Smoothly increase left hand IK weight
    private void UpdateLeftHandWeight()
    {
        if (shouldIncrease_LeftHandIK_Weight)
        {
            leftHandIK.weight += leftHandIK_WeightIncrease * Time.deltaTime;
            if (leftHandIK.weight >= 1)
            {
                shouldIncrease_LeftHandIK_Weight = false;
            }
        }
    }

    // Smoothly increase rig weight
    private void UpdateRigWeight()
    {
        if (shouldIncrease_Rig_Weight)
        {
            rig.weight += rig_WeightIncrease * Time.deltaTime;
            if (rig.weight >= 1)
            {
                shouldIncrease_Rig_Weight = false;
            }
        }
    }

    // Instantly reduce rig weight (for equip/reload)
    private void ReduceRigWeight()
    {
        rig.weight = 0.15f;
    }

    public void MaximizeRigWeight() => shouldIncrease_Rig_Weight = true;
    public void MaximizeLeftHandWeight() => shouldIncrease_LeftHandIK_Weight = true;

    #endregion
}

