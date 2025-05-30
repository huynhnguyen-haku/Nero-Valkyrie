using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Enemy Data/Melee Weapon Data", order = 1)]
public class Enemy_MeleeWeaponData : ScriptableObject
{
    public List<AttackData_EnemyMelee> attackData;
    public float turnSpeed = 10;
}
