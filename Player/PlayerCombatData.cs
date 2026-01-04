using UnityEngine;

/// <summary>
/// 玩家战斗数据 - 包含所有战斗相关的属性和计算
/// </summary>
public class PlayerCombatData
{
    // ===== 基础属性（来自 CharacterCreation） =====
    public int strength;        // 体魄
    public int agility;         // 反应/敏捷
    public int intelligence;    // 智力
    public int vitality;        // 体质
    public int willpower;       // 意志
    public int charisma;        // 魅力
    public int mobility;        // 机动力

    // ===== 战斗数值 =====
    public int maxHP;
    public int currentHP;

    // ===== AC 组成部分 =====
    public int baseAC;          // 基础AC（固定10）
    public int agilityAC;       // 临时敏捷AC加值（由专长等添加，每回合重置）
    public int armorAC;         // 盔甲AC
    public int otherAC;         // 其他AC加值（等级加成等）
    
    // ===== 先攻值 =====
    public int initiative;      // 计算后的先攻值

    // ===== 状态标记 =====
    public bool isGrappledByMantis = false;     // 被螳螂擒抱（敏捷AC失效）
    public int movementSquares = 0;              // 移动格数

    // ===== 专长系统 =====
    public FeatBase feat;

    // ===== 不屈意志相关 =====
    public bool hasUsedUnyieldingWillThisTurn = false;
    public bool isEnemyAllDead = false;
    public bool hasUnyieldingWillThisRound = false;

    // ===== 计算属性 =====

    /// <summary>
    /// 敏捷加值（用于武器命中和伤害计算）
    /// 公式：敏捷 - 3
    /// </summary>
    public int AgilityModifier => agility - 3;

    /// <summary>
    /// 体魄加值
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
    /// 公式：10 + 敏捷加值 + 临时敏捷AC + 专长AC + 盔甲AC + 其他AC
    /// 注意：被螳螂擒抱时敏捷相关AC失效
    /// </summary>
    public int CurrentAC
    {
        get
        {
            if (isGrappledByMantis)
            {
                // 被擒抱时敏捷AC失效（基础敏捷加值和临时敏捷AC都失效）
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
    /// 【新增】从静态 CharacterData 读取数据的构造函数（推荐使用）
    /// </summary>
    public PlayerCombatData()
    {
        // 从静态类读取属性
        strength = CharacterData.Strength;
        agility = CharacterData.Agility;
        intelligence = CharacterData.Intelligence;
        vitality = CharacterData.Vitality;
        willpower = CharacterData.Willpower;
        charisma = CharacterData.Charisma;
        mobility = CharacterData.Mobility;

        // ===== 计算 HP =====
        maxHP = vitality * 5;
        currentHP = maxHP;

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

        Debug.Log($"[PlayerCombatData] 从 CharacterData 初始化完成 - HP:{maxHP}, AC:{CurrentAC}, 先攻:{initiative}");
    }

    /// <summary>
    /// 【保留】原有的构造函数（需要 CharacterCreation 引用）
    /// </summary>
    public PlayerCombatData(CharacterCreation cc)
    {
        // 读取属性
        strength = cc.strength;
        agility = cc.agility;
        intelligence = cc.intelligence;
        vitality = cc.vitality;
        willpower = cc.willpower;
        charisma = cc.charisma;
        mobility = cc.mobility;

        // ===== 计算 HP =====
        // 每1点体质 = 5HP
        maxHP = vitality * 5;
        currentHP = maxHP;

        // ===== 初始化 AC =====
        // AC = 10 + 敏捷加值 + 临时敏捷AC + 专长AC + 盔甲AC + 其他AC
        baseAC = 10;
        agilityAC = 0;  // 临时加值，每回合重置
        armorAC = 0;    // 初始无盔甲
        otherAC = 0;    // 初始无其他加值

        // ===== 加载专长 =====
        feat = FeatSlot.LoadFeat(cc.selectedFeat);
        if (feat != null)
        {
            feat.OnBattleStart(this);
        }

        // ===== 计算先攻值 =====
        // 先攻 = 反应 × 5 + 专长先攻加值
        initiative = CalculateInitiative();

        // ===== 计算移动力 =====
        CalculateMovementSquares(BattleFieldSize.Small);

        Debug.Log($"[PlayerCombatData] 初始化完成 - HP:{maxHP}, AC:{CurrentAC}, 先攻:{initiative}");
    }

    // ===== 回合管理 =====

    /// <summary>
    /// 回合开始时重置临时AC加值
    /// </summary>
    public void OnTurnStart()
    {
        // 重置临时敏捷AC（每回合由专长重新赋予）
        agilityAC = 0;
        
        // 调用专长的回合开始效果
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

    /// <summary>
    /// 玩家受到伤害
    /// </summary>
    public void TakeDamage(ref int damage)
    {
        // 专长可能减免伤害
        if (feat != null)
            feat.OnPlayerTakeDamage(this, ref damage);

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        Debug.Log($"[PlayerCombatData] 受到 {damage} 伤害, 剩余HP: {currentHP}/{maxHP}");
    }

    /// <summary>
    /// 玩家造成伤害（专长可能增加伤害）
    /// </summary>
    public void DealDamage(ref int damage)
    {
        if (feat != null)
            feat.OnPlayerDealDamage(this, ref damage);
    }

    // ===== 盔甲系统 =====

    /// <summary>
    /// 装备盔甲
    /// </summary>
    public void EquipArmor(int armorValue)
    {
        armorAC = armorValue;
        Debug.Log($"[PlayerCombatData] 装备盔甲, AC+{armorValue}, 当前AC: {CurrentAC}");
    }

    /// <summary>
    /// 卸下盔甲
    /// </summary>
    public void UnequipArmor()
    {
        armorAC = 0;
        Debug.Log($"[PlayerCombatData] 卸下盔甲, 当前AC: {CurrentAC}");
    }

    // ===== 不屈意志 =====

    /// <summary>
    /// 触发不屈意志
    /// </summary>
    public void TriggerUnyieldingWill()
    {
        if (currentHP <= 0 && !hasUnyieldingWillThisRound)
        {
            hasUnyieldingWillThisRound = true;
            Debug.Log("[PlayerCombatData] 触发不屈意志，获得反击机会！");
        }
    }

    /// <summary>
    /// 回合结束重置
    /// </summary>
    public void EndTurnReset()
    {
        hasUnyieldingWillThisRound = false;
    }

    /// <summary>
    /// 战斗结束后恢复HP（如果消灭了所有敌人）
    /// </summary>
    public void RestoreHPIfNeeded()
    {
        if (currentHP == 0 && isEnemyAllDead)
        {
            currentHP = 1;
            Debug.Log("[PlayerCombatData] 战斗结束后恢复到 1HP！");
        }
    }

    /// <summary>
    /// 设置敌人全灭标记
    /// </summary>
    public void SetEnemyAllDead(bool value)
    {
        isEnemyAllDead = value;
    }

    // ===== 调试信息 =====

    /// <summary>
    /// 获取状态摘要
    /// </summary>
    public string GetStatusSummary()
    {
        return $"HP:{currentHP}/{maxHP} | AC:{CurrentAC} | 先攻:{initiative} | 移动:{movementSquares}格";
    }
}
