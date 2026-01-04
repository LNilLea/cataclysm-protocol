using UnityEngine;
using MyGame;

/// <summary>
/// 怪物基类 - 提供统一的Grid移动和攻击范围逻辑
/// 所有怪物脚本应该继承此类
/// </summary>
public abstract class MonsterBase : MonoBehaviour, ICombatTarget, IMobAction
{
    [Header("基础属性")]
    public string monsterName = "怪物";
    public int maxHP = 30;
    public int currentHP = 30;
    public int AC = 10;
    public int initiative = 10;

    [Header("移动设置")]
    public int movementPoints = 3;       // 每回合移动格数
    public float moveSpeed = 5f;         // 移动动画速度（暂未使用）

    [Header("攻击范围（格数）")]
    public int attackRangeMin = 1;       // 最小攻击距离
    public int attackRangeMax = 1;       // 最大攻击距离

    // ICombatTarget 实现
    public string Name => monsterName;
    public int CurrentAC => AC;
    public int CurrentHP => currentHP;   // 【修复】添加CurrentHP属性以符合ICombatTarget接口

    // 引用
    protected Player targetPlayer;
    protected GridManager2D gridManager;

    // ===== 生命周期 =====

    protected virtual void Awake()
    {
        currentHP = maxHP;
    }

    protected virtual void Start()
    {
        targetPlayer = FindObjectOfType<Player>();
        gridManager = FindObjectOfType<GridManager2D>();

        // 将初始位置对齐到格子中心
        SnapToGrid();
    }

    // ===== ICombatTarget 实现 =====

    public virtual void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        Debug.Log($"[{GetType().Name}] {monsterName} 受到 {damage} 伤害, HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            OnDeath();
        }
    }

    protected virtual void OnDeath()
    {
        Debug.Log($"[{GetType().Name}] {monsterName} 被击败！");
    }

    public bool IsAlive() => currentHP > 0;

    // ===== IMobAction 实现 =====

    public int GetInitiative() => initiative;

    public float GetAttackRange() => attackRangeMax;

    public void Move()
    {
        if (targetPlayer == null) targetPlayer = FindObjectOfType<Player>();
        if (targetPlayer != null)
        {
            MoveTowardsPlayer(targetPlayer);
        }
    }

    /// <summary>
    /// 执行回合行动（子类必须实现）
    /// </summary>
    public abstract string PerformAction(Player player);

    // ===== Grid 移动系统 =====

    /// <summary>
    /// 将位置对齐到格子中心
    /// </summary>
    protected void SnapToGrid()
    {
        if (gridManager != null)
        {
            Vector2 snappedPos = gridManager.SnapToGrid(transform.position);
            transform.position = new Vector3(snappedPos.x, snappedPos.y, transform.position.z);
        }
    }

    /// <summary>
    /// 获取到玩家的格子距离（曼哈顿距离）
    /// </summary>
    protected int GetGridDistanceToPlayer()
    {
        if (targetPlayer == null) return int.MaxValue;

        if (gridManager != null)
        {
            return gridManager.GetGridDistance(transform.position, targetPlayer.transform.position);
        }

        // 后备方案：用世界距离估算
        float worldDist = Vector2.Distance(transform.position, targetPlayer.transform.position);
        return Mathf.RoundToInt(worldDist);
    }

    /// <summary>
    /// 检查是否在攻击范围内
    /// </summary>
    protected bool IsInAttackRange()
    {
        int distance = GetGridDistanceToPlayer();
        return distance >= attackRangeMin && distance <= attackRangeMax;
    }

    /// <summary>
    /// 向玩家移动（基于格子系统）
    /// </summary>
    protected virtual string MoveTowardsPlayer(Player player)
    {
        if (gridManager == null)
        {
            return MoveTowardsPlayerLegacy(player);
        }

        Vector2Int myGrid = gridManager.WorldToGrid(transform.position);
        Vector2Int playerGrid = gridManager.WorldToGrid(player.transform.position);

        int movedGrids = 0;
        int remainingMoves = movementPoints;

        // 逐格移动
        while (remainingMoves > 0)
        {
            int currentDistance = Mathf.Abs(playerGrid.x - myGrid.x) + Mathf.Abs(playerGrid.y - myGrid.y);

            // 如果已经在攻击范围内，停止移动
            if (currentDistance <= attackRangeMax)
            {
                break;
            }

            // 找到最佳移动方向
            Vector2Int bestMove = FindBestMove(myGrid, playerGrid);

            if (bestMove == myGrid)
            {
                // 无法继续移动
                break;
            }

            // 移动到新格子
            myGrid = bestMove;
            movedGrids++;
            remainingMoves--;
        }

        // 应用移动
        if (movedGrids > 0)
        {
            Vector2 newWorldPos = gridManager.GridToWorld(myGrid.x, myGrid.y);
            transform.position = new Vector3(newWorldPos.x, newWorldPos.y, transform.position.z);
            return $"{monsterName} 移动了 {movedGrids} 格\n";
        }

        return "";
    }

    /// <summary>
    /// 找到最佳移动格子（靠近玩家）
    /// </summary>
    protected Vector2Int FindBestMove(Vector2Int current, Vector2Int target)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // 上
            new Vector2Int(0, -1),  // 下
            new Vector2Int(1, 0),   // 右
            new Vector2Int(-1, 0)   // 左
        };

        Vector2Int bestPos = current;
        int bestDistance = int.MaxValue;

        foreach (var dir in directions)
        {
            Vector2Int next = current + dir;

            // 检查是否可行走
            if (!gridManager.IsWalkable(next.x, next.y))
                continue;

            // 检查是否被占据
            if (IsGridOccupied(next))
                continue;

            // 计算到目标的距离
            int dist = Mathf.Abs(target.x - next.x) + Mathf.Abs(target.y - next.y);
            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestPos = next;
            }
        }

        return bestPos;
    }

    /// <summary>
    /// 检查格子是否被占据
    /// </summary>
    protected bool IsGridOccupied(Vector2Int gridPos)
    {
        if (gridManager == null) return false;

        Vector2 worldPos = gridManager.GridToWorld(gridPos.x, gridPos.y);
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, gridManager.gridSize * 0.3f);

        foreach (var hit in hits)
        {
            // 跳过自己
            if (hit.gameObject == gameObject) continue;

            // 检查是否是玩家或其他怪物
            if (hit.GetComponent<Player>() != null ||
                hit.GetComponent<ICombatTarget>() != null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 旧的移动逻辑（后备方案，无GridManager时使用）
    /// </summary>
    protected string MoveTowardsPlayerLegacy(Player player)
    {
        Vector2 direction = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.transform.position);

        float maxMove = movementPoints;
        float desiredMove = Mathf.Max(0, distance - attackRangeMax + 0.5f);
        float actualMove = Mathf.Min(maxMove, desiredMove);

        if (actualMove > 0.1f)
        {
            Vector3 newPos = (Vector2)transform.position + direction * actualMove;
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
            return $"{monsterName} 移动了 {actualMove:F1} 格\n";
        }
        return "";
    }

    // ===== 工具方法 =====

    /// <summary>
    /// 骰子投掷
    /// </summary>
    protected int Roll(int diceCount, int diceSides)
    {
        int sum = 0;
        for (int i = 0; i < diceCount; i++)
        {
            sum += Random.Range(1, diceSides + 1);
        }
        return sum;
    }

    /// <summary>
    /// 执行攻击检定
    /// </summary>
    protected string DoAttackRoll(Player player, string attackName, int hitBonus, int damageDiceCount, int damageDiceSides, int damageBonus)
    {
        var pData = player.combatData;
        int d20 = Random.Range(1, 21);
        int hitValue = d20 + hitBonus;

        string log = $"{monsterName} 使用 [{attackName}]！命中: d20({d20})+{hitBonus}={hitValue} vs AC{pData.CurrentAC}";

        if (hitValue >= pData.CurrentAC)
        {
            int diceRoll = Roll(damageDiceCount, damageDiceSides);
            int damage = Mathf.Max(1, diceRoll + damageBonus);
            log += $"\n→ 命中！{damageDiceCount}d{damageDiceSides}({diceRoll})+{damageBonus} = {damage} 点伤害";
            player.TakeDamage(damage);
        }
        else
        {
            log += "\n→ 未命中";
        }

        return log;
    }
}
