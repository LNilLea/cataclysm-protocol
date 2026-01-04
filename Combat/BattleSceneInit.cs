using UnityEngine;
using MyGame;

/// <summary>
/// 战斗场景初始化 - 确保玩家有武器
/// </summary>
public class BattleSceneInit : MonoBehaviour
{
    [Header("初始武器（如果背包为空）")]
    public bool giveStarterWeapons = true;
    public WeaponChoice[] starterWeapons = { WeaponChoice.匕首, WeaponChoice.手枪 };

    [Header("调试")]
    public bool forceResetInventory = false;  // 强制重置背包（测试用）

    private void Awake()
    {
        // 强制重置（仅测试）
        if (forceResetInventory)
        {
            PlayerInventoryData.Reset();
            Debug.Log("[BattleSceneInit] 背包已重置");
        }

        // 如果没有武器，给予初始武器
        if (giveStarterWeapons && PlayerInventoryData.GetWeaponCount() == 0)
        {
            foreach (var weapon in starterWeapons)
            {
                PlayerInventoryData.AddWeapon(weapon);
                Debug.Log($"[BattleSceneInit] 给予初始武器: {weapon}");
            }
        }

        // 确保玩家装备第一把武器
        Player player = FindObjectOfType<Player>();
        WeaponManager weaponManager = player?.GetComponent<WeaponManager>();

        if (player != null && weaponManager != null)
        {
            if (PlayerInventoryData.GetWeaponCount() > 0)
            {
                weaponManager.SwitchToWeapon(0);
                Debug.Log("[BattleSceneInit] 玩家已装备武器");
            }
        }

        // 初始化玩家HP（如果需要）
        if (player != null && player.combatData != null)
        {
            if (player.combatData.currentHP <= 0)
            {
                player.combatData.currentHP = player.combatData.maxHP;
            }
        }

        Debug.Log("[BattleSceneInit] 战斗场景初始化完成");
    }
}
