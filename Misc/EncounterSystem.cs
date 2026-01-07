using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MyGame;

/// <summary>
/// 遭遇系统 - 管理探索模式下的战斗触发
/// </summary>
public class EncounterSystem : MonoBehaviour
{
    public static EncounterSystem Instance { get; private set; }

    [Header("引用")]
    public Player player;
    public PlayerVision playerVision;
    public GameModeManager gameModeManager;
    public BattleManager battleManager;

    [Header("设置")]
    public float encounterCooldown = 1f;        // 遭遇冷却时间（防止连续触发）

    [Header("状态")]
    public bool isInEncounter = false;          // 是否正在遭遇战斗
    public bool wasAmbushed = false;            // 是否被偷袭

    // 当前遭遇的怪物列表
    private List<MonsterPatrol> encounteredMonsters = new List<MonsterPatrol>();

    // 场景中所有的怪物
    private List<MonsterPatrol> allMonsters = new List<MonsterPatrol>();

    // 冷却计时器
    private float cooldownTimer = 0f;

    // 事件
    public event System.Action<List<MonsterPatrol>, bool> OnEncounterStart; // 参数：怪物列表，是否被偷袭
    public event System.Action OnEncounterEnd;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 自动获取引用
        if (player == null)
            player = FindObjectOfType<Player>();

        if (playerVision == null)
            playerVision = FindObjectOfType<PlayerVision>();

        if (gameModeManager == null)
            gameModeManager = FindObjectOfType<GameModeManager>();

        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();

        // 查找所有怪物并订阅事件
        RegisterAllMonsters();

        // 订阅玩家视野事件
        if (playerVision != null)
        {
            playerVision.OnMonsterSpotted += OnPlayerSpottedMonster;
        }
    }

    private void OnDestroy()
    {
        // 取消订阅
        if (playerVision != null)
        {
            playerVision.OnMonsterSpotted -= OnPlayerSpottedMonster;
        }

        UnregisterAllMonsters();
    }

    private void Update()
    {
        // 更新冷却
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 注册所有怪物
    /// </summary>
    public void RegisterAllMonsters()
    {
        UnregisterAllMonsters();

        MonsterPatrol[] monsters = FindObjectsOfType<MonsterPatrol>();
        foreach (var monster in monsters)
        {
            RegisterMonster(monster);
        }

        Debug.Log($"注册了 {allMonsters.Count} 个怪物");
    }

    /// <summary>
    /// 注册单个怪物
    /// </summary>
    public void RegisterMonster(MonsterPatrol monster)
    {
        if (allMonsters.Contains(monster)) return;

        allMonsters.Add(monster);

        // 订阅怪物事件
        monster.OnPlayerDetected += OnMonsterDetectedPlayer;
        monster.OnAmbushAttack += OnMonsterAmbushPlayer;
    }

    /// <summary>
    /// 取消注册所有怪物
    /// </summary>
    private void UnregisterAllMonsters()
    {
        foreach (var monster in allMonsters)
        {
            if (monster != null)
            {
                monster.OnPlayerDetected -= OnMonsterDetectedPlayer;
                monster.OnAmbushAttack -= OnMonsterAmbushPlayer;
            }
        }
        allMonsters.Clear();
    }

    /// <summary>
    /// 玩家发现怪物
    /// </summary>
    private void OnPlayerSpottedMonster(GameObject monsterObj)
    {
        if (isInEncounter || cooldownTimer > 0) return;

        MonsterPatrol monster = monsterObj.GetComponent<MonsterPatrol>();
        if (monster == null) return;

        Debug.Log($"玩家发现怪物: {monsterObj.name}");

        // 触发正常战斗
        StartEncounter(monster, false);
    }

    /// <summary>
    /// 怪物发现玩家（玩家也看到了怪物）
    /// </summary>
    private void OnMonsterDetectedPlayer(MonsterPatrol monster)
    {
        if (isInEncounter || cooldownTimer > 0) return;

        Debug.Log($"怪物 {monster.name} 发现玩家，双方对视");

        // 触发正常战斗
        StartEncounter(monster, false);
    }

    /// <summary>
    /// 怪物偷袭玩家
    /// </summary>
    private void OnMonsterAmbushPlayer(MonsterPatrol monster)
    {
        if (isInEncounter || cooldownTimer > 0) return;

        Debug.Log($"怪物 {monster.name} 偷袭玩家！");

        // 触发偷袭战斗
        StartEncounter(monster, true);
    }

    /// <summary>
    /// 开始遭遇战斗
    /// </summary>
    private void StartEncounter(MonsterPatrol initialMonster, bool isAmbush)
    {
        isInEncounter = true;
        wasAmbushed = isAmbush;
        cooldownTimer = encounterCooldown;

        encounteredMonsters.Clear();
        encounteredMonsters.Add(initialMonster);

        // 检查附近是否有其他怪物也应该加入战斗
        GatherNearbyMonsters(initialMonster.transform.position);

        // 让所有参战怪物进入战斗状态
        foreach (var monster in encounteredMonsters)
        {
            monster.EnterCombat();
        }

        Debug.Log($"遭遇战斗开始！怪物数量: {encounteredMonsters.Count}, 偷袭: {isAmbush}");

        // 切换到战斗模式
        if (gameModeManager != null)
        {
            gameModeManager.SwitchToCombatMode();
        }

        // 触发事件
        OnEncounterStart?.Invoke(encounteredMonsters, isAmbush);

        // 处理偷袭
        if (isAmbush)
        {
            StartCoroutine(HandleAmbushAttack(initialMonster));
        }
        else
        {
            // 正常战斗，初始化 BattleManager
            InitializeBattle();
        }
    }

    /// <summary>
    /// 收集附近的怪物加入战斗
    /// </summary>
    private void GatherNearbyMonsters(Vector3 encounterPosition)
    {
        float gatherRadius = 10f; // 附近怪物加入战斗的范围

        foreach (var monster in allMonsters)
        {
            if (encounteredMonsters.Contains(monster)) continue;

            float distance = Vector3.Distance(monster.transform.position, encounterPosition);
            if (distance <= gatherRadius)
            {
                encounteredMonsters.Add(monster);
                Debug.Log($"附近怪物 {monster.name} 加入战斗");
            }
        }
    }

    /// <summary>
    /// 处理偷袭攻击
    /// </summary>
    private IEnumerator HandleAmbushAttack(MonsterPatrol ambusher)
    {
        Debug.Log($"{ambusher.name} 进行偷袭攻击！");

        // 显示偷袭提示（可以在这里添加 UI 提示）
        yield return new WaitForSeconds(0.5f);

        // 执行偷袭攻击
        IMobAction mobAction = ambusher.GetComponent<IMobAction>();
        if (mobAction != null && player != null)
        {
            string attackLog = mobAction.PerformAction(player);
            Debug.Log($"偷袭攻击: {attackLog}");
        }

        yield return new WaitForSeconds(0.5f);

        // 偷袭攻击后，初始化正常战斗
        InitializeBattle();
    }

    /// <summary>
    /// 初始化战斗
    /// </summary>
    private void InitializeBattle()
    {
        if (battleManager == null)
        {
            Debug.LogError("EncounterSystem: 找不到 BattleManager！");
            return;
        }

        // 设置战斗管理器的怪物列表
        List<MonoBehaviour> monsterBehaviours = new List<MonoBehaviour>();
        foreach (var monster in encounteredMonsters)
        {
            monsterBehaviours.Add(monster.GetComponent<MonoBehaviour>());
        }

        battleManager.monsterObjects = monsterBehaviours.ToArray();
        battleManager.player = player;

        // 初始化战斗
        battleManager.InitializeBattle();
    }

    /// <summary>
    /// 结束遭遇战斗
    /// </summary>
    public void EndEncounter()
    {
        if (!isInEncounter) return;

        isInEncounter = false;
        wasAmbushed = false;

        // 让所有怪物退出战斗状态（存活的）
        foreach (var monster in encounteredMonsters)
        {
            if (monster != null)
            {
                monster.ExitCombat();
            }
        }

        encounteredMonsters.Clear();

        // 切换回探索模式
        if (gameModeManager != null)
        {
            gameModeManager.SwitchToExplorationMode();
        }

        OnEncounterEnd?.Invoke();

        Debug.Log("遭遇战斗结束");
    }

    /// <summary>
    /// 获取当前遭遇的怪物列表
    /// </summary>
    public List<MonsterPatrol> GetEncounteredMonsters()
    {
        return new List<MonsterPatrol>(encounteredMonsters);
    }

    /// <summary>
    /// 移除已死亡的怪物
    /// </summary>
    public void RemoveDeadMonster(MonsterPatrol monster)
    {
        if (encounteredMonsters.Contains(monster))
        {
            encounteredMonsters.Remove(monster);
        }

        if (allMonsters.Contains(monster))
        {
            monster.OnPlayerDetected -= OnMonsterDetectedPlayer;
            monster.OnAmbushAttack -= OnMonsterAmbushPlayer;
            allMonsters.Remove(monster);
        }
    }
}
