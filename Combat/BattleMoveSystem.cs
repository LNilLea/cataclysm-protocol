using UnityEngine;
using System.Collections.Generic;
using MyGame;

/// <summary>
/// 战斗移动系统 - 处理玩家在战斗中的网格移动
/// </summary>
public class BattleMoveSystem : MonoBehaviour
{
    public static BattleMoveSystem Instance { get; private set; }

    [Header("引用")]
    public Player player;
    public GridManager gridManager;
    public ActionPointSystem actionPointSystem;
    public BattleManager battleManager;

    [Header("移动设置")]
    public float moveSpeed = 5f;                    // 移动速度
    public PlayerCombatData.BattleFieldSize battleFieldSize = PlayerCombatData.BattleFieldSize.Small;

    [Header("移动范围显示")]
    public GameObject moveRangeIndicatorPrefab;     // 移动范围指示器预制体
    public Color validMoveColor = new Color(0, 1, 0, 0.3f);     // 可移动格子颜色
    public Color invalidMoveColor = new Color(1, 0, 0, 0.3f);   // 不可移动格子颜色

    [Header("可视化系统")]
    public RangeVisualizer rangeVisualizer;         // 范围可视化系统（可选）

    [Header("状态")]
    public bool isMoving = false;                   // 是否正在移动
    public bool isSelectingMoveTarget = false;      // 是否正在选择移动目标
    public int remainingMoveSquares = 0;            // 本次移动剩余格数

    // 当前可移动的格子列表
    private List<Vector3> validMovePositions = new List<Vector3>();
    // 移动范围指示器对象
    private List<GameObject> moveRangeIndicators = new List<GameObject>();
    // 移动目标位置
    private Vector3 moveTargetPosition;

    // 事件
    public event System.Action OnMoveStart;
    public event System.Action OnMoveComplete;
    public event System.Action OnMoveRangeChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 自动获取引用
        if (player == null)
            player = FindObjectOfType<Player>();

        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        if (actionPointSystem == null)
            actionPointSystem = FindObjectOfType<ActionPointSystem>();

        if (rangeVisualizer == null)
            rangeVisualizer = FindObjectOfType<RangeVisualizer>();

        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();
    }

    private void Update()
    {
        // 正在移动时，执行移动
        if (isMoving)
        {
            PerformMovement();
        }

        // 选择移动目标时，检测鼠标点击
        if (isSelectingMoveTarget && !isMoving)
        {
            HandleMoveTargetSelection();
        }
    }

    /// <summary>
    /// 获取玩家的移动格数
    /// </summary>
    public int GetPlayerMoveSquares()
    {
        if (player == null || player.combatData == null) return 0;

        int mobility = player.combatData.mobility;

        switch (battleFieldSize)
        {
            case PlayerCombatData.BattleFieldSize.Small:
                return mobility * 1;
            case PlayerCombatData.BattleFieldSize.Medium:
                return mobility * 3;
            case PlayerCombatData.BattleFieldSize.Large:
                return mobility * 5;
            default:
                return mobility;
        }
    }

    /// <summary>
    /// 开始选择移动目标
    /// </summary>
    public bool StartMoveSelection()
    {
        // 检查是否可以移动
        if (actionPointSystem == null || !actionPointSystem.CanMove())
        {
            Debug.Log("无法移动：没有移动动作点");
            return false;
        }

        if (battleManager == null || !battleManager.IsPlayerTurn)
        {
            Debug.Log("无法移动：不是玩家回合");
            return false;
        }

        if (isMoving)
        {
            Debug.Log("无法移动：正在移动中");
            return false;
        }

        isSelectingMoveTarget = true;
        remainingMoveSquares = GetPlayerMoveSquares();

        // 计算并显示移动范围
        CalculateMoveRange();
        ShowMoveRange();

        Debug.Log($"开始选择移动目标，可移动 {remainingMoveSquares} 格");
        return true;
    }

    /// <summary>
    /// 取消移动选择
    /// </summary>
    public void CancelMoveSelection()
    {
        isSelectingMoveTarget = false;
        HideMoveRange();
        Debug.Log("取消移动选择");
    }

    /// <summary>
    /// 计算移动范围
    /// </summary>
    private void CalculateMoveRange()
    {
        validMovePositions.Clear();

        if (player == null || gridManager == null) return;

        Vector3 playerPos = player.transform.position;
        float gridSize = gridManager.gridSize;
        int moveRange = remainingMoveSquares;

        // 获取玩家当前所在的格子坐标
        int playerGridX = Mathf.RoundToInt(playerPos.x / gridSize);
        int playerGridZ = Mathf.RoundToInt(playerPos.z / gridSize);

        // 使用 BFS 计算所有可达格子
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> visited = new Dictionary<Vector2Int, int>();

        Vector2Int startPos = new Vector2Int(playerGridX, playerGridZ);
        queue.Enqueue(startPos);
        visited[startPos] = 0;

        // 四方向移动
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // 上
            new Vector2Int(0, -1),  // 下
            new Vector2Int(1, 0),   // 右
            new Vector2Int(-1, 0)   // 左
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentDistance = visited[current];

            if (currentDistance >= moveRange) continue;

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;

                // 检查是否在网格范围内
                if (next.x < 0 || next.x >= gridManager.gridWidth ||
                    next.y < 0 || next.y >= gridManager.gridHeight)
                    continue;

                // 检查是否已访问
                if (visited.ContainsKey(next)) continue;

                // 检查是否有障碍物（可以扩展）
                Vector3 worldPos = new Vector3(next.x * gridSize, 0, next.y * gridSize);
                if (!IsPositionWalkable(worldPos)) continue;

                visited[next] = currentDistance + 1;
                queue.Enqueue(next);

                // 添加到有效移动位置（排除起始位置）
                if (next != startPos)
                {
                    validMovePositions.Add(worldPos);
                }
            }
        }

        OnMoveRangeChanged?.Invoke();
        Debug.Log($"计算移动范围完成，有效格子数: {validMovePositions.Count}");
    }

    /// <summary>
    /// 检查位置是否可行走
    /// </summary>
    private bool IsPositionWalkable(Vector3 position)
    {
        // 检查是否有障碍物
        Collider[] colliders = Physics.OverlapSphere(position + Vector3.up * 0.5f, 0.3f);
        foreach (var col in colliders)
        {
            // 跳过玩家自己
            if (col.GetComponent<Player>() != null) continue;

            // 检查是否是障碍物
            if (col.CompareTag("Obstacle") || col.CompareTag("Wall"))
            {
                return false;
            }

            // 检查是否是怪物（不能穿过怪物）
            if (col.GetComponent<ICombatTarget>() != null)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 显示移动范围
    /// </summary>
    private void ShowMoveRange()
    {
        HideMoveRange(); // 先清除旧的

        // 优先使用 RangeVisualizer
        if (rangeVisualizer != null)
        {
            rangeVisualizer.ShowMoveRange(player.transform.position, remainingMoveSquares);
            return;
        }

        // 后备方案：使用自己的显示逻辑
        if (moveRangeIndicatorPrefab == null)
        {
            // 如果没有预制体，使用简单的方式创建指示器
            foreach (var pos in validMovePositions)
            {
                GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
                indicator.name = "MoveRangeIndicator";
                indicator.transform.position = pos + Vector3.up * 0.01f;
                indicator.transform.rotation = Quaternion.Euler(90, 0, 0);
                indicator.transform.localScale = new Vector3(gridManager.gridSize * 0.9f, gridManager.gridSize * 0.9f, 1);

                // 设置材质
                Renderer renderer = indicator.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = validMoveColor;

                // 移除碰撞体
                Destroy(indicator.GetComponent<Collider>());

                moveRangeIndicators.Add(indicator);
            }
        }
        else
        {
            // 使用预制体
            foreach (var pos in validMovePositions)
            {
                GameObject indicator = Instantiate(moveRangeIndicatorPrefab, pos, Quaternion.identity);
                moveRangeIndicators.Add(indicator);
            }
        }
    }

    /// <summary>
    /// 隐藏移动范围
    /// </summary>
    private void HideMoveRange()
    {
        // 使用 RangeVisualizer
        if (rangeVisualizer != null)
        {
            rangeVisualizer.HideMoveRange();
        }

        // 清除自己的指示器
        foreach (var indicator in moveRangeIndicators)
        {
            Destroy(indicator);
        }
        moveRangeIndicators.Clear();
    }

    /// <summary>
    /// 处理移动目标选择（鼠标点击）
    /// </summary>
    private void HandleMoveTargetSelection()
    {
        // 右键取消
        if (Input.GetMouseButtonDown(1))
        {
            CancelMoveSelection();
            return;
        }

        // 左键选择
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 clickPos = hit.point;

                // 将点击位置转换为网格位置
                float gridSize = gridManager != null ? gridManager.gridSize : 1f;
                int gridX = Mathf.RoundToInt(clickPos.x / gridSize);
                int gridZ = Mathf.RoundToInt(clickPos.z / gridSize);
                Vector3 gridPosition = new Vector3(gridX * gridSize, 0, gridZ * gridSize);

                // 检查是否是有效移动位置
                if (IsValidMovePosition(gridPosition))
                {
                    // 开始移动
                    StartMoveTo(gridPosition);
                }
                else
                {
                    Debug.Log("无效的移动位置");
                }
            }
        }
    }

    /// <summary>
    /// 检查是否是有效的移动位置
    /// </summary>
    private bool IsValidMovePosition(Vector3 position)
    {
        foreach (var validPos in validMovePositions)
        {
            if (Vector3.Distance(validPos, position) < 0.1f)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 开始移动到目标位置
    /// </summary>
    private void StartMoveTo(Vector3 targetPosition)
    {
        // 消耗移动动作
        if (actionPointSystem != null)
        {
            actionPointSystem.UseMoveAction();
        }

        moveTargetPosition = targetPosition;
        isMoving = true;
        isSelectingMoveTarget = false;

        // 隐藏移动范围
        HideMoveRange();

        OnMoveStart?.Invoke();
        Debug.Log($"开始移动到 {targetPosition}");
    }

    /// <summary>
    /// 执行移动（每帧调用）
    /// </summary>
    private void PerformMovement()
    {
        if (player == null) return;

        // 保持 Y 坐标不变
        Vector3 targetPos = new Vector3(moveTargetPosition.x, player.transform.position.y, moveTargetPosition.z);

        // 移动玩家
        player.transform.position = Vector3.MoveTowards(
            player.transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        // 检查是否到达目标
        if (Vector3.Distance(player.transform.position, targetPos) < 0.01f)
        {
            player.transform.position = targetPos;
            isMoving = false;

            OnMoveComplete?.Invoke();
            Debug.Log("移动完成");
        }
    }

    /// <summary>
    /// 获取有效移动位置列表
    /// </summary>
    public List<Vector3> GetValidMovePositions()
    {
        return new List<Vector3>(validMovePositions);
    }

    /// <summary>
    /// 设置战场大小
    /// </summary>
    public void SetBattleFieldSize(PlayerCombatData.BattleFieldSize size)
    {
        battleFieldSize = size;
        Debug.Log($"战场大小设置为: {size}");
    }

    /// <summary>
    /// 计算两点之间的格子距离
    /// </summary>
    public int GetGridDistance(Vector3 from, Vector3 to)
    {
        float gridSize = gridManager != null ? gridManager.gridSize : 1f;

        int fromX = Mathf.RoundToInt(from.x / gridSize);
        int fromZ = Mathf.RoundToInt(from.z / gridSize);
        int toX = Mathf.RoundToInt(to.x / gridSize);
        int toZ = Mathf.RoundToInt(to.z / gridSize);

        // 曼哈顿距离
        return Mathf.Abs(toX - fromX) + Mathf.Abs(toZ - fromZ);
    }
}
