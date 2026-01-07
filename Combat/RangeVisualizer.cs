using UnityEngine;
using System.Collections.Generic;
using MyGame;

/// <summary>
/// 范围可视化系统 - 统一管理移动范围和攻击范围的显示
/// 【修改】使用 GridManager2D 和 BattleMoveSystem2D
/// </summary>
public class RangeVisualizer : MonoBehaviour
{
    public static RangeVisualizer Instance { get; private set; }

    [Header("引用")]
    public Player player;
    public GridManager2D gridManager;              // 【修改】改为 GridManager2D
    public BattleMoveSystem2D moveSystem;          // 【修改】改为 BattleMoveSystem2D
    public TargetSelector targetSelector;

    [Header("移动范围设置")]
    public GameObject moveRangePrefab;          // 移动范围指示器预制体
    public Color moveRangeColor = new Color(0, 0.8f, 0, 0.4f);      // 绿色
    public Color moveRangeBorderColor = new Color(0, 1f, 0, 0.8f);

    [Header("攻击范围设置")]
    public GameObject attackRangePrefab;        // 攻击范围指示器预制体
    public Color attackRangeColor = new Color(0.8f, 0, 0, 0.4f);    // 红色
    public Color attackRangeBorderColor = new Color(1f, 0, 0, 0.8f);
    public Color attackRangeValidColor = new Color(1f, 0.5f, 0, 0.6f); // 有效目标橙色

    [Header("目标高亮设置")]
    public GameObject targetHighlightPrefab;    // 目标高亮预制体
    public Color targetHighlightColor = new Color(1f, 1f, 0, 0.6f); // 黄色

    [Header("格子设置")]
    public float gridSize = 1f;
    public float indicatorHeight = 0.02f;       // 指示器离地高度

    // 指示器对象池
    private List<GameObject> moveRangeIndicators = new List<GameObject>();
    private List<GameObject> attackRangeIndicators = new List<GameObject>();
    private List<GameObject> targetHighlights = new List<GameObject>();

    // 当前显示状态
    private bool showingMoveRange = false;
    private bool showingAttackRange = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 自动获取引用
        if (player == null)
            player = FindObjectOfType<Player>();

        // 【修改】查找 GridManager2D
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager2D>();

        // 【修改】查找 BattleMoveSystem2D
        if (moveSystem == null)
            moveSystem = FindObjectOfType<BattleMoveSystem2D>();

        if (targetSelector == null)
            targetSelector = FindObjectOfType<TargetSelector>();

        // 【修改】从 GridManager2D 获取格子大小
        if (gridManager != null)
            gridSize = gridManager.gridSize;
    }

    // ===== 移动范围可视化 =====

    /// <summary>
    /// 显示移动范围
    /// </summary>
    public void ShowMoveRange(Vector3 centerPosition, int moveDistance)
    {
        HideMoveRange();
        showingMoveRange = true;

        List<Vector3> validPositions = CalculateMoveRange(centerPosition, moveDistance);

        foreach (var pos in validPositions)
        {
            GameObject indicator = CreateIndicator(pos, moveRangeColor, moveRangeBorderColor, moveRangePrefab);
            moveRangeIndicators.Add(indicator);
        }
    }

    /// <summary>
    /// 显示玩家移动范围
    /// </summary>
    public void ShowPlayerMoveRange()
    {
        if (player == null || moveSystem == null) return;

        int moveDistance = moveSystem.GetPlayerMovePoints();
        ShowMoveRange(player.transform.position, moveDistance);
    }

    /// <summary>
    /// 隐藏移动范围
    /// </summary>
    public void HideMoveRange()
    {
        showingMoveRange = false;
        foreach (var indicator in moveRangeIndicators)
        {
            if (indicator != null)
                Destroy(indicator);
        }
        moveRangeIndicators.Clear();
    }

    /// <summary>
    /// 计算移动范围（BFS）
    /// </summary>
    private List<Vector3> CalculateMoveRange(Vector3 center, int range)
    {
        List<Vector3> result = new List<Vector3>();

        int centerX = Mathf.RoundToInt(center.x / gridSize);
        int centerZ = Mathf.RoundToInt(center.z / gridSize);

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> visited = new Dictionary<Vector2Int, int>();

        Vector2Int startPos = new Vector2Int(centerX, centerZ);
        queue.Enqueue(startPos);
        visited[startPos] = 0;

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentDistance = visited[current];

            if (currentDistance >= range) continue;

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;

                if (visited.ContainsKey(next)) continue;

                Vector3 worldPos = new Vector3(next.x * gridSize, 0, next.y * gridSize);

                // 检查是否可行走
                if (!IsPositionWalkable(worldPos)) continue;

                visited[next] = currentDistance + 1;
                queue.Enqueue(next);

                // 添加到结果（排除起始位置）
                if (next != startPos)
                {
                    result.Add(worldPos);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 检查位置是否可行走
    /// </summary>
    private bool IsPositionWalkable(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position + Vector3.up * 0.5f, 0.3f);
        foreach (var col in colliders)
        {
            if (col.GetComponent<Player>() != null) continue;

            if (col.CompareTag("Obstacle") || col.CompareTag("Wall"))
                return false;

            // 不能穿过怪物
            if (col.GetComponent<MonsterAI>() != null)
                return false;
        }
        return true;
    }

    // ===== 攻击范围可视化 =====

    /// <summary>
    /// 显示攻击范围
    /// </summary>
    public void ShowAttackRange(Vector3 centerPosition, int minRange, int maxRange)
    {
        HideAttackRange();
        showingAttackRange = true;

        List<Vector3> rangePositions = CalculateAttackRange(centerPosition, minRange, maxRange);

        foreach (var pos in rangePositions)
        {
            // 检查该位置是否有有效目标
            bool hasTarget = HasTargetAtPosition(pos);
            Color color = hasTarget ? attackRangeValidColor : attackRangeColor;
            Color borderColor = hasTarget ? Color.yellow : attackRangeBorderColor;

            GameObject indicator = CreateIndicator(pos, color, borderColor, attackRangePrefab);
            attackRangeIndicators.Add(indicator);
        }

        // 高亮所有有效目标
        HighlightValidTargets(centerPosition, minRange, maxRange);
    }

    /// <summary>
    /// 显示武器攻击范围
    /// </summary>
    public void ShowWeaponAttackRange(Weapon weapon)
    {
        if (player == null || weapon == null) return;

        ShowAttackRange(player.transform.position, weapon.AttackRangeMin, weapon.AttackRangeMax);
    }

    /// <summary>
    /// 隐藏攻击范围
    /// </summary>
    public void HideAttackRange()
    {
        showingAttackRange = false;
        foreach (var indicator in attackRangeIndicators)
        {
            if (indicator != null)
                Destroy(indicator);
        }
        attackRangeIndicators.Clear();

        HideTargetHighlights();
    }

    /// <summary>
    /// 计算攻击范围（环形）
    /// </summary>
    private List<Vector3> CalculateAttackRange(Vector3 center, int minRange, int maxRange)
    {
        List<Vector3> result = new List<Vector3>();

        int centerX = Mathf.RoundToInt(center.x / gridSize);
        int centerZ = Mathf.RoundToInt(center.z / gridSize);

        // 遍历范围内的所有格子
        for (int x = -maxRange; x <= maxRange; x++)
        {
            for (int z = -maxRange; z <= maxRange; z++)
            {
                int distance = Mathf.Abs(x) + Mathf.Abs(z); // 曼哈顿距离

                if (distance >= minRange && distance <= maxRange)
                {
                    Vector3 worldPos = new Vector3(
                        (centerX + x) * gridSize,
                        0,
                        (centerZ + z) * gridSize
                    );
                    result.Add(worldPos);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 检查位置是否有目标
    /// </summary>
    private bool HasTargetAtPosition(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position + Vector3.up * 0.5f, 0.4f);
        foreach (var col in colliders)
        {
            if (col.GetComponent<ICombatTarget>() != null)
            {
                // 排除玩家自己
                if (col.GetComponent<Player>() == null)
                    return true;
            }
        }
        return false;
    }

    // ===== 目标高亮 =====

    /// <summary>
    /// 高亮所有有效目标
    /// </summary>
    private void HighlightValidTargets(Vector3 center, int minRange, int maxRange)
    {
        HideTargetHighlights();

        // 查找所有怪物
        MonsterAI[] monsters = FindObjectsOfType<MonsterAI>();

        foreach (var monster in monsters)
        {
            if (!monster.IsAlive()) continue;

            float distance = Vector3.Distance(center, monster.transform.position);
            int gridDistance = Mathf.RoundToInt(distance / gridSize);

            if (gridDistance >= minRange && gridDistance <= maxRange)
            {
                GameObject highlight = CreateTargetHighlight(monster.transform.position);
                targetHighlights.Add(highlight);
            }
        }
    }

    /// <summary>
    /// 隐藏目标高亮
    /// </summary>
    private void HideTargetHighlights()
    {
        foreach (var highlight in targetHighlights)
        {
            if (highlight != null)
                Destroy(highlight);
        }
        targetHighlights.Clear();
    }

    /// <summary>
    /// 创建目标高亮
    /// </summary>
    private GameObject CreateTargetHighlight(Vector3 position)
    {
        if (targetHighlightPrefab != null)
        {
            return Instantiate(targetHighlightPrefab, position + Vector3.up * 0.1f, Quaternion.identity);
        }

        // 默认：创建一个环形高亮
        GameObject highlight = new GameObject("TargetHighlight");
        highlight.transform.position = position + Vector3.up * 0.1f;

        // 创建环形（用 LineRenderer）
        LineRenderer line = highlight.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.positionCount = 32;

        // 创建材质
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = targetHighlightColor;
        line.endColor = targetHighlightColor;

        // 绘制圆形
        float radius = gridSize * 0.6f;
        for (int i = 0; i < 32; i++)
        {
            float angle = i * Mathf.PI * 2f / 32;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            line.SetPosition(i, new Vector3(x, 0, z));
        }

        // 添加动画效果
        highlight.AddComponent<TargetHighlightAnimation>();

        return highlight;
    }

    // ===== 指示器创建 =====

    /// <summary>
    /// 创建指示器
    /// </summary>
    private GameObject CreateIndicator(Vector3 position, Color fillColor, Color borderColor, GameObject prefab)
    {
        if (prefab != null)
        {
            GameObject indicator = Instantiate(prefab, position + Vector3.up * indicatorHeight, Quaternion.identity);
            
            // 尝试设置颜色
            Renderer renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = fillColor;
            }

            return indicator;
        }

        // 默认：创建方形指示器
        return CreateDefaultIndicator(position, fillColor, borderColor);
    }

    /// <summary>
    /// 创建默认指示器（方形）
    /// </summary>
    private GameObject CreateDefaultIndicator(Vector3 position, Color fillColor, Color borderColor)
    {
        GameObject indicator = new GameObject("RangeIndicator");
        indicator.transform.position = position + Vector3.up * indicatorHeight;

        // 创建填充面
        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fill.name = "Fill";
        fill.transform.SetParent(indicator.transform);
        fill.transform.localPosition = Vector3.zero;
        fill.transform.localRotation = Quaternion.Euler(90, 0, 0);
        fill.transform.localScale = new Vector3(gridSize * 0.9f, gridSize * 0.9f, 1);

        Renderer fillRenderer = fill.GetComponent<Renderer>();
        fillRenderer.material = new Material(Shader.Find("Sprites/Default"));
        fillRenderer.material.color = fillColor;

        // 移除碰撞体
        Destroy(fill.GetComponent<Collider>());

        // 创建边框
        GameObject border = new GameObject("Border");
        border.transform.SetParent(indicator.transform);
        border.transform.localPosition = Vector3.up * 0.01f;

        LineRenderer line = border.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.positionCount = 4;

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = borderColor;
        line.endColor = borderColor;

        float half = gridSize * 0.45f;
        line.SetPosition(0, new Vector3(-half, 0, -half));
        line.SetPosition(1, new Vector3(-half, 0, half));
        line.SetPosition(2, new Vector3(half, 0, half));
        line.SetPosition(3, new Vector3(half, 0, -half));

        return indicator;
    }

    // ===== 公共方法 =====

    /// <summary>
    /// 隐藏所有范围显示
    /// </summary>
    public void HideAll()
    {
        HideMoveRange();
        HideAttackRange();
    }

    /// <summary>
    /// 是否正在显示移动范围
    /// </summary>
    public bool IsShowingMoveRange => showingMoveRange;

    /// <summary>
    /// 是否正在显示攻击范围
    /// </summary>
    public bool IsShowingAttackRange => showingAttackRange;
}

/// <summary>
/// 目标高亮动画
/// </summary>
public class TargetHighlightAnimation : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float minScale = 0.8f;
    public float maxScale = 1.2f;

    private float time = 0f;

    private void Update()
    {
        time += Time.deltaTime * pulseSpeed;
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(time) + 1f) / 2f);
        transform.localScale = new Vector3(scale, 1, scale);

        // 旋转效果
        transform.Rotate(0, 30 * Time.deltaTime, 0);
    }
}
