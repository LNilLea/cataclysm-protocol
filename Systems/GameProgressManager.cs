using UnityEngine;

/// <summary>
/// 游戏进度管理器 - 跨场景保存玩家进度
/// 使用单例模式 + DontDestroyOnLoad
/// </summary>
public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    [Header("关卡进度")]
    public int currentChapter = 1;          // 当前章节（1 = 教程）
    public int currentStage = 0;            // 0 = 叙事房间, 1 = 战斗关卡

    [Header("玩家等级")]
    public int playerLevel = 1;             // 玩家等级
    public int currentExp = 0;              // 当前经验值
    public int expToNextLevel = 100;        // 升级所需经验

    [Header("场景名称配置")]
    public string[] narrativeScenes;        // 叙事场景名称列表，如 ["Narrative1", "Narrative2", ...]
    public string[] battleScenes;           // 战斗场景名称列表，如 ["Battle1", "Battle2", ...]

    [Header("升级奖励配置")]
    public int hpPerLevel = 10;             // 每级增加的 HP
    public int acPerLevel = 1;              // 每 2 级增加的 AC

    // 事件：升级时触发
    public event System.Action<int> OnLevelUp;
    // 事件：章节完成时触发
    public event System.Action<int> OnChapterComplete;

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 获取当前应该加载的场景名
    /// </summary>
    public string GetCurrentSceneName()
    {
        if (currentStage == 0)
        {
            // 叙事房间
            int index = Mathf.Clamp(currentChapter - 1, 0, narrativeScenes.Length - 1);
            return narrativeScenes[index];
        }
        else
        {
            // 战斗关卡
            int index = Mathf.Clamp(currentChapter - 1, 0, battleScenes.Length - 1);
            return battleScenes[index];
        }
    }

    /// <summary>
    /// 叙事房间完成，进入战斗
    /// </summary>
    public void CompleteNarrativeRoom()
    {
        currentStage = 1;  // 切换到战斗关卡
        Debug.Log($"叙事房间完成，准备进入战斗关卡：{GetCurrentSceneName()}");
    }

    /// <summary>
    /// 战斗胜利，给予经验并进入下一章节
    /// </summary>
    public void CompleteBattle(int expReward)
    {
        // 给予经验
        AddExp(expReward);

        // 触发章节完成事件
        OnChapterComplete?.Invoke(currentChapter);

        // 进入下一章节的叙事房间
        currentChapter++;
        currentStage = 0;

        Debug.Log($"战斗胜利！进入第 {currentChapter} 章叙事房间：{GetCurrentSceneName()}");
    }

    /// <summary>
    /// 添加经验值
    /// </summary>
    public void AddExp(int amount)
    {
        currentExp += amount;
        Debug.Log($"获得 {amount} 经验值，当前经验：{currentExp}/{expToNextLevel}");

        // 检查升级
        while (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }

    /// <summary>
    /// 升级
    /// </summary>
    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        playerLevel++;

        // 增加下一级所需经验（每级增加 50）
        expToNextLevel += 50;

        Debug.Log($"升级！当前等级：{playerLevel}");

        // 触发升级事件
        OnLevelUp?.Invoke(playerLevel);
    }

    /// <summary>
    /// 应用等级加成到玩家数据
    /// </summary>
    public void ApplyLevelBonusToPlayer(PlayerCombatData combatData)
    {
        // HP 加成
        int bonusHP = (playerLevel - 1) * hpPerLevel;
        combatData.maxHP += bonusHP;
        combatData.currentHP = combatData.maxHP;

        // AC 加成（每 2 级 +1）
        int bonusAC = (playerLevel - 1) / 2 * acPerLevel;
        combatData.otherAC += bonusAC;

        Debug.Log($"应用等级加成：HP +{bonusHP}, AC +{bonusAC}");
    }

    /// <summary>
    /// 重置游戏进度（新游戏）
    /// </summary>
    public void ResetProgress()
    {
        currentChapter = 1;
        currentStage = 0;
        playerLevel = 1;
        currentExp = 0;
        expToNextLevel = 100;

        Debug.Log("游戏进度已重置");
    }

    /// <summary>
    /// 检查是否是最后一章
    /// </summary>
    public bool IsLastChapter()
    {
        return currentChapter >= narrativeScenes.Length;
    }
}
