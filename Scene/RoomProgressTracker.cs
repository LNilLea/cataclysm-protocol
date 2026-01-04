using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 房间进度追踪器 - 追踪叙事房间内的必要交互物品
/// 放在每个叙事房间场景中
/// </summary>
public class RoomProgressTracker : MonoBehaviour
{
    public static RoomProgressTracker Instance { get; private set; }

    [Header("必要交互物品")]
    [Tooltip("房间内所有标记为'必要'的可交互物品，全部完成才能离开")]
    public List<InteractableItem> requiredItems = new List<InteractableItem>();

    [Header("出口")]
    [Tooltip("房间出口，完成所有必要交互后解锁")]
    public ExitPortal exitPortal;

    [Header("状态")]
    public int completedCount = 0;
    public bool allCompleted = false;

    // 事件：当所有必要物品交互完成
    public event System.Action OnAllRequiredCompleted;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 自动查找场景中所有标记为必要的物品
        if (requiredItems.Count == 0)
        {
            FindRequiredItems();
        }

        // 初始化出口为锁定状态
        if (exitPortal != null)
        {
            exitPortal.SetLocked(requiredItems.Count > 0);
        }

        Debug.Log($"房间进度追踪器启动，需要完成 {requiredItems.Count} 个必要交互");
    }

    /// <summary>
    /// 自动查找场景中所有必要的交互物品
    /// </summary>
    private void FindRequiredItems()
    {
        InteractableItem[] allItems = FindObjectsOfType<InteractableItem>();
        foreach (var item in allItems)
        {
            if (item.isRequired)
            {
                requiredItems.Add(item);
            }
        }
    }

    /// <summary>
    /// 当一个必要物品被交互后调用
    /// </summary>
    public void OnItemCompleted(InteractableItem item)
    {
        if (!requiredItems.Contains(item)) return;
        if (item.hasBeenInteracted) return;  // 防止重复计数

        completedCount++;
        Debug.Log($"完成交互：{item.itemName} ({completedCount}/{requiredItems.Count})");

        // 检查是否全部完成
        if (completedCount >= requiredItems.Count)
        {
            allCompleted = true;
            OnAllRequiredCompleted?.Invoke();

            // 解锁出口
            if (exitPortal != null)
            {
                exitPortal.SetLocked(false);
                Debug.Log("所有必要交互完成，出口已解锁！");
            }
        }
    }

    /// <summary>
    /// 获取进度百分比
    /// </summary>
    public float GetProgressPercent()
    {
        if (requiredItems.Count == 0) return 1f;
        return (float)completedCount / requiredItems.Count;
    }

    /// <summary>
    /// 获取进度文本
    /// </summary>
    public string GetProgressText()
    {
        return $"{completedCount}/{requiredItems.Count}";
    }
}
