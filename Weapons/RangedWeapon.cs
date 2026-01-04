using UnityEngine;
using MyGame;

/// <summary>
/// 远程武器（枪械）- 支持连射减值版本
/// </summary>
public class RangedWeapon : Weapon
{
    public enum WeaponType
    {
        Crossbow,       // 弩
        Pistol,         // 手枪
        AssaultRifle,   // 突击步枪
        SniperRifle,    // 狙击枪
        Shotgun,        // 霰弹枪
        RocketLauncher, // 火箭筒
        SMG             // 冲锋枪
    }

    [Header("连射设置")]
    public int MaxBurst;            // 最大连射次数
    public int BurstCount = 0;      // 当前连射次数
    public int BurstPenalty;        // 连射减值（第2发开始每发减去此值）

    [Header("连射减值修正")]
    public int BurstPenaltyModifier = 0;  // 连射减值修正（可通过专长/装备修改）

    [Header("弹药设置")]
    public int MaxAmmo;             // 弹匣容量
    public int CurrentAmmo;         // 当前子弹数
    public int ReserveAmmo;         // 备用弹药
    public int MaxReserveAmmo;      // 最大备用弹药

    [Header("换弹设置")]
    public float ReloadTime = 1.5f;

    public bool NeedsReload => CurrentAmmo <= 0;

    /// <summary>
    /// 获取实际连射减值（基础减值 + 修正）
    /// </summary>
    public int GetEffectiveBurstPenalty()
    {
        int effective = BurstPenalty + BurstPenaltyModifier;
        return Mathf.Max(0, effective);  // 最小为0
    }

    /// <summary>
    /// 修改连射减值（通过专长/装备等）
    /// </summary>
    public void ModifyBurstPenalty(int modifier)
    {
        BurstPenaltyModifier += modifier;
    }

    /// <summary>
    /// 重置连射减值修正
    /// </summary>
    public void ResetBurstPenaltyModifier()
    {
        BurstPenaltyModifier = 0;
    }

    // 构造函数（旧版兼容）
    public RangedWeapon(string name, WeaponType type, Vector2Int damageRange, int hitBonus,
        int requiredStrength, int additionalBonus, string effect, int weaponSize,
        float range, int attackRangeMin, int attackRangeMax, int maxBurst, int maxAmmo)
        : base(name, (global::WeaponType)(int)type, damageRange, hitBonus, requiredStrength,
               additionalBonus, effect, weaponSize, range, attackRangeMin, attackRangeMax)
    {
        MaxBurst = maxBurst;
        MaxAmmo = maxAmmo;
        CurrentAmmo = maxAmmo;
        MaxReserveAmmo = maxAmmo * 3;
        ReserveAmmo = MaxReserveAmmo;
        BurstPenalty = 2;  // 默认连射减值
    }

    // 构造函数（新版，支持连射减值）
    public RangedWeapon(string name, WeaponType type, Vector2Int damageRange, int hitBonus,
        int requiredStrength, int additionalBonus, string effect, int weaponSize,
        float range, int attackRangeMin, int attackRangeMax, int maxBurst, int maxAmmo, int burstPenalty)
        : base(name, (global::WeaponType)(int)type, damageRange, hitBonus, requiredStrength,
               additionalBonus, effect, weaponSize, range, attackRangeMin, attackRangeMax)
    {
        MaxBurst = maxBurst;
        MaxAmmo = maxAmmo;
        CurrentAmmo = maxAmmo;
        MaxReserveAmmo = maxAmmo * 3;
        ReserveAmmo = MaxReserveAmmo;
        BurstPenalty = burstPenalty;
    }

    public bool CanFire() => CurrentAmmo > 0;

    public bool CanBurst(int burstCount) => CurrentAmmo >= burstCount && burstCount <= MaxBurst;

    public int CalculateStrengthPenalty(int playerStrength)
    {
        int deficit = RequiredStrength - playerStrength;
        return deficit > 0 ? -2 * deficit : 0;
    }

    public int CalculateSelfDamage(int playerStrength)
    {
        int deficit = RequiredStrength - playerStrength;
        return deficit > 0 ? deficit * WeaponSize : 0;
    }

    public int CalculateHitWithStrength(int agility, int strength)
    {
        int baseHit = base.CalculateHit(agility);
        return baseHit + CalculateStrengthPenalty(strength);
    }

    public RangedAttackResult Fire(ICombatTarget target, int agility, int strength)
    {
        RangedAttackResult result = new RangedAttackResult
        {
            weaponName = Name,
            targetName = target.Name
        };

        if (!CanFire())
        {
            result.success = false;
            result.log = $"{Name} 需要换弹！";
            return result;
        }

        CurrentAmmo--;
        result.ammoUsed = 1;

        int hitRoll = CalculateHitWithStrength(agility, strength);
        result.hitRoll = hitRoll;
        result.targetAC = target.CurrentAC;

        result.log = $"{Name} 对 {target.Name} 开火！命中: {hitRoll} vs AC {target.CurrentAC} → ";

        if (hitRoll >= target.CurrentAC)
        {
            int damage = CalculateDamage(agility);
            target.TakeDamage(damage);
            result.success = true;
            result.damageDealt = damage;
            result.log += $"命中！造成 {damage} 点伤害";
        }
        else
        {
            result.success = false;
            result.damageDealt = 0;
            result.log += "未命中";
        }

        int selfDamage = CalculateSelfDamage(strength);
        result.selfDamage = selfDamage;
        if (selfDamage > 0)
        {
            result.log += $"\n⚠ 后坐力对自己造成 {selfDamage} 点伤害";
        }

        result.log += $"\n剩余子弹: {CurrentAmmo}/{MaxAmmo}";
        return result;
    }

    public RangedAttackResult Burst(ICombatTarget target, int agility, int strength, int burstCount)
    {
        RangedAttackResult result = new RangedAttackResult
        {
            weaponName = Name,
            targetName = target.Name
        };

        int actualBurst = Mathf.Min(burstCount, MaxBurst, CurrentAmmo);

        if (actualBurst <= 0)
        {
            result.success = false;
            result.log = $"{Name} 没有足够的子弹进行连射！";
            return result;
        }

        result.log = $"{Name} 对 {target.Name} 进行 {actualBurst} 连射！\n";

        int totalDamage = 0;
        int hits = 0;
        int totalSelfDamage = 0;
        int effectivePenalty = GetEffectiveBurstPenalty();

        for (int i = 0; i < actualBurst; i++)
        {
            CurrentAmmo--;
            result.ammoUsed++;

            int hitRoll = CalculateHitWithStrength(agility, strength);
            
            // 应用连射减值（第2发开始）
            int burstPenaltyThisShot = i * effectivePenalty;
            hitRoll -= burstPenaltyThisShot;
            
            int targetAC = target.CurrentAC;

            if (burstPenaltyThisShot > 0)
            {
                result.log += $"第 {i + 1} 发: 命中 {hitRoll} (连射-{burstPenaltyThisShot}) vs AC {targetAC} → ";
            }
            else
            {
                result.log += $"第 {i + 1} 发: 命中 {hitRoll} vs AC {targetAC} → ";
            }

            if (hitRoll >= targetAC)
            {
                int damage = CalculateDamage(agility);
                target.TakeDamage(damage);
                totalDamage += damage;
                hits++;
                result.log += $"命中！{damage} 伤害\n";
            }
            else
            {
                result.log += "未命中\n";
            }

            int selfDamage = CalculateSelfDamage(strength);
            totalSelfDamage += selfDamage;
        }

        result.success = hits > 0;
        result.damageDealt = totalDamage;
        result.selfDamage = totalSelfDamage;

        result.log += $"连射结果: {hits}/{actualBurst} 命中，共造成 {totalDamage} 点伤害";

        if (totalSelfDamage > 0)
        {
            result.log += $"\n⚠ 后坐力累计对自己造成 {totalSelfDamage} 点伤害";
        }

        result.log += $"\n剩余子弹: {CurrentAmmo}/{MaxAmmo}";
        return result;
    }

    public bool Reload()
    {
        if (CurrentAmmo >= MaxAmmo)
        {
            Debug.Log($"{Name} 弹匣已满，无需换弹");
            return false;
        }

        if (ReserveAmmo <= 0)
        {
            Debug.Log($"{Name} 没有备用弹药！");
            return false;
        }

        int ammoNeeded = MaxAmmo - CurrentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, ReserveAmmo);

        CurrentAmmo += ammoToLoad;
        ReserveAmmo -= ammoToLoad;

        Debug.Log($"{Name} 换弹完成！{CurrentAmmo}/{MaxAmmo}，备用: {ReserveAmmo}");
        return true;
    }

    public void AddAmmo(int amount)
    {
        ReserveAmmo = Mathf.Min(ReserveAmmo + amount, MaxReserveAmmo);
        Debug.Log($"{Name} 获得 {amount} 发弹药，备用: {ReserveAmmo}/{MaxReserveAmmo}");
    }

    public string GetAmmoStatus() => $"{CurrentAmmo}/{MaxAmmo} (备用: {ReserveAmmo})";

    public new bool IsInRange(int targetDistance)
    {
        return targetDistance >= AttackRangeMin && targetDistance <= AttackRangeMax;
    }

    /// <summary>
    /// 单发射击（兼容 RangedCombatController）
    /// </summary>
    public RangedAttackResult FireSingle(ICombatTarget target, int agility, int strength)
    {
        return Fire(target, agility, strength);
    }

    /// <summary>
    /// 连射（兼容 RangedCombatController，参数顺序不同）
    /// </summary>
    public RangedAttackResult FireBurst(ICombatTarget target, int burstCount, int agility, int strength)
    {
        return Burst(target, agility, strength, burstCount);
    }

    // ===== 静态工厂方法 =====

    /// <summary>
    /// 创建手枪 - 范围1-4格，连射减值-2
    /// </summary>
    public static RangedWeapon CreatePistol()
    {
        return new RangedWeapon(
            "手枪",
            WeaponType.Pistol,
            new Vector2Int(5, 5),    // 伤害
            2,                       // 命中加值
            3,                       // 需求体魄
            0,                       // 额外伤害
            "每次开火对自己造成伤害",
            1,                       // 武器尺寸
            10,                      // 射程
            1,                       // 最小距离
            4,                       // 最大距离
            2,                       // 最大连射
            12,                      // 弹匣容量
            2                        // 连射减值
        );
    }

    /// <summary>
    /// 创建弩 - 范围1-8格，不支持连射
    /// </summary>
    public static RangedWeapon CreateCrossbow()
    {
        return new RangedWeapon(
            "弩",
            WeaponType.Crossbow,
            new Vector2Int(9, 9),
            2,
            4,
            0,
            "适中伤害，射程适中",
            3,
            15,
            1,
            8,
            1,                       // 最大连射1（不支持连射）
            1,                       // 弹匣容量
            0                        // 连射减值（不适用）
        );
    }

    /// <summary>
    /// 创建突击步枪 - 范围1-8格，连射减值-2
    /// </summary>
    public static RangedWeapon CreateAssaultRifle()
    {
        return new RangedWeapon(
            "突击步枪",
            WeaponType.AssaultRifle,
            new Vector2Int(10, 10),
            3,                       // 命中加值+3
            5,
            0,
            "适用于中距离战斗",
            2,
            30,
            1,
            8,
            3,                       // 最大连射3
            30,
            2                        // 连射减值-2
        );
    }

    /// <summary>
    /// 创建冲锋枪 - 范围1-8格，连射减值-1
    /// </summary>
    public static RangedWeapon CreateSMG()
    {
        return new RangedWeapon(
            "冲锋枪",
            WeaponType.SMG,
            new Vector2Int(6, 6),
            1,                       // 命中加值+1（比之前的-1提高了）
            4,
            1,
            "连射，命中难度增加",
            2,
            25,
            1,
            8,
            5,                       // 最大连射5
            25,
            1                        // 连射减值-1（比其他枪更稳定）
        );
    }

    /// <summary>
    /// 创建狙击枪 - 范围1-100格，不支持连射
    /// </summary>
    public static RangedWeapon CreateSniperRifle()
    {
        return new RangedWeapon(
            "狙击枪",
            WeaponType.SniperRifle,
            new Vector2Int(18, 18),
            5,
            6,
            0,
            "高精度，长距离攻击",
            4,
            100,
            1,
            100,
            1,                       // 最大连射1
            5,
            0                        // 连射减值（不适用）
        );
    }

    /// <summary>
    /// 创建霰弹枪 - 范围1-4格，不支持连射
    /// </summary>
    public static RangedWeapon CreateShotgun()
    {
        return new RangedWeapon(
            "霰弹枪",
            WeaponType.Shotgun,
            new Vector2Int(12, 12),
            -2,
            5,
            4,
            "近距离大范围伤害",
            2,
            15,
            1,
            4,
            1,                       // 最大连射1
            8,
            0                        // 连射减值（不适用）
        );
    }

    /// <summary>
    /// 创建火箭筒 - 范围1-100格，不支持连射
    /// </summary>
    public static RangedWeapon CreateRocketLauncher()
    {
        return new RangedWeapon(
            "火箭筒",
            WeaponType.RocketLauncher,
            new Vector2Int(35, 35),
            -5,
            6,
            10,
            "爆炸伤害，适合对抗重型目标",
            5,
            50,
            1,
            100,
            1,                       // 最大连射1
            1,
            0                        // 连射减值（不适用）
        );
    }
}

/// <summary>
/// 远程攻击结果
/// </summary>
public class RangedAttackResult
{
    public string weaponName;
    public string targetName;
    public bool success;
    public int hitRoll;
    public int targetAC;
    public int damageDealt;
    public int selfDamage;
    public int ammoUsed;
    public string log;
}
