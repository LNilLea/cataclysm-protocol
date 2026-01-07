using UnityEngine;
using MyGame;

/// <summary>
/// 怪物AI控制器 - 回合制版本
/// 整合行为树到 BattleManager 的回合系统
/// </summary>
public class MonsterAI : MonoBehaviour, ICombatTarget, IMobAction
{
    [Header("战斗数据")]
    public MonsterCombatData combatData = new MonsterCombatData();

    [Header("行为设置")]
    public float moveSpeed = 5f;            // 移动动画速度（视觉效果）
    public float gridSize = 1f;             // 格子大小

    [Header("引用")]
    public Player targetPlayer;
    public GridManager gridManager;

    // 行为树
    private BehaviorNode behaviorTree;

    // ICombatTarget 实现
    public string Name => gameObject.name;
    public int CurrentAC => combatData.ac;
    public int CurrentHP => combatData.currentHP;

    // 当前回合上下文
    private BehaviorContext currentContext;

    private void Start()
    {
        // 获取引用
        if (targetPlayer == null)
            targetPlayer = FindObjectOfType<Player>();

        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        if (gridManager != null)
            gridSize = gridManager.gridSize;

        // 构建行为树
        BuildBehaviorTree();
    }

    /// <summary>
    /// 构建行为树
    /// 逻辑：
    /// 1. 如果在攻击范围内 → 攻击
    /// 2. 否则 → 移动接近 → 再次检查是否能攻击
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
            attackSequence,         // 先尝试直接攻击
            moveAndAttackSequence   // 否则移动后攻击
        );
    }

    /// <summary>
    /// 执行回合（由 BattleManager 调用）
    /// </summary>
    public string ExecuteTurn(Player player)
    {
        targetPlayer = player;

        // 创建回合上下文
        currentContext = CreateContext();

        // 执行行为树
        BehaviorResult result = behaviorTree.Execute(currentContext);

        // 返回行动日志
        string log = currentContext.actionLog;
        if (string.IsNullOrEmpty(log))
        {
            log = $"{Name} 无法行动";
        }

        return log;
    }

    /// <summary>
    /// 创建行为上下文
    /// </summary>
    private BehaviorContext CreateContext()
    {
        BehaviorContext context = new BehaviorContext
        {
            mobAction = this,
            mobTransform = transform,
            combatData = combatData,
            targetPlayer = targetPlayer,
            remainingMovement = combatData.mobility,
            hasMainAction = true,
            hasMinorAction = true,
            attackRange = combatData.attackRange,
            gridSize = gridSize
        };

        context.UpdateDistanceToPlayer();

        return context;
    }

    // ===== ICombatTarget 实现 =====

    public void TakeDamage(int damage)
    {
        combatData.currentHP -= damage;
        Debug.Log($"{Name} 受到 {damage} 点伤害，剩余HP: {combatData.currentHP}");

        if (combatData.currentHP <= 0)
        {
            combatData.currentHP = 0;
            OnDeath();
        }
    }

    private void OnDeath()
    {
        Debug.Log($"{Name} 被击败！");
        // 可以在这里添加死亡效果
        // gameObject.SetActive(false);
    }

    // ===== IMobAction 实现 =====

    public void Move()
    {
        // 在回合制中，移动由行为树控制
        // 这个方法保留用于兼容旧接口
        if (currentContext != null && currentContext.remainingMovement > 0)
        {
            Vector3 direction = (targetPlayer.transform.position - transform.position).normalized;
            direction.y = 0;

            transform.position += direction * gridSize;
            currentContext.remainingMovement--;
        }
    }

    public float GetAttackRange()
    {
        return combatData.attackRange;
    }

    public int GetInitiative()
    {
        return combatData.initiative;
    }

    public string PerformAction(Player player)
    {
        // 这是旧接口，现在由 ExecuteTurn 处理
        return ExecuteTurn(player);
    }

    // ===== 公共方法 =====

    /// <summary>
    /// 治疗
    /// </summary>
    public void Heal(int amount)
    {
        combatData.currentHP = Mathf.Min(combatData.currentHP + amount, combatData.maxHP);
    }

    /// <summary>
    /// 是否存活
    /// </summary>
    public bool IsAlive()
    {
        return combatData.currentHP > 0;
    }

    /// <summary>
    /// 获取到玩家的距离（格数）
    /// </summary>
    public float GetDistanceToPlayer()
    {
        if (targetPlayer == null) return float.MaxValue;

        float worldDistance = Vector3.Distance(transform.position, targetPlayer.transform.position);
        return worldDistance / gridSize;
    }

    /// <summary>
    /// 设置战斗数据
    /// </summary>
    public void SetCombatData(MonsterCombatData data)
    {
        combatData = data;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, combatData.attackRange * gridSize);

        // 绘制移动范围
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, combatData.mobility * gridSize);
    }
#endif
}
