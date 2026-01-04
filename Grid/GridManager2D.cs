using UnityEngine;
using System.Collections.Generic;
using MyGame;
/// <summary>
/// 2D网格管理器 - 管理战斗场景的格子系统
/// </summary>
public class GridManager2D : MonoBehaviour
{
    public static GridManager2D Instance { get; private set; }

    [Header("网格设置")]
    public int gridWidth = 10;              // 网格宽度（格数）
    public int gridHeight = 10;             // 网格高度（格数）
    public float gridSize = 1.0f;           // 每个格子的大小（单位）
    public Vector2 gridOrigin = Vector2.zero; // 网格原点位置

    [Header("可视化")]
    public bool showGrid = true;            // 是否显示网格线
    public Color gridLineColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    public Color gridBorderColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

    [Header("障碍物层")]
    public LayerMask obstacleLayer;         // 障碍物层

    // 网格数据
    private bool[,] walkableGrid;           // 可行走的格子

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeGrid();
    }

    /// <summary>
    /// 初始化网格
    /// </summary>
    public void InitializeGrid()
    {
        walkableGrid = new bool[gridWidth, gridHeight];

        // 默认所有格子可行走
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                walkableGrid[x, y] = true;
            }
        }

        // 检测障碍物
        UpdateObstacles();

        Debug.Log($"[GridManager2D] 网格初始化完成: {gridWidth}x{gridHeight}，格子大小: {gridSize}");
    }

    /// <summary>
    /// 更新障碍物
    /// </summary>
    public void UpdateObstacles()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 worldPos = GridToWorld(x, y);
                
                // 检测该位置是否有障碍物
                Collider2D hit = Physics2D.OverlapCircle(worldPos, gridSize * 0.4f, obstacleLayer);
                walkableGrid[x, y] = (hit == null);
            }
        }
    }

    /// <summary>
    /// 网格坐标转世界坐标
    /// </summary>
    public Vector2 GridToWorld(int gridX, int gridY)
    {
        float worldX = gridOrigin.x + gridX * gridSize + gridSize / 2f;
        float worldY = gridOrigin.y + gridY * gridSize + gridSize / 2f;
        return new Vector2(worldX, worldY);
    }

    /// <summary>
    /// 世界坐标转网格坐标
    /// </summary>
    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        int gridX = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / gridSize);
        int gridY = Mathf.FloorToInt((worldPos.y - gridOrigin.y) / gridSize);
        return new Vector2Int(gridX, gridY);
    }

    /// <summary>
    /// 将世界坐标对齐到网格中心
    /// </summary>
    public Vector2 SnapToGrid(Vector2 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);
        return GridToWorld(gridPos.x, gridPos.y);
    }

    /// <summary>
    /// 检查网格坐标是否在范围内
    /// </summary>
    public bool IsInBounds(int gridX, int gridY)
    {
        return gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight;
    }

    /// <summary>
    /// 检查网格坐标是否可行走
    /// </summary>
    public bool IsWalkable(int gridX, int gridY)
    {
        if (!IsInBounds(gridX, gridY)) return false;
        return walkableGrid[gridX, gridY];
    }

    /// <summary>
    /// 检查世界坐标是否可行走
    /// </summary>
    public bool IsWalkable(Vector2 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);
        return IsWalkable(gridPos.x, gridPos.y);
    }

    /// <summary>
    /// 设置格子是否可行走
    /// </summary>
    public void SetWalkable(int gridX, int gridY, bool walkable)
    {
        if (IsInBounds(gridX, gridY))
        {
            walkableGrid[gridX, gridY] = walkable;
        }
    }

    /// <summary>
    /// 获取移动范围内的所有可行走格子（BFS）
    /// </summary>
    public List<Vector2Int> GetMovementRange(Vector2 startWorldPos, int movePoints)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        Vector2Int startGrid = WorldToGrid(startWorldPos);

        if (!IsInBounds(startGrid.x, startGrid.y)) return result;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> visited = new Dictionary<Vector2Int, int>();

        queue.Enqueue(startGrid);
        visited[startGrid] = 0;

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
            int currentDist = visited[current];

            if (currentDist >= movePoints) continue;

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;

                // 检查边界
                if (!IsInBounds(next.x, next.y)) continue;

                // 检查是否已访问
                if (visited.ContainsKey(next)) continue;

                // 检查是否可行走
                if (!IsWalkable(next.x, next.y)) continue;

                // 检查是否有其他单位占据
                if (IsOccupied(next)) continue;

                visited[next] = currentDist + 1;
                queue.Enqueue(next);

                // 添加到结果（不包括起始位置）
                if (next != startGrid)
                {
                    result.Add(next);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 检查格子是否被占据（有单位）
    /// </summary>
    private bool IsOccupied(Vector2Int gridPos)
    {
        Vector2 worldPos = GridToWorld(gridPos.x, gridPos.y);
        
        // 检测玩家
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, gridSize * 0.3f);
        foreach (var hit in hits)
        {
            if (hit.GetComponent<Player>() != null) return true;
            if (hit.GetComponent<Beaver>() != null) return true;
            if (hit.GetComponent<Mantis>() != null) return true;
            if (hit.GetComponent<MonsterAI>() != null) return true;
        }

        return false;
    }

    /// <summary>
    /// 计算两点间的格子距离（曼哈顿距离）
    /// </summary>
    public int GetGridDistance(Vector2 from, Vector2 to)
    {
        Vector2Int fromGrid = WorldToGrid(from);
        Vector2Int toGrid = WorldToGrid(to);

        return Mathf.Abs(toGrid.x - fromGrid.x) + Mathf.Abs(toGrid.y - fromGrid.y);
    }

    /// <summary>
    /// 获取攻击范围内的所有格子
    /// </summary>
    public List<Vector2Int> GetAttackRange(Vector2 centerWorldPos, int minRange, int maxRange)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        Vector2Int centerGrid = WorldToGrid(centerWorldPos);

        for (int x = -maxRange; x <= maxRange; x++)
        {
            for (int y = -maxRange; y <= maxRange; y++)
            {
                int distance = Mathf.Abs(x) + Mathf.Abs(y);

                if (distance >= minRange && distance <= maxRange)
                {
                    int gridX = centerGrid.x + x;
                    int gridY = centerGrid.y + y;

                    if (IsInBounds(gridX, gridY))
                    {
                        result.Add(new Vector2Int(gridX, gridY));
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 在 Scene 视图中绘制网格
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGrid) return;

        // 绘制网格线
        Gizmos.color = gridLineColor;

        // 垂直线
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = new Vector3(gridOrigin.x + x * gridSize, gridOrigin.y, 0);
            Vector3 end = new Vector3(gridOrigin.x + x * gridSize, gridOrigin.y + gridHeight * gridSize, 0);
            Gizmos.DrawLine(start, end);
        }

        // 水平线
        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 start = new Vector3(gridOrigin.x, gridOrigin.y + y * gridSize, 0);
            Vector3 end = new Vector3(gridOrigin.x + gridWidth * gridSize, gridOrigin.y + y * gridSize, 0);
            Gizmos.DrawLine(start, end);
        }

        // 绘制边框
        Gizmos.color = gridBorderColor;
        Vector3 bottomLeft = new Vector3(gridOrigin.x, gridOrigin.y, 0);
        Vector3 bottomRight = new Vector3(gridOrigin.x + gridWidth * gridSize, gridOrigin.y, 0);
        Vector3 topLeft = new Vector3(gridOrigin.x, gridOrigin.y + gridHeight * gridSize, 0);
        Vector3 topRight = new Vector3(gridOrigin.x + gridWidth * gridSize, gridOrigin.y + gridHeight * gridSize, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);

        // 绘制不可行走的格子
        if (walkableGrid != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (!walkableGrid[x, y])
                    {
                        Vector2 worldPos = GridToWorld(x, y);
                        Vector3 center = new Vector3(worldPos.x, worldPos.y, 0);
                        Gizmos.DrawCube(center, new Vector3(gridSize * 0.8f, gridSize * 0.8f, 0.1f));
                    }
                }
            }
        }
    }
}
