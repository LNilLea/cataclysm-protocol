using UnityEngine;

/// <summary>
/// 武器类型枚举
/// </summary>
public enum WeaponType
{
    // 钝器
    BaseballBat,    // 棒球棍
    SteelPipe,      // 钢管
    GreatHammer,    // 大锤
    
    // 锐器
    Dagger,         // 匕首
    Stiletto,       // 胁差
    SwiftSword,     // 迅捷剑
    
    // 远程武器
    Crossbow,       // 弩
    Pistol,         // 手枪
    AssaultRifle,   // 突击步枪
    SMG,            // 冲锋枪
    SniperRifle,    // 狙击枪
    Shotgun,        // 霰弹枪
    Slingshot,      // 弹弓
    RocketLauncher  // 火箭筒
}

/// <summary>
/// 武器基类 - 所有武器的父类
/// </summary>

[System.Serializable]
public class Weapon
{
    public string Name;               // 武器名称
    public WeaponType Type;           // 武器类型
    public Vector2Int DamageRange;    // 伤害范围
    public int HitBonus;              // 命中加值
    public int RequiredStrength;      // 使用该武器所需的力量/体魄
    public int AdditionalBonus;       // 额外伤害加减
    public string Effect;             // 武器的特殊效果
    public int WeaponSize;            // 武器尺寸（影响物理攻击判定）
    public float Range;               // 武器射程（用于远程武器）

    // 攻击范围（格数）
    public int AttackRangeMin;        // 最小攻击距离（格）
    public int AttackRangeMax;        // 最大攻击距离（格）

    /// <summary>
    /// 完整构造函数
    /// </summary>
    public Weapon(string name, WeaponType type, Vector2Int damageRange, int hitBonus, 
                  int requiredStrength, int additionalBonus, string effect, int weaponSize, 
                  float range, int attackRangeMin, int attackRangeMax)
    {
        Name = name;
        Type = type;
        DamageRange = damageRange;
        HitBonus = hitBonus;
        RequiredStrength = requiredStrength;
        AdditionalBonus = additionalBonus;
        Effect = effect;
        WeaponSize = weaponSize;
        Range = range;
        AttackRangeMin = attackRangeMin;
        AttackRangeMax = attackRangeMax;
    }

    /// <summary>
    /// 简化构造函数（用于近战武器）
    /// </summary>
    public Weapon(string name, WeaponType type, Vector2Int damageRange, int hitBonus, 
                  int requiredStrength, int additionalBonus, string effect, int weaponSize)
    {
        Name = name;
        Type = type;
        DamageRange = damageRange;
        HitBonus = hitBonus;
        RequiredStrength = requiredStrength;
        AdditionalBonus = additionalBonus;
        Effect = effect;
        WeaponSize = weaponSize;
        Range = 0;
        AttackRangeMin = 1;
        AttackRangeMax = 1;
    }

    /// <summary>
    /// 计算命中值（子类可重写）
    /// 公式：d20 + (属性-3) + 命中加值
    /// </summary>
    public virtual int CalculateHit(int attributeValue)
    {
        return Random.Range(1, 21) + (attributeValue - 3) + HitBonus;
    }

    /// <summary>
    /// 计算伤害（子类可重写）
    /// 公式：武器伤害 + (属性-3) + 额外加值
    /// </summary>
    public virtual int CalculateDamage(int attributeValue)
    {
        int baseDamage = Random.Range(DamageRange.x, DamageRange.y + 1);
        return baseDamage + (attributeValue - 3) + AdditionalBonus;
    }

    /// <summary>
    /// 判断目标是否在攻击范围内
    /// </summary>
    public bool IsInRange(int targetDistance)
    {
        return targetDistance >= AttackRangeMin && targetDistance <= AttackRangeMax;
    }

    /// <summary>
    /// 获取武器信息摘要
    /// </summary>
    public virtual string GetWeaponInfo()
    {
        return $"{Name} | 伤害:{DamageRange.x}-{DamageRange.y} | 命中:+{HitBonus} | 范围:{AttackRangeMin}-{AttackRangeMax}格";
    }
}
