using System.Collections.Generic;
using UnityEngine;
using MyGame;

/// <summary>
/// 简单敌人AI - 使用新的行为树系统
/// 注意：推荐使用更完善的 MonsterAI.cs 代替此脚本
/// </summary>
public class EnemyAI : MonoBehaviour, ICombatTarget, IMobAction
{
    [Header("战斗属性")]
    public int maxHP = 30;
    public int currentHP = 30;
    public int ac = 10;
    
    [Header("行为设置")]
    public float attackRange = 1f;  // 攻击范围（格数）
    public float moveSpeed = 2f;    // 移动速度
    public int mobility = 3;        // 移动力
    public int initiative = 10;     // 先攻值
    public float gridSize = 1f;     // 格子大小
    
    [Header("攻击数据")]
    public int hitBonus = 2;
    public int damageDiceCount = 1;
    public int damageDiceSides = 6;
    public int damageBonus = 2;

    public Player targetPlayer;     // 玩家目标

    private BehaviorNode behaviorTree;

    // ICombatTarget 实现
    public string Name => gameObject.name;
    public int CurrentAC => ac;
    public int CurrentHP => currentHP;

    void Start()
    {
        targetPlayer = FindObjectOfType<Player>();  // 获取玩家目标
        currentHP = maxHP;

        // 构建行为树
        BuildBehaviorTree();
    }

    /// <summary>
    /// 构建行为树
    /// </summary>
    private void BuildBehaviorTree()
    {
        // 攻击序列：检查范围 → 攻击
        var attackSequence = new SequenceNode(
            new CheckAttackRangeNode(),
            new CheckHasMainActionNode(),
            new AttackNode()
        );

        // 移动后攻击序列：移动 → 攻击
        var moveAndAttackSequence = new SequenceNode(
            new MoveToPlayerNode(),
            new CheckHasMainActionNode(),
            new AttackNode()
        );

        // 主选择器：优先直接攻击，否则移动后攻击
        behaviorTree = new SelectorNode(
            attackSequence,
            moveAndAttackSequence
        );
    }

    void Update()
    {
        // 回合制游戏中，AI行为由BattleManager控制
        // 这里不再自动执行
    }

    /// <summary>
    /// 执行回合（由BattleManager调用）
    /// </summary>
    public string ExecuteTurn()
    {
        if (targetPlayer == null)
        {
            targetPlayer = FindObjectOfType<Player>();
        }

        // 创建行为上下文
        BehaviorContext context = CreateContext();

        // 执行行为树
        BehaviorResult result = behaviorTree.Execute(context);

        return context.actionLog;
    }

    /// <summary>
    /// 创建行为上下文
    /// </summary>
    private BehaviorContext CreateContext()
    {
        MonsterCombatData combatData = new MonsterCombatData
        {
            maxHP = this.maxHP,
            currentHP = this.currentHP,
            mobility = this.mobility,
            attackRange = (int)this.attackRange,
            initiative = this.initiative,
            ac = this.ac,
            hitBonus = this.hitBonus,
            damageDiceCount = this.damageDiceCount,
            damageDiceSides = this.damageDiceSides,
            damageBonus = this.damageBonus
        };

        BehaviorContext context = new BehaviorContext
        {
            mobAction = this,
            mobTransform = transform,
            combatData = combatData,
            targetPlayer = targetPlayer,
            remainingMovement = mobility,
            hasMainAction = true,
            hasMinorAction = true,
            attackRange = attackRange,
            gridSize = gridSize
        };

        context.UpdateDistanceToPlayer();

        return context;
    }

    // ===== ICombatTarget 实现 =====

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{Name} 受到 {damage} 点伤害，剩余HP: {currentHP}");

        if (currentHP <= 0)
        {
            currentHP = 0;
            OnDeath();
        }
    }

    private void OnDeath()
    {
        Debug.Log($"{Name} 被击败！");
        // gameObject.SetActive(false);
    }

    // ===== IMobAction 实现 =====

    public void Move()
    {
        // 根据攻击范围，决定敌人是否接近玩家
        if (targetPlayer == null) return;
        
        if (Vector3.Distance(transform.position, targetPlayer.transform.position) > attackRange * gridSize)
        {
            // 移动到玩家位置
            Vector3 direction = (targetPlayer.transform.position - transform.position).normalized;
            direction.y = 0;
            transform.position += direction * gridSize;
        }
    }

    public float GetAttackRange()
    {
        return attackRange;
    }

    public int GetInitiative()
    {
        return initiative;
    }

    public string PerformAction(Player player)
    {
        targetPlayer = player;
        return ExecuteTurn();
    }
}
