using UnityEngine;

/// <summary>
/// 武器选择枚举 - 用于 Inspector 下拉菜单
/// </summary>
public enum WeaponChoice
{
    None,           // 无武器
    
    // === 钝器 ===
    棒球棍,
    钢管,
    大锤,
    
    // === 锐器 ===
    匕首,
    胁差,
    迅捷剑,
    
    // === 远程武器 ===
    手枪,
    弩,
    突击步枪,
    冲锋枪,
    狙击枪,
    霰弹枪,
    火箭筒
}

/// <summary>
/// 武器工厂 - 通过枚举获取武器实例
/// </summary>
public static class WeaponFactory
{
    /// <summary>
    /// 通过枚举获取武器
    /// </summary>
    public static Weapon GetWeapon(WeaponChoice choice)
    {
        switch (choice)
        {
            // === 钝器 ===
            case WeaponChoice.棒球棍:
                return BluntWeapon.CreateBaseballBat();
            case WeaponChoice.钢管:
                return BluntWeapon.CreateSteelPipe();
            case WeaponChoice.大锤:
                return BluntWeapon.CreateGreatHammer();
            
            // === 锐器 ===
            case WeaponChoice.匕首:
                return SharpWeapon.CreateDagger();
            case WeaponChoice.胁差:
                return SharpWeapon.CreateStiletto();
            case WeaponChoice.迅捷剑:
                return SharpWeapon.CreateSwiftSword();
            
            // === 远程武器 ===
            case WeaponChoice.手枪:
                return RangedWeapon.CreatePistol();
            case WeaponChoice.弩:
                return RangedWeapon.CreateCrossbow();
            case WeaponChoice.突击步枪:
                return RangedWeapon.CreateAssaultRifle();
            case WeaponChoice.冲锋枪:
                return RangedWeapon.CreateSMG();
            case WeaponChoice.狙击枪:
                return RangedWeapon.CreateSniperRifle();
            case WeaponChoice.霰弹枪:
                return RangedWeapon.CreateShotgun();
            case WeaponChoice.火箭筒:
                return RangedWeapon.CreateRocketLauncher();
            
            // === 无武器 ===
            case WeaponChoice.None:
            default:
                return null;
        }
    }

    /// <summary>
    /// 通过武器名字获取武器（备用方法）
    /// </summary>
    public static Weapon GetWeaponByName(string weaponName)
    {
        switch (weaponName)
        {
            // 钝器
            case "棒球棍": return BluntWeapon.CreateBaseballBat();
            case "钢管": return BluntWeapon.CreateSteelPipe();
            case "大锤": return BluntWeapon.CreateGreatHammer();
            
            // 锐器
            case "匕首": return SharpWeapon.CreateDagger();
            case "胁差": return SharpWeapon.CreateStiletto();
            case "迅捷剑": return SharpWeapon.CreateSwiftSword();
            
            // 远程武器
            case "手枪": return RangedWeapon.CreatePistol();
            case "弩": return RangedWeapon.CreateCrossbow();
            case "突击步枪": return RangedWeapon.CreateAssaultRifle();
            case "冲锋枪": return RangedWeapon.CreateSMG();
            case "狙击枪": return RangedWeapon.CreateSniperRifle();
            case "霰弹枪": return RangedWeapon.CreateShotgun();
            case "火箭筒": return RangedWeapon.CreateRocketLauncher();
            
            default:
                Debug.LogWarning("WeaponFactory: 找不到武器 - " + weaponName);
                return null;
        }
    }

    /// <summary>
    /// 获取武器信息（用于显示）
    /// </summary>
    public static string GetWeaponInfo(WeaponChoice choice)
    {
        Weapon weapon = GetWeapon(choice);
        if (weapon != null)
        {
            return weapon.GetWeaponInfo();
        }
        return "无武器";
    }
}
