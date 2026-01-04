using UnityEngine;

/// <summary>
/// 锐器武器类 - 使用反应进行命中和伤害计算
/// 攻击范围：匕首/胁差 1格（近距离），迅捷剑 2格（中距离）
/// </summary>
public class SharpWeapon : Weapon
{
    public enum SharpWeaponType
    {
        Dagger,         // 匕首
        Stiletto,       // 胁差
        SwiftSword      // 迅捷剑
    }

    /// <summary>
    /// 锐器构造函数
    /// </summary>
    /// <param name="name">武器名称</param>
    /// <param name="damageRange">伤害范围</param>
    /// <param name="hitBonus">命中加值</param>
    /// <param name="requiredAgility">反应要求</param>
    /// <param name="additionalBonus">额外伤害加值</param>
    /// <param name="effect">特殊效果</param>
    /// <param name="weaponSize">武器尺寸</param>
    /// <param name="attackRangeMin">最小攻击距离（格）</param>
    /// <param name="attackRangeMax">最大攻击距离（格）</param>
    public SharpWeapon(string name, Vector2Int damageRange, int hitBonus, int requiredAgility,
                       int additionalBonus, string effect, int weaponSize,
                       int attackRangeMin = 1, int attackRangeMax = 1)
        : base(name, WeaponType.Dagger, damageRange, hitBonus, requiredAgility,
               additionalBonus, effect, weaponSize, 0f, attackRangeMin, attackRangeMax)
    {
        this.HitBonus = hitBonus;
        this.AdditionalBonus = additionalBonus;
    }

    // ===== 工厂方法 =====

    /// <summary>
    /// 创建匕首
    /// 伤害：2-4，命中：+2，反应要求：3，额外伤害：+1，攻击范围：1格
    /// </summary>
    public static SharpWeapon CreateDagger()
    {
        return new SharpWeapon(
            "匕首",
            new Vector2Int(2, 4),       // 伤害范围：2~4
            2,                          // 命中加值：+2
            3,                          // 反应要求：3
            1,                          // 额外加值：+1
            "",                         // 无特殊效果
            1,                          // 武器尺寸：1
            1,                          // 最小攻击距离：1格
            1                           // 最大攻击距离：1格（近距离）
        );
    }

    /// <summary>
    /// 创建胁差
    /// 伤害：5-7，命中：+4，反应要求：4，额外伤害：+2，攻击范围：1格
    /// </summary>
    public static SharpWeapon CreateStiletto()
    {
        return new SharpWeapon(
            "胁差",
            new Vector2Int(5, 7),       // 伤害范围：5~7
            4,                          // 命中加值：+4
            4,                          // 反应要求：4
            2,                          // 额外加值：+2
            "",                         // 无特殊效果
            1,                          // 武器尺寸：1
            1,                          // 最小攻击距离：1格
            1                           // 最大攻击距离：1格（近距离）
        );
    }

    /// <summary>
    /// 创建迅捷剑
    /// 伤害：8-10，命中：+6，反应要求：5，额外伤害：+3，攻击范围：1-2格
    /// </summary>
    public static SharpWeapon CreateSwiftSword()
    {
        return new SharpWeapon(
            "迅捷剑",
            new Vector2Int(8, 10),      // 伤害范围：8~10
            6,                          // 命中加值：+6
            5,                          // 反应要求：5
            3,                          // 额外加值：+3
            "",                         // 无特殊效果
            2,                          // 武器尺寸：2
            1,                          // 最小攻击距离：1格
            2                           // 最大攻击距离：2格（中距离）
        );
    }

    // ===== 战斗计算 =====

    /// <summary>
    /// 命中计算：d20 + (反应-3) + 命中加值
    /// 注：设计文档中锐器有额外+2命中，已包含在各武器的HitBonus中
    /// </summary>
    public override int CalculateHit(int agility)
    {
        int roll = Random.Range(1, 21);
        int modifier = agility - 3;
        int total = roll + modifier + HitBonus;
        
        Debug.Log($"[锐器命中] d20({roll}) + 反应加值({modifier}) + 武器加值({HitBonus}) = {total}");
        return total;
    }

    /// <summary>
    /// 伤害计算：武器伤害 + (反应-3) + 额外加值
    /// </summary>
    public override int CalculateDamage(int agility)
    {
        int baseDamage = Random.Range(DamageRange.x, DamageRange.y + 1);
        int modifier = agility - 3;
        int total = baseDamage + modifier + AdditionalBonus;
        
        Debug.Log($"[锐器伤害] 基础({baseDamage}) + 反应加值({modifier}) + 额外({AdditionalBonus}) = {total}");
        return total;
    }
}
