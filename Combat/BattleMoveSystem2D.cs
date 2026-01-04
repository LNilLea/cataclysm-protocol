using UnityEngine;
using System.Collections.Generic;
using MyGame;

/// <summary>
/// 2D战斗移动系统 - 处理玩家在战斗中的网格移动
/// </summary>
public class BattleMoveSystem2D : MonoBehaviour
{
    public static BattleMoveSystem2D Instance { get; private set; }

    [Header("引用")]
    public Player player;
    public GridManager2D gridManager;
    public ActionPointSystem actionPointSystem;
    public BattleManager battleManager;

    [Header("移动设置")]
    public float moveSpeed = 5f;            // 移动速度
    public int baseMovePoints = 3;          // 基础移动点数

    [Header("移动范围显示")]
    public Color moveRangeColor = new Color(0f, 0.8f, 0f, 0.4f);
    public Color hoverColor = new Color(0f, 1f, 0f, 0.6f);
    public int sortingOrder = 5;

    [Header("状态（只读）")]
    public bool isMoving = false;
    public bool isSelectingMoveTarget = false;
    public int remainingMoveSquares = 0;

    // 移动范围数据
    private List<Vector2Int> validMoveGrids = new List<Vector2Int>();
    private List<GameObject> moveRangeIndicators = new List<GameObject>();
    private Vector2 moveTargetPosition;

    // 事件
    public event System.Action OnMoveStart;
    public event System.Action OnMoveComplete;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (player == null)
            player = FindObjectOfType<Player>();

        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager2D>();

        if (actionPointSystem == null)
            actionPointSystem = FindObjectOfType<ActionPointSystem>();

        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();
    }

    private void Update()
    {
        if (isMoving)
        {
            PerformMovement();
        }

        if (isSelectingMoveTarget && !isMoving)
        {
            HandleMoveTargetSelection();
        }
    }

    /// <summary>
    /// 获取玩家移动点数
    /// </summary>
    public int GetPlayerMovePoints()
    {
        if (player != null && player.combatData != null)
        {
            return player.combatData.mobility;
        }
        return baseMovePoints;
    }

    /// <summary>
    /// 开始选择移动目标
    /// </summary>
    public bool StartMoveSelection()
    {
        // 检查条件
        if (actionPointSystem == null || !actionPointSystem.CanMove())
        {
            Debug.Log("[BattleMoveSystem2D] 没有移动动作点");
            return false;
        }

        if (battleManager == null || !battleManager.IsPlayerTurn)
        {
            Debug.Log("[BattleMoveSystem2D] 不是玩家回合");
            return false;
        }

        if (isMoving)
        {
            Debug.Log("[BattleMoveSystem2D] 正在移动中");
            return false;
        }

        isSelectingMoveTarget = true;
        remainingMoveSquares = GetPlayerMovePoints();

        // 计算并显示移动范围
        CalculateMoveRange();
        ShowMoveRange();

        Debug.Log($"[BattleMoveSystem2D] 开始选择移动目标，可移动 {remainingMoveSquares} 格");
        return true;
    }

    /// <summary>
    /// 取消移动选择
    /// </summary>
    public void CancelMoveSelection()
    {
        isSelectingMoveTarget = false;
        HideMoveRange();
        Debug.Log("[BattleMoveSystem2D] 取消移动选择");
    }

    /// <summary>
    /// 计算移动范围
    /// </summary>
    private void CalculateMoveRange()
    {
        validMoveGrids.Clear();

        if (player == null || gridManager == null) return;

        validMoveGrids = gridManager.GetMovementRange(player.transform.position, remainingMoveSquares);

        Debug.Log($"[BattleMoveSystem2D] 计算移动范围完成，有效格子: {validMoveGrids.Count}");
    }

    /// <summary>
    /// 显示移动范围
    /// </summary>
    private void ShowMoveRange()
    {
        HideMoveRange();

        if (gridManager == null) return;

        foreach (var gridPos in validMoveGrids)
        {
            Vector2 worldPos = gridManager.GridToWorld(gridPos.x, gridPos.y);
            CreateMoveIndicator(worldPos);
        }
    }

    /// <summary>
    /// 创建移动指示器
    /// </summary>
    private void CreateMoveIndicator(Vector2 position)
    {
        GameObject indicator = new GameObject("MoveIndicator");
        indicator.transform.position = new Vector3(position.x, position.y, 0);

        SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = moveRangeColor;
        sr.sortingOrder = sortingOrder;

        float size = gridManager != null ? gridManager.gridSize * 0.9f : 0.9f;
        indicator.transform.localScale = new Vector3(size, size, 1f);

        // 添加点击检测
        BoxCollider2D col = indicator.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        moveRangeIndicators.Add(indicator);
    }

    /// <summary>
    /// 创建方形 Sprite
    /// </summary>
    private Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < 16; i++)
            colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }

    /// <summary>
    /// 隐藏移动范围
    /// </summary>
    private void HideMoveRange()
    {
        foreach (var indicator in moveRangeIndicators)
        {
            if (indicator != null)
                Destroy(indicator);
        }
        moveRangeIndicators.Clear();
    }

    /// <summary>
    /// 处理移动目标选择
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
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            if (gridManager == null) return;

            Vector2Int clickedGrid = gridManager.WorldToGrid(mouseWorldPos);

            // 检查是否是有效移动位置
            if (validMoveGrids.Contains(clickedGrid))
            {
                Vector2 targetPos = gridManager.GridToWorld(clickedGrid.x, clickedGrid.y);
                StartMoveTo(targetPos);
            }
            else
            {
                Debug.Log("[BattleMoveSystem2D] 无效的移动位置");
            }
        }

        // 高亮鼠标悬停的格子
        UpdateHoverHighlight();
    }

    /// <summary>
    /// 更新鼠标悬停高亮
    /// </summary>
    private void UpdateHoverHighlight()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        if (gridManager == null) return;

        Vector2Int hoverGrid = gridManager.WorldToGrid(mouseWorldPos);

        foreach (var indicator in moveRangeIndicators)
        {
            if (indicator == null) continue;

            SpriteRenderer sr = indicator.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            Vector2Int indicatorGrid = gridManager.WorldToGrid(indicator.transform.position);

            if (indicatorGrid == hoverGrid && validMoveGrids.Contains(hoverGrid))
            {
                sr.color = hoverColor;
            }
            else
            {
                sr.color = moveRangeColor;
            }
        }
    }

    /// <summary>
    /// 开始移动
    /// </summary>
    private void StartMoveTo(Vector2 targetPosition)
    {
        // 消耗移动动作
        if (actionPointSystem != null)
        {
            actionPointSystem.UseMoveAction();
        }

        moveTargetPosition = targetPosition;
        isMoving = true;
        isSelectingMoveTarget = false;

        HideMoveRange();

        OnMoveStart?.Invoke();
        Debug.Log($"[BattleMoveSystem2D] 开始移动到 {targetPosition}");
    }

    /// <summary>
    /// 执行移动
    /// </summary>
    private void PerformMovement()
    {
        if (player == null) return;

        Vector3 currentPos = player.transform.position;
        Vector3 targetPos = new Vector3(moveTargetPosition.x, moveTargetPosition.y, currentPos.z);

        player.transform.position = Vector3.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);

        // 到达目标
        if (Vector2.Distance(player.transform.position, moveTargetPosition) < 0.01f)
        {
            player.transform.position = targetPos;
            isMoving = false;

            OnMoveComplete?.Invoke();
            Debug.Log("[BattleMoveSystem2D] 移动完成");
        }
    }

    /// <summary>
    /// 获取有效移动位置（世界坐标）
    /// </summary>
    public List<Vector2> GetValidMovePositions()
    {
        List<Vector2> result = new List<Vector2>();
        if (gridManager == null) return result;

        foreach (var gridPos in validMoveGrids)
        {
            result.Add(gridManager.GridToWorld(gridPos.x, gridPos.y));
        }
        return result;
    }
}
