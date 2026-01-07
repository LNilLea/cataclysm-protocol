using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using MyGame;

/// <summary>
/// 战斗结束处理器（增强版）- 处理战斗结束后的流程
/// 包含黑屏转场效果、经验结算、场景切换
/// </summary>
public class BattleEndHandlerEnhanced : MonoBehaviour
{
    [Header("经验奖励")]
    public int baseExpReward = 100;           // 基础经验奖励
    public int expPerEnemy = 20;              // 每个敌人额外经验

    [Header("时间设置")]
    public float resultDisplayTime = 2f;      // 结果显示时间
    public float fadeOutDuration = 1f;        // 淡出时间
    public float blackScreenDuration = 0.5f;  // 黑屏持续时间
    public float fadeInDuration = 1f;         // 淡入时间

    [Header("目标场景")]
    [Tooltip("留空则使用SceneFlowManager的下一个场景")]
    public string nextSceneOnVictory = "";    // 胜利后的场景
    public string nextSceneOnDefeat = "";     // 失败后的场景（留空则重试当前场景）
    public string targetSpawnPointID = "";    // 目标出生点ID

    [Header("引用")]
    public BattleManager battleManager;

    [Header("UI元素")]
    public GameObject victoryPanel;           // 胜利面板
    public GameObject defeatPanel;            // 失败面板
    public TMP_Text expGainText;              // 经验获得文本
    public TMP_Text levelUpText;              // 升级文本
    public TMP_Text resultTitleText;          // 结果标题（胜利/失败）
    public Button continueButton;             // 继续按钮（可选）
    public Button retryButton;                // 重试按钮（可选）

    [Header("转场效果")]
    public Image fadeImage;                   // 淡入淡出Image（如果不设置会自动创建）
    public Color fadeColor = Color.black;     // 转场颜色
    public bool autoCreateFadeImage = true;   // 自动创建淡入淡出Image

    [Header("音效（可选）")]
    public AudioClip victorySound;
    public AudioClip defeatSound;
    public AudioClip levelUpSound;
    private AudioSource audioSource;

    [Header("调试")]
    public bool debugMode = false;

    // 状态
    private bool hasHandledEnd = false;
    private bool isTransitioning = false;
    private bool waitingForInput = false;
    private Canvas fadeCanvas;

    private void Start()
    {
        // 自动查找引用
        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();

        // 设置音频
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (victorySound != null || defeatSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 隐藏UI
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (levelUpText != null) levelUpText.gameObject.SetActive(false);

        // 设置按钮
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
            continueButton.gameObject.SetActive(false);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
            retryButton.gameObject.SetActive(false);
        }

        // 创建淡入淡出Image
        if (fadeImage == null && autoCreateFadeImage)
        {
            CreateFadeImage();
        }

        // 确保淡入淡出Image初始透明
        if (fadeImage != null)
        {
            SetFadeAlpha(0f);
        }

        // 订阅升级事件
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.OnLevelUp += HandleLevelUp;
        }

        // 订阅战斗结束事件
        if (battleManager != null)
        {
            battleManager.OnBattleEnd += OnBattleEndEvent;
        }
    }

    private void OnDestroy()
    {
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.OnLevelUp -= HandleLevelUp;
        }

        if (battleManager != null)
        {
            battleManager.OnBattleEnd -= OnBattleEndEvent;
        }
    }

    private void Update()
    {
        // 备用检测（如果没有订阅事件）
        if (battleManager != null && battleManager.BattleEnded && !hasHandledEnd)
        {
            hasHandledEnd = true;
            HandleBattleEnd();
        }

        // 等待输入时检测按键
        if (waitingForInput && Input.anyKeyDown)
        {
            waitingForInput = false;
            
            // 根据结果决定下一步
            Player player = FindObjectOfType<Player>();
            bool playerWon = player != null && player.currentHP > 0;

            if (playerWon)
            {
                StartCoroutine(TransitionToNextScene());
            }
            else
            {
                StartCoroutine(TransitionToRetry());
            }
        }
    }

    /// <summary>
    /// 战斗结束事件处理
    /// </summary>
    private void OnBattleEndEvent()
    {
        if (!hasHandledEnd)
        {
            hasHandledEnd = true;
            HandleBattleEnd();
        }
    }

    /// <summary>
    /// 创建淡入淡出Image
    /// </summary>
    private void CreateFadeImage()
    {
        // 创建Canvas
        GameObject canvasObj = new GameObject("BattleEndFadeCanvas");
        canvasObj.transform.SetParent(transform);

        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 10000;  // 最顶层

        canvasObj.AddComponent<CanvasScaler>();

        // 创建Image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.raycastTarget = false;

        // 全屏
        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// 设置淡入淡出透明度
    /// </summary>
    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
    }

    /// <summary>
    /// 处理战斗结束
    /// </summary>
    private void HandleBattleEnd()
    {
        Player player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("[BattleEndHandler] 找不到 Player！");
            return;
        }

        bool playerWon = player.currentHP > 0;

        if (debugMode)
        {
            Debug.Log($"[BattleEndHandler] 战斗结束 - 玩家HP: {player.currentHP}, 胜利: {playerWon}");
        }

        if (playerWon)
        {
            StartCoroutine(HandleVictorySequence());
        }
        else
        {
            StartCoroutine(HandleDefeatSequence());
        }
    }

    /// <summary>
    /// 胜利处理流程
    /// </summary>
    private IEnumerator HandleVictorySequence()
    {
        Debug.Log("===== 战斗胜利！=====");

        // 播放胜利音效
        if (victorySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(victorySound);
        }

        // 计算经验
        int enemyCount = battleManager != null ? battleManager.EnemyCount : 1;
        int totalExp = baseExpReward + (expPerEnemy * enemyCount);

        // 显示胜利UI
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (resultTitleText != null)
        {
            resultTitleText.text = "胜利！";
        }

        if (expGainText != null)
        {
            expGainText.text = $"获得经验：{totalExp}";
        }

        // 保存玩家数据
        SavePlayerData();

        // 通知进度管理器
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.CompleteBattle(totalExp);
        }

        // 显示继续按钮或等待输入
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            // 等待按钮点击（在OnContinueClicked中处理）
        }
        else
        {
            // 等待一段时间后自动继续
            yield return new WaitForSeconds(resultDisplayTime);
            StartCoroutine(TransitionToNextScene());
        }
    }

    /// <summary>
    /// 失败处理流程
    /// </summary>
    private IEnumerator HandleDefeatSequence()
    {
        Debug.Log("===== 战斗失败... =====");

        // 播放失败音效
        if (defeatSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(defeatSound);
        }

        // 显示失败UI
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }

        if (resultTitleText != null)
        {
            resultTitleText.text = "战斗失败";
        }

        // 显示重试按钮或等待输入
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(true);
            // 等待按钮点击（在OnRetryClicked中处理）
        }
        else
        {
            // 等待一段时间后自动重试
            yield return new WaitForSeconds(resultDisplayTime);
            StartCoroutine(TransitionToRetry());
        }
    }

    /// <summary>
    /// 处理升级
    /// </summary>
    private void HandleLevelUp(int newLevel)
    {
        Debug.Log($"升级到 {newLevel} 级！");

        // 播放升级音效
        if (levelUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }

        if (levelUpText != null)
        {
            levelUpText.text = $"升级！当前等级：{newLevel}";
            levelUpText.gameObject.SetActive(true);
        }

        // 应用等级加成
        Player player = FindObjectOfType<Player>();
        if (player != null && GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ApplyLevelBonusToPlayer(player.combatData);
        }
    }

    /// <summary>
    /// 保存玩家数据
    /// </summary>
    private void SavePlayerData()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            CharacterData.SaveFromPlayer(player);
        }

        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.SavePlayerData();
        }
    }

    /// <summary>
    /// 转场到下一个场景（胜利后）
    /// </summary>
    private IEnumerator TransitionToNextScene()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        // 隐藏UI
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(false);

        // 淡出到黑屏
        yield return StartCoroutine(FadeOut(fadeOutDuration));

        // 黑屏持续
        yield return new WaitForSeconds(blackScreenDuration);

        // 确定目标场景
        string targetScene = GetNextSceneName();

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("[BattleEndHandler] 无法确定下一个场景！");
            yield break;
        }

        // 设置出生点
        if (SceneFlowManager.Instance != null && !string.IsNullOrEmpty(targetSpawnPointID))
        {
            SceneFlowManager.Instance.nextSpawnPointID = targetSpawnPointID;
        }

        Debug.Log($"[BattleEndHandler] 切换到场景: {targetScene}");

        // 加载场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);

        // 注意：淡入效果由SceneTransitionEffect在新场景处理
    }

    /// <summary>
    /// 转场重试（失败后）
    /// </summary>
    private IEnumerator TransitionToRetry()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        // 隐藏UI
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (retryButton != null) retryButton.gameObject.SetActive(false);

        // 淡出到黑屏
        yield return StartCoroutine(FadeOut(fadeOutDuration));

        // 黑屏持续
        yield return new WaitForSeconds(blackScreenDuration);

        // 恢复玩家HP
        CharacterData.FullRestore();

        // 确定重试场景
        string retryScene = nextSceneOnDefeat;

        if (string.IsNullOrEmpty(retryScene))
        {
            // 默认重新加载当前场景
            retryScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }

        Debug.Log($"[BattleEndHandler] 重试场景: {retryScene}");

        // 重置战斗状态
        hasHandledEnd = false;

        // 加载场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(retryScene);
    }

    /// <summary>
    /// 获取下一个场景名称
    /// </summary>
    private string GetNextSceneName()
    {
        // 优先使用手动设置的场景
        if (!string.IsNullOrEmpty(nextSceneOnVictory))
        {
            return nextSceneOnVictory;
        }

        // 使用SceneFlowManager
        if (SceneFlowManager.Instance != null)
        {
            return SceneFlowManager.Instance.GetNextSceneName();
        }

        // 使用GameProgressManager
        if (GameProgressManager.Instance != null)
        {
            return GameProgressManager.Instance.GetCurrentSceneName();
        }

        return null;
    }

    /// <summary>
    /// 淡出效果
    /// </summary>
    private IEnumerator FadeOut(float duration)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            color.a = Mathf.Lerp(0f, 1f, t);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
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
    }

    /// <summary>
    /// 继续按钮点击
    /// </summary>
    private void OnContinueClicked()
    {
        StartCoroutine(TransitionToNextScene());
    }

    /// <summary>
    /// 重试按钮点击
    /// </summary>
    private void OnRetryClicked()
    {
        StartCoroutine(TransitionToRetry());
    }

    /// <summary>
    /// 手动触发胜利（用于测试）
    /// </summary>
    [ContextMenu("Test Victory")]
    public void TestVictory()
    {
        if (!hasHandledEnd)
        {
            hasHandledEnd = true;
            StartCoroutine(HandleVictorySequence());
        }
    }

    /// <summary>
    /// 手动触发失败（用于测试）
    /// </summary>
    [ContextMenu("Test Defeat")]
    public void TestDefeat()
    {
        if (!hasHandledEnd)
        {
            hasHandledEnd = true;
            StartCoroutine(HandleDefeatSequence());
        }
    }
}
