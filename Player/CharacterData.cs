using UnityEngine;
using MyGame;

/// <summary>
/// 静态角色数据 - 跨场景存储角色属性和状态
/// </summary>
public static class CharacterData
{
    // === 基础属性 ===
    public static int Strength = 3;
    public static int Agility = 3;
    public static int Intelligence = 3;
    public static int Vitality = 3;
    public static int Willpower = 3;
    public static int Charisma = 3;
    public static int Mobility = 3;

    // === 战斗属性 ===
    public static int MaxHP = 100;
    public static int CurrentHP = 100;
    public static int AC = 10;              // 护甲等级

    // === 专长 ===
    public static string SelectedFeat = "None";

    // === 状态 ===
    public static bool IsCharacterCreated = false;
    public static bool IsInitialized = false;

    // === 经验和等级 ===
    public static int Level = 1;
    public static int Experience = 0;

    /// <summary>
    /// 从角色创建界面保存数据
    /// </summary>
    public static void SaveFromCreation(CharacterCreation creation)
    {
        Strength = creation.strength;
        Agility = creation.agility;
        Intelligence = creation.intelligence;
        Vitality = creation.vitality;
        Willpower = creation.willpower;
        Charisma = creation.charisma;
        Mobility = creation.mobility;
        SelectedFeat = creation.selectedFeat;

        // 根据体质计算最大HP
        MaxHP = 50 + Vitality * 10;
        CurrentHP = MaxHP;

        // 根据反应计算AC
        AC = 10 + Agility;

        IsCharacterCreated = true;
        IsInitialized = true;

        Debug.Log($"[CharacterData] 角色创建数据已保存 - HP:{MaxHP}, AC:{AC}");
    }

    /// <summary>
    /// 从玩家对象保存数据（进入战斗场景前调用）
    /// </summary>
    public static void SaveFromPlayer(Player player)
    {
        if (player == null) return;

        if (player.combatData != null)
        {
            MaxHP = player.combatData.maxHP;
            CurrentHP = player.combatData.currentHP;
            Mobility = player.combatData.mobility;
        }

        IsInitialized = true;
        Debug.Log($"[CharacterData] 从玩家保存数据 - HP:{CurrentHP}/{MaxHP}");
    }

    /// <summary>
    /// 应用数据到玩家对象（进入场景后调用）
    /// </summary>
    public static void ApplyToPlayer(Player player)
    {
        if (player == null || !IsInitialized) return;

        if (player.combatData != null)
        {
            player.combatData.maxHP = MaxHP;
            player.combatData.currentHP = CurrentHP;
            player.combatData.mobility = Mobility;
        }

        Debug.Log($"[CharacterData] 应用数据到玩家 - HP:{CurrentHP}/{MaxHP}");
    }

    /// <summary>
    /// 治疗玩家
    /// </summary>
    public static void Heal(int amount)
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public static void TakeDamage(int damage)
    {
        CurrentHP = Mathf.Max(CurrentHP - damage, 0);
    }

    /// <summary>
    /// 完全恢复
    /// </summary>
    public static void FullRestore()
    {
        CurrentHP = MaxHP;
    }

    /// <summary>
    /// 获得经验
    /// </summary>
    public static void GainExperience(int exp)
    {
        Experience += exp;

        // 检查升级
        int expNeeded = Level * 100;
        while (Experience >= expNeeded)
        {
            Experience -= expNeeded;
            Level++;
            OnLevelUp();
            expNeeded = Level * 100;
        }
    }

    /// <summary>
    /// 升级时调用
    /// </summary>
    private static void OnLevelUp()
    {
        // 升级增加最大HP
        MaxHP += 10;
        CurrentHP = MaxHP;

        Debug.Log($"[CharacterData] 升级！Level:{Level}, MaxHP:{MaxHP}");
    }

    /// <summary>
    /// 重置为默认值
    /// </summary>
    public static void Reset()
    {
        Strength = 3;
        Agility = 3;
        Intelligence = 3;
        Vitality = 3;
        Willpower = 3;
        Charisma = 3;
        Mobility = 3;

        MaxHP = 100;
        CurrentHP = 100;
        AC = 10;

        Level = 1;
        Experience = 0;

        SelectedFeat = "None";
        IsCharacterCreated = false;
        IsInitialized = false;
    }

    /// <summary>
    /// 获取调试信息
    /// </summary>
    public static string GetDebugInfo()
    {
        return $"=== CharacterData ===\n" +
               $"Level: {Level} (Exp: {Experience})\n" +
               $"HP: {CurrentHP}/{MaxHP}\n" +
               $"AC: {AC}\n" +
               $"属性: 体魄{Strength} 反应{Agility} 智力{Intelligence}\n" +
               $"      体质{Vitality} 意志{Willpower} 魅力{Charisma}\n" +
               $"移动力: {Mobility}\n" +
               $"专长: {SelectedFeat}";
    }
}
