using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MyGame;

/// <summary>
/// 武器背包UI - 简化版（配合 BurstUI 使用）
/// 支持主手/副手武器，切换消耗次要动作
/// 连射功能由 BurstUI 独立处理
/// </summary>
public class WeaponInventoryUI : MonoBehaviour
{
    [Header("主手槽位")]
    public Button mainHandSlot;
    public TMP_Text mainHandText;
    public Image mainHandIcon;

    [Header("副手槽位")]
    public Button offHandSlot;
    public TMP_Text offHandText;
    public Image offHandIcon;

    [Header("武器选单")]
    public GameObject weaponMenuPanel;
    public Transform weaponMenuContent;
    public GameObject weaponMenuItemPrefab;
    public Button closeMenuButton;

    [Header("攻击按钮")]
    public Button attackWithMainButton;
    public Button attackWithOffButton;

    [Header("确认弹窗 - 替换远程武器")]
    public GameObject confirmPanel;
    public TMP_Text confirmText;
    public Button confirmYesButton;
    public Button confirmNoButton;

    [Header("引用")]
    public Player player;
    public WeaponManager weaponManager;
    public ActionPointSystem actionPointSystem;
    public TargetSelector targetSelector;
    public BattleManager battleManager;
    public BurstUI burstUI;  // 【新增】连射UI引用

    [Header("颜色")]
    public Color emptySlotColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public Color filledSlotColor = Color.white;

    // 当前装备的武器
    private Weapon mainHandWeapon;
    private Weapon offHandWeapon;

    // 当前正在选择的槽位
    private int selectingSlot = -1;

    // 武器菜单项列表
    private List<GameObject> menuItems = new List<GameObject>();

    // 等待攻击的槽位
    private int pendingAttackSlot = -1;

    // 等待确认的武器（用于替换远程武器）
    private Weapon pendingReplaceWeapon;

    // 事件
    public event System.Action<Weapon, int> OnWeaponEquipped;

    private void Start()
    {
        // 自动获取引用
        if (player == null)
            player = FindObjectOfType<Player>();
        if (weaponManager == null)
            weaponManager = FindObjectOfType<WeaponManager>();
        if (actionPointSystem == null)
            actionPointSystem = FindObjectOfType<ActionPointSystem>();
        if (targetSelector == null)
            targetSelector = FindObjectOfType<TargetSelector>();
        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();
        if (burstUI == null)
            burstUI = FindObjectOfType<BurstUI>();

        SetupButtons();

        if (weaponMenuPanel != null)
            weaponMenuPanel.SetActive(false);

        InitializeSlots();

        // 监听目标选择事件
        if (targetSelector != null)
        {
            targetSelector.OnAttackConfirmed += OnTargetConfirmed;
        }
    }

    private void OnDestroy()
    {
        if (targetSelector != null)
        {
            targetSelector.OnAttackConfirmed -= OnTargetConfirmed;
        }
    }

    /// <summary>
    /// 设置按钮事件
    /// </summary>
    private void SetupButtons()
    {
        // 武器槽点击
        if (mainHandSlot != null)
            mainHandSlot.onClick.AddListener(() => OpenWeaponMenu(0));
        if (offHandSlot != null)
            offHandSlot.onClick.AddListener(() => OpenWeaponMenu(1));

        // 关闭选单
        if (closeMenuButton != null)
            closeMenuButton.onClick.AddListener(CloseWeaponMenu);

        // 攻击按钮
        if (attackWithMainButton != null)
            attackWithMainButton.onClick.AddListener(() => StartAttackWithSlot(0));
        if (attackWithOffButton != null)
            attackWithOffButton.onClick.AddListener(() => StartAttackWithSlot(1));

        // 确认弹窗按钮
        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(OnConfirmReplaceYes);
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(OnConfirmReplaceNo);

        // 初始隐藏确认弹窗
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    /// <summary>
    /// 初始化武器槽位
    /// </summary>
    private void InitializeSlots()
    {
        if (weaponManager != null)
            mainHandWeapon = weaponManager.GetCurrentWeapon();
        else if (player != null)
            mainHandWeapon = player.currentWeapon;

        offHandWeapon = null;

        RefreshSlotDisplay();
    }

    /// <summary>
    /// 刷新槽位显示
    /// </summary>
    public void RefreshSlotDisplay()
    {
        // 主手显示
        RefreshSingleSlot(mainHandWeapon, mainHandText, mainHandIcon);

        // 副手显示
        RefreshSingleSlot(offHandWeapon, offHandText, offHandIcon);

        UpdateAttackButtons();

        // 刷新连射UI
        if (burstUI != null)
        {
            burstUI.RefreshUI();
        }
    }

    /// <summary>
    /// 刷新单个槽位
    /// </summary>
    private void RefreshSingleSlot(Weapon weapon, TMP_Text nameText, Image icon)
    {
        // 武器名称
        if (nameText != null)
        {
            if (weapon != null)
            {
                string weaponInfo = weapon.Name;
                // 如果是远程武器，显示弹药
                if (weapon is RangedWeapon ranged)
                {
                    weaponInfo += $"\n弹药:{ranged.CurrentAmmo}/{ranged.MaxAmmo}";
                }
                nameText.text = weaponInfo;
            }
            else
            {
                nameText.text = "空";
            }
        }

        // 图标颜色
        if (icon != null)
            icon.color = weapon != null ? filledSlotColor : emptySlotColor;
    }

    /// <summary>
    /// 更新攻击按钮状态
    /// </summary>
    private void UpdateAttackButtons()
    {
        bool isPlayerTurn = battleManager != null && battleManager.IsPlayerTurn;
        bool canDoMain = actionPointSystem != null && actionPointSystem.CanDoMainAction();
        bool isSelecting = targetSelector != null && targetSelector.isSelectingTarget;

        if (attackWithMainButton != null)
        {
            bool hasAmmo = true;
            if (mainHandWeapon is RangedWeapon ranged)
                hasAmmo = ranged.CurrentAmmo > 0;
            attackWithMainButton.interactable = isPlayerTurn && canDoMain && mainHandWeapon != null && hasAmmo && !isSelecting;
        }

        if (attackWithOffButton != null)
        {
            bool hasAmmo = true;
            if (offHandWeapon is RangedWeapon ranged)
                hasAmmo = ranged.CurrentAmmo > 0;
            attackWithOffButton.interactable = isPlayerTurn && canDoMain && offHandWeapon != null && hasAmmo && !isSelecting;
        }
    }

    /// <summary>
    /// 打开武器选单
    /// </summary>
    public void OpenWeaponMenu(int slotIndex)
    {
        if (battleManager != null && battleManager.IsPlayerTurn)
        {
            if (actionPointSystem == null || !actionPointSystem.CanDoMinorAction())
            {
                Debug.Log("[WeaponInventoryUI] 没有次要动作，无法更换武器");
                return;
            }
        }

        selectingSlot = slotIndex;

        if (weaponMenuPanel != null)
            weaponMenuPanel.SetActive(true);

        PopulateWeaponMenu();
    }

    /// <summary>
    /// 关闭武器选单
    /// </summary>
    public void CloseWeaponMenu()
    {
        selectingSlot = -1;
        if (weaponMenuPanel != null)
            weaponMenuPanel.SetActive(false);
        ClearMenuItems();
    }

    /// <summary>
    /// 填充武器菜单
    /// </summary>
    private void PopulateWeaponMenu()
    {
        ClearMenuItems();

        if (weaponMenuContent == null || weaponMenuItemPrefab == null) return;

        List<Weapon> allWeapons = new List<Weapon>();
        if (weaponManager != null)
            allWeapons = weaponManager.GetAllWeapons();

        // 添加"空"选项
        CreateWeaponMenuItem(null, "卸下武器", false);

        // 添加所有武器
        foreach (var weapon in allWeapons)
        {
            bool isEquippedElsewhere = false;
            if (selectingSlot == 0 && weapon == offHandWeapon)
                isEquippedElsewhere = true;
            if (selectingSlot == 1 && weapon == mainHandWeapon)
                isEquippedElsewhere = true;

            CreateWeaponMenuItem(weapon, weapon.Name, isEquippedElsewhere);
        }
    }

    /// <summary>
    /// 创建武器菜单项
    /// </summary>
    private void CreateWeaponMenuItem(Weapon weapon, string displayName, bool isEquippedElsewhere)
    {
        GameObject item = Instantiate(weaponMenuItemPrefab, weaponMenuContent);
        menuItems.Add(item);

        TMP_Text text = item.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            if (weapon != null)
            {
                string suffix = isEquippedElsewhere ? " (已装备)" : "";
                string extraInfo = "";

                // 远程武器显示额外信息
                if (weapon is RangedWeapon ranged)
                {
                    extraInfo = $" 连射:{ranged.MaxBurst} 弹药:{ranged.CurrentAmmo}/{ranged.MaxAmmo}";
                }

                text.text = $"{displayName}{suffix}\n伤害:{weapon.DamageRange.x}-{weapon.DamageRange.y} 范围:{weapon.AttackRangeMin}-{weapon.AttackRangeMax}格{extraInfo}";
            }
            else
            {
                text.text = displayName;
            }
        }

        Button btn = item.GetComponent<Button>();
        if (btn != null)
        {
            if (isEquippedElsewhere)
                btn.onClick.AddListener(() => SwapWeaponToSlot(weapon));
            else
                btn.onClick.AddListener(() => TryEquipWeapon(weapon));
        }
    }

    /// <summary>
    /// 尝试装备武器（检查是否需要确认替换）
    /// </summary>
    private void TryEquipWeapon(Weapon weapon)
    {
        if (selectingSlot < 0) return;

        // 检查是否需要替换远程武器
        if (weapon is RangedWeapon)
        {
            Weapon otherSlotWeapon = selectingSlot == 0 ? offHandWeapon : mainHandWeapon;
            if (otherSlotWeapon is RangedWeapon otherRanged)
            {
                // 需要确认替换
                ShowReplaceConfirm(weapon, otherRanged);
                return;
            }
        }

        // 不需要替换，直接装备
        EquipWeaponToSlot(weapon);
    }

    /// <summary>
    /// 显示替换远程武器确认弹窗
    /// </summary>
    private void ShowReplaceConfirm(Weapon newWeapon, RangedWeapon existingWeapon)
    {
        pendingReplaceWeapon = newWeapon;

        if (confirmText != null)
        {
            string slotName = selectingSlot == 0 ? "副手" : "主手";
            confirmText.text = $"是否用 [{newWeapon.Name}] 替换{slotName}的 [{existingWeapon.Name}]？";
        }

        if (confirmPanel != null)
            confirmPanel.SetActive(true);

        // 隐藏武器菜单
        if (weaponMenuPanel != null)
            weaponMenuPanel.SetActive(false);
    }

    /// <summary>
    /// 确认替换 - 是
    /// </summary>
    private void OnConfirmReplaceYes()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        if (pendingReplaceWeapon != null)
        {
            // 先卸下另一只手的远程武器
            if (selectingSlot == 0)
            {
                // 要装备到主手，卸下副手的远程武器
                offHandWeapon = null;
            }
            else
            {
                // 要装备到副手，卸下主手的远程武器
                mainHandWeapon = null;
                if (player != null)
                    player.currentWeapon = null;
            }

            // 装备新武器
            EquipWeaponToSlot(pendingReplaceWeapon);
        }

        pendingReplaceWeapon = null;
    }

    /// <summary>
    /// 确认替换 - 否
    /// </summary>
    private void OnConfirmReplaceNo()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        pendingReplaceWeapon = null;

        // 重新打开武器菜单
        if (weaponMenuPanel != null)
            weaponMenuPanel.SetActive(true);
    }

    /// <summary>
    /// 装备武器到槽位（内部方法，已通过检查）
    /// </summary>
    private void EquipWeaponToSlot(Weapon weapon)
    {
        if (selectingSlot < 0) return;

        // 消耗次要动作
        if (battleManager != null && battleManager.IsPlayerTurn && actionPointSystem != null)
        {
            if (!actionPointSystem.CanDoMinorAction())
            {
                Debug.Log("[WeaponInventoryUI] 没有次要动作！");
                CloseWeaponMenu();
                return;
            }
            actionPointSystem.UseMinorAction();
        }

        if (selectingSlot == 0)
        {
            mainHandWeapon = weapon;
            if (player != null)
                player.currentWeapon = weapon;
        }
        else
        {
            offHandWeapon = weapon;
        }

        // 重置连射次数
        if (burstUI != null)
        {
            burstUI.ResetBurstCount();
        }

        OnWeaponEquipped?.Invoke(weapon, selectingSlot);
        RefreshSlotDisplay();
        CloseWeaponMenu();
    }

    /// <summary>
    /// 交换武器到槽位
    /// </summary>
    private void SwapWeaponToSlot(Weapon weapon)
    {
        if (selectingSlot < 0) return;

        if (battleManager != null && battleManager.IsPlayerTurn && actionPointSystem != null)
        {
            if (!actionPointSystem.CanDoMinorAction())
            {
                CloseWeaponMenu();
                return;
            }
            actionPointSystem.UseMinorAction();
        }

        // 交换
        Weapon temp = mainHandWeapon;
        mainHandWeapon = offHandWeapon;
        offHandWeapon = temp;

        if (player != null)
            player.currentWeapon = mainHandWeapon;

        // 重置连射次数
        if (burstUI != null)
        {
            burstUI.ResetBurstCount();
        }

        RefreshSlotDisplay();
        CloseWeaponMenu();
    }

    /// <summary>
    /// 使用指定槽位的武器攻击
    /// </summary>
    public void StartAttackWithSlot(int slotIndex)
    {
        Weapon weapon = slotIndex == 0 ? mainHandWeapon : offHandWeapon;

        if (weapon == null)
        {
            Debug.Log($"[WeaponInventoryUI] {(slotIndex == 0 ? "主手" : "副手")}没有武器！");
            return;
        }

        // 检查远程武器弹药
        if (weapon is RangedWeapon ranged && ranged.CurrentAmmo <= 0)
        {
            Debug.Log($"[WeaponInventoryUI] {weapon.Name} 没有弹药！");
            return;
        }

        if (targetSelector == null)
        {
            Debug.LogError("[WeaponInventoryUI] TargetSelector 未找到！");
            return;
        }

        if (battleManager != null && !battleManager.IsPlayerTurn)
        {
            Debug.Log("[WeaponInventoryUI] 不是玩家回合！");
            return;
        }

        if (actionPointSystem != null && !actionPointSystem.CanDoMainAction())
        {
            Debug.Log("[WeaponInventoryUI] 没有主要动作！");
            return;
        }

        // 记录待攻击的槽位
        pendingAttackSlot = slotIndex;

        // 开始目标选择
        targetSelector.StartTargetSelection(weapon);

        // 获取连射次数
        int burstCount = 1;
        if (burstUI != null)
        {
            burstCount = burstUI.GetCurrentBurstCount();
        }

        Debug.Log($"[WeaponInventoryUI] 使用 {(slotIndex == 0 ? "主手" : "副手")} [{weapon.Name}] 攻击，连射: {burstCount}");
    }

    /// <summary>
    /// 目标确认后的回调
    /// </summary>
    private void OnTargetConfirmed(ICombatTarget target, Weapon weapon)
    {
        // 攻击后刷新显示
        RefreshSlotDisplay();

        // 通知 BurstUI 攻击完成
        if (burstUI != null)
        {
            burstUI.OnAttackPerformed();
        }

        // 重置状态
        pendingAttackSlot = -1;
    }

    /// <summary>
    /// 获取当前待攻击的连射次数（供 BattleManager 调用）
    /// </summary>
    public int GetPendingBurstCount()
    {
        if (burstUI != null)
        {
            return burstUI.GetCurrentBurstCount();
        }
        return 1;
    }

    /// <summary>
    /// 获取主手武器
    /// </summary>
    public Weapon GetMainHandWeapon() => mainHandWeapon;

    /// <summary>
    /// 获取副手武器
    /// </summary>
    public Weapon GetOffHandWeapon() => offHandWeapon;

    /// <summary>
    /// 清除菜单项
    /// </summary>
    private void ClearMenuItems()
    {
        foreach (var item in menuItems)
        {
            if (item != null)
                Destroy(item);
        }
        menuItems.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && weaponMenuPanel != null && weaponMenuPanel.activeSelf)
            CloseWeaponMenu();

        UpdateAttackButtons();
    }
}
