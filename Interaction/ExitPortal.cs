using UnityEngine;

/// <summary>
/// 出口传送门 - 玩家走到这里并按 E 可以进入下一个场景
/// </summary>
public class ExitPortal : MonoBehaviour
{
    [Header("设置")]
    public string portalName = "出口";
    public bool isLocked = true;              // 是否锁定
    public string lockedMessage = "还有事情没有完成...";
    public string unlockedMessage = "按 E 进入下一关";

    [Header("视觉效果（可选）")]
    public GameObject lockedVisual;           // 锁定时显示的物体
    public GameObject unlockedVisual;         // 解锁时显示的物体

    [Header("引用")]
    public SceneManager sceneManager;         // 你的场景管理器

    private bool isPlayerNearby = false;

    private void Start()
    {
        // 自动查找 SceneManager
        if (sceneManager == null)
        {
            sceneManager = FindObjectOfType<SceneManager>();
        }

        UpdateVisuals();
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            TryEnterPortal();
        }
    }

    /// <summary>
    /// 尝试进入传送门
    /// </summary>
    private void TryEnterPortal()
    {
        if (isLocked)
        {
            Debug.Log(lockedMessage);
            // TODO: 可以在这里显示 UI 提示
            return;
        }

        // 传送到下一个场景
        EnterNextScene();
    }

    /// <summary>
    /// 进入下一个场景
    /// </summary>
    private void EnterNextScene()
    {
        if (GameProgressManager.Instance == null)
        {
            Debug.LogError("ExitPortal: 找不到 GameProgressManager！");
            return;
        }

        // 通知进度管理器：叙事房间完成
        GameProgressManager.Instance.CompleteNarrativeRoom();

        // 获取下一个场景名
        string nextScene = GameProgressManager.Instance.GetCurrentSceneName();

        Debug.Log($"传送到：{nextScene}");

        // 加载下一个场景
        if (sceneManager != null)
        {
            sceneManager.LoadSceneAsync(nextScene);
        }
        else
        {
            // 备用方案：使用 Unity 自带的场景管理
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
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
            Debug.Log($"{portalName} 已解锁！");
        }
    }

    /// <summary>
    /// 更新视觉效果
    /// </summary>
    private void UpdateVisuals()
    {
        if (lockedVisual != null)
            lockedVisual.SetActive(isLocked);

        if (unlockedVisual != null)
            unlockedVisual.SetActive(!isLocked);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;

            if (isLocked)
                Debug.Log(lockedMessage);
            else
                Debug.Log(unlockedMessage);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
