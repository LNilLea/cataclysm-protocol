using MyGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗管理器 - 支持连射版本（修复版）
/// 修复内容：
/// 1. 每发连射独立输出判定日志
/// 2. 确保所有连射都正确进行命中判定
/// 3. 每次连射应用正确的命中减值
/// 4. 【新增】枪械使用固定伤害，不受属性加值和架势加成影响
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("战斗单位")]
    public Player player;
    public MonoBehaviour[] monsterObjects;

    [Header("系统组件")]
    public ActionPointSystem actionPointSystem;
    public TargetSelector targetSelector;
    public StanceSystem stanceSystem;
    public GridManager2D gridManager;

    [Header("战斗配置")]
    public int ACTION_THRESHOLD = 50;   // 先攻阈值

    [Header("状态")]
    public bool battleStarted = false;
    public bool battleEnded = false;
    public bool isPlayerTurn = false;

    [Header("自动检测设置")]
    [Tooltip("如果 monsterObjects 为空，自动查找场景中的敌人")]
    public bool autoDetectMonsters = true;

    // 战斗单位列表
    private List<BattleUnit> units = new List<BattleUnit>();

    // 当前行动的单位
    private BattleUnit currentActingUnit;

    // 玩家行动请求标记
    private bool playerActionCompleted = false;

    // 【新增】武器UI引用（用于获取连射次数）
    private WeaponInventoryUI weaponInventoryUI;

    // 只读属性
    public bool BattleEnded => battleEnded;
    public int EnemyCount => units.Count - 1;
    public bool IsPlayerTurn => isPlayerTurn;

    // 事件
    public event System.Action OnBattleStart;
    public event System.Action OnBattleEnd;
    public event System.Action<BattleUnit> OnUnitTurnStart;
    public event System.Action<BattleUnit> OnUnitTurnEnd;
    public event System.Action<string> OnBattleLog;

    private void Start()
    {
        // 自动获取组件
        if (actionPointSystem == null)
            actionPointSystem = GetComponent<ActionPointSystem>() ?? gameObject.AddComponent<ActionPointSystem>();

        if (targetSelector == null)
            targetSelector = FindObjectOfType<TargetSelector>();

        if (stanceSystem == null)
            stanceSystem = FindObjectOfType<StanceSystem>();

        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager2D>();

        // 【新增】获取武器UI
        weaponInventoryUI = FindObjectOfType<WeaponInventoryUI>();

        // 订阅事件
        if (targetSelector != null)
        {
            targetSelector.OnAttackConfirmed += HandlePlayerAttack;
        }

        if (actionPointSystem != null)
        {
            actionPointSystem.OnAllActionsUsed += OnPlayerActionsExhausted;
        }

        // 自动查找玩家
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }

        // 自动检测怪物
        if (autoDetectMonsters && (monsterObjects == null || monsterObjects.Length == 0))
        {
            AutoDetectMonsters();
        }

        // 初始化战斗
        if (player != null && monsterObjects != null && monsterObjects.Length > 0)
        {
            InitializeBattle();
        }
        else
        {
            Debug.LogWarning("[BattleManager] 无法初始化战斗：缺少玩家或怪物！" +
                $" Player: {(player != null ? "OK" : "NULL")}, " +
                $" Monsters: {(monsterObjects != null ? monsterObjects.Length.ToString() : "NULL")}");
        }
    }

    private void OnDestroy()
    {
        if (targetSelector != null)
        {
            targetSelector.OnAttackConfirmed -= HandlePlayerAttack;
        }

        if (actionPointSystem != null)
        {
            actionPointSystem.OnAllActionsUsed -= OnPlayerActionsExhausted;
        }
    }

    /// <summary>
    /// 自动检测场景中的怪物
    /// </summary>
    private void AutoDetectMonsters()
    {
        List<MonoBehaviour> foundMonsters = new List<MonoBehaviour>();

        foreach (var target in FindObjectsOfType<MonoBehaviour>())
        {
            if (target is Player) continue;

            if (target is ICombatTarget && target is IMobAction)
            {
                foundMonsters.Add(target);
                Debug.Log($"[BattleManager] 自动检测到敌人: {target.name}");
            }
        }

        monsterObjects = foundMonsters.ToArray();
        Debug.Log($"[BattleManager] 共检测到 {monsterObjects.Length} 个敌人");
    }

    /// <summary>
    /// 公开的战斗启动方法
    /// </summary>
    public void StartBattle()
    {
        if (battleStarted) return;

        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }

        if (autoDetectMonsters && (monsterObjects == null || monsterObjects.Length == 0))
        {
            AutoDetectMonsters();
        }

        if (player != null && monsterObjects != null && monsterObjects.Length > 0)
        {
            InitializeBattle();
        }
        else
        {
            Debug.LogError("[BattleManager] StartBattle 失败：缺少玩家或怪物！");
        }
    }

    /// <summary>
    /// 获取所有战斗单位
    /// </summary>
    public List<BattleUnit> GetUnits()
    {
        return units;
    }

    /// <summary>
    /// 初始化战斗
    /// </summary>
    public void InitializeBattle()
    {
        Log("===== 战斗开始 =====");

        units.Clear();
        battleStarted = true;
        battleEnded = false;

        if (player.combatData == null)
        {
            Debug.LogWarning("[BattleManager] player.combatData 为空，尝试创建...");
            player.combatData = new PlayerCombatData();
            player.combatData.currentHP = player.combatData.maxHP;
            Debug.Log($"[BattleManager] 创建 combatData 完成 - HP:{player.combatData.maxHP}, 先攻:{player.combatData.initiative}");
        }

        // 添加玩家
        units.Add(new BattleUnit(
            "玩家",
            player.combatData.initiative,
            player,
            player.GetComponent<IMobAction>(),
            true
        ));

        // 添加怪物
        foreach (var mobObj in monsterObjects)
        {
            if (mobObj == null) continue;

            ICombatTarget t = mobObj as ICombatTarget;
            IMobAction a = mobObj as IMobAction;

            if (t == null || a == null)
            {
                Debug.LogError($"[BattleManager] {mobObj.name} 未实现 ICombatTarget 或 IMobAction，跳过");
                continue;
            }

            units.Add(new BattleUnit(
                mobObj.name,
                a.GetInitiative(),
                t,
                a,
                false
            ));

            Debug.Log($"[BattleManager] 添加敌人: {mobObj.name}, 先攻: {a.GetInitiative()}");
        }

        OnBattleStart?.Invoke();
        StartCoroutine(BattleLoop());
    }

    /// <summary>
    /// 战斗主循环 - 先攻轴系统
    /// </summary>
    private IEnumerator BattleLoop()
    {
        while (!battleEnded)
        {
            yield return new WaitForSeconds(0.1f);

            foreach (var unit in units)
            {
                if (IsUnitAlive(unit))
                {
                    unit.gauge += unit.initiativePerRound;
                }
            }

            foreach (var unit in units)
            {
                if (!IsUnitAlive(unit)) continue;

                while (unit.gauge >= ACTION_THRESHOLD && !battleEnded)
                {
                    unit.gauge -= ACTION_THRESHOLD;
                    currentActingUnit = unit;

                    if (unit.isPlayer)
                    {
                        yield return StartCoroutine(PlayerTurn());
                    }
                    else
                    {
                        yield return StartCoroutine(MonsterTurn(unit));
                    }

                    CheckBattleEnd();
                }
            }
        }

        OnBattleEnd?.Invoke();
        Log("===== 战斗结束 =====");
    }

    /// <summary>
    /// 玩家回合
    /// </summary>
    private IEnumerator PlayerTurn()
    {
        isPlayerTurn = true;
        playerActionCompleted = false;

        Log("【玩家回合开始】");
        OnUnitTurnStart?.Invoke(currentActingUnit);

        actionPointSystem.StartPlayerTurn();

        if (player.combatData != null)
        {
            player.combatData.OnTurnStart();
        }

        while (!playerActionCompleted && !battleEnded)
        {
            yield return null;
        }

        if (stanceSystem != null)
        {
            stanceSystem.OnTurnEnd();
        }

        isPlayerTurn = false;
        OnUnitTurnEnd?.Invoke(currentActingUnit);
        Log("【玩家回合结束】");
    }

    /// <summary>
    /// 怪物回合
    /// </summary>
    private IEnumerator MonsterTurn(BattleUnit unit)
    {
        Log($"【{unit.name} 回合开始】");
        OnUnitTurnStart?.Invoke(unit);

        yield return new WaitForSeconds(0.5f);

        MonsterAI monsterAI = null;
        if (unit.targetComponent is MonoBehaviour mono)
        {
            monsterAI = mono.GetComponent<MonsterAI>();
        }

        if (monsterAI != null)
        {
            string actionLog = monsterAI.ExecuteTurn(player);
            Log(actionLog);
        }
        else if (unit.actionComponent != null)
        {
            string actionLog = unit.actionComponent.PerformAction(player);
            Log(actionLog);
        }

        yield return new WaitForSeconds(0.5f);

        OnUnitTurnEnd?.Invoke(unit);
        Log($"【{unit.name} 回合结束】");

        CheckBattleEnd();
    }

    /// <summary>
    /// 当玩家动作用完时
    /// </summary>
    private void OnPlayerActionsExhausted()
    {
        Log("所有动作已用完，回合自动结束");
        EndPlayerTurn();
    }

    /// <summary>
    /// 手动结束玩家回合
    /// </summary>
    public void EndPlayerTurn()
    {
        if (!isPlayerTurn) return;

        actionPointSystem.EndPlayerTurn();
        playerActionCompleted = true;
    }

    /// <summary>
    /// 【修改】处理玩家攻击（支持连射）
    /// </summary>
    private void HandlePlayerAttack(ICombatTarget target, Weapon weapon)
    {
        if (!isPlayerTurn || !actionPointSystem.CanDoMainAction())
        {
            Log("无法攻击：不是玩家回合或没有主要动作");
            return;
        }

        // 消耗主要动作
        actionPointSystem.UseMainAction();

        string log;

        // 检查是否是远程武器
        if (weapon is RangedWeapon rangedWeapon)
        {
            // 获取连射次数
            int burstCount = 1;
            if (weaponInventoryUI != null)
            {
                burstCount = weaponInventoryUI.GetPendingBurstCount();
                Debug.Log($"[BattleManager] 获取连射次数: {burstCount}, 武器最大连射: {rangedWeapon.MaxBurst}, 当前弹药: {rangedWeapon.CurrentAmmo}");
            }
            else
            {
                Debug.LogWarning("[BattleManager] weaponInventoryUI 为空，使用默认连射次数 1");
            }

            // 执行远程攻击（支持连射）
            log = PerformRangedAttack(target, rangedWeapon, burstCount);
        }
        else
        {
            // 执行近战攻击
            log = PerformPlayerAttack(target, weapon);
        }

        Log(log);

        // 刷新武器UI（更新弹药显示）
        if (weaponInventoryUI != null)
        {
            weaponInventoryUI.RefreshSlotDisplay();
        }

        // 检查战斗结束
        CheckBattleEnd();
    }

    /// <summary>
    /// 【修复版】执行远程攻击（支持连射，每发独立判定和日志输出）
    /// 
    /// 【重要】枪械伤害规则（根据设计文档）：
    /// - 伤害 = 武器固定伤害（不受属性加值、架势加成、专长加成影响！）
    /// - 命中 = d20 + (反应-3) + 命中加值 + 架势命中修正
    /// 
    /// 连射规则：
    /// - 第1发：正常命中判定
    /// - 第2发：命中 - 武器连射减值
    /// - 第3发：命中 - 武器连射减值 * 2
    /// - 以此类推
    /// </summary>
    private string PerformRangedAttack(ICombatTarget target, RangedWeapon weapon, int burstCount)
    {
        Debug.Log($"[PerformRangedAttack] 开始执行，burstCount={burstCount}, MaxBurst={weapon.MaxBurst}, CurrentAmmo={weapon.CurrentAmmo}");
        
        // 检查弹药
        if (weapon.CurrentAmmo <= 0)
        {
            return $"{weapon.Name} 没有弹药！需要换弹。";
        }

        // 确保连射次数有效
        int originalBurstCount = burstCount;
        burstCount = Mathf.Clamp(burstCount, 1, Mathf.Min(weapon.MaxBurst, weapon.CurrentAmmo));
        Debug.Log($"[PerformRangedAttack] Clamp后 burstCount={burstCount} (原始={originalBurstCount})");

        // 获取武器的连射减值
        int burstPenalty = weapon.GetEffectiveBurstPenalty();
        Debug.Log($"[PerformRangedAttack] 连射减值={burstPenalty}");

        // 【新增】获取武器固定伤害（用于显示）
        int fixedDamage = weapon.CalculateDamage(0);  // 参数不影响结果
        Debug.Log($"[PerformRangedAttack] 武器固定伤害={fixedDamage}");

        // 构建总结日志
        string summaryLog;
        if (burstCount > 1)
        {
            summaryLog = $"玩家使用 [{weapon.Name}] 对 {target.Name} 进行 {burstCount} 连射！（每发伤害: {fixedDamage}）\n";
            Debug.Log($"[Battle] 玩家使用 [{weapon.Name}] 对 {target.Name} 进行 {burstCount} 连射！");
        }
        else
        {
            summaryLog = $"玩家使用 [{weapon.Name}] 攻击 {target.Name}！\n";
        }

        int totalDamage = 0;
        int hits = 0;
        int attributeValue = GetWeaponAttribute(weapon);

        // ========== 每发独立进行判定并输出日志 ==========
        for (int i = 0; i < burstCount; i++)
        {
            Debug.Log($"[PerformRangedAttack] === 第{i + 1}发开始 ===");
            
            // 消耗弹药
            weapon.CurrentAmmo--;

            // 计算基础命中（每发重新投掷d20）
            int hitRoll = weapon.CalculateHit(attributeValue);
            Debug.Log($"[PerformRangedAttack] 第{i + 1}发基础命中值={hitRoll}");

            // 应用架势命中修正（命中可以受架势影响）
            if (stanceSystem != null)
            {
                int stanceBonus = stanceSystem.GetTotalHitModifier();
                hitRoll += stanceBonus;
                if (stanceBonus != 0)
                    Debug.Log($"[PerformRangedAttack] 架势命中修正={stanceBonus}，修正后={hitRoll}");
            }

            // 应用体魄惩罚（远程武器特有）
            int strengthPenalty = weapon.CalculateStrengthPenalty(player.combatData.strength);
            hitRoll += strengthPenalty;
            if (strengthPenalty != 0)
                Debug.Log($"[PerformRangedAttack] 体魄惩罚={strengthPenalty}，修正后={hitRoll}");

            // 应用连射命中减值（第2发开始）
            int burstPenaltyThisShot = i * burstPenalty;
            hitRoll -= burstPenaltyThisShot;
            Debug.Log($"[PerformRangedAttack] 第{i + 1}发连射减值={burstPenaltyThisShot}，最终命中={hitRoll}");

            // ========== 构建本发的判定日志 ==========
            string shotLog;
            if (burstCount > 1)
            {
                if (burstPenaltyThisShot > 0)
                {
                    shotLog = $"第{i + 1}发: 命中 {hitRoll} (连射-{burstPenaltyThisShot}) vs AC {target.CurrentAC} → ";
                }
                else
                {
                    shotLog = $"第{i + 1}发: 命中 {hitRoll} vs AC {target.CurrentAC} → ";
                }
            }
            else
            {
                shotLog = $"命中检定: {hitRoll} vs AC {target.CurrentAC} → ";
            }

            // ========== 命中判定 ==========
            if (hitRoll >= target.CurrentAC)
            {
                // 【核心修改】枪械使用固定伤害，不加任何额外加成！
                int damage = weapon.CalculateDamage(attributeValue);
                
                // 【删除】不再应用架势伤害修正
                // if (stanceSystem != null)
                // {
                //     damage += stanceSystem.GetTotalDamageModifier();
                // }

                // 【删除】不再应用专长伤害加成
                // if (player.combatData != null)
                // {
                //     player.combatData.DealDamage(ref damage);
                // }

                if (damage < 0) damage = 0;

                // 造成伤害
                target.TakeDamage(damage);
                totalDamage += damage;
                hits++;

                shotLog += $"命中！{damage}伤害";
                Debug.Log($"[PerformRangedAttack] 第{i + 1}发命中！固定伤害={damage}");
            }
            else
            {
                shotLog += "未命中";
                Debug.Log($"[PerformRangedAttack] 第{i + 1}发未命中");
            }

            // ========== 每发立即输出判定日志 ==========
            Debug.Log($"[Battle] {shotLog}");
            summaryLog += shotLog + "\n";

            // 检查目标是否死亡
            if (target.CurrentHP <= 0)
            {
                string deathLog = $"{target.Name} 被击杀！";
                Debug.Log($"[Battle] {deathLog}");
                summaryLog += deathLog + "\n";
                break;
            }
        }

        // 计算自伤（体魄不足的惩罚，只有命中时才计算）
        int selfDamage = weapon.CalculateSelfDamage(player.combatData.strength);
        int totalSelfDamage = selfDamage * hits;
        if (totalSelfDamage > 0)
        {
            player.TakeDamage(totalSelfDamage);
            string selfDamageLog = $"⚠ 后坐力造成 {totalSelfDamage} 点自伤";
            Debug.Log($"[Battle] {selfDamageLog}");
            summaryLog += selfDamageLog + "\n";
        }

        // 连射统计
        if (burstCount > 1)
        {
            string resultLog = $"连射结果: {hits}/{burstCount} 命中，共造成 {totalDamage} 点伤害";
            Debug.Log($"[Battle] {resultLog}");
            summaryLog += resultLog + "\n";
        }

        summaryLog += $"剩余弹药: {weapon.CurrentAmmo}/{weapon.MaxAmmo}";

        return summaryLog;
    }

    /// <summary>
    /// 执行玩家近战攻击
    /// 【注意】近战武器仍然受架势和专长加成影响！
    /// </summary>
    private string PerformPlayerAttack(ICombatTarget target, Weapon weapon)
    {
        string log = $"玩家使用 [{weapon.Name}] 攻击 {target.Name}！\n";

        int attributeValue = GetWeaponAttribute(weapon);
        int hitRoll = weapon.CalculateHit(attributeValue);

        if (stanceSystem != null)
        {
            hitRoll += stanceSystem.GetTotalHitModifier();
        }

        log += $"命中检定: {hitRoll} vs AC {target.CurrentAC}\n";

        if (hitRoll >= target.CurrentAC)
        {
            int damage = weapon.CalculateDamage(attributeValue);

            // 近战武器受架势伤害修正
            if (stanceSystem != null)
            {
                damage += stanceSystem.GetTotalDamageModifier();
            }

            // 近战武器受专长伤害加成
            if (player.combatData != null)
            {
                player.combatData.DealDamage(ref damage);
            }

            if (damage < 0) damage = 0;

            target.TakeDamage(damage);
            log += $"★ 命中！造成 {damage} 点伤害";
        }
        else
        {
            log += "★ 未命中";
        }

        return log;
    }

    /// <summary>
    /// 根据武器类型获取对应属性值
    /// </summary>
    private int GetWeaponAttribute(Weapon weapon)
    {
        if (weapon is BluntWeapon)
        {
            return player.combatData.strength;
        }
        else
        {
            return player.combatData.agility;
        }
    }

    /// <summary>
    /// 玩家使用武器攻击指定索引的敌人（兼容旧接口）
    /// </summary>
    public void PlayerAction_AttackEnemyIndex(int enemyIndex, out string log)
    {
        log = string.Empty;

        if (battleEnded || !isPlayerTurn)
        {
            log = "无法攻击";
            return;
        }

        if (!actionPointSystem.CanDoMainAction())
        {
            log = "没有主要动作点";
            return;
        }

        int idx = Mathf.Clamp(enemyIndex + 1, 1, units.Count - 1);
        ICombatTarget target = units[idx].targetComponent;

        if (player.currentWeapon != null)
        {
            actionPointSystem.UseMainAction();
            log = PerformPlayerAttack(target, player.currentWeapon);
            Log(log);
            CheckBattleEnd();
        }
        else
        {
            log = "没有装备武器";
        }
    }

    /// <summary>
    /// 玩家使用指定武器攻击指定目标
    /// </summary>
    public void PlayerAttackWithWeapon(ICombatTarget target, Weapon weapon, out string log)
    {
        log = string.Empty;

        if (battleEnded || !isPlayerTurn)
        {
            log = "无法攻击";
            return;
        }

        if (!actionPointSystem.CanDoMainAction())
        {
            log = "没有主要动作点";
            return;
        }

        actionPointSystem.UseMainAction();
        log = PerformPlayerAttack(target, weapon);
        Log(log);
        CheckBattleEnd();
    }

    /// <summary>
    /// 玩家移动
    /// </summary>
    public bool PlayerMove(Vector3 targetPosition)
    {
        if (!isPlayerTurn || !actionPointSystem.CanMove())
        {
            Log("无法移动：不是玩家回合或没有移动动作");
            return false;
        }

        actionPointSystem.UseMoveAction();
        Log("玩家移动");
        return true;
    }

    /// <summary>
    /// 玩家切换架势
    /// </summary>
    public bool PlayerSwitchStance(StanceType stance)
    {
        if (!isPlayerTurn || stanceSystem == null)
        {
            return false;
        }

        bool success = stanceSystem.SwitchStance(stance, actionPointSystem);
        if (success)
        {
            Log($"切换架势: {stanceSystem.GetStanceName(stance)}");
        }
        return success;
    }

    /// <summary>
    /// 获取敌人HP
    /// </summary>
    public int GetEnemyHP(int enemyIndex)
    {
        int idx = enemyIndex + 1;
        if (idx < 1 || idx >= units.Count) return 0;
        return GetMonsterHP(units[idx].targetComponent);
    }

    /// <summary>
    /// 获取怪物HP
    /// </summary>
    private int GetMonsterHP(ICombatTarget target)
    {
        var mono = target as MonoBehaviour;
        if (mono == null) return 0;

        var field = mono.GetType().GetField("currentHP");
        if (field != null)
        {
            return (int)field.GetValue(mono);
        }

        var prop = mono.GetType().GetProperty("CurrentHP");
        if (prop != null)
        {
            return (int)prop.GetValue(mono);
        }

        return 0;
    }

    /// <summary>
    /// 检查单位是否存活
    /// </summary>
    private bool IsUnitAlive(BattleUnit unit)
    {
        if (unit.isPlayer)
        {
            return player.currentHP > 0;
        }
        else
        {
            return GetMonsterHP(unit.targetComponent) > 0;
        }
    }

    /// <summary>
    /// 检查战斗结束
    /// </summary>
    private void CheckBattleEnd()
    {
        if (player.currentHP <= 0)
        {
            Log("玩家死亡，战斗失败！");
            battleEnded = true;
            return;
        }

        bool allEnemiesDead = true;
        for (int i = 1; i < units.Count; i++)
        {
            if (GetMonsterHP(units[i].targetComponent) > 0)
            {
                allEnemiesDead = false;
                break;
            }
        }

        if (allEnemiesDead)
        {
            player.combatData.SetEnemyAllDead(true);
            Log("所有敌人被消灭，战斗胜利！");
            battleEnded = true;

            if (stanceSystem != null)
            {
                stanceSystem.ClearAllBuffs();
            }
        }
    }

    /// <summary>
    /// 输出战斗日志
    /// </summary>
    private void Log(string message)
    {
        Debug.Log($"[Battle] {message}");
        OnBattleLog?.Invoke(message);
    }
}
