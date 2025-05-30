using UnityEngine;

public enum Enemy_RangeWeaponHoldType { Common, Low, High }

public class Enemy_RangeWeaponModel : MonoBehaviour
{
    public Transform gunPoint;
    [Space]
    public Enemy_RangeWeaponType weaponType;
    public Enemy_RangeWeaponHoldType weaponHoldType;

    public Transform leftHandTarget;
    public Transform leftElbowTarget;

    [Header("Audio")]
    public AudioSource fireSFX;
}
