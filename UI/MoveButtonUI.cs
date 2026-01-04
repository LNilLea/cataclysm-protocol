using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 移动按钮UI - 控制移动功能
/// </summary>
public class MoveButtonUI : MonoBehaviour
{
    [Header("引用")]
    public BattleMoveSystem2D moveSystem;
    public ActionPointSystem actionPointSystem;
    public BattleManager battleManager;

    [Header("UI")]
    public Button moveButton;
    public TMP_Text moveButtonText;
    public TMP_Text movePointsText;         // 显示剩余移动点

    [Header("快捷键")]
    public KeyCode moveKey = KeyCode.M;     // 移动快捷键

    private void Start()
    {
        if (moveSystem == null)
            moveSystem = FindObjectOfType<BattleMoveSystem2D>();

        if (actionPointSystem == null)
            actionPointSystem = FindObjectOfType<ActionPointSystem>();

        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();

        if (moveButton != null)
        {
            moveButton.onClick.AddListener(OnMoveButtonClicked);
        }
    }

    private void Update()
    {
        // 快捷键
        if (Input.GetKeyDown(moveKey))
        {
            OnMoveButtonClicked();
        }

        UpdateUI();
    }

    /// <summary>
    /// 移动按钮点击
    /// </summary>
    private void OnMoveButtonClicked()
    {
        if (moveSystem == null) return;

        if (moveSystem.isSelectingMoveTarget)
        {
            // 如果正在选择，则取消
            moveSystem.CancelMoveSelection();
        }
        else
        {
            // 开始选择移动目标
            moveSystem.StartMoveSelection();
        }
    }

    /// <summary>
    /// 更新UI
    /// </summary>
    private void UpdateUI()
    {
        bool canMove = CanMove();
        bool isSelecting = moveSystem != null && moveSystem.isSelectingMoveTarget;
        bool isMoving = moveSystem != null && moveSystem.isMoving;

        // 更新按钮状态
        if (moveButton != null)
        {
            moveButton.interactable = canMove || isSelecting;

            // 更新颜色
            ColorBlock colors = moveButton.colors;
            if (isSelecting)
            {
                colors.normalColor = new Color(0.3f, 0.6f, 1f);  // 蓝色表示选择中
            }
            else
            {
                colors.normalColor = Color.white;
            }
            moveButton.colors = colors;
        }

        // 更新按钮文字
        if (moveButtonText != null)
        {
            if (isMoving)
            {
                moveButtonText.text = "移动中...";
            }
            else if (isSelecting)
            {
                moveButtonText.text = "取消移动";
            }
            else if (canMove)
            {
                moveButtonText.text = $"移动 [M]";
            }
            else
            {
                moveButtonText.text = "无法移动";
            }
        }

        // 更新移动点显示
        if (movePointsText != null && moveSystem != null)
        {
            int movePoints = moveSystem.GetPlayerMovePoints();
            movePointsText.text = $"移动力: {movePoints}";
        }
    }

    /// <summary>
    /// 是否可以移动
    /// </summary>
    private bool CanMove()
    {
        // 检查是否是玩家回合
        if (battleManager == null || !battleManager.IsPlayerTurn)
            return false;

        // 检查是否有移动动作
        if (actionPointSystem == null || !actionPointSystem.CanMove())
            return false;

        // 检查是否正在移动
        if (moveSystem != null && moveSystem.isMoving)
            return false;

        return true;
    }
}
