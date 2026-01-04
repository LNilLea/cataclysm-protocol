using UnityEngine;

/// <summary>
/// 钝器武器类 - 使用体魄进行命中和伤害计算
/// 攻击范围：2格（中距离）
/// </summary>
public class BluntWeapon : Weapon
{
    public enum BluntWeaponType
    {
        BaseballBat,    // 棒球棍
        SteelPipe,      // 钢管
        GreatHammer     // 大锤
    }

    /// <summary>
    /// 钝器构造函数
    /// </summary>
    /// <param name="name">武器名称</param>
    /// <param name="damageRange">伤害范围</param>
    /// <param name="hitBonus">命中加值</param>
    /// <param name="requiredStrength">体魄要求</param>
    /// <param name="additionalBonus">额外伤害加值</param>
    /// <param name="effect">特殊效果</param>
    /// <param name="weaponSize">武器尺寸</param>
    /// <param name="attackRangeMin">最小攻击距离（格）</param>
    /// <param name="attackRangeMax">最大攻击距离（格）</param>
    public BluntWeapon(string name, Vector2Int damageRange, int hitBonus, int requiredStrength, 
                       int additionalBonus, string effect, int weaponSize,
                       int attackRangeMin = 1, int attackRangeMax = 2)
        : base(name, WeaponType.BaseballBat, damageRange, hitBonus, requiredStrength, 
               additionalBonus, effect, weaponSize, 0f, attackRangeMin, attackRangeMax)
    {
        this.HitBonus = hitBonus;
        this.AdditionalBonus = additionalBonus;
    }

    // ===== 工厂方法 =====

    /// <summary>
    /// 创建棒球棍
    /// 伤害：4-6，命中：+0，体魄要求：3，攻击范围：1-2格
    /// </summary>
    public static BluntWeapon CreateBaseballBat()
    {
        return new BluntWeapon(
            "棒球棍",
            new Vector2Int(4, 6),      // 伤害范围：4~6
            0,                          // 命中加值：0
            3,                          // 体魄要求：3
            0,                          // 额外加值：+0
            "",                         // 无特殊效果
            2,                          // 武器尺寸：2
            1,                          // 最小攻击距离：1格
            2                           // 最大攻击距离：2格
        );
    }

    /// <summary>
    /// 创建钢管
    /// 伤害：6-8，命中：+0，体魄要求：4，额外伤害：+3，攻击范围：1-2格
    /// </summary>
    public static BluntWeapon CreateSteelPipe()
    {
        return new BluntWeapon(
            "钢管",
            new Vector2Int(6, 8),       // 伤害范围：6~8
            0,                          // 命中加值：0
            4,                          // 体魄要求：4
            3,                          // 额外加值：+3
            "",                         // 无特殊效果
            3,                          // 武器尺寸：3
            1,                          // 最小攻击距离：1格
            2                           // 最大攻击距离：2格
        );
    }

    /// <summary>
    /// 创建大锤
    /// 伤害：15-18，命中：-2，体魄要求：6，额外伤害：+6，效果：破甲，攻击范围：1-2格
    /// </summary>
    public static BluntWeapon CreateGreatHammer()
    {
        return new BluntWeapon(
            "大锤",
            new Vector2Int(15, 18),     // 伤害范围：15~18
            -2,                         // 命中加值：-2
            6,                          // 体魄要求：6
            6,                          // 额外加值：+6
            "破甲",                     // 特殊效果：破甲
            4,                          // 武器尺寸：4
            1,                          // 最小攻击距离：1格
            2                           // 最大攻击距离：2格
        );
    }

    // ===== 战斗计算 =====

    /// <summary>
    /// 命中计算：d20 + (体魄-3) + 命中加值
    /// </summary>
    public override int CalculateHit(int strength)
    {
        int roll = Random.Range(1, 21);
        int modifier = strength - 3;
        int total = roll + modifier + HitBonus;
        
        Debug.Log($"[钝器命中] d20({roll}) + 体魄加值({modifier}) + 武器加值({HitBonus}) = {total}");
        return total;
    }

    /// <summary>
    /// 伤害计算：武器伤害 + (体魄-3) + 额外加值
    /// </summary>
    public override int CalculateDamage(int strength)
    {
        int baseDamage = Random.Range(DamageRange.x, DamageRange.y + 1);
        int modifier = strength - 3;
        int total = baseDamage + modifier + AdditionalBonus;
        
        Debug.Log($"[钝器伤害] 基础({baseDamage}) + 体魄加值({modifier}) + 额外({AdditionalBonus}) = {total}");
        return total;
    }
}
