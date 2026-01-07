using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "游戏/武器")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public WeaponType weaponType;
    public Vector2Int damageRange;
    public int hitBonus;
    public int requiredStrength;
    public int additionalBonus;
    public string effect;
    public int weaponSize;
    public float range;
    public int attackRangeMin = 1;
    public int attackRangeMax = 1;

    // 转换成 Weapon 对象
    public Weapon ToWeapon()
    {
        return new Weapon(
            weaponName, weaponType, damageRange, hitBonus,
            requiredStrength, additionalBonus, effect, weaponSize,
            range, attackRangeMin, attackRangeMax
        );
    }
}