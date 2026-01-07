using UnityEngine;
using MyGame;

/// <summary>
/// 远程武器（枪械）- 固定伤害版本
/// 
/// 【重要】枪械伤害计算规则（根据设计文档）：
/// - 伤害 = 武器基础伤害（固定值，不受属性加值、架势加成影响！）
/// - 命中 = d20 + (反应-3) + 命中加值 + 其他加值
/// - 使用要求：体魄达到要求
/// - 副作用：体魄不足时，每1点差值 -2命中，每次开火对自己造成 差值×武器尺寸 的伤害
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
    public int BurstPenaltyModifier = 0;

    [Header("弹药设置")]
    public int MaxAmmo;             // 弹匣容量
    public int CurrentAmmo;         // 当前子弹数
    public int ReserveAmmo;         // 备用弹药
    public int MaxReserveAmmo;      // 最大备用弹药

    [Header("换弹设置")]
    public float ReloadTime = 1.5f;

    public bool NeedsReload => CurrentAmmo <= 0;

    /// <summary>
    /// 获取实际连射减值
    /// </summary>
    public int GetEffectiveBurstPenalty()
    {
        int effective = BurstPenalty + BurstPenaltyModifier;
        return Mathf.Max(0, effective);
    }

    public void ModifyBurstPenalty(int modifier)
    {
        BurstPenaltyModifier += modifier;
    }

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

    /// <summary>
    /// 计算体魄不足的命中惩罚
    /// 每1点差值 = -2命中
    /// </summary>
    public int CalculateStrengthPenalty(int playerStrength)
    {
        int deficit = RequiredStrength - playerStrength;
        return deficit > 0 ? -2 * deficit : 0;
    }

    /// <summary>
    /// 计算体魄不足的自伤
    /// 自伤 = 差值 × 武器尺寸
    /// </summary>
    public int CalculateSelfDamage(int playerStrength)
    {
        int deficit = RequiredStrength - playerStrength;
        return deficit > 0 ? deficit * WeaponSize : 0;
    }

    /// <summary>
    /// 计算命中（考虑体魄惩罚）
    /// 命中 = d20 + (反应-3) + 命中加值 + 体魄惩罚
    /// </summary>
    public int CalculateHitWithStrength(int agility, int strength)
    {
        int baseHit = base.CalculateHit(agility);
        return baseHit + CalculateStrengthPenalty(strength);
    }

    /// <summary>
    /// 【核心修改】重写伤害计算 - 枪械使用固定伤害！
    /// 不受属性加值影响，直接返回武器的基础伤害
    /// </summary>
    public override int CalculateDamage(int attributeValue)
    {
        // 枪械固定伤害 = DamageRange.x（因为枪械的min和max相同）
        // 【重要】不加属性加值！不加AdditionalBonus！
        return DamageRange.x;
    }

    /// <summary>
    /// 单发射击
    /// </summary>
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
            // 固定伤害
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

    /// <summary>
    /// 连射
    /// </summary>
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
                // 固定伤害
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

    public RangedAttackResult FireSingle(ICombatTarget target, int agility, int strength)
    {
        return Fire(target, agility, strength);
    }

    public RangedAttackResult FireBurst(ICombatTarget target, int burstCount, int agility, int strength)
    {
        return Burst(target, agility, strength, burstCount);
    }

    // ===== 静态工厂方法（保持原有数值不变）=====

    /// <summary>
    /// 创建手枪 - 固定伤害5
    /// </summary>
    public static RangedWeapon CreatePistol()
    {
        return new RangedWeapon(
            "手枪",
            WeaponType.Pistol,
            new Vector2Int(5, 5),    // 固定伤害5
            2,                       // 命中加值
            3,                       // 需求体魄
            0,                       // 额外伤害（不使用）
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
    /// 创建弩 - 固定伤害9
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
            1,
            1,
            0
        );
    }

    /// <summary>
    /// 创建突击步枪 - 固定伤害10
    /// </summary>
    public static RangedWeapon CreateAssaultRifle()
    {
        return new RangedWeapon(
            "突击步枪",
            WeaponType.AssaultRifle,
            new Vector2Int(10, 10),
            3,
            5,
            0,
            "适用于中距离战斗",
            2,
            30,
            1,
            8,
            3,
            30,
            2
        );
    }

    /// <summary>
    /// 创建冲锋枪 - 固定伤害6
    /// </summary>
    public static RangedWeapon CreateSMG()
    {
        return new RangedWeapon(
            "冲锋枪",
            WeaponType.SMG,
            new Vector2Int(6, 6),
            1,
            4,
            0,
            "连射，命中难度增加",
            2,
            25,
            1,
            8,
            5,
            25,
            1
        );
    }

    /// <summary>
    /// 创建狙击枪 - 固定伤害18
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
            1,
            5,
            0
        );
    }

    /// <summary>
    /// 创建霰弹枪 - 固定伤害12
    /// </summary>
    public static RangedWeapon CreateShotgun()
    {
        return new RangedWeapon(
            "霰弹枪",
            WeaponType.Shotgun,
            new Vector2Int(12, 12),
            -2,
            5,
            0,
            "近距离大范围伤害",
            2,
            15,
            1,
            4,
            1,
            8,
            0
        );
    }

    /// <summary>
    /// 创建火箭筒 - 固定伤害35
    /// </summary>
    public static RangedWeapon CreateRocketLauncher()
    {
        return new RangedWeapon(
            "火箭筒",
            WeaponType.RocketLauncher,
            new Vector2Int(35, 35),
            -5,
            6,
            0,
            "爆炸伤害，适合对抗重型目标",
            5,
            50,
            1,
            100,
            1,
            1,
            0
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
