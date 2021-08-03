using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon",menuName ="ScriptableObjects/Equipment/Weapon",order = 1)]
public class Weapon : ScriptableObject
{
    public enum WeaponType
    {
        MELEE,
        RANGED,
        NULL
    }
    public string weaponName;
    public WeaponType weaponType;
    public int meleeDamage;
    public int rangedDamage = 0;
    public int range;
    public int numberOfAttacks;
}
