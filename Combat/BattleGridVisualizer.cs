using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 战斗网格可视化 - 在战斗时显示格子
/// </summary>
public class BattleGridVisualizer : MonoBehaviour
{
    public static BattleGridVisualizer Instance { get; private set; }

    [Header("引用")]
    public GridManager2D gridManager;

    [Header("显示设置")]
    public bool showGridDuringBattle = true;
    public Color gridColor = new Color(1f, 1f, 1f, 0.15f);
    public Color gridBorderColor = new Color(1f, 1f, 1f, 0.3f);
    public int sortingOrder = -1;

    [Header("格子样式")]
    public float lineWidth = 0.02f;

    // 网格线对象
    private List<GameObject> gridLines = new List<GameObject>();
    private bool isShowing = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager2D>();

        if (showGridDuringBattle)
        {
            ShowGrid();
        }
    }

    /// <summary>
    /// 显示网格
    /// </summary>
    public void ShowGrid()
    {
        if (isShowing) return;
        if (gridManager == null) return;

        isShowing = true;

        float gridSize = gridManager.gridSize;
        int width = gridManager.gridWidth;
        int height = gridManager.gridHeight;
        Vector2 origin = gridManager.gridOrigin;

        // 创建垂直线
        for (int x = 0; x <= width; x++)
        {
            float posX = origin.x + x * gridSize;
            Vector2 start = new Vector2(posX, origin.y);
            Vector2 end = new Vector2(posX, origin.y + height * gridSize);

            bool isBorder = (x == 0 || x == width);
            CreateLine(start, end, isBorder ? gridBorderColor : gridColor);
        }

        // 创建水平线
        for (int y = 0; y <= height; y++)
        {
            float posY = origin.y + y * gridSize;
            Vector2 start = new Vector2(origin.x, posY);
            Vector2 end = new Vector2(origin.x + width * gridSize, posY);

            bool isBorder = (y == 0 || y == height);
            CreateLine(start, end, isBorder ? gridBorderColor : gridColor);
        }

        Debug.Log("[BattleGridVisualizer] 网格显示");
    }

    /// <summary>
    /// 隐藏网格
    /// </summary>
    public void HideGrid()
    {
        isShowing = false;

        foreach (var line in gridLines)
        {
            if (line != null)
                Destroy(line);
        }
        gridLines.Clear();

        Debug.Log("[BattleGridVisualizer] 网格隐藏");
    }

    /// <summary>
    /// 切换网格显示
    /// </summary>
    public void ToggleGrid()
    {
        if (isShowing)
            HideGrid();
        else
            ShowGrid();
    }

    /// <summary>
    /// 创建线条
    /// </summary>
    private void CreateLine(Vector2 start, Vector2 end, Color color)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.SetParent(transform);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(start.x, start.y, 0));
        lr.SetPosition(1, new Vector3(end.x, end.y, 0));

        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        // 使用默认材质
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;

        lr.sortingOrder = sortingOrder;

        gridLines.Add(lineObj);
    }

    /// <summary>
    /// 刷新网格（当网格设置改变时调用）
    /// </summary>
    public void RefreshGrid()
    {
        if (isShowing)
        {
            HideGrid();
            ShowGrid();
        }
    }
}
