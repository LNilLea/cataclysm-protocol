using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int gridWidth = 5;  // 网格宽度
    public int gridHeight = 5;  // 网格高度
    public float gridSize = 1.0f;  // 每个格子的大小

    private Vector3[,] grid;

    void Start()
    {
        CreateGrid();
    }

    // 创建网格
    void CreateGrid()
    {
        grid = new Vector3[gridWidth, gridHeight];

        // 初始化网格的每个格子
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = new Vector3(x * gridSize, 0, y * gridSize);
            }
        }
    }

    // 获取某个格子的世界位置
    public Vector3 GetGridWorldPosition(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return grid[x, y];
        }
        return Vector3.zero;  // 返回空值
    }

    // 计算角色的移动范围（例如：角色行动力为3，能够移动3格以内）
    public List<Vector3> GetMovementRange(Vector3 startPosition, int movementPoints)
    {
        List<Vector3> validPositions = new List<Vector3>();
        int startX = Mathf.FloorToInt(startPosition.x / gridSize);
        int startY = Mathf.FloorToInt(startPosition.z / gridSize);

        // 使用 BFS（广度优先搜索）来计算移动范围
        Queue<Vector3> queue = new Queue<Vector3>();
        queue.Enqueue(startPosition);
        HashSet<Vector3> visited = new HashSet<Vector3>();
        visited.Add(startPosition);

        int currentMovement = 0;
        while (queue.Count > 0 && currentMovement < movementPoints)
        {
            int count = queue.Count;
            for (int i = 0; i < count; i++)
            {
                Vector3 currentPosition = queue.Dequeue();

                // 获取周围相邻的格子
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;  // 排除当前格子

                        int newX = startX + dx;
                        int newY = startY + dy;

                        if (newX >= 0 && newX < gridWidth && newY >= 0 && newY < gridHeight)
                        {
                            Vector3 newPosition = grid[newX, newY];
                            if (!visited.Contains(newPosition))
                            {
                                visited.Add(newPosition);
                                queue.Enqueue(newPosition);
                                validPositions.Add(newPosition);
                            }
                        }
                    }
                }
            }
            currentMovement++;
        }

        return validPositions;
    }
}
