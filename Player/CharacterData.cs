using UnityEngine;
using MyGame;

/// <summary>
/// 静态角色数据 - 跨场景存储角色属性和状态
/// 
/// 属性系统（基于游戏规则）：
/// - 智力 (Intelligence): 判定属性
/// - 体魄 (Strength): 战斗属性，决定HP
/// - 反应 (Agility): 战斗属性，决定AC和先攻
/// - 技术 (Technology): 判定属性
/// - 意志 (Willpower): 抵抗属性
/// - 人性 (Humanity): 社交属性，改造关键属性
/// - 魅力 (Charisma): 社交属性
/// - 移动力 (Mobility): 战斗属性
/// 
/// HP计算：MaxHP = 体魄 × 5
/// AC计算：AC = 10 + 反应
/// </summary>
public static class CharacterData
{
    // ===== 默认值常量（普通人平均值为3）=====
    private const int DEFAULT_ATTRIBUTE = 3;
    private const int DEFAULT_MAX_HP = 15;  // 3 * 5 = 15

    // === 基础属性 ===
    public static int Intelligence = DEFAULT_ATTRIBUTE;  // 智力
    public static int Strength = DEFAULT_ATTRIBUTE;      // 体魄 - 决定HP！
    public static int Agility = DEFAULT_ATTRIBUTE;       // 反应
    public static int Technology = DEFAULT_ATTRIBUTE;    // 技术（新增）
    public static int Willpower = DEFAULT_ATTRIBUTE;     // 意志
    public static int Humanity = DEFAULT_ATTRIBUTE;      // 人性（新增）
    public static int Charisma = DEFAULT_ATTRIBUTE;      // 魅力
    public static int Mobility = DEFAULT_ATTRIBUTE;      // 移动力

    // === 战斗属性 ===
    public static int MaxHP = DEFAULT_MAX_HP;
    public static int CurrentHP = DEFAULT_MAX_HP;
    public static int AC = 10 + DEFAULT_ATTRIBUTE;  // 10 + 反应

    // === 专长 ===
    public static string SelectedFeat = "None";

    // === 状态 ===
    public static bool IsCharacterCreated = false;
    public static bool IsInitialized = false;

    // === 经验和等级 ===
    public static int Level = 1;
    public static int Experience = 0;

    /// <summary>
    /// 确保数据已初始化（如果还没初始化，则使用默认值）
    /// </summary>
    public static void EnsureInitialized()
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("[CharacterData] 数据未初始化，使用默认值");
            
            // 确保属性不为0
            if (Intelligence <= 0) Intelligence = DEFAULT_ATTRIBUTE;
            if (Strength <= 0) Strength = DEFAULT_ATTRIBUTE;
            if (Agility <= 0) Agility = DEFAULT_ATTRIBUTE;
            if (Technology <= 0) Technology = DEFAULT_ATTRIBUTE;
            if (Willpower <= 0) Willpower = DEFAULT_ATTRIBUTE;
            if (Humanity <= 0) Humanity = DEFAULT_ATTRIBUTE;
            if (Charisma <= 0) Charisma = DEFAULT_ATTRIBUTE;
            if (Mobility <= 0) Mobility = DEFAULT_ATTRIBUTE;

            // 计算HP（MaxHP = 体魄 × 5）
            MaxHP = Strength * 5;
            if (MaxHP <= 0) MaxHP = DEFAULT_MAX_HP;
            CurrentHP = MaxHP;
            
            // 计算AC（AC = 10 + 反应）
            AC = 10 + Agility;

            IsInitialized = true;
            
            Debug.Log($"[CharacterData] 已使用默认值初始化 - HP:{MaxHP}, AC:{AC}, 体魄:{Strength}");
        }
    }

    /// <summary>
    /// 从角色创建界面保存数据
    /// </summary>
    public static void SaveFromCreation(CharacterCreation creation)
    {
        // 读取属性，确保最小值为1
        Intelligence = Mathf.Max(creation.intelligence, 1);
        Strength = Mathf.Max(creation.strength, 1);
        Agility = Mathf.Max(creation.agility, 1);
        Technology = Mathf.Max(creation.technology, 1);
        Willpower = Mathf.Max(creation.willpower, 1);
        Humanity = Mathf.Max(creation.humanity, 1);
        Charisma = Mathf.Max(creation.charisma, 1);
        Mobility = Mathf.Max(creation.mobility, 1);
        SelectedFeat = creation.selectedFeat;

        // 计算HP（MaxHP = 体魄 × 5）
        MaxHP = Strength * 5;
        if (MaxHP <= 0) MaxHP = DEFAULT_MAX_HP;
        CurrentHP = MaxHP;

        // 计算AC（AC = 10 + 反应）
        AC = 10 + Agility;

        IsCharacterCreated = true;
        IsInitialized = true;

        Debug.Log($"[CharacterData] 角色创建数据已保存 - HP:{MaxHP}, AC:{AC}, 体魄:{Strength}, 反应:{Agility}");
    }

    /// <summary>
    /// 从玩家对象保存数据
    /// </summary>
    public static void SaveFromPlayer(Player player)
    {
        if (player == null) return;

        if (player.combatData != null)
        {
            MaxHP = player.combatData.maxHP;
            CurrentHP = player.combatData.currentHP;
            Mobility = player.combatData.mobility;
            Strength = player.combatData.strength;
            Agility = player.combatData.agility;
        }

        // 确保HP有效
        if (MaxHP <= 0) MaxHP = DEFAULT_MAX_HP;
        if (CurrentHP <= 0) CurrentHP = MaxHP;

        IsInitialized = true;
        Debug.Log($"[CharacterData] 从玩家保存数据 - HP:{CurrentHP}/{MaxHP}");
    }

    /// <summary>
    /// 应用数据到玩家对象
    /// </summary>
    public static void ApplyToPlayer(Player player)
    {
        if (player == null) return;
        
        // 确保数据已初始化
        EnsureInitialized();

        if (player.combatData != null)
        {
            player.combatData.maxHP = MaxHP;
            
            // 确保currentHP > 0
            if (CurrentHP <= 0)
            {
                CurrentHP = MaxHP;
                Debug.LogWarning($"[CharacterData] CurrentHP为0，重置为 {MaxHP}");
            }
            
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
        if (MaxHP <= 0) MaxHP = DEFAULT_MAX_HP;
        CurrentHP = MaxHP;
    }

    /// <summary>
    /// 获得经验
    /// </summary>
    public static void GainExperience(int exp)
    {
        Experience += exp;

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
    /// 每升2级获得1属性点（由玩家自己分配，这里只增加HP）
    /// </summary>
    private static void OnLevelUp()
    {
        // 升级时HP增加（基于当前体魄重新计算）
        MaxHP = Strength * 5 + (Level - 1) * 5;  // 每级+5 HP
        CurrentHP = MaxHP;

        Debug.Log($"[CharacterData] 升级！Level:{Level}, MaxHP:{MaxHP}");
    }

    /// <summary>
    /// 重置为默认值
    /// </summary>
    public static void Reset()
    {
        Intelligence = DEFAULT_ATTRIBUTE;
        Strength = DEFAULT_ATTRIBUTE;
        Agility = DEFAULT_ATTRIBUTE;
        Technology = DEFAULT_ATTRIBUTE;
        Willpower = DEFAULT_ATTRIBUTE;
        Humanity = DEFAULT_ATTRIBUTE;
        Charisma = DEFAULT_ATTRIBUTE;
        Mobility = DEFAULT_ATTRIBUTE;

        MaxHP = DEFAULT_MAX_HP;
        CurrentHP = DEFAULT_MAX_HP;
        AC = 10 + DEFAULT_ATTRIBUTE;

        Level = 1;
        Experience = 0;

        SelectedFeat = "None";
        IsCharacterCreated = false;
        IsInitialized = false;
        
        Debug.Log("[CharacterData] 已重置为默认值");
    }

    /// <summary>
    /// 获取调试信息
    /// </summary>
    public static string GetDebugInfo()
    {
        return $"=== CharacterData ===\n" +
               $"IsInitialized: {IsInitialized}\n" +
               $"Level: {Level} (Exp: {Experience})\n" +
               $"HP: {CurrentHP}/{MaxHP}\n" +
               $"AC: {AC}\n" +
               $"战斗属性: 体魄{Strength} 反应{Agility} 移动力{Mobility}\n" +
               $"判定属性: 智力{Intelligence} 技术{Technology}\n" +
               $"社交属性: 人性{Humanity} 魅力{Charisma}\n" +
               $"其他: 意志{Willpower}\n" +
               $"专长: {SelectedFeat}";
    }
}
