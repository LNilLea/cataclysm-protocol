using UnityEngine;
using MyGame;
using System.Reflection;

/// <summary>
/// 战斗测试初始化 - 确保战斗能正常运行
/// 用于直接测试战斗场景（不经过角色创建）
/// </summary>
public class BattleTestInit : MonoBehaviour
{
    [Header("测试设置")]
    public bool initializeOnStart = true;
    public bool forcePlayerTurn = true;

    [Header("玩家属性（测试用）")]
    public int testHP = 100;
    public int testMobility = 3;
    public int testAC = 12;

    [Header("测试武器")]
    public bool giveTestWeapons = true;
    public WeaponChoice[] testWeapons = { WeaponChoice.匕首, WeaponChoice.手枪 };

    [Header("引用（自动获取）")]
    public Player player;
    public BattleManager battleManager;
    public ActionPointSystem actionPointSystem;
    public BattleMoveSystem2D moveSystem;

    private void Start()
    {
        if (initializeOnStart)
        {
            Initialize();
        }
    }

    [ContextMenu("初始化战斗")]
    public void Initialize()
    {
        Debug.Log("========== 战斗测试初始化 ==========");

        // 获取引用
        if (player == null) player = FindObjectOfType<Player>();
        if (battleManager == null) battleManager = FindObjectOfType<BattleManager>();
        if (actionPointSystem == null) actionPointSystem = FindObjectOfType<ActionPointSystem>();
        if (moveSystem == null) moveSystem = FindObjectOfType<BattleMoveSystem2D>();

        // 1. 初始化角色数据
        InitializeCharacterData();

        // 2. 初始化玩家
        InitializePlayer();

        // 3. 初始化武器
        InitializeWeapons();

        // 4. 初始化动作点
        InitializeActionPoints();

        // 5. 设置玩家回合
        if (forcePlayerTurn)
        {
            SetPlayerTurn();
        }

        Debug.Log("========== 初始化完成 ==========");
    }

    void InitializeCharacterData()
    {
        // 如果角色数据未初始化，使用测试数据
        if (!CharacterData.IsInitialized)
        {
            CharacterData.MaxHP = testHP;
            CharacterData.CurrentHP = testHP;
            CharacterData.Mobility = testMobility;
            CharacterData.AC = testAC;
            CharacterData.IsInitialized = true;

            Debug.Log($"[BattleTestInit] 角色数据初始化: HP={testHP}, 移动力={testMobility}, AC={testAC}");
        }
        else
        {
            Debug.Log($"[BattleTestInit] 角色数据已存在: HP={CharacterData.CurrentHP}/{CharacterData.MaxHP}, 移动力={CharacterData.Mobility}");
        }
    }

    void InitializePlayer()
    {
        if (player == null)
        {
            Debug.LogError("[BattleTestInit] 找不到 Player！");
            return;
        }

        // 确保 combatData 存在
        if (player.combatData == null)
        {
            Debug.LogWarning("[BattleTestInit] Player.combatData 为空，尝试创建...");
            // 可能需要在 Player 类中处理
        }
        else
        {
            // 从 CharacterData 应用数据
            player.combatData.maxHP = CharacterData.MaxHP;
            player.combatData.currentHP = CharacterData.CurrentHP;
            player.combatData.mobility = CharacterData.Mobility;

            Debug.Log($"[BattleTestInit] 玩家数据: HP={player.combatData.currentHP}/{player.combatData.maxHP}, 移动力={player.combatData.mobility}");
        }
    }

    void InitializeWeapons()
    {
        if (!giveTestWeapons) return;

        // 如果背包为空，给测试武器
        if (PlayerInventoryData.GetWeaponCount() == 0)
        {
            foreach (var weapon in testWeapons)
            {
                PlayerInventoryData.AddWeapon(weapon);
                Debug.Log($"[BattleTestInit] 添加武器: {weapon}");
            }
        }

        // 装备第一把武器
        WeaponManager wm = player?.GetComponent<WeaponManager>();
        if (wm != null && PlayerInventoryData.GetWeaponCount() > 0)
        {
            wm.SwitchToWeapon(0);
            Debug.Log("[BattleTestInit] 已装备武器");
        }
    }

    void InitializeActionPoints()
    {
        if (actionPointSystem == null)
        {
            Debug.LogError("[BattleTestInit] 找不到 ActionPointSystem！");
            return;
        }

        // 开始玩家回合（这会重置动作点）
        actionPointSystem.StartPlayerTurn();
        Debug.Log($"[BattleTestInit] 动作点已重置");
    }

    void SetPlayerTurn()
    {
        if (battleManager == null)
        {
            Debug.LogError("[BattleTestInit] 找不到 BattleManager！");
            return;
        }

        // 使用反射设置 isPlayerTurn（因为 IsPlayerTurn 是只读属性）
        var field = typeof(BattleManager).GetField("isPlayerTurn", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(battleManager, true);
            Debug.Log("[BattleTestInit] 设置为玩家回合");
        }
        else
        {
            Debug.LogWarning("[BattleTestInit] 无法找到 isPlayerTurn 字段");
        }
    }

    // 调试用
    private void Update()
    {
        // 按 F5 重新初始化
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Initialize();
        }

        // 按 F1 显示状态
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ShowDebugInfo();
        }
    }

    [ContextMenu("显示调试信息")]
    void ShowDebugInfo()
    {
        Debug.Log("========== 调试信息 ==========");
        Debug.Log($"CharacterData: HP={CharacterData.CurrentHP}/{CharacterData.MaxHP}, 移动力={CharacterData.Mobility}, 初始化={CharacterData.IsInitialized}");
        
        if (player != null && player.combatData != null)
        {
            Debug.Log($"Player: HP={player.combatData.currentHP}/{player.combatData.maxHP}, 移动力={player.combatData.mobility}");
        }
        
        if (actionPointSystem != null)
        {
            Debug.Log($"ActionPoints: 移动={actionPointSystem.CanMove()}, 主要={actionPointSystem.CanDoMainAction()}, 次要={actionPointSystem.CanDoMinorAction()}");
        }
        
        if (battleManager != null)
        {
            Debug.Log($"BattleManager: 玩家回合={battleManager.IsPlayerTurn}");
        }

        if (moveSystem != null)
        {
            Debug.Log($"MoveSystem: 选择中={moveSystem.isSelectingMoveTarget}, 移动中={moveSystem.isMoving}");
        }
        
        Debug.Log($"武器数量: {PlayerInventoryData.GetWeaponCount()}");
        Debug.Log("================================");
    }
}
