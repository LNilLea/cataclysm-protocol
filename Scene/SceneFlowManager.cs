using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using MyGame;
/// <summary>
/// 场景流程管理器 - 管理场景之间的切换和数据持久化
/// 单例模式，跨场景保留
/// </summary>
public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager Instance { get; private set; }

    [Header("场景流程配置")]
    [Tooltip("游戏场景顺序")]
    public List<string> sceneFlow = new List<string>
    {
        "start",
        "Tips", 
        "CharacterCreation",
        "ResearchInstitute",
        "Forest",
        "Path1",
        "Hunterscabins",
        "Path2",
        "Police",
        "End"
    };

    [Header("当前状态")]
    public int currentSceneIndex = 0;
    public string currentSceneName;
    public string previousSceneName;

    [Header("出生点")]
    [Tooltip("下一个场景的出生点ID")]
    public string nextSpawnPointID = "";

    [Header("玩家数据缓存")]
    public Vector3 playerPosition;
    public int playerHP;
    public int playerMaxHP;

    [Header("设置")]
    public bool debugMode = false;

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 订阅场景加载事件
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Start()
    {
        // 初始化当前场景名称
        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UpdateSceneIndex();
    }

    /// <summary>
    /// 场景加载完成时调用
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        previousSceneName = currentSceneName;
        currentSceneName = scene.name;
        UpdateSceneIndex();

        if (debugMode)
        {
            Debug.Log($"[SceneFlowManager] 场景已加载: {currentSceneName} (索引: {currentSceneIndex})");
        }

        // 恢复玩家数据
        RestorePlayerData();
    }

    /// <summary>
    /// 更新当前场景索引
    /// </summary>
    private void UpdateSceneIndex()
    {
        currentSceneIndex = sceneFlow.IndexOf(currentSceneName);
        if (currentSceneIndex < 0)
        {
            // 场景不在流程中，可能是战斗场景或其他特殊场景
            if (debugMode)
            {
                Debug.Log($"[SceneFlowManager] 场景 {currentSceneName} 不在主流程中");
            }
        }
    }

    /// <summary>
    /// 前往下一个场景
    /// </summary>
    public void GoToNextScene()
    {
        if (currentSceneIndex >= 0 && currentSceneIndex < sceneFlow.Count - 1)
        {
            string nextScene = sceneFlow[currentSceneIndex + 1];
            LoadScene(nextScene);
        }
        else
        {
            Debug.LogWarning("[SceneFlowManager] 已经是最后一个场景或场景不在流程中");
        }
    }

    /// <summary>
    /// 前往上一个场景
    /// </summary>
    public void GoToPreviousScene()
    {
        if (currentSceneIndex > 0)
        {
            string prevScene = sceneFlow[currentSceneIndex - 1];
            LoadScene(prevScene);
        }
        else
        {
            Debug.LogWarning("[SceneFlowManager] 已经是第一个场景或场景不在流程中");
        }
    }

    /// <summary>
    /// 加载指定场景
    /// </summary>
    public void LoadScene(string sceneName, string spawnPointID = "")
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneFlowManager] 场景名称不能为空");
            return;
        }

        // 保存出生点ID
        nextSpawnPointID = spawnPointID;

        // 保存玩家数据
        SavePlayerData();

        Debug.Log($"[SceneFlowManager] 加载场景: {sceneName}");

        // 使用过渡效果加载
        if (SceneTransitionEffect.Instance != null)
        {
            SceneTransitionEffect.Instance.FadeOutAndLoadScene(sceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// 保存玩家数据
    /// </summary>
    public void SavePlayerData()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        // 保存位置
        playerPosition = player.transform.position;

        // 保存到CharacterData
        Player playerComponent = player.GetComponent<Player>();
        if (playerComponent != null)
        {
            CharacterData.SaveFromPlayer(playerComponent);
        }

        if (debugMode)
        {
            Debug.Log($"[SceneFlowManager] 玩家数据已保存 - 位置: {playerPosition}");
        }
    }

    /// <summary>
    /// 恢复玩家数据
    /// </summary>
    public void RestorePlayerData()
    {
        // 等待一帧确保玩家对象已创建
        StartCoroutine(RestorePlayerDataDelayed());
    }

    private System.Collections.IEnumerator RestorePlayerDataDelayed()
    {
        yield return null; // 等待一帧

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) yield break;

        // 应用CharacterData
        Player playerComponent = player.GetComponent<Player>();
        if (playerComponent != null && CharacterData.IsInitialized)
        {
            CharacterData.ApplyToPlayer(playerComponent);
        }

        if (debugMode)
        {
            Debug.Log($"[SceneFlowManager] 玩家数据已恢复");
        }
    }

    /// <summary>
    /// 获取下一个场景名称
    /// </summary>
    public string GetNextSceneName()
    {
        if (currentSceneIndex >= 0 && currentSceneIndex < sceneFlow.Count - 1)
        {
            return sceneFlow[currentSceneIndex + 1];
        }
        return null;
    }

    /// <summary>
    /// 获取上一个场景名称
    /// </summary>
    public string GetPreviousSceneName()
    {
        if (currentSceneIndex > 0)
        {
            return sceneFlow[currentSceneIndex - 1];
        }
        return null;
    }

    /// <summary>
    /// 检查是否是最后一个场景
    /// </summary>
    public bool IsLastScene()
    {
        return currentSceneIndex >= sceneFlow.Count - 1;
    }

    /// <summary>
    /// 检查是否是第一个场景
    /// </summary>
    public bool IsFirstScene()
    {
        return currentSceneIndex <= 0;
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        // 重置角色数据
        CharacterData.Reset();
        
        // 重置进度
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ResetProgress();
        }

        // 加载第一个场景
        currentSceneIndex = 0;
        nextSpawnPointID = "";
        
        if (sceneFlow.Count > 0)
        {
            LoadScene(sceneFlow[0]);
        }
    }

    /// <summary>
    /// 获取场景在流程中的位置
    /// </summary>
    public int GetSceneIndex(string sceneName)
    {
        return sceneFlow.IndexOf(sceneName);
    }

    /// <summary>
    /// 场景是否在流程中
    /// </summary>
    public bool IsSceneInFlow(string sceneName)
    {
        return sceneFlow.Contains(sceneName);
    }
}
