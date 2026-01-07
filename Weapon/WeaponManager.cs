using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器管理器 - 管理玩家的武器库存（支持跨场景数据同步）
/// </summary>
public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [Header("武器库存")]
    private List<Weapon> inventory = new List<Weapon>();

    [Header("当前武器索引")]
    private int currentWeaponIndex = 0;

    // 事件
    public event System.Action<Weapon> OnWeaponAdded;
    public event System.Action<Weapon> OnWeaponRemoved;
    public event System.Action<Weapon> OnWeaponChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 从静态数据恢复武器库存
        RestoreFromStaticData();
    }

    /// <summary>
    /// 从 PlayerInventoryData 恢复武器库存
    /// </summary>
    public void RestoreFromStaticData()
    {
        inventory.Clear();

        List<WeaponChoice> savedWeapons = PlayerInventoryData.GetAllWeapons();
        foreach (var weaponChoice in savedWeapons)
        {
            Weapon weapon = WeaponFactory.GetWeapon(weaponChoice);
            if (weapon != null)
            {
                // 如果是远程武器，恢复弹药数据
                if (weapon is RangedWeapon ranged)
                {
                    AmmoData ammo = PlayerInventoryData.GetAmmoData(weaponChoice);
                    if (ammo != null)
                    {
                        ranged.CurrentAmmo = ammo.currentAmmo;
                        ranged.ReserveAmmo = ammo.reserveAmmo;
                    }
                }

                inventory.Add(weapon);
            }
        }

        // 恢复当前武器索引
        currentWeaponIndex = Mathf.Clamp(PlayerInventoryData.CurrentWeaponIndex, 0, Mathf.Max(0, inventory.Count - 1));

        Debug.Log($"[WeaponManager] 从静态数据恢复了 {inventory.Count} 把武器");
    }

    /// <summary>
    /// 保存当前状态到静态数据
    /// </summary>
    public void SaveToStaticData()
    {
        // 保存当前武器索引
        PlayerInventoryData.CurrentWeaponIndex = currentWeaponIndex;

        // 保存所有远程武器的弹药数据
        foreach (var weapon in inventory)
        {
            if (weapon is RangedWeapon ranged)
            {
                PlayerInventoryData.UpdateAmmo(weapon.Name, ranged.CurrentAmmo, ranged.ReserveAmmo);
            }
        }

        Debug.Log("[WeaponManager] 数据已保存到静态存储");
    }

    /// <summary>
    /// 添加武器到库存
    /// </summary>
    public void AddWeapon(Weapon weapon)
    {
        if (weapon == null) return;

        // 检查是否已拥有同名武器
        if (HasWeapon(weapon.Name))
        {
            Debug.Log($"已拥有武器：{weapon.Name}");
            return;
        }

        inventory.Add(weapon);
        Debug.Log($"获得新武器：{weapon.Name}");

        OnWeaponAdded?.Invoke(weapon);
    }

    /// <summary>
    /// 添加武器（通过枚举）
    /// </summary>
    public void AddWeapon(WeaponChoice weaponChoice)
    {
        if (weaponChoice == WeaponChoice.None) return;

        // 同步到静态数据
        PlayerInventoryData.AddWeapon(weaponChoice);

        // 创建武器实例
        Weapon weapon = WeaponFactory.GetWeapon(weaponChoice);
        if (weapon != null)
        {
            // 如果是远程武器，从静态数据读取弹药
            if (weapon is RangedWeapon ranged)
            {
                AmmoData ammo = PlayerInventoryData.GetAmmoData(weaponChoice);
                if (ammo != null)
                {
                    ranged.CurrentAmmo = ammo.currentAmmo;
                    ranged.ReserveAmmo = ammo.reserveAmmo;
                }
            }

            AddWeapon(weapon);
        }
    }

    /// <summary>
    /// 移除武器
    /// </summary>
    public void RemoveWeapon(Weapon weapon)
    {
        if (weapon == null || !inventory.Contains(weapon)) return;

        inventory.Remove(weapon);
        Debug.Log($"移除武器：{weapon.Name}");

        // 调整当前武器索引
        if (currentWeaponIndex >= inventory.Count)
        {
            currentWeaponIndex = Mathf.Max(0, inventory.Count - 1);
        }

        OnWeaponRemoved?.Invoke(weapon);
    }

    /// <summary>
    /// 获取当前武器
    /// </summary>
    public Weapon GetCurrentWeapon()
    {
        if (inventory.Count == 0) return null;
        return inventory[currentWeaponIndex];
    }

    /// <summary>
    /// 切换到下一个武器
    /// </summary>
    public Weapon SwitchToNextWeapon()
    {
        if (inventory.Count <= 1) return GetCurrentWeapon();

        // 切换前保存当前武器的弹药
        SaveCurrentWeaponAmmo();

        currentWeaponIndex = (currentWeaponIndex + 1) % inventory.Count;
        Weapon newWeapon = inventory[currentWeaponIndex];

        // 更新静态数据的索引
        PlayerInventoryData.CurrentWeaponIndex = currentWeaponIndex;

        Debug.Log($"切换武器：{newWeapon.Name}");
        OnWeaponChanged?.Invoke(newWeapon);

        return newWeapon;
    }

    /// <summary>
    /// 切换到上一个武器
    /// </summary>
    public Weapon SwitchToPreviousWeapon()
    {
        if (inventory.Count <= 1) return GetCurrentWeapon();

        // 切换前保存当前武器的弹药
        SaveCurrentWeaponAmmo();

        currentWeaponIndex = (currentWeaponIndex - 1 + inventory.Count) % inventory.Count;
        Weapon newWeapon = inventory[currentWeaponIndex];

        // 更新静态数据的索引
        PlayerInventoryData.CurrentWeaponIndex = currentWeaponIndex;

        Debug.Log($"切换武器：{newWeapon.Name}");
        OnWeaponChanged?.Invoke(newWeapon);

        return newWeapon;
    }

    /// <summary>
    /// 切换到指定索引的武器
    /// </summary>
    public Weapon SwitchToWeapon(int index)
    {
        if (index < 0 || index >= inventory.Count) return GetCurrentWeapon();

        // 切换前保存当前武器的弹药
        SaveCurrentWeaponAmmo();

        currentWeaponIndex = index;
        Weapon newWeapon = inventory[currentWeaponIndex];

        // 更新静态数据的索引
        PlayerInventoryData.CurrentWeaponIndex = currentWeaponIndex;

        Debug.Log($"切换武器：{newWeapon.Name}");
        OnWeaponChanged?.Invoke(newWeapon);

        return newWeapon;
    }

    /// <summary>
    /// 保存当前武器的弹药数据
    /// </summary>
    private void SaveCurrentWeaponAmmo()
    {
        Weapon current = GetCurrentWeapon();
        if (current is RangedWeapon ranged)
        {
            PlayerInventoryData.UpdateAmmo(current.Name, ranged.CurrentAmmo, ranged.ReserveAmmo);
        }
    }

    /// <summary>
    /// 获取所有武器
    /// </summary>
    public List<Weapon> GetAllWeapons()
    {
        return new List<Weapon>(inventory);
    }

    /// <summary>
    /// 获取武器数量
    /// </summary>
    public int GetWeaponCount()
    {
        return inventory.Count;
    }

    /// <summary>
    /// 清空武器库存
    /// </summary>
    public void ClearInventory()
    {
        inventory.Clear();
        currentWeaponIndex = 0;
        Debug.Log("武器库存已清空");
    }

    /// <summary>
    /// 检查是否拥有某武器
    /// </summary>
    public bool HasWeapon(string weaponName)
    {
        return inventory.Exists(w => w.Name == weaponName);
    }

    /// <summary>
    /// 根据名称获取武器
    /// </summary>
    public Weapon GetWeaponByName(string weaponName)
    {
        return inventory.Find(w => w.Name == weaponName);
    }

    /// <summary>
    /// 当前武器开火后同步弹药
    /// </summary>
    public void OnWeaponFired()
    {
        SaveCurrentWeaponAmmo();
    }

    /// <summary>
    /// 当前武器换弹后同步弹药
    /// </summary>
    public void OnWeaponReloaded()
    {
        SaveCurrentWeaponAmmo();
    }

    /// <summary>
    /// 场景切换前调用
    /// </summary>
    private void OnDestroy()
    {
        // 保存所有数据
        if (Instance == this)
        {
            SaveToStaticData();
        }
    }
}
