using UnityEngine;
using MyGame;
/// <summary>
/// 玩家战斗数据 - 包含所有战斗相关的属性和计算
/// 
/// HP计算：MaxHP = 体魄 × 5
/// AC计算：AC = 10 + 反应加值
/// 先攻计算：反应 × 5 + 专长加值
/// </summary>
public class PlayerCombatData
{
    // ===== 默认值常量（普通人平均值为3）=====
    private const int DEFAULT_ATTRIBUTE = 3;
    private const int DEFAULT_MAX_HP = 15;  // 3 * 5 = 15
    private const int DEFAULT_MOBILITY = 3;

    // ===== 基础属性 =====
    public int intelligence;    // 智力
    public int strength;        // 体魄 - 决定HP！
    public int agility;         // 反应 - 决定AC和先攻
    public int technology;      // 技术
    public int willpower;       // 意志
    public int humanity;        // 人性
    public int charisma;        // 魅力
    public int mobility;        // 移动力

    // ===== 战斗数值 =====
    public int maxHP;
    public int currentHP;

    // ===== AC 组成部分 =====
    public int baseAC;          // 基础AC（固定10）
    public int agilityAC;       // 临时敏捷AC加值（由专长等添加，每回合重置）
    public int armorAC;         // 盔甲AC
    public int otherAC;         // 其他AC加值（等级加成等）
    
    // ===== 先攻值 =====
    public int initiative;

    // ===== 状态标记 =====
    public bool isGrappledByMantis = false;
    public int movementSquares = 0;

    // ===== 专长系统 =====
    public FeatBase feat;

    // ===== 不屈意志相关 =====
    public bool hasUsedUnyieldingWillThisTurn = false;
    public bool isEnemyAllDead = false;
    public bool hasUnyieldingWillThisRound = false;

    // ===== 计算属性 =====
    
    /// <summary>
    /// 反应加值（用于AC和命中计算）
    /// 公式：反应 - 3（3是普通人基准）
    /// </summary>
    public int AgilityModifier => agility - 3;

    /// <summary>
    /// 体魄加值（用于近战伤害计算）
    /// </summary>
    public int StrengthModifier => strength - 3;

    /// <summary>
    /// 专长提供的AC加值
    /// </summary>
    public int FeatACBonus => feat != null ? feat.ACBonus : 0;

    /// <summary>
    /// 专长提供的先攻加值
    /// </summary>
    public int FeatInitiativeBonus => feat != null ? feat.InitiativeBonus : 0;

    /// <summary>
    /// 当前 AC
    /// 公式：10 + 反应加值 + 临时AC + 专长AC + 盔甲AC + 其他AC
    /// </summary>
    public int CurrentAC
    {
        get
        {
            if (isGrappledByMantis)
            {
                // 被擒抱时反应AC失效
                return baseAC + FeatACBonus + armorAC + otherAC;
            }
            return baseAC + AgilityModifier + agilityAC + FeatACBonus + armorAC + otherAC;
        }
    }

    /// <summary>
    /// 计算先攻值
    /// 公式：反应 × 5 + 专长先攻加值
    /// </summary>
    public int CalculateInitiative()
    {
        return agility * 5 + FeatInitiativeBonus;
    }

    // ===== 构造函数 =====
    
    /// <summary>
    /// 从静态 CharacterData 读取数据的构造函数
    /// </summary>
    public PlayerCombatData()
    {
        // 从静态类读取属性，如果为0则使用默认值
        intelligence = GetValidValue(CharacterData.Intelligence, DEFAULT_ATTRIBUTE);
        strength = GetValidValue(CharacterData.Strength, DEFAULT_ATTRIBUTE);
        agility = GetValidValue(CharacterData.Agility, DEFAULT_ATTRIBUTE);
        technology = GetValidValue(CharacterData.Technology, DEFAULT_ATTRIBUTE);
        willpower = GetValidValue(CharacterData.Willpower, DEFAULT_ATTRIBUTE);
        humanity = GetValidValue(CharacterData.Humanity, DEFAULT_ATTRIBUTE);
        charisma = GetValidValue(CharacterData.Charisma, DEFAULT_ATTRIBUTE);
        mobility = GetValidValue(CharacterData.Mobility, DEFAULT_MOBILITY);

        // ===== 计算 HP（MaxHP = 体魄 × 5）=====
        if (CharacterData.IsInitialized && CharacterData.MaxHP > 0)
        {
            maxHP = CharacterData.MaxHP;
            currentHP = CharacterData.CurrentHP > 0 ? CharacterData.CurrentHP : maxHP;
        }
        else
        {
            maxHP = strength * 5;
            currentHP = maxHP;
        }

        // 安全检查
        if (maxHP <= 0)
        {
            maxHP = DEFAULT_MAX_HP;
            Debug.LogWarning($"[PlayerCombatData] maxHP计算为0，使用默认值 {DEFAULT_MAX_HP}");
        }
        if (currentHP <= 0)
        {
            currentHP = maxHP;
            Debug.LogWarning($"[PlayerCombatData] currentHP为0，重置为 {maxHP}");
        }

        // ===== 初始化 AC =====
        baseAC = 10;
        agilityAC = 0;
        armorAC = 0;
        otherAC = 0;

        // ===== 加载专长 =====
        feat = FeatSlot.LoadFeat(CharacterData.SelectedFeat);
        if (feat != null)
        {
            feat.OnBattleStart(this);
        }

        // ===== 计算先攻值 =====
        initiative = CalculateInitiative();

        // ===== 计算移动力 =====
        CalculateMovementSquares(BattleFieldSize.Small);

        Debug.Log($"[PlayerCombatData] 初始化完成 - HP:{currentHP}/{maxHP}, AC:{CurrentAC}, 先攻:{initiative}, 体魄:{strength}, 反应:{agility}");
    }

    /// <summary>
    /// 从 CharacterCreation 读取数据的构造函数
    /// </summary>
    public PlayerCombatData(CharacterCreation cc)
    {
        // 读取属性
        intelligence = cc.intelligence > 0 ? cc.intelligence : DEFAULT_ATTRIBUTE;
        strength = cc.strength > 0 ? cc.strength : DEFAULT_ATTRIBUTE;
        agility = cc.agility > 0 ? cc.agility : DEFAULT_ATTRIBUTE;
        technology = cc.technology > 0 ? cc.technology : DEFAULT_ATTRIBUTE;
        willpower = cc.willpower > 0 ? cc.willpower : DEFAULT_ATTRIBUTE;
        humanity = cc.humanity > 0 ? cc.humanity : DEFAULT_ATTRIBUTE;
        charisma = cc.charisma > 0 ? cc.charisma : DEFAULT_ATTRIBUTE;
        mobility = cc.mobility > 0 ? cc.mobility : DEFAULT_MOBILITY;

        // ===== 计算 HP（MaxHP = 体魄 × 5）=====
        maxHP = strength * 5;
        if (maxHP <= 0) maxHP = DEFAULT_MAX_HP;
        currentHP = maxHP;

        // ===== 初始化 AC =====
        baseAC = 10;
        agilityAC = 0;
        armorAC = 0;
        otherAC = 0;

        // ===== 加载专长 =====
        feat = FeatSlot.LoadFeat(cc.selectedFeat);
        if (feat != null)
        {
            feat.OnBattleStart(this);
        }

        // ===== 计算先攻值 =====
        initiative = CalculateInitiative();

        // ===== 计算移动力 =====
        CalculateMovementSquares(BattleFieldSize.Small);

        Debug.Log($"[PlayerCombatData] 初始化完成 - HP:{maxHP}, AC:{CurrentAC}, 先攻:{initiative}");
    }

    /// <summary>
    /// 获取有效值，如果原值<=0则返回默认值
    /// </summary>
    private int GetValidValue(int value, int defaultValue)
    {
        if (value <= 0)
        {
            Debug.LogWarning($"[PlayerCombatData] 属性值为{value}，使用默认值{defaultValue}");
            return defaultValue;
        }
        return value;
    }

    // ===== 回合管理 =====

    public void OnTurnStart()
    {
        agilityAC = 0;
        
        if (feat != null)
        {
            feat.OnTurnStart(this);
        }
    }

    // ===== 移动力计算 =====
    
    public enum BattleFieldSize
    {
        Small,
        Medium,
        Large
    }

    public void CalculateMovementSquares(BattleFieldSize size)
    {
        switch (size)
        {
            case BattleFieldSize.Small:
                movementSquares = mobility * 1;
                break;
            case BattleFieldSize.Medium:
                movementSquares = mobility * 3;
                break;
            case BattleFieldSize.Large:
                movementSquares = mobility * 5;
                break;
        }
    }

    // ===== 伤害处理 =====

    public void TakeDamage(ref int damage)
    {
        if (feat != null)
            feat.OnPlayerTakeDamage(this, ref damage);

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        Debug.Log($"[PlayerCombatData] 受到 {damage} 伤害, 剩余HP: {currentHP}/{maxHP}");
    }

    public void DealDamage(ref int damage)
    {
        if (feat != null)
            feat.OnPlayerDealDamage(this, ref damage);
    }

    // ===== 盔甲系统 =====

    public void EquipArmor(int armorValue)
    {
        armorAC = armorValue;
        Debug.Log($"[PlayerCombatData] 装备盔甲, AC+{armorValue}, 当前AC: {CurrentAC}");
    }

    public void UnequipArmor()
    {
        armorAC = 0;
        Debug.Log($"[PlayerCombatData] 卸下盔甲, 当前AC: {CurrentAC}");
    }

    // ===== 不屈意志 =====

    public void TriggerUnyieldingWill()
    {
        if (currentHP <= 0 && !hasUnyieldingWillThisRound)
        {
            hasUnyieldingWillThisRound = true;
            Debug.Log("[PlayerCombatData] 触发不屈意志，获得反击机会！");
        }
    }

    public void EndTurnReset()
    {
        hasUnyieldingWillThisRound = false;
    }

    public void RestoreHPIfNeeded()
    {
        if (currentHP == 0 && isEnemyAllDead)
        {
            currentHP = 1;
            Debug.Log("[PlayerCombatData] 战斗结束后恢复到 1HP！");
        }
    }

    public void SetEnemyAllDead(bool value)
    {
        isEnemyAllDead = value;
    }

    // ===== 调试信息 =====

    public string GetStatusSummary()
    {
        return $"HP:{currentHP}/{maxHP} | AC:{CurrentAC} | 先攻:{initiative} | 移动:{movementSquares}格";
    }
}
