using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MyGame;

/// <summary>
/// 战斗UI - 完整版本
/// 包含：HP显示、动作点、武器选择、目标选择、架势切换、回合结束
/// </summary>
public class BattleUI : MonoBehaviour
{
    [Header("引用")]
    public BattleManager battleManager;
    public ActionPointSystem actionPointSystem;
    public TargetSelector targetSelector;
    public StanceSystem stanceSystem;
    public Player player;
    public WeaponManager weaponManager;

    [Header("HP 显示")]
    public TMP_Text playerHPText;
    public TMP_Text enemyHPText;
    public Image playerHPBar;
    public Image enemyHPBar;

    [Header("动作点显示")]
    public TMP_Text actionPointsText;
    public Image moveActionIcon;
    public Image mainActionIcon;
    public Image minorActionIcon1;
    public Image minorActionIcon2;

    [Header("回合提示")]
    public TMP_Text turnHintText;
    public GameObject playerTurnPanel;      // 玩家回合时显示的面板

    [Header("武器列表")]
    public Transform weaponListContainer;   // 武器按钮的父物体
    public GameObject weaponButtonPrefab;   // 武器按钮预制体
    private List<GameObject> weaponButtons = new List<GameObject>();

    [Header("架势按钮")]
    public Button stanceDefensiveBtn;
    public Button stanceAggressiveBtn;
    public Button stanceFocusedBtn;
    public Button stanceEvasiveBtn;
    public TMP_Text currentStanceText;

    [Header("控制按钮")]
    public Button endTurnButton;            // 结束回合按钮
    public Button cancelButton;             // 取消选择按钮
    public Button moveButton;               // 移动按钮

    [Header("移动系统")]
    public BattleMoveSystem moveSystem;

    [Header("战斗日志")]
    public TMP_Text battleLogText;
    public ScrollRect battleLogScrollRect;
    private List<string> battleLogs = new List<string>();
    public int maxLogLines = 50;

    [Header("先攻条")]
    public Image playerInitiativeBar;
    public Image enemyInitiativeBar;
    public TMP_Text playerInitiativeText;
    public TMP_Text enemyInitiativeText;

    [Header("目标选择提示")]
    public GameObject targetSelectionPanel;
    public TMP_Text targetSelectionHint;

    [Header("颜色配置")]
    public Color actionAvailableColor = Color.green;
    public Color actionUsedColor = Color.gray;

    private void Start()
    {
        // 自动获取引用
        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();

        if (actionPointSystem == null)
            actionPointSystem = FindObjectOfType<ActionPointSystem>();

        if (targetSelector == null)
            targetSelector = FindObjectOfType<TargetSelector>();

        if (stanceSystem == null)
            stanceSystem = FindObjectOfType<StanceSystem>();

        if (player == null)
            player = FindObjectOfType<Player>();

        if (weaponManager == null && player != null)
            weaponManager = player.GetComponent<WeaponManager>();

        if (moveSystem == null)
            moveSystem = FindObjectOfType<BattleMoveSystem>();

        // 绑定按钮事件
        SetupButtons();

        // 订阅事件
        SubscribeEvents();

        // 初始化UI
        RefreshUI();
        RefreshWeaponList();

        // 初始隐藏某些面板
        if (targetSelectionPanel != null)
            targetSelectionPanel.SetActive(false);

        if (playerTurnPanel != null)
            playerTurnPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void Update()
    {
        RefreshUI();
        UpdateInitiativeBars();
    }

    /// <summary>
    /// 设置按钮事件
    /// </summary>
    private void SetupButtons()
    {
        // 结束回合按钮
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
        }

        // 取消按钮
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        // 移动按钮
        if (moveButton != null)
        {
            moveButton.onClick.AddListener(OnMoveClicked);
        }

        // 架势按钮
        if (stanceDefensiveBtn != null)
            stanceDefensiveBtn.onClick.AddListener(() => OnStanceClicked(StanceType.Defensive));

        if (stanceAggressiveBtn != null)
            stanceAggressiveBtn.onClick.AddListener(() => OnStanceClicked(StanceType.Aggressive));

        if (stanceFocusedBtn != null)
            stanceFocusedBtn.onClick.AddListener(() => OnStanceClicked(StanceType.Focused));

        if (stanceEvasiveBtn != null)
            stanceEvasiveBtn.onClick.AddListener(() => OnStanceClicked(StanceType.Evasive));
    }

    /// <summary>
    /// 订阅事件
    /// </summary>
    private void SubscribeEvents()
    {
        if (battleManager != null)
        {
            battleManager.OnBattleLog += AddBattleLog;
            battleManager.OnUnitTurnStart += OnUnitTurnStart;
            battleManager.OnUnitTurnEnd += OnUnitTurnEnd;
        }

        if (actionPointSystem != null)
        {
            actionPointSystem.OnActionPointsChanged += RefreshActionPoints;
        }

        if (targetSelector != null)
        {
            targetSelector.OnSelectionCancelled += OnTargetSelectionCancelled;
        }

        if (stanceSystem != null)
        {
            stanceSystem.OnStanceChanged += OnStanceChanged;
        }

        if (moveSystem != null)
        {
            moveSystem.OnMoveStart += OnMoveStart;
            moveSystem.OnMoveComplete += OnMoveComplete;
        }
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeEvents()
    {
        if (battleManager != null)
        {
            battleManager.OnBattleLog -= AddBattleLog;
            battleManager.OnUnitTurnStart -= OnUnitTurnStart;
            battleManager.OnUnitTurnEnd -= OnUnitTurnEnd;
        }

        if (actionPointSystem != null)
        {
            actionPointSystem.OnActionPointsChanged -= RefreshActionPoints;
        }

        if (targetSelector != null)
        {
            targetSelector.OnSelectionCancelled -= OnTargetSelectionCancelled;
        }

        if (stanceSystem != null)
        {
            stanceSystem.OnStanceChanged -= OnStanceChanged;
        }

        if (moveSystem != null)
        {
            moveSystem.OnMoveStart -= OnMoveStart;
            moveSystem.OnMoveComplete -= OnMoveComplete;
        }
    }

    /// <summary>
    /// 刷新UI
    /// </summary>
    private void RefreshUI()
    {
        RefreshHP();
        RefreshActionPoints();
        RefreshTurnHint();
        RefreshStanceDisplay();
        RefreshButtonStates();
    }

    /// <summary>
    /// 刷新HP显示
    /// </summary>
    private void RefreshHP()
    {
        if (player != null && playerHPText != null)
        {
            playerHPText.text = $"HP: {player.currentHP}/{player.combatData.maxHP}";

            if (playerHPBar != null)
            {
                playerHPBar.fillAmount = (float)player.currentHP / player.combatData.maxHP;
            }
        }

        if (battleManager != null && enemyHPText != null)
        {
            int enemyHP = battleManager.GetEnemyHP(0);
            enemyHPText.text = $"敌人 HP: {enemyHP}";
        }
    }

    /// <summary>
    /// 刷新动作点显示
    /// </summary>
    private void RefreshActionPoints()
    {
        if (actionPointSystem == null) return;

        if (actionPointsText != null)
        {
            actionPointsText.text = actionPointSystem.GetStatusText();
        }

        // 更新动作图标颜色
        if (moveActionIcon != null)
            moveActionIcon.color = actionPointSystem.currentMoveActions > 0 ? actionAvailableColor : actionUsedColor;

        if (mainActionIcon != null)
            mainActionIcon.color = actionPointSystem.currentMainActions > 0 ? actionAvailableColor : actionUsedColor;

        if (minorActionIcon1 != null)
            minorActionIcon1.color = actionPointSystem.currentMinorActions >= 1 ? actionAvailableColor : actionUsedColor;

        if (minorActionIcon2 != null)
            minorActionIcon2.color = actionPointSystem.currentMinorActions >= 2 ? actionAvailableColor : actionUsedColor;
    }

    /// <summary>
    /// 刷新回合提示
    /// </summary>
    private void RefreshTurnHint()
    {
        if (turnHintText == null) return;

        if (battleManager == null || battleManager.BattleEnded)
        {
            turnHintText.text = "战斗结束";
        }
        else if (battleManager.IsPlayerTurn)
        {
            if (moveSystem != null && moveSystem.isSelectingMoveTarget)
            {
                turnHintText.text = $"选择移动目标（可移动 {moveSystem.remainingMoveSquares} 格，右键取消）";
            }
            else if (moveSystem != null && moveSystem.isMoving)
            {
                turnHintText.text = "移动中...";
            }
            else if (targetSelector != null && targetSelector.isSelectingTarget)
            {
                turnHintText.text = "选择攻击目标（右键取消）";
            }
            else
            {
                turnHintText.text = "你的回合 - 选择武器攻击、移动或切换架势";
            }
        }
        else
        {
            turnHintText.text = "敌人回合...";
        }

        // 显示/隐藏玩家回合面板
        if (playerTurnPanel != null)
        {
            playerTurnPanel.SetActive(battleManager != null && battleManager.IsPlayerTurn && !battleManager.BattleEnded);
        }
    }

    /// <summary>
    /// 刷新架势显示
    /// </summary>
    private void RefreshStanceDisplay()
    {
        if (stanceSystem == null || currentStanceText == null) return;

        currentStanceText.text = $"当前架势: {stanceSystem.GetStanceName(stanceSystem.currentStance)}";
    }

    /// <summary>
    /// 刷新按钮状态
    /// </summary>
    private void RefreshButtonStates()
    {
        bool isPlayerTurn = battleManager != null && battleManager.IsPlayerTurn && !battleManager.BattleEnded;
        bool canDoMain = actionPointSystem != null && actionPointSystem.CanDoMainAction();
        bool canDoMinor = actionPointSystem != null && actionPointSystem.CanDoMinorAction();
        bool canMove = actionPointSystem != null && actionPointSystem.CanMove();
        bool isSelecting = targetSelector != null && targetSelector.isSelectingTarget;
        bool isSelectingMove = moveSystem != null && moveSystem.isSelectingMoveTarget;
        bool isMoving = moveSystem != null && moveSystem.isMoving;
        bool isBusy = isSelecting || isSelectingMove || isMoving;

        // 武器按钮
        foreach (var btn in weaponButtons)
        {
            Button b = btn.GetComponent<Button>();
            if (b != null)
            {
                b.interactable = isPlayerTurn && canDoMain && !isBusy;
            }
        }

        // 移动按钮
        if (moveButton != null)
            moveButton.interactable = isPlayerTurn && canMove && !isBusy;

        // 架势按钮
        if (stanceDefensiveBtn != null)
            stanceDefensiveBtn.interactable = isPlayerTurn && canDoMinor && !isBusy;

        if (stanceAggressiveBtn != null)
            stanceAggressiveBtn.interactable = isPlayerTurn && canDoMinor && !isBusy;

        if (stanceFocusedBtn != null)
            stanceFocusedBtn.interactable = isPlayerTurn && canDoMinor && !isBusy;

        if (stanceEvasiveBtn != null)
            stanceEvasiveBtn.interactable = isPlayerTurn && canDoMinor && !isBusy;

        // 结束回合按钮
        if (endTurnButton != null)
            endTurnButton.interactable = isPlayerTurn && !isBusy;

        // 取消按钮（选择目标或选择移动时显示）
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(isSelecting || isSelectingMove);

        // 目标选择面板
        if (targetSelectionPanel != null)
            targetSelectionPanel.SetActive(isSelecting || isSelectingMove);
    }

    /// <summary>
    /// 刷新武器列表
    /// </summary>
    public void RefreshWeaponList()
    {
        // 清除旧按钮
        foreach (var btn in weaponButtons)
        {
            Destroy(btn);
        }
        weaponButtons.Clear();

        if (weaponListContainer == null || weaponButtonPrefab == null) return;

        // 获取玩家的武器
        List<Weapon> weapons = new List<Weapon>();

        // 如果有 WeaponManager，从中获取武器
        if (weaponManager != null)
        {
            // 通过反射获取 inventory（因为是 private）
            var field = weaponManager.GetType().GetField("inventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                weapons = field.GetValue(weaponManager) as List<Weapon>;
            }
        }

        // 如果没有武器但玩家有当前武器
        if ((weapons == null || weapons.Count == 0) && player != null && player.currentWeapon != null)
        {
            weapons = new List<Weapon> { player.currentWeapon };
        }

        // 创建武器按钮
        if (weapons != null)
        {
            foreach (var weapon in weapons)
            {
                CreateWeaponButton(weapon);
            }
        }
    }

    /// <summary>
    /// 创建武器按钮
    /// </summary>
    private void CreateWeaponButton(Weapon weapon)
    {
        if (weapon == null) return;

        GameObject btnObj = Instantiate(weaponButtonPrefab, weaponListContainer);
        weaponButtons.Add(btnObj);

        // 设置按钮文本
        TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
        if (btnText != null)
        {
            btnText.text = $"{weapon.Name}\n伤害:{weapon.DamageRange.x}-{weapon.DamageRange.y} 范围:{weapon.AttackRangeMin}-{weapon.AttackRangeMax}格";
        }

        // 绑定点击事件
        Button btn = btnObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => OnWeaponClicked(weapon));
        }
    }

    /// <summary>
    /// 武器按钮点击
    /// </summary>
    private void OnWeaponClicked(Weapon weapon)
    {
        if (targetSelector == null) return;

        // 开始目标选择
        targetSelector.StartTargetSelection(weapon);

        if (targetSelectionHint != null)
        {
            targetSelectionHint.text = $"使用 [{weapon.Name}] - 点击目标进行攻击";
        }

        AddBattleLog($"选择武器: {weapon.Name}，请选择目标");
    }

    /// <summary>
    /// 架势按钮点击
    /// </summary>
    private void OnStanceClicked(StanceType stance)
    {
        if (battleManager != null)
        {
            battleManager.PlayerSwitchStance(stance);
        }
    }

    /// <summary>
    /// 结束回合按钮点击
    /// </summary>
    private void OnEndTurnClicked()
    {
        if (battleManager != null)
        {
            battleManager.EndPlayerTurn();
        }
    }

    /// <summary>
    /// 取消按钮点击
    /// </summary>
    private void OnCancelClicked()
    {
        // 取消目标选择
        if (targetSelector != null && targetSelector.isSelectingTarget)
        {
            targetSelector.CancelSelection();
        }

        // 取消移动选择
        if (moveSystem != null && moveSystem.isSelectingMoveTarget)
        {
            moveSystem.CancelMoveSelection();
        }
    }

    /// <summary>
    /// 移动按钮点击
    /// </summary>
    private void OnMoveClicked()
    {
        if (moveSystem == null) return;

        bool success = moveSystem.StartMoveSelection();
        if (success)
        {
            AddBattleLog($"选择移动目标，可移动 {moveSystem.remainingMoveSquares} 格");

            if (targetSelectionHint != null)
            {
                targetSelectionHint.text = $"点击绿色格子移动（可移动 {moveSystem.remainingMoveSquares} 格）";
            }
        }
    }

    /// <summary>
    /// 移动开始事件
    /// </summary>
    private void OnMoveStart()
    {
        AddBattleLog("开始移动");
    }

    /// <summary>
    /// 移动完成事件
    /// </summary>
    private void OnMoveComplete()
    {
        AddBattleLog("移动完成");
    }

    /// <summary>
    /// 更新先攻条
    /// </summary>
    private void UpdateInitiativeBars()
    {
        if (battleManager == null) return;

        var units = battleManager.GetUnits();
        if (units.Count < 2) return;

        // 玩家先攻条
        if (playerInitiativeBar != null)
        {
            float progress = Mathf.Clamp01((float)units[0].gauge / battleManager.ACTION_THRESHOLD);
            playerInitiativeBar.fillAmount = progress;
        }

        if (playerInitiativeText != null)
        {
            playerInitiativeText.text = $"{units[0].gauge}/50";
        }

        // 敌人先攻条（显示第一个敌人）
        if (enemyInitiativeBar != null && units.Count > 1)
        {
            float progress = Mathf.Clamp01((float)units[1].gauge / battleManager.ACTION_THRESHOLD);
            enemyInitiativeBar.fillAmount = progress;
        }

        if (enemyInitiativeText != null && units.Count > 1)
        {
            enemyInitiativeText.text = $"{units[1].gauge}/50";
        }
    }

    /// <summary>
    /// 添加战斗日志
    /// </summary>
    public void AddBattleLog(string message)
    {
        battleLogs.Add(message);

        // 限制日志行数
        while (battleLogs.Count > maxLogLines)
        {
            battleLogs.RemoveAt(0);
        }

        // 更新显示
        if (battleLogText != null)
        {
            battleLogText.text = string.Join("\n", battleLogs);
        }

        // 滚动到底部
        if (battleLogScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            battleLogScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    /// <summary>
    /// 单位回合开始事件
    /// </summary>
    private void OnUnitTurnStart(BattleUnit unit)
    {
        if (unit.isPlayer)
        {
            RefreshWeaponList();
        }
    }

    /// <summary>
    /// 单位回合结束事件
    /// </summary>
    private void OnUnitTurnEnd(BattleUnit unit)
    {
        // 可以在这里添加回合结束的UI效果
    }

    /// <summary>
    /// 目标选择取消事件
    /// </summary>
    private void OnTargetSelectionCancelled()
    {
        AddBattleLog("取消目标选择");
    }

    /// <summary>
    /// 架势变化事件
    /// </summary>
    private void OnStanceChanged(StanceType newStance)
    {
        RefreshStanceDisplay();
    }
}
