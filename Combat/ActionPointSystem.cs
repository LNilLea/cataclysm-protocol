using UnityEngine;
using System;

/// <summary>
/// 动作点系统 - 管理玩家每回合的动作点
/// 移动动作(1) + 主要动作(1) + 次要动作(2)
/// </summary>
public class ActionPointSystem : MonoBehaviour
{
    [Header("动作点配置")]
    public int maxMoveActions = 1;      // 最大移动动作数
    public int maxMainActions = 1;      // 最大主要动作数
    public int maxMinorActions = 2;     // 最大次要动作数

    [Header("当前动作点")]
    public int currentMoveActions;      // 当前移动动作
    public int currentMainActions;      // 当前主要动作
    public int currentMinorActions;     // 当前次要动作

    [Header("状态")]
    public bool isPlayerTurn = false;   // 是否是玩家回合

    // 事件
    public event Action OnActionPointsChanged;  // 动作点变化时触发
    public event Action OnTurnStart;            // 回合开始
    public event Action OnTurnEnd;              // 回合结束
    public event Action OnAllActionsUsed;       // 所有动作用完

    /// <summary>
    /// 开始玩家回合，重置所有动作点
    /// </summary>
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        currentMoveActions = maxMoveActions;
        currentMainActions = maxMainActions;
        currentMinorActions = maxMinorActions;

        OnTurnStart?.Invoke();
        OnActionPointsChanged?.Invoke();

        Debug.Log($"玩家回合开始！移动:{currentMoveActions} 主要:{currentMainActions} 次要:{currentMinorActions}");
    }

    /// <summary>
    /// 结束玩家回合
    /// </summary>
    public void EndPlayerTurn()
    {
        isPlayerTurn = false;
        OnTurnEnd?.Invoke();

        Debug.Log("玩家回合结束");
    }

    /// <summary>
    /// 检查是否还有任何动作可用
    /// </summary>
    public bool HasAnyAction()
    {
        return currentMoveActions > 0 || currentMainActions > 0 || currentMinorActions > 0;
    }

    /// <summary>
    /// 检查是否可以移动
    /// </summary>
    public bool CanMove()
    {
        return isPlayerTurn && currentMoveActions > 0;
    }

    /// <summary>
    /// 检查是否可以执行主要动作（攻击）
    /// </summary>
    public bool CanDoMainAction()
    {
        return isPlayerTurn && currentMainActions > 0;
    }

    /// <summary>
    /// 检查是否可以执行次要动作
    /// </summary>
    public bool CanDoMinorAction()
    {
        return isPlayerTurn && currentMinorActions > 0;
    }

    /// <summary>
    /// 消耗移动动作
    /// </summary>
    public bool UseMoveAction()
    {
        if (!CanMove())
        {
            Debug.Log("无法移动：没有移动动作点");
            return false;
        }

        currentMoveActions--;
        OnActionPointsChanged?.Invoke();
        CheckAllActionsUsed();

        Debug.Log($"使用移动动作，剩余移动动作: {currentMoveActions}");
        return true;
    }

    /// <summary>
    /// 消耗主要动作（攻击）
    /// </summary>
    public bool UseMainAction()
    {
        if (!CanDoMainAction())
        {
            Debug.Log("无法攻击：没有主要动作点");
            return false;
        }

        currentMainActions--;
        OnActionPointsChanged?.Invoke();
        CheckAllActionsUsed();

        Debug.Log($"使用主要动作，剩余主要动作: {currentMainActions}");
        return true;
    }

    /// <summary>
    /// 消耗次要动作
    /// </summary>
    public bool UseMinorAction()
    {
        if (!CanDoMinorAction())
        {
            Debug.Log("无法执行：没有次要动作点");
            return false;
        }

        currentMinorActions--;
        OnActionPointsChanged?.Invoke();
        CheckAllActionsUsed();

        Debug.Log($"使用次要动作，剩余次要动作: {currentMinorActions}");
        return true;
    }

    /// <summary>
    /// 检查是否所有动作都用完了
    /// </summary>
    private void CheckAllActionsUsed()
    {
        if (!HasAnyAction())
        {
            Debug.Log("所有动作已用完");
            OnAllActionsUsed?.Invoke();
        }
    }

    /// <summary>
    /// 获取动作点状态文本
    /// </summary>
    public string GetStatusText()
    {
        return $"移动:{currentMoveActions}/{maxMoveActions} | 主要:{currentMainActions}/{maxMainActions} | 次要:{currentMinorActions}/{maxMinorActions}";
    }
}
