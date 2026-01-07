using UnityEngine;
using MyGame;

/// <summary>
/// 回合制行为节点基类
/// </summary>
public abstract class BehaviorNode
{
    /// <summary>
    /// 执行节点
    /// </summary>
    /// <param name="context">行为上下文（包含移动力等信息）</param>
    /// <returns>执行结果</returns>
    public abstract BehaviorResult Execute(BehaviorContext context);
}

/// <summary>
/// 行为执行结果
/// </summary>
public enum BehaviorResult
{
    Success,    // 成功
    Failure,    // 失败
    Running     // 运行中（用于多帧执行）
}

/// <summary>
/// 行为上下文 - 包含怪物回合所需的所有信息
/// </summary>
public class BehaviorContext
{
    // 怪物自身
    public IMobAction mobAction;
    public Transform mobTransform;
    public MonsterCombatData combatData;

    // 目标
    public Player targetPlayer;

    // 回合资源
    public int remainingMovement;       // 剩余移动力（格数）
    public bool hasMainAction;          // 是否有主要动作
    public bool hasMinorAction;         // 是否有次要动作

    // 战斗信息
    public float distanceToPlayer;      // 到玩家的距离（格数）
    public float attackRange;           // 攻击范围

    // 网格信息
    public float gridSize = 1f;

    // 日志
    public string actionLog = "";

    /// <summary>
    /// 消耗移动力
    /// </summary>
    public bool UseMovement(int amount)
    {
        if (remainingMovement >= amount)
        {
            remainingMovement -= amount;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 消耗主要动作
    /// </summary>
    public bool UseMainAction()
    {
        if (hasMainAction)
        {
            hasMainAction = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 添加日志
    /// </summary>
    public void Log(string message)
    {
        actionLog += message + "\n";
        Debug.Log(message);
    }

    /// <summary>
    /// 更新到玩家的距离
    /// </summary>
    public void UpdateDistanceToPlayer()
    {
        if (mobTransform != null && targetPlayer != null)
        {
            float worldDistance = Vector3.Distance(mobTransform.position, targetPlayer.transform.position);
            distanceToPlayer = worldDistance / gridSize;
        }
    }
}

/// <summary>
/// 怪物战斗数据
/// </summary>
[System.Serializable]
public class MonsterCombatData
{
    public int maxHP = 50;
    public int currentHP = 50;
    public int mobility = 3;            // 移动力
    public int attackRange = 1;         // 攻击范围（格数）
    public int initiative = 10;         // 先攻值
    public int ac = 12;                 // AC

    // 攻击数据
    public int hitBonus = 2;
    public int damageDiceCount = 1;
    public int damageDiceSides = 6;
    public int damageBonus = 2;
}
