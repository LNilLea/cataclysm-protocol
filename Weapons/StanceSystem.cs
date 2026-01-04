using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 架势/Buff类型
/// </summary>
public enum StanceType
{
    None,
    Defensive,      // 防御架势：AC +2
    Aggressive,     // 进攻架势：伤害 +2，AC -1
    Focused,        // 专注架势：命中 +2
    Evasive         // 闪避架势：被攻击时敌人命中 -2
}

/// <summary>
/// Buff数据
/// </summary>
[System.Serializable]
public class BuffData
{
    public string buffName;
    public StanceType stanceType;
    public int duration;            // 持续回合数（-1 = 永久直到切换）
    public int acModifier;          // AC 修正
    public int hitModifier;         // 命中修正
    public int damageModifier;      // 伤害修正
    public int enemyHitModifier;    // 敌人命中修正（负数 = 敌人更难命中）

    public BuffData(string name, StanceType type, int dur, int ac = 0, int hit = 0, int dmg = 0, int enemyHit = 0)
    {
        buffName = name;
        stanceType = type;
        duration = dur;
        acModifier = ac;
        hitModifier = hit;
        damageModifier = dmg;
        enemyHitModifier = enemyHit;
    }
}

/// <summary>
/// 架势/Buff系统 - 管理玩家的架势和临时Buff
/// </summary>
public class StanceSystem : MonoBehaviour
{
    public static StanceSystem Instance { get; private set; }

    [Header("当前架势")]
    public StanceType currentStance = StanceType.None;

    [Header("当前Buff列表")]
    public List<BuffData> activeBuffs = new List<BuffData>();

    // 预定义的架势
    private Dictionary<StanceType, BuffData> stanceDefinitions;

    // 事件
    public event System.Action<StanceType> OnStanceChanged;
    public event System.Action<BuffData> OnBuffAdded;
    public event System.Action<BuffData> OnBuffRemoved;

    private void Awake()
    {
        Instance = this;
        InitializeStances();
    }

    /// <summary>
    /// 初始化架势定义
    /// </summary>
    private void InitializeStances()
    {
        stanceDefinitions = new Dictionary<StanceType, BuffData>
        {
            { StanceType.None, new BuffData("无架势", StanceType.None, -1) },
            { StanceType.Defensive, new BuffData("防御架势", StanceType.Defensive, -1, ac: 2) },
            { StanceType.Aggressive, new BuffData("进攻架势", StanceType.Aggressive, -1, ac: -1, dmg: 2) },
            { StanceType.Focused, new BuffData("专注架势", StanceType.Focused, -1, hit: 2) },
            { StanceType.Evasive, new BuffData("闪避架势", StanceType.Evasive, -1, enemyHit: -2) }
        };
    }

    /// <summary>
    /// 切换架势（消耗次要动作）
    /// </summary>
    public bool SwitchStance(StanceType newStance, ActionPointSystem actionSystem)
    {
        if (actionSystem != null && !actionSystem.CanDoMinorAction())
        {
            Debug.Log("无法切换架势：没有次要动作点");
            return false;
        }

        // 消耗次要动作
        if (actionSystem != null)
        {
            actionSystem.UseMinorAction();
        }

        // 移除旧架势的Buff
        if (currentStance != StanceType.None)
        {
            RemoveStanceBuff(currentStance);
        }

        // 应用新架势
        currentStance = newStance;

        if (newStance != StanceType.None)
        {
            BuffData stanceBuff = stanceDefinitions[newStance];
            activeBuffs.Add(stanceBuff);
            OnBuffAdded?.Invoke(stanceBuff);
        }

        OnStanceChanged?.Invoke(newStance);

        Debug.Log($"切换架势: {GetStanceName(newStance)}");
        return true;
    }

    /// <summary>
    /// 移除架势Buff
    /// </summary>
    private void RemoveStanceBuff(StanceType stance)
    {
        BuffData toRemove = activeBuffs.Find(b => b.stanceType == stance);
        if (toRemove != null)
        {
            activeBuffs.Remove(toRemove);
            OnBuffRemoved?.Invoke(toRemove);
        }
    }

    /// <summary>
    /// 添加临时Buff
    /// </summary>
    public void AddBuff(BuffData buff)
    {
        activeBuffs.Add(buff);
        OnBuffAdded?.Invoke(buff);
        Debug.Log($"获得Buff: {buff.buffName}，持续 {buff.duration} 回合");
    }

    /// <summary>
    /// 回合结束时处理Buff持续时间
    /// </summary>
    public void OnTurnEnd()
    {
        List<BuffData> toRemove = new List<BuffData>();

        foreach (var buff in activeBuffs)
        {
            // 架势类Buff不会自动消失
            if (buff.duration == -1) continue;

            buff.duration--;
            if (buff.duration <= 0)
            {
                toRemove.Add(buff);
            }
        }

        foreach (var buff in toRemove)
        {
            activeBuffs.Remove(buff);
            OnBuffRemoved?.Invoke(buff);
            Debug.Log($"Buff消失: {buff.buffName}");
        }
    }

    /// <summary>
    /// 获取总AC修正
    /// </summary>
    public int GetTotalACModifier()
    {
        int total = 0;
        foreach (var buff in activeBuffs)
        {
            total += buff.acModifier;
        }
        return total;
    }

    /// <summary>
    /// 获取总命中修正
    /// </summary>
    public int GetTotalHitModifier()
    {
        int total = 0;
        foreach (var buff in activeBuffs)
        {
            total += buff.hitModifier;
        }
        return total;
    }

    /// <summary>
    /// 获取总伤害修正
    /// </summary>
    public int GetTotalDamageModifier()
    {
        int total = 0;
        foreach (var buff in activeBuffs)
        {
            total += buff.damageModifier;
        }
        return total;
    }

    /// <summary>
    /// 获取敌人命中修正（用于敌人攻击玩家时）
    /// </summary>
    public int GetEnemyHitModifier()
    {
        int total = 0;
        foreach (var buff in activeBuffs)
        {
            total += buff.enemyHitModifier;
        }
        return total;
    }

    /// <summary>
    /// 获取架势名称
    /// </summary>
    public string GetStanceName(StanceType stance)
    {
        switch (stance)
        {
            case StanceType.None: return "无";
            case StanceType.Defensive: return "防御架势";
            case StanceType.Aggressive: return "进攻架势";
            case StanceType.Focused: return "专注架势";
            case StanceType.Evasive: return "闪避架势";
            default: return "未知";
        }
    }

    /// <summary>
    /// 获取架势描述
    /// </summary>
    public string GetStanceDescription(StanceType stance)
    {
        switch (stance)
        {
            case StanceType.None: return "没有特殊效果";
            case StanceType.Defensive: return "AC +2";
            case StanceType.Aggressive: return "伤害 +2, AC -1";
            case StanceType.Focused: return "命中 +2";
            case StanceType.Evasive: return "敌人命中 -2";
            default: return "";
        }
    }

    /// <summary>
    /// 获取当前所有Buff的描述
    /// </summary>
    public string GetBuffSummary()
    {
        if (activeBuffs.Count == 0) return "无";

        List<string> buffNames = new List<string>();
        foreach (var buff in activeBuffs)
        {
            buffNames.Add(buff.buffName);
        }
        return string.Join(", ", buffNames);
    }

    /// <summary>
    /// 清除所有Buff（战斗结束时调用）
    /// </summary>
    public void ClearAllBuffs()
    {
        activeBuffs.Clear();
        currentStance = StanceType.None;
        Debug.Log("所有Buff已清除");
    }
}
