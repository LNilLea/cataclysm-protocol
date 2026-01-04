using UnityEngine;

/// <summary>
/// 场景传送门 - 纯传送功能，不包含出生点
/// 玩家靠近并按E键传送到目标场景
/// </summary>
public class ScenePortal : MonoBehaviour
{
    [Header("目标设置")]
    public string targetSceneName;              // 目标场景名称
    public string targetSpawnPointID = "";      // 目标场景的出生点ID（留空则使用默认出生点）

    [Header("解锁设置")]
    public bool isLocked = false;               // 是否锁定
    public string lockedMessage = "这个传送门目前无法使用...";

    [Header("交互设置")]
    public float interactDistance = 2f;         // 交互距离
    public string promptText = "按 E 传送";     // 提示文字
    public KeyCode interactKey = KeyCode.E;     // 交互按键

    [Header("过渡效果")]
    public bool useFade = true;                 // 是否使用淡入淡出
    public float fadeDuration = 0.5f;           // 淡入淡出时长

    [Header("视觉效果")]
    public GameObject lockedVisual;             // 锁定时显示的物体
    public GameObject unlockedVisual;           // 解锁时显示的物体
    public SpriteRenderer portalRenderer;       // 传送门渲染器
    public Color lockedColor = Color.gray;      // 锁定时的颜色
    public Color unlockedColor = Color.cyan;    // 解锁时的颜色

    [Header("调试")]
    public bool showGizmos = true;

    private Transform playerTransform;
    private bool isPlayerNearby = false;
    private bool isTransitioning = false;

    private void Start()
    {
        // 查找玩家
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        UpdateVisuals();
    }

    private void Update()
    {
        // 检查基本条件
        if (string.IsNullOrEmpty(targetSceneName)) return;
        if (playerTransform == null || isTransitioning) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // 玩家进入交互范围
        if (distance <= interactDistance)
        {
            if (!isPlayerNearby)
            {
                isPlayerNearby = true;
                ShowPrompt();
            }

            // 检测交互按键
            if (Input.GetKeyDown(interactKey))
            {
                TryTeleport();
            }
        }
        else
        {
            if (isPlayerNearby)
            {
                isPlayerNearby = false;
                HidePrompt();
            }
        }
    }

    /// <summary>
    /// 尝试传送
    /// </summary>
    private void TryTeleport()
    {
        if (isLocked)
        {
            ShowLockedMessage();
            return;
        }

        StartTeleport();
    }

    /// <summary>
    /// 开始传送
    /// </summary>
    private void StartTeleport()
    {
        isTransitioning = true;
        HidePrompt();

        // 保存目标出生点ID
        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.nextSpawnPointID = targetSpawnPointID;
            SceneFlowManager.Instance.SavePlayerData();
        }

        Debug.Log($"[ScenePortal] 传送到: {targetSceneName} (出生点: {targetSpawnPointID})");

        // 执行场景切换
        if (useFade && SceneTransitionEffect.Instance != null)
        {
            SceneTransitionEffect.Instance.FadeOutAndLoadScene(targetSceneName, fadeDuration);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }
    }

    /// <summary>
    /// 设置锁定状态
    /// </summary>
    public void SetLocked(bool locked)
    {
        isLocked = locked;
        UpdateVisuals();

        if (!locked)
        {
            Debug.Log($"[ScenePortal] 传送门已解锁 -> {targetSceneName}");
        }
    }

    /// <summary>
    /// 更新视觉效果
    /// </summary>
    private void UpdateVisuals()
    {
        // 切换锁定/解锁视觉
        if (lockedVisual != null)
            lockedVisual.SetActive(isLocked);

        if (unlockedVisual != null)
            unlockedVisual.SetActive(!isLocked);

        // 更新颜色
        if (portalRenderer != null)
        {
            portalRenderer.color = isLocked ? lockedColor : unlockedColor;
        }
    }

    /// <summary>
    /// 显示提示
    /// </summary>
    private void ShowPrompt()
    {
        string message = isLocked ? lockedMessage : promptText;
        
        if (InteractPromptUI.Instance != null)
        {
            InteractPromptUI.Instance.Show(message);
        }
    }

    /// <summary>
    /// 隐藏提示
    /// </summary>
    private void HidePrompt()
    {
        if (InteractPromptUI.Instance != null)
        {
            InteractPromptUI.Instance.Hide();
        }
    }

    /// <summary>
    /// 显示锁定消息
    /// </summary>
    private void ShowLockedMessage()
    {
        Debug.Log($"[ScenePortal] {lockedMessage}");
        
        // 可以在这里显示UI提示
        if (InteractPromptUI.Instance != null)
        {
            InteractPromptUI.Instance.Show(lockedMessage);
        }
    }

    // 也支持触发器方式
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            playerTransform = other.transform;
            ShowPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            HidePrompt();
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // 传送门范围
        Gizmos.color = isLocked ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactDistance);

        // 传送门中心
        Gizmos.color = isLocked ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);

        // 显示目标场景名称
        #if UNITY_EDITOR
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up, targetSceneName);
        }
        #endif
    }
}
