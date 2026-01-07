using UnityEngine;
using System.Collections;
using MyGame;
/// <summary>
/// 战斗结束处理器 - 处理战斗结束后的流程
/// 挂载在战斗场景中，监听 BattleManager 的战斗结束
/// </summary>
public class BattleEndHandler : MonoBehaviour
{
    [Header("经验奖励")]
    public int baseExpReward = 100;           // 基础经验奖励
    public int expPerEnemy = 20;              // 每个敌人额外经验

    [Header("场景切换延迟")]
    public float delayBeforeTransition = 2f;  // 战斗结束后延迟多久切换场景

    [Header("引用")]
    public BattleManager battleManager;
    public SceneManager sceneManager;

    [Header("UI（可选）")]
    public GameObject victoryPanel;           // 胜利面板
    public GameObject defeatPanel;            // 失败面板
    public TMPro.TMP_Text expGainText;        // 经验获得文本
    public TMPro.TMP_Text levelUpText;        // 升级文本

    private bool hasHandledEnd = false;

    private void Start()
    {
        // 自动查找引用
        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();

        if (sceneManager == null)
            sceneManager = FindObjectOfType<SceneManager>();

        // 隐藏 UI
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        // 订阅升级事件
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.OnLevelUp += HandleLevelUp;
        }
    }

    private void OnDestroy()
    {
        // 取消订阅
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.OnLevelUp -= HandleLevelUp;
        }
    }

    private void Update()
    {
        // 检查战斗是否结束
        if (battleManager != null && battleManager.BattleEnded && !hasHandledEnd)
        {
            hasHandledEnd = true;
            HandleBattleEnd();
        }
    }

    /// <summary>
    /// 处理战斗结束
    /// </summary>
    private void HandleBattleEnd()
    {
        // 获取玩家
        Player player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("BattleEndHandler: 找不到 Player！");
            return;
        }

        // 判断胜负
        bool playerWon = player.currentHP > 0;

        if (playerWon)
        {
            HandleVictory();
        }
        else
        {
            HandleDefeat();
        }
    }

    /// <summary>
    /// 处理胜利
    /// </summary>
    private void HandleVictory()
    {
        Debug.Log("===== 战斗胜利！=====");

        // 计算经验
        int enemyCount = battleManager != null ? battleManager.EnemyCount : 1;
        int totalExp = baseExpReward + (expPerEnemy * enemyCount);

        // 显示胜利 UI
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (expGainText != null)
        {
            expGainText.text = $"获得经验：{totalExp}";
        }

        // 通知进度管理器
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.CompleteBattle(totalExp);
        }

        // 延迟切换场景
        StartCoroutine(TransitionToNextScene());
    }

    /// <summary>
    /// 处理失败
    /// </summary>
    private void HandleDefeat()
    {
        Debug.Log("===== 战斗失败... =====");

        // 显示失败 UI
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }

        // 可以在这里加入重试逻辑
        // StartCoroutine(RetryBattle());
    }

    /// <summary>
    /// 处理升级
    /// </summary>
    private void HandleLevelUp(int newLevel)
    {
        Debug.Log($"升级到 {newLevel} 级！");

        if (levelUpText != null)
        {
            levelUpText.text = $"升级！当前等级：{newLevel}";
            levelUpText.gameObject.SetActive(true);
        }

        // 应用等级加成到玩家
        Player player = FindObjectOfType<Player>();
        if (player != null && GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ApplyLevelBonusToPlayer(player.combatData);
        }
    }

    /// <summary>
    /// 延迟切换到下一个场景
    /// </summary>
    private IEnumerator TransitionToNextScene()
    {
        yield return new WaitForSeconds(delayBeforeTransition);

        if (GameProgressManager.Instance == null)
        {
            Debug.LogError("BattleEndHandler: 找不到 GameProgressManager！");
            yield break;
        }

        // 检查是否是最后一章
        if (GameProgressManager.Instance.IsLastChapter())
        {
            Debug.Log("恭喜！你完成了所有关卡！");
            // TODO: 加载结局场景或显示结局画面
            yield break;
        }

        // 获取下一个场景名
        string nextScene = GameProgressManager.Instance.GetCurrentSceneName();
        Debug.Log($"切换到：{nextScene}");

        // 加载场景
        if (sceneManager != null)
        {
            sceneManager.LoadSceneAsync(nextScene);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
    }

    /// <summary>
    /// 重试战斗（失败后调用）
    /// </summary>
    public void RetryBattle()
    {
        hasHandledEnd = false;

        // 重新加载当前战斗场景
        if (GameProgressManager.Instance != null)
        {
            string currentScene = GameProgressManager.Instance.GetCurrentSceneName();
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }
    }
}
