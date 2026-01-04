using UnityEngine;
using System.Collections;
using MyGame;
/// <summary>
/// 战斗场景入口 - 处理进入战斗场景时的初始化和淡入效果
/// 包含静态方法用于场景切换
/// </summary>
public class BattleSceneEntry : MonoBehaviour
{
    // ============ 静态数据（跨场景保存） ============
    private static string returnSceneName = "";      // 战斗后返回的场景
    private static Vector2 returnPlayerPosition;     // 返回时的玩家位置
    private static bool hasReturnData = false;

    [Header("淡入设置")]
    public bool fadeInOnStart = true;         // 开始时淡入
    public float fadeInDuration = 1f;         // 淡入时间
    public float delayBeforeBattle = 0.5f;    // 淡入后延迟多久开始战斗
    public Color fadeColor = Color.black;

    [Header("战斗开始设置")]
    public bool autoStartBattle = true;       // 自动开始战斗
    public BattleManager battleManager;

    [Header("玩家设置")]
    public Transform playerSpawnPoint;        // 玩家出生点
    public bool applyCharacterData = true;    // 应用角色数据

    [Header("UI引用")]
    public GameObject battleUI;               // 战斗UI（淡入后显示）

    [Header("调试")]
    public bool debugMode = false;

    private UnityEngine.UI.Image fadeImage;
    private Canvas fadeCanvas;

    // ============ 静态方法（供其他脚本调用） ============

    /// <summary>
    /// 进入战斗场景（静态方法）
    /// </summary>
    /// <param name="battleSceneName">战斗场景名称</param>
    /// <param name="currentSceneName">当前场景名称（战斗后返回）</param>
    /// <param name="playerPos">玩家当前位置（返回时恢复）</param>
    public static void EnterBattleScene(string battleSceneName, string currentSceneName, Vector2 playerPos)
    {
        // 保存返回数据
        returnSceneName = currentSceneName;
        returnPlayerPosition = playerPos;
        hasReturnData = true;

        // 保存玩家数据
        Player player = Object.FindObjectOfType<Player>();
        if (player != null)
        {
            CharacterData.SaveFromPlayer(player);
        }

        Debug.Log($"[BattleSceneEntry] 进入战斗: {battleSceneName}, 返回场景: {currentSceneName}");

        // 使用过渡效果
        if (SceneTransitionEffect.Instance != null)
        {
            SceneTransitionEffect.Instance.FadeOutAndLoadScene(battleSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(battleSceneName);
        }
    }

    /// <summary>
    /// 退出战斗场景，返回之前的场景（静态方法）
    /// </summary>
    public static void ExitBattleScene()
    {
        if (!hasReturnData || string.IsNullOrEmpty(returnSceneName))
        {
            Debug.LogWarning("[BattleSceneEntry] 没有返回场景数据，尝试使用SceneFlowManager");
            
            // 尝试使用SceneFlowManager
            if (SceneFlowManager.Instance != null)
            {
                string nextScene = SceneFlowManager.Instance.GetNextSceneName();
                if (!string.IsNullOrEmpty(nextScene))
                {
                    SceneFlowManager.Instance.LoadScene(nextScene);
                    return;
                }
            }

            Debug.LogError("[BattleSceneEntry] 无法确定返回场景！");
            return;
        }

        Debug.Log($"[BattleSceneEntry] 退出战斗，返回: {returnSceneName}");

        // 保存返回位置给SpawnPoint使用
        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.playerPosition = returnPlayerPosition;
        }

        string targetScene = returnSceneName;
        
        // 清除数据
        hasReturnData = false;
        returnSceneName = "";

        // 加载返回场景
        if (SceneTransitionEffect.Instance != null)
        {
            SceneTransitionEffect.Instance.FadeOutAndLoadScene(targetScene);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        }
    }

    /// <summary>
    /// 退出战斗并前往指定场景（静态方法）
    /// </summary>
    public static void ExitBattleToScene(string sceneName)
    {
        Debug.Log($"[BattleSceneEntry] 退出战斗，前往: {sceneName}");

        // 清除返回数据
        hasReturnData = false;
        returnSceneName = "";

        // 加载场景
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
    /// 获取返回场景名称
    /// </summary>
    public static string GetReturnSceneName()
    {
        return returnSceneName;
    }

    /// <summary>
    /// 获取返回位置
    /// </summary>
    public static Vector2 GetReturnPosition()
    {
        return returnPlayerPosition;
    }

    /// <summary>
    /// 是否有返回数据
    /// </summary>
    public static bool HasReturnData()
    {
        return hasReturnData;
    }

    // ============ 实例方法 ============

    private void Start()
    {
        // 隐藏战斗UI
        if (battleUI != null)
        {
            battleUI.SetActive(false);
        }

        // 自动查找BattleManager
        if (battleManager == null)
        {
            battleManager = FindObjectOfType<BattleManager>();
        }

        // 初始化玩家
        InitializePlayer();

        // 创建淡入淡出效果
        if (fadeInOnStart)
        {
            CreateFadeImage();
            StartCoroutine(BattleEntrySequence());
        }
        else
        {
            // 直接开始战斗
            if (autoStartBattle && battleManager != null)
            {
                StartBattle();
            }
        }
    }

    /// <summary>
    /// 初始化玩家
    /// </summary>
    private void InitializePlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            if (debugMode) Debug.Log("[BattleSceneEntry] 找不到玩家");
            return;
        }

        // 设置出生点
        if (playerSpawnPoint != null)
        {
            player.transform.position = playerSpawnPoint.position;
            if (debugMode) Debug.Log($"[BattleSceneEntry] 玩家位置设置为: {playerSpawnPoint.position}");
        }

        // 应用角色数据
        if (applyCharacterData && CharacterData.IsInitialized)
        {
            Player playerComponent = player.GetComponent<Player>();
            if (playerComponent != null)
            {
                CharacterData.ApplyToPlayer(playerComponent);
                if (debugMode) Debug.Log($"[BattleSceneEntry] 角色数据已应用 - HP: {CharacterData.CurrentHP}/{CharacterData.MaxHP}");
            }
        }
    }

    /// <summary>
    /// 创建淡入淡出Image
    /// </summary>
    private void CreateFadeImage()
    {
        // 检查是否已有SceneTransitionEffect
        if (SceneTransitionEffect.Instance != null)
        {
            // 使用现有的转场效果
            return;
        }

        // 创建自己的淡入效果
        GameObject canvasObj = new GameObject("BattleEntryFadeCanvas");
        canvasObj.transform.SetParent(transform);

        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999;

        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);

        fadeImage = imageObj.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);  // 开始时不透明
        fadeImage.raycastTarget = false;

        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// 战斗入场流程
    /// </summary>
    private IEnumerator BattleEntrySequence()
    {
        if (debugMode) Debug.Log("[BattleSceneEntry] 开始入场流程");

        // 使用SceneTransitionEffect的淡入效果（如果存在）
        if (SceneTransitionEffect.Instance != null)
        {
            // SceneTransitionEffect会自动处理淡入
            yield return new WaitForSeconds(fadeInDuration);
        }
        else if (fadeImage != null)
        {
            // 使用自己的淡入效果
            yield return StartCoroutine(FadeIn(fadeInDuration));
        }

        // 显示战斗UI
        if (battleUI != null)
        {
            battleUI.SetActive(true);
        }

        // 延迟后开始战斗
        yield return new WaitForSeconds(delayBeforeBattle);

        // 开始战斗
        if (autoStartBattle)
        {
            StartBattle();
        }
    }

    /// <summary>
    /// 淡入效果
    /// </summary>
    private IEnumerator FadeIn(float duration)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            color.a = Mathf.Lerp(1f, 0f, t);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;

        // 销毁淡入Canvas
        if (fadeCanvas != null)
        {
            Destroy(fadeCanvas.gameObject);
        }
    }

    /// <summary>
    /// 开始战斗
    /// </summary>
    public void StartBattle()
    {
        if (battleManager != null)
        {
            if (!battleManager.battleStarted)
            {
                battleManager.StartBattle();
                if (debugMode) Debug.Log("[BattleSceneEntry] 战斗开始");
            }
        }
        else
        {
            Debug.LogWarning("[BattleSceneEntry] 找不到 BattleManager！");
        }
    }

    /// <summary>
    /// 手动开始战斗（供外部调用）
    /// </summary>
    public void ManualStartBattle()
    {
        StartBattle();
    }
}
