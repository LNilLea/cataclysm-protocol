using UnityEngine;
using System.Collections.Generic;
using MyGame;
/// <summary>
/// 2D攻击范围可视化 - 显示武器的攻击范围
/// 【改进】自动从 GridManager2D 获取格子大小和对齐
/// </summary>
public class RangeVisualizer2D : MonoBehaviour
{
    public static RangeVisualizer2D Instance { get; private set; }

    [Header("设置")]
    public int sortingOrder = 10;

    [Header("颜色")]
    public Color meleeColor = new Color(1f, 0.3f, 0.3f, 0.3f);
    public Color rangedColor = new Color(0.3f, 0.5f, 1f, 0.3f);
    public Color validTargetColor = new Color(1f, 1f, 0f, 0.5f);

    // 范围指示器
    private List<GameObject> rangeIndicators = new List<GameObject>();
    private bool isShowing = false;
    private Color currentColor;

    // 引用
    private GridManager2D gridManager;
    private float gridSize = 1f;

    // 缓存的 Sprite
    private Sprite cachedSquareSprite;

    private void Awake()
    {
        Instance = this;
        currentColor = meleeColor;
    }

    private void Start()
    {
        // 获取 GridManager2D
        gridManager = FindObjectOfType<GridManager2D>();
        if (gridManager != null)
        {
            gridSize = gridManager.gridSize;
            Debug.Log($"[RangeVisualizer2D] 已连接 GridManager2D，格子大小: {gridSize}");
        }
        else
        {
            Debug.LogWarning("[RangeVisualizer2D] 未找到 GridManager2D，使用默认格子大小 1.0");
        }

        // 预创建 Sprite
        cachedSquareSprite = CreateSquareSprite();
    }

    /// <summary>
    /// 显示攻击范围
    /// </summary>
    public void ShowRange(Vector3 center, int minRange, int maxRange)
    {
        HideRange();
        isShowing = true;

        // 确保格子大小是最新的
        if (gridManager != null)
        {
            gridSize = gridManager.gridSize;
        }

        // 将中心点对齐到格子
        Vector3 alignedCenter = center;
        if (gridManager != null)
        {
            Vector2Int centerGrid = gridManager.WorldToGrid(center);
            Vector2 snapped = gridManager.GridToWorld(centerGrid.x, centerGrid.y);
            alignedCenter = new Vector3(snapped.x, snapped.y, 0);
        }

        // 生成范围内的格子
        for (int x = -maxRange; x <= maxRange; x++)
        {
            for (int y = -maxRange; y <= maxRange; y++)
            {
                int distance = Mathf.Abs(x) + Mathf.Abs(y);  // 曼哈顿距离

                if (distance >= minRange && distance <= maxRange)
                {
                    Vector3 pos = alignedCenter + new Vector3(x * gridSize, y * gridSize, 0);

                    // 检查是否有目标
                    bool hasTarget = CheckForTarget(pos);
                    Color color = hasTarget ? validTargetColor : currentColor;

                    CreateIndicator(pos, color);
                }
            }
        }

        Debug.Log($"[RangeVisualizer2D] 显示攻击范围: {minRange}-{maxRange}格，创建了 {rangeIndicators.Count} 个指示器");
    }

    /// <summary>
    /// 隐藏攻击范围
    /// </summary>
    public void HideRange()
    {
        isShowing = false;
        foreach (var indicator in rangeIndicators)
        {
            if (indicator != null)
                Destroy(indicator);
        }
        rangeIndicators.Clear();
    }

    /// <summary>
    /// 设置范围颜色
    /// </summary>
    public void SetRangeColor(Color color)
    {
        currentColor = color;
    }

    /// <summary>
    /// 设置为近战颜色
    /// </summary>
    public void SetMeleeColor()
    {
        currentColor = meleeColor;
    }

    /// <summary>
    /// 设置为远程颜色
    /// </summary>
    public void SetRangedColor()
    {
        currentColor = rangedColor;
    }

    /// <summary>
    /// 检查位置是否有目标
    /// </summary>
    private bool CheckForTarget(Vector3 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, gridSize * 0.4f);
        foreach (var hit in hits)
        {
            // 跳过玩家
            if (hit.GetComponent<Player>() != null)
                continue;

            // 检查是否是敌人（常见怪物类型）
            if (hit.GetComponent<Beaver>() != null ||
                hit.GetComponent<Mantis>() != null ||
                hit.GetComponent<PorcupineBoss>() != null ||
                hit.GetComponent<MaleRedtailHawk>() != null ||
                hit.GetComponent<FemaleRedtailHawk>() != null ||
                hit.GetComponent<MonsterAI>() != null ||
                hit.GetComponent<MonsterBase>() != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 创建范围指示器
    /// </summary>
    private void CreateIndicator(Vector3 position, Color color)
    {
        GameObject indicator = new GameObject("RangeIndicator");
        indicator.transform.position = position;
        indicator.transform.SetParent(transform);

        // 添加 SpriteRenderer
        SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
        sr.sprite = cachedSquareSprite ?? CreateSquareSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;

        // 设置大小
        indicator.transform.localScale = new Vector3(gridSize * 0.9f, gridSize * 0.9f, 1f);

        rangeIndicators.Add(indicator);
    }

    /// <summary>
    /// 创建方形 Sprite
    /// </summary>
    private Sprite CreateSquareSprite()
    {
        // 创建一个简单的白色方形纹理
        Texture2D tex = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < 16; i++)
        {
            colors[i] = Color.white;
        }
        tex.SetPixels(colors);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }

    /// <summary>
    /// 是否正在显示
    /// </summary>
    public bool IsShowing => isShowing;
}
