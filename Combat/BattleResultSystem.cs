using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MyGame;
/// <summary>
/// 战斗结算系统 - 处理战斗胜利/失败和奖励
/// </summary>
public class BattleResultSystem : MonoBehaviour
{
    public static BattleResultSystem Instance { get; private set; }

    [Header("结算面板")]
    public GameObject resultPanel;
    public TMP_Text resultTitleText;        // "战斗胜利！" / "战斗失败"
    public TMP_Text defeatedEnemiesText;    // 击败敌人列表
    public TMP_Text expGainedText;          // 获得经验
    public TMP_Text levelUpText;            // 升级提示
    public Button continueButton;           // 继续按钮

    [Header("音效")]
    public AudioClip victorySound;
    public AudioClip defeatSound;
    private AudioSource audioSource;

    [Header("奖励设置")]
    public int baseExpPerEnemy = 50;        // 每个敌人基础经验

    // 战斗数据
    private List<string> defeatedEnemies = new List<string>();
    private int totalExpGained = 0;
    private bool isVictory = false;

    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    /// <summary>
    /// 记录击杀敌人
    /// </summary>
    public void RecordEnemyDefeated(string enemyName, int expValue = 0)
    {
        defeatedEnemies.Add(enemyName);

        int exp = expValue > 0 ? expValue : baseExpPerEnemy;
        totalExpGained += exp;

        Debug.Log($"[BattleResultSystem] 击败: {enemyName}, 经验: {exp}");
    }

    /// <summary>
    /// 显示胜利结算
    /// </summary>
    public void ShowVictory()
    {
        isVictory = true;
        ShowResult();
    }

    /// <summary>
    /// 显示失败结算
    /// </summary>
    public void ShowDefeat()
    {
        isVictory = false;
        ShowResult();
    }

    /// <summary>
    /// 显示结算界面
    /// </summary>
    private void ShowResult()
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);

        // 标题
        if (resultTitleText != null)
        {
            resultTitleText.text = isVictory ? "战斗胜利！" : "战斗失败";
            resultTitleText.color = isVictory ? Color.yellow : Color.red;
        }

        // 击败敌人列表
        if (defeatedEnemiesText != null)
        {
            if (defeatedEnemies.Count > 0)
            {
                string enemies = "击败敌人:\n";
                Dictionary<string, int> enemyCount = new Dictionary<string, int>();

                foreach (var enemy in defeatedEnemies)
                {
                    if (enemyCount.ContainsKey(enemy))
                        enemyCount[enemy]++;
                    else
                        enemyCount[enemy] = 1;
                }

                foreach (var pair in enemyCount)
                {
                    enemies += $"  • {pair.Key} x{pair.Value}\n";
                }

                defeatedEnemiesText.text = enemies;
            }
            else
            {
                defeatedEnemiesText.text = "";
            }
        }

        // 经验奖励
        if (expGainedText != null)
        {
            if (isVictory && totalExpGained > 0)
            {
                expGainedText.text = $"获得经验: {totalExpGained}";
                expGainedText.gameObject.SetActive(true);

                // 实际给予经验
                int oldLevel = CharacterData.Level;
                CharacterData.GainExperience(totalExpGained);

                // 检查是否升级
                if (levelUpText != null)
                {
                    if (CharacterData.Level > oldLevel)
                    {
                        levelUpText.text = $"等级提升！ Lv.{oldLevel} → Lv.{CharacterData.Level}";
                        levelUpText.gameObject.SetActive(true);
                    }
                    else
                    {
                        levelUpText.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                expGainedText.gameObject.SetActive(false);
                if (levelUpText != null) levelUpText.gameObject.SetActive(false);
            }
        }

        // 播放音效
        if (audioSource != null)
        {
            AudioClip clip = isVictory ? victorySound : defeatSound;
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        // 暂停游戏
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 继续按钮点击
    /// </summary>
    private void OnContinueClicked()
    {
        Time.timeScale = 1f;

        if (isVictory)
        {
            // 胜利后返回上一个场景
            ReturnToPreviousScene();
        }
        else
        {
            // 失败后的处理（可以选择重试或返回）
            ReturnToPreviousScene();
        }
    }

    /// <summary>
    /// 返回上一个场景
    /// </summary>
    private void ReturnToPreviousScene()
    {
        // 保存玩家数据
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            CharacterData.SaveFromPlayer(player);
        }

        // 清理战斗数据
        ClearBattleData();

        // 返回
        BattleSceneEntry.ExitBattleScene();
    }

    /// <summary>
    /// 清理战斗数据
    /// </summary>
    public void ClearBattleData()
    {
        defeatedEnemies.Clear();
        totalExpGained = 0;
    }

    /// <summary>
    /// 获取击杀数
    /// </summary>
    public int GetKillCount()
    {
        return defeatedEnemies.Count;
    }
}
