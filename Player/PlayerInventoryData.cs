using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 远程武器弹药数据
/// </summary>
[System.Serializable]
public class AmmoData
{
    public int currentAmmo;      // 当前弹匣
    public int reserveAmmo;      // 备用弹药

    public AmmoData(int current, int reserve)
    {
        currentAmmo = current;
        reserveAmmo = reserve;
    }
}

/// <summary>
/// 玩家背包静态数据 - 跨场景存储武器和弹药
/// </summary>
public static class PlayerInventoryData
{
    // ===== 武器列表 =====
    // 存储武器选择枚举，而不是武器实例（实例在场景切换时会丢失）
    private static List<WeaponChoice> _ownedWeapons = new List<WeaponChoice>();

    // 公开只读访问
    public static List<WeaponChoice> OwnedWeapons => _ownedWeapons;

    // ===== 弹药数据 =====
    // Key: 武器名称, Value: 弹药数据
    private static Dictionary<string, AmmoData> ammoStorage = new Dictionary<string, AmmoData>();

    // ===== 当前装备的武器索引 =====
    public static int CurrentWeaponIndex = 0;

    // ===== 是否已初始化 =====
    public static bool IsInitialized = false;

    // ========== 武器管理 ==========

    /// <summary>
    /// 添加武器到背包
    /// </summary>
    public static void AddWeapon(WeaponChoice weapon)
    {
        if (weapon == WeaponChoice.None) return;

        // 检查是否已拥有
        if (!_ownedWeapons.Contains(weapon))
        {
            _ownedWeapons.Add(weapon);
            Debug.Log($"[PlayerInventoryData] 添加武器: {weapon}");

            // 如果是远程武器，初始化弹药
            InitializeAmmoForWeapon(weapon);
        }
        else
        {
            Debug.Log($"[PlayerInventoryData] 已拥有武器: {weapon}");
        }
    }

    /// <summary>
    /// 移除武器
    /// </summary>
    public static void RemoveWeapon(WeaponChoice weapon)
    {
        if (_ownedWeapons.Contains(weapon))
        {
            _ownedWeapons.Remove(weapon);

            // 移除弹药数据
            string weaponName = weapon.ToString();
            if (ammoStorage.ContainsKey(weaponName))
            {
                ammoStorage.Remove(weaponName);
            }

            Debug.Log($"[PlayerInventoryData] 移除武器: {weapon}");
        }
    }

    /// <summary>
    /// 检查是否拥有某武器
    /// </summary>
    public static bool HasWeapon(WeaponChoice weapon)
    {
        return _ownedWeapons.Contains(weapon);
    }

    /// <summary>
    /// 获取所有武器
    /// </summary>
    public static List<WeaponChoice> GetAllWeapons()
    {
        return new List<WeaponChoice>(_ownedWeapons);
    }

    /// <summary>
    /// 获取武器数量
    /// </summary>
    public static int GetWeaponCount()
    {
        return _ownedWeapons.Count;
    }

    // ========== 弹药管理 ==========

    /// <summary>
    /// 初始化武器的弹药数据（根据武器类型给予充足弹药）
    /// </summary>
    private static void InitializeAmmoForWeapon(WeaponChoice weapon)
    {
        // 检查是否是远程武器
        if (!IsRangedWeapon(weapon)) return;

        string weaponName = weapon.ToString();
        if (ammoStorage.ContainsKey(weaponName)) return;  // 已有数据

        // 根据武器类型设置初始弹药
        int maxAmmo = 0;
        int reserveAmmo = 0;

        switch (weapon)
        {
            case WeaponChoice.手枪:
                maxAmmo = 12;
                reserveAmmo = 60;      // 总共72发
                break;

            case WeaponChoice.弩:
                maxAmmo = 1;
                reserveAmmo = 15;      // 总共16发
                break;

            case WeaponChoice.突击步枪:
                maxAmmo = 30;
                reserveAmmo = 150;     // 总共180发
                break;

            case WeaponChoice.冲锋枪:
                maxAmmo = 25;
                reserveAmmo = 125;     // 总共150发
                break;

            case WeaponChoice.狙击枪:
                maxAmmo = 5;
                reserveAmmo = 25;      // 总共30发
                break;

            case WeaponChoice.霰弹枪:
                maxAmmo = 8;
                reserveAmmo = 40;      // 总共48发
                break;

            case WeaponChoice.火箭筒:
                maxAmmo = 1;
                reserveAmmo = 2;       // 总共只有3发！
                break;

            default:
                return;
        }

        ammoStorage[weaponName] = new AmmoData(maxAmmo, reserveAmmo);
        Debug.Log($"[PlayerInventoryData] 初始化弹药 {weaponName}: {maxAmmo} + {reserveAmmo} = {maxAmmo + reserveAmmo} 发");
    }

    /// <summary>
    /// 获取武器的弹药数据
    /// </summary>
    public static AmmoData GetAmmoData(WeaponChoice weapon)
    {
        string weaponName = weapon.ToString();
        if (ammoStorage.ContainsKey(weaponName))
        {
            return ammoStorage[weaponName];
        }
        return null;
    }

    /// <summary>
    /// 获取武器的弹药数据（通过武器名）
    /// </summary>
    public static AmmoData GetAmmoData(string weaponName)
    {
        if (ammoStorage.ContainsKey(weaponName))
        {
            return ammoStorage[weaponName];
        }
        return null;
    }

    /// <summary>
    /// 更新武器的弹药数据
    /// </summary>
    public static void UpdateAmmo(WeaponChoice weapon, int currentAmmo, int reserveAmmo)
    {
        string weaponName = weapon.ToString();
        if (ammoStorage.ContainsKey(weaponName))
        {
            ammoStorage[weaponName].currentAmmo = currentAmmo;
            ammoStorage[weaponName].reserveAmmo = reserveAmmo;
        }
        else
        {
            ammoStorage[weaponName] = new AmmoData(currentAmmo, reserveAmmo);
        }
    }

    /// <summary>
    /// 更新武器的弹药数据（通过武器名）
    /// </summary>
    public static void UpdateAmmo(string weaponName, int currentAmmo, int reserveAmmo)
    {
        if (ammoStorage.ContainsKey(weaponName))
        {
            ammoStorage[weaponName].currentAmmo = currentAmmo;
            ammoStorage[weaponName].reserveAmmo = reserveAmmo;
        }
        else
        {
            ammoStorage[weaponName] = new AmmoData(currentAmmo, reserveAmmo);
        }
    }

    /// <summary>
    /// 消耗弹药
    /// </summary>
    public static bool ConsumeAmmo(string weaponName, int amount = 1)
    {
        if (!ammoStorage.ContainsKey(weaponName)) return false;

        AmmoData data = ammoStorage[weaponName];
        if (data.currentAmmo >= amount)
        {
            data.currentAmmo -= amount;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 换弹
    /// </summary>
    public static bool ReloadWeapon(string weaponName, int maxAmmo)
    {
        if (!ammoStorage.ContainsKey(weaponName)) return false;

        AmmoData data = ammoStorage[weaponName];

        if (data.currentAmmo >= maxAmmo) return false;  // 已满
        if (data.reserveAmmo <= 0) return false;        // 没有备用弹药

        int ammoNeeded = maxAmmo - data.currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, data.reserveAmmo);

        data.currentAmmo += ammoToLoad;
        data.reserveAmmo -= ammoToLoad;

        Debug.Log($"[PlayerInventoryData] {weaponName} 换弹: {data.currentAmmo}/{maxAmmo}, 备用: {data.reserveAmmo}");
        return true;
    }

    /// <summary>
    /// 添加备用弹药
    /// </summary>
    public static void AddReserveAmmo(string weaponName, int amount, int maxReserve)
    {
        if (!ammoStorage.ContainsKey(weaponName))
        {
            ammoStorage[weaponName] = new AmmoData(0, 0);
        }

        AmmoData data = ammoStorage[weaponName];
        data.reserveAmmo = Mathf.Min(data.reserveAmmo + amount, maxReserve);
        Debug.Log($"[PlayerInventoryData] {weaponName} 获得弹药: +{amount}, 备用: {data.reserveAmmo}");
    }

    // ========== 辅助方法 ==========

    /// <summary>
    /// 判断是否是远程武器
    /// </summary>
    public static bool IsRangedWeapon(WeaponChoice weapon)
    {
        switch (weapon)
        {
            case WeaponChoice.手枪:
            case WeaponChoice.弩:
            case WeaponChoice.突击步枪:
            case WeaponChoice.冲锋枪:
            case WeaponChoice.狙击枪:
            case WeaponChoice.霰弹枪:
            case WeaponChoice.火箭筒:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 判断是否是远程武器（通过武器名称）
    /// </summary>
    public static bool IsRangedWeapon(string weaponName)
    {
        switch (weaponName)
        {
            case "手枪":
            case "弩":
            case "突击步枪":
            case "冲锋枪":
            case "狙击枪":
            case "霰弹枪":
            case "火箭筒":
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 通过武器名称获取WeaponChoice
    /// </summary>
    public static WeaponChoice GetWeaponChoice(string weaponName)
    {
        switch (weaponName)
        {
            case "棒球棍": return WeaponChoice.棒球棍;
            case "钢管": return WeaponChoice.钢管;
            case "大锤": return WeaponChoice.大锤;
            case "匕首": return WeaponChoice.匕首;
            case "胁差": return WeaponChoice.胁差;
            case "迅捷剑": return WeaponChoice.迅捷剑;
            case "手枪": return WeaponChoice.手枪;
            case "弩": return WeaponChoice.弩;
            case "突击步枪": return WeaponChoice.突击步枪;
            case "冲锋枪": return WeaponChoice.冲锋枪;
            case "狙击枪": return WeaponChoice.狙击枪;
            case "霰弹枪": return WeaponChoice.霰弹枪;
            case "火箭筒": return WeaponChoice.火箭筒;
            default: return WeaponChoice.None;
        }
    }

    /// <summary>
    /// 获取武器最大弹匣容量
    /// </summary>
    public static int GetMaxMagazine(WeaponChoice weapon)
    {
        switch (weapon)
        {
            case WeaponChoice.手枪: return 12;
            case WeaponChoice.弩: return 1;
            case WeaponChoice.突击步枪: return 30;
            case WeaponChoice.冲锋枪: return 25;
            case WeaponChoice.狙击枪: return 5;
            case WeaponChoice.霰弹枪: return 8;
            case WeaponChoice.火箭筒: return 1;
            default: return 1;
        }
    }

    /// <summary>
    /// 获取武器最大弹匣容量（通过武器名称）
    /// </summary>
    public static int GetMaxMagazine(string weaponName)
    {
        return GetMaxMagazine(GetWeaponChoice(weaponName));
    }

    /// <summary>
    /// 判断是否是近战武器
    /// </summary>
    public static bool IsMeleeWeapon(WeaponChoice weapon)
    {
        switch (weapon)
        {
            case WeaponChoice.棒球棍:
            case WeaponChoice.钢管:
            case WeaponChoice.大锤:
            case WeaponChoice.匕首:
            case WeaponChoice.胁差:
            case WeaponChoice.迅捷剑:
                return true;
            default:
                return false;
        }
    }

    // ========== 数据管理 ==========

    /// <summary>
    /// 重置所有数据
    /// </summary>
    public static void Reset()
    {
        _ownedWeapons.Clear();
        ammoStorage.Clear();
        CurrentWeaponIndex = 0;
        IsInitialized = false;
        Debug.Log("[PlayerInventoryData] 数据已重置");
    }

    /// <summary>
    /// 获取所有弹药数据（用于存档）
    /// </summary>
    public static Dictionary<string, AmmoData> GetAllAmmoData()
    {
        return new Dictionary<string, AmmoData>(ammoStorage);
    }

    /// <summary>
    /// 加载弹药数据（用于读档）
    /// </summary>
    public static void LoadAmmoData(Dictionary<string, AmmoData> data)
    {
        ammoStorage = new Dictionary<string, AmmoData>(data);
    }

    /// <summary>
    /// 加载武器列表（用于读档）
    /// </summary>
    public static void LoadWeapons(List<WeaponChoice> weapons)
    {
        _ownedWeapons = new List<WeaponChoice>(weapons);
    }

    /// <summary>
    /// 获取调试信息
    /// </summary>
    public static string GetDebugInfo()
    {
        string info = "=== PlayerInventoryData ===\n";
        info += $"武器数量: {_ownedWeapons.Count}\n";

        foreach (var weapon in _ownedWeapons)
        {
            info += $"- {weapon}";
            if (IsRangedWeapon(weapon))
            {
                var ammo = GetAmmoData(weapon);
                if (ammo != null)
                {
                    info += $" (弹药: {ammo.currentAmmo}, 备用: {ammo.reserveAmmo})";
                }
            }
            info += "\n";
        }

        return info;
    }
}
