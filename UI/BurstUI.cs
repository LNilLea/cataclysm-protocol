using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyGame;

/// <summary>
/// 连射UI控制器
/// 检测当前武器，显示弹药信息和连射调整
/// 放置在武器按钮下方，仅在装备远程武器时显示
/// </summary>
public class BurstUI : MonoBehaviour
{
    [Header("UI 组件 - 弹药信息（所有远程武器显示）")]
    [Tooltip("整个远程武器面板（用于显示/隐藏）")]
    public GameObject rangedWeaponPanel;

    [Tooltip("武器名称显示")]
    public TMP_Text weaponNameText;

    [Tooltip("弹药显示：当前/最大")]
    public TMP_Text ammoText;

    [Tooltip("备弹显示")]
    public TMP_Text reserveAmmoText;

    [Tooltip("换弹按钮")]
    public Button reloadButton;

    [Header("UI 组件 - 连射控制（仅可连射武器显示）")]
    [Tooltip("连射控制面板（MaxBurst > 1 时才显示）")]
    public GameObject burstControlPanel;

    [Tooltip("连射次数显示")]
    public TMP_Text burstCountText;

    [Tooltip("最大连射数显示")]
    public TMP_Text maxBurstText;

    [Tooltip("减少连射按钮")]
    public Button decreaseButton;

    [Tooltip("增加连射按钮")]
    public Button increaseButton;

    [Header("引用")]
    public Player player;
    public WeaponManager weaponManager;
    public WeaponInventoryUI weaponInventoryUI;  // 【新增】武器背包UI引用
    public ActionPointSystem actionPointSystem;
    public BattleManager battleManager;

    [Header("设置")]
    [Tooltip("换弹是否消耗次要动作")]
    public bool reloadCostsMinorAction = true;

    // 当前连射次数
    private int currentBurstCount = 1;

    // 当前远程武器引用
    private RangedWeapon currentRangedWeapon;

    // ========== 生命周期 ==========

    private void Start()
    {
        Debug.Log("[BurstUI] Start 开始执行");
        
        // 自动获取引用
        if (player == null)
            player = FindObjectOfType<Player>();
        if (weaponManager == null && player != null)
            weaponManager = player.GetComponent<WeaponManager>();
        if (weaponInventoryUI == null)
            weaponInventoryUI = FindObjectOfType<WeaponInventoryUI>();
        if (actionPointSystem == null)
            actionPointSystem = FindObjectOfType<ActionPointSystem>();
        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();

        // 绑定按钮事件
        if (decreaseButton != null)
        {
            decreaseButton.onClick.AddListener(DecreaseBurst);
            Debug.Log("[BurstUI] DecreaseButton 绑定成功");
        }
        else
        {
            Debug.LogError("[BurstUI] DecreaseButton 为空！");
        }
        
        if (increaseButton != null)
        {
            increaseButton.onClick.AddListener(IncreaseBurst);
            Debug.Log("[BurstUI] IncreaseButton 绑定成功");
        }
        else
        {
            Debug.LogError("[BurstUI] IncreaseButton 为空！");
        }
        
        if (reloadButton != null)
            reloadButton.onClick.AddListener(Reload);

        // 初始刷新
        RefreshUI();
        Debug.Log("[BurstUI] Start 执行完毕");
    }

    private void Update()
    {
        // 每帧检测武器变化并刷新
        RefreshUI();
    }

    // ========== 核心逻辑 ==========

    /// <summary>
    /// 刷新整个UI
    /// </summary>
    public void RefreshUI()
    {
        // 获取当前武器
        Weapon currentWeapon = GetCurrentWeapon();

        // 检测是否是远程武器
        currentRangedWeapon = currentWeapon as RangedWeapon;

        if (currentRangedWeapon == null)
        {
            // 不是远程武器，隐藏所有面板
            if (rangedWeaponPanel != null)
                rangedWeaponPanel.SetActive(false);
            if (burstControlPanel != null)
                burstControlPanel.SetActive(false);
            return;
        }

        // 是远程武器，显示弹药面板
        if (rangedWeaponPanel != null)
            rangedWeaponPanel.SetActive(true);

        // 检测是否可以连射（MaxBurst > 1）
        bool canBurst = currentRangedWeapon.MaxBurst > 1;
        if (burstControlPanel != null)
            burstControlPanel.SetActive(canBurst);

        // 如果不能连射，重置连射次数为1
        if (!canBurst)
        {
            currentBurstCount = 1;
        }

        // 更新各项显示
        UpdateWeaponName();
        UpdateAmmoDisplay();
        UpdateBurstDisplay();
        UpdateButtonStates();
    }

    /// <summary>
    /// 获取当前远程武器
    /// 规则：主手副手同时只能有一把远程武器
    /// 检测哪只手是远程武器就返回哪只
    /// </summary>
    private Weapon GetCurrentWeapon()
    {
        if (weaponInventoryUI != null)
        {
            Weapon mainHand = weaponInventoryUI.GetMainHandWeapon();
            Weapon offHand = weaponInventoryUI.GetOffHandWeapon();

            // 副手是远程武器
            if (offHand is RangedWeapon)
            {
                return offHand;
            }

            // 主手是远程武器（或都不是远程）
            return mainHand;
        }

        // 备用：从 WeaponManager 获取
        if (weaponManager != null)
        {
            return weaponManager.GetCurrentWeapon();
        }

        // 备用：从 Player 获取
        if (player != null)
        {
            return player.currentWeapon;
        }

        return null;
    }

    /// <summary>
    /// 更新武器名称显示
    /// </summary>
    private void UpdateWeaponName()
    {
        if (weaponNameText != null && currentRangedWeapon != null)
        {
            weaponNameText.text = currentRangedWeapon.Name;
        }
    }

    /// <summary>
    /// 更新弹药显示
    /// </summary>
    private void UpdateAmmoDisplay()
    {
        if (currentRangedWeapon == null) return;

        // 当前弹药 / 弹匣容量
        if (ammoText != null)
        {
            ammoText.text = $"弹药: {currentRangedWeapon.CurrentAmmo}/{currentRangedWeapon.MaxAmmo}";
        }

        // 备用弹药
        if (reserveAmmoText != null)
        {
            reserveAmmoText.text = $"备弹: {currentRangedWeapon.ReserveAmmo}";
        }
    }

    /// <summary>
    /// 更新连射显示
    /// </summary>
    private void UpdateBurstDisplay()
    {
        if (currentRangedWeapon == null) return;

        // 计算可用的最大连射次数（不超过剩余弹药）
        int maxAvailableBurst = Mathf.Min(currentRangedWeapon.MaxBurst, currentRangedWeapon.CurrentAmmo);
        maxAvailableBurst = Mathf.Max(1, maxAvailableBurst);

        // 确保当前连射次数在有效范围内
        currentBurstCount = Mathf.Clamp(currentBurstCount, 1, maxAvailableBurst);

        // 显示当前连射次数
        if (burstCountText != null)
        {
            burstCountText.text = currentBurstCount.ToString();
        }

        // 显示最大连射数
        if (maxBurstText != null)
        {
            maxBurstText.text = $"最大连射: {currentRangedWeapon.MaxBurst}";
        }
    }

    /// <summary>
    /// 更新按钮状态
    /// </summary>
    private void UpdateButtonStates()
    {
        if (currentRangedWeapon == null) return;

        int maxAvailableBurst = Mathf.Min(currentRangedWeapon.MaxBurst, currentRangedWeapon.CurrentAmmo);
        maxAvailableBurst = Mathf.Max(1, maxAvailableBurst);

        // 减少按钮：连射次数 > 1 时可用
        if (decreaseButton != null)
        {
            decreaseButton.interactable = currentBurstCount > 1;
        }

        // 增加按钮：连射次数 < 最大可用连射 时可用
        if (increaseButton != null)
        {
            increaseButton.interactable = currentBurstCount < maxAvailableBurst;
        }

        // 换弹按钮：弹药未满且有备弹时可用
        if (reloadButton != null)
        {
            bool canReload = currentRangedWeapon.CurrentAmmo < currentRangedWeapon.MaxAmmo
                          && currentRangedWeapon.ReserveAmmo > 0;

            // 如果在战斗中，还需要检查次要动作
            if (reloadCostsMinorAction && battleManager != null && battleManager.IsPlayerTurn)
            {
                canReload = canReload && actionPointSystem != null && actionPointSystem.CanDoMinorAction();
            }

            reloadButton.interactable = canReload;
        }
    }

    // ========== 按钮回调 ==========

    /// <summary>
    /// 减少连射次数
    /// </summary>
    public void DecreaseBurst()
    {
        if (currentBurstCount > 1)
        {
            currentBurstCount--;
            UpdateBurstDisplay();
            UpdateButtonStates();
            Debug.Log($"[BurstUI] 连射次数减少到: {currentBurstCount}");
        }
    }

    /// <summary>
    /// 增加连射次数
    /// </summary>
    public void IncreaseBurst()
    {
        if (currentRangedWeapon == null) return;

        int maxAvailableBurst = Mathf.Min(currentRangedWeapon.MaxBurst, currentRangedWeapon.CurrentAmmo);

        if (currentBurstCount < maxAvailableBurst)
        {
            currentBurstCount++;
            UpdateBurstDisplay();
            UpdateButtonStates();
            Debug.Log($"[BurstUI] 连射次数增加到: {currentBurstCount}");
        }
    }

    /// <summary>
    /// 换弹
    /// </summary>
    public void Reload()
    {
        if (currentRangedWeapon == null) return;

        // 检查是否需要消耗次要动作
        if (reloadCostsMinorAction && battleManager != null && battleManager.IsPlayerTurn)
        {
            if (actionPointSystem == null || !actionPointSystem.CanDoMinorAction())
            {
                Debug.Log("[BurstUI] 没有次要动作，无法换弹！");
                return;
            }
            actionPointSystem.UseMinorAction();
        }

        // 执行换弹
        bool success = currentRangedWeapon.Reload();

        if (success)
        {
            // 重置连射次数
            currentBurstCount = 1;
            RefreshUI();
            Debug.Log($"[BurstUI] 换弹完成: {currentRangedWeapon.CurrentAmmo}/{currentRangedWeapon.MaxAmmo}");
        }
    }

    // ========== 公共接口 ==========

    /// <summary>
    /// 获取当前设置的连射次数（供 BattleManager 调用）
    /// </summary>
    public int GetCurrentBurstCount()
    {
        return currentBurstCount;
    }

    /// <summary>
    /// 获取当前远程武器
    /// </summary>
    public RangedWeapon GetCurrentRangedWeapon()
    {
        return currentRangedWeapon;
    }

    /// <summary>
    /// 重置连射次数为1
    /// </summary>
    public void ResetBurstCount()
    {
        currentBurstCount = 1;
        UpdateBurstDisplay();
        UpdateButtonStates();
    }

    /// <summary>
    /// 攻击后调用（用于刷新弹药显示）
    /// </summary>
    public void OnAttackPerformed()
    {
        RefreshUI();
    }
}
