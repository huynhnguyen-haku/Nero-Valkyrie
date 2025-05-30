using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public enum Enemy_MeleeWeaponType { OneHand, Throw, Unarmed }
public enum Enemy_RangeWeaponType { Pistol, Revolver, Shotgun, Rifle, Sniper, Random }

public class Enemy_Visual : MonoBehaviour
{
    public GameObject currentWeaponModel;
    public GameObject grenadeModel;

    [Header("Color")]
    [SerializeField] private Texture[] colorTexures;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderers;

    [Header("Crystal")]
    [SerializeField] private GameObject[] crystals;
    [SerializeField] private int crystalAmount;

    [Header("Rig Reference")]
    [SerializeField] private Transform leftHandIK;
    [SerializeField] private Transform leftElbowIK;
    [SerializeField] private TwoBoneIKConstraint leftHandIKConstraint;
    [SerializeField] private MultiAimConstraint weaponAimConstraint;

    private float leftHandTargetWeight;
    private float weaponAimTargetWeight;
    private float rigChangeRate;

    private Enemy_Range enemyRange;

    private void Awake()
    {
        enemyRange = GetComponent<Enemy_Range>();
    }

    private void Update()
    {
        // Smoothly update IK weights for ranged enemies
        if (enemyRange != null)
        {
            leftHandIKConstraint.weight = AdjustIKWeight(leftHandIKConstraint.weight, leftHandTargetWeight);
            weaponAimConstraint.weight = AdjustIKWeight(weaponAimConstraint.weight, weaponAimTargetWeight);
        }
    }

    #region Weapon Methods

    public void EnableWeaponTrail(bool enable)
    {
        Enemy_WeaponModel currentWeaponScript = currentWeaponModel.GetComponent<Enemy_WeaponModel>();
        currentWeaponScript.EnableTrailEffect(enable);
    }

    public void EnableWeaponModel(bool active)
    {
        currentWeaponModel?.gameObject.SetActive(active);
    }

    public void EnableHoldingWeaponModel(bool active)
    {
        FindHoldingWeaponModel()?.SetActive(active);
    }

    public void EnableGrenadeModel(bool active)
    {
        grenadeModel?.SetActive(active);
    }

    // Find and return the correct range weapon model for this enemy
    private GameObject FindRangeWeaponModel()
    {
        Enemy_RangeWeaponModel[] weaponModels = GetComponentsInChildren<Enemy_RangeWeaponModel>(true);
        Enemy_RangeWeaponType weaponType = enemyRange.weaponType;

        foreach (var weaponModel in weaponModels)
        {
            if (weaponModel.weaponType == weaponType)
            {
                SwitchAnimationLayer(((int)weaponModel.weaponHoldType));
                SetupLeftHandIK(weaponModel.leftHandTarget, weaponModel.leftElbowTarget);
                return weaponModel.gameObject;
            }
        }

        Debug.LogError("No matching weapon model found for type: " + weaponType);
        return null;
    }

    // Find and return a random melee weapon model for this enemy
    private GameObject FindMeleeWeaponModel()
    {
        Enemy_WeaponModel[] weaponModels = GetComponentsInChildren<Enemy_WeaponModel>(true);
        Enemy_MeleeWeaponType weaponType = GetComponent<Enemy_Melee>().weaponType;
        List<Enemy_WeaponModel> filteredWeaponModels = new List<Enemy_WeaponModel>();

        foreach (var weaponModel in weaponModels)
        {
            if (weaponModel.weaponType == weaponType)
                filteredWeaponModels.Add(weaponModel);
        }

        int randomIndex = Random.Range(0, filteredWeaponModels.Count);
        return filteredWeaponModels[randomIndex].gameObject;
    }

    // Find and return the holding weapon model for this enemy
    private GameObject FindHoldingWeaponModel()
    {
        Enemy_HoldWeaponModel[] weaponModels = GetComponentsInChildren<Enemy_HoldWeaponModel>(true);
        Enemy_RangeWeaponType weaponType = GetComponentInParent<Enemy_Range>().weaponType;

        foreach (var weaponModel in weaponModels)
        {
            if (weaponModel.weaponType == weaponType)
                return weaponModel.gameObject;
        }
        return null;
    }

    // Override the animator controller with the weapon's override if available
    private void OverrideAnimatorController()
    {
        AnimatorOverrideController overrideController = currentWeaponModel.GetComponent<Enemy_WeaponModel>()?.overrideController;
        if (overrideController != null)
            GetComponentInChildren<Animator>().runtimeAnimatorController = overrideController;
    }

    // Switch animation layer for weapon holding type
    private void SwitchAnimationLayer(int layerIndex)
    {
        Animator animator = GetComponentInChildren<Animator>();

        // Turn off all layers
        for (int i = 1; i < animator.layerCount; i++)
            animator.SetLayerWeight(i, 0);

        // Turn on the layer we want
        animator.SetLayerWeight(layerIndex, 1);
    }
    #endregion

    #region Visual Setup Methods

    // Randomize color, weapon, and crystals for enemy
    public void SetupVisual()
    {
        SetupRandomColor();
        SetupRandomWeapon();
        SetupRandomCrystals();
    }

    private void SetupRandomColor()
    {
        int randomIndex = Random.Range(0, colorTexures.Length);

        Material newMat = new Material(skinnedMeshRenderers.material);
        newMat.mainTexture = colorTexures[randomIndex];

        skinnedMeshRenderers.material = newMat;
    }

    private void SetupRandomWeapon()
    {
        bool thisEnemyIsMelee = GetComponent<Enemy_Melee>() != null;
        bool thisEnemyIsRange = enemyRange != null;

        if (thisEnemyIsMelee)
            currentWeaponModel = FindMeleeWeaponModel();

        if (thisEnemyIsRange)
            currentWeaponModel = FindRangeWeaponModel();

        currentWeaponModel.SetActive(true);
        OverrideAnimatorController();
    }

    private void SetupRandomCrystals()
    {
        List<int> availableIndexs = new List<int>();
        crystals = CollectCrystals();

        for (int i = 0; i < crystals.Length; i++)
        {
            availableIndexs.Add(i);
            crystals[i].SetActive(false);
        }

        for (int i = 0; i < crystalAmount; i++)
        {
            if (availableIndexs.Count == 0)
                break;

            int randomIndex = Random.Range(0, availableIndexs.Count);
            int objectIndex = availableIndexs[randomIndex];

            crystals[objectIndex].SetActive(true);
            availableIndexs.RemoveAt(randomIndex);
        }
    }

    // Collect all crystal GameObjects in children (to randomize active)
    private GameObject[] CollectCrystals()
    {
        Enemy_Crystal[] crystalComponents = GetComponentsInChildren<Enemy_Crystal>(true);
        GameObject[] crystals = new GameObject[crystalComponents.Length];

        for (int i = 0; i < crystalComponents.Length; i++)
            crystals[i] = crystalComponents[i].gameObject;

        return crystals;
    }
    #endregion

    #region IK Methods

    // Enable/disable IK for left hand and weapon aim
    public void EnableIK(bool enableLeftHand, bool enableAim, float changeRate = 10)
    {
        if (enemyRange != null)
        {
            rigChangeRate = changeRate;
            leftHandTargetWeight = enableLeftHand ? 1 : 0;
            weaponAimTargetWeight = enableAim ? 1 : 0;
        }
    }

    // Set up IK targets for left hand and elbow
    private void SetupLeftHandIK(Transform leftHandTarget, Transform leftElbowTarget)
    {
        if (enemyRange != null)
        {
            leftHandIK.localPosition = leftHandTarget.localPosition;
            leftHandIK.localRotation = leftHandTarget.localRotation;

            leftElbowIK.localPosition = leftElbowTarget.localPosition;
            leftElbowIK.localRotation = leftElbowTarget.localRotation;
        }
    }

    // Smoothly adjust IK weight toward target
    private float AdjustIKWeight(float currentWeight, float targetWeight)
    {
        if (Mathf.Abs(currentWeight - targetWeight) > 0.05f)
            return Mathf.Lerp(currentWeight, targetWeight, rigChangeRate * Time.deltaTime);

        else
            return targetWeight;
    }
    #endregion
}
