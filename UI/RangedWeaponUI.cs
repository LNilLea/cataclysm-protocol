using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyGame;

/// <summary>
/// 枪械 UI - 显示弹药状态、换弹按钮、连射选项
/// </summary>
public class RangedWeaponUI : MonoBehaviour
{
    [Header("引用")]
    public Player player;
    public RangedCombatController rangedController;
    public BattleManager battleManager;
    public ActionPointSystem actionPointSystem;
    public TargetSelector targetSelector;

    [Header("弹药显示")]
    public GameObject ammoPanel;
    public TMP_Text ammoText;               // 当前弹药 / 弹匣容量
    public TMP_Text reserveAmmoText;        // 备用弹药
    public Image ammoBar;                   // 弹药条

    [Header("换弹")]
    public Button reloadButton;
    public TMP_Text reloadButtonText;
    public Image reloadProgressBar;

    [Header("连射选项")]
    public GameObject burstPanel;
    public Button singleShotButton;         // 单发
    public Button burstButton;              // 连射
    public TMP_Text burstCountText;         // 显示连射数
    public Slider burstSlider;              // 连射数量滑块

    [Header("警告提示")]
    public GameObject lowAmmoWarning;       // 低弹药警告
    public GameObject noAmmoWarning;        // 无弹药警告
    public GameObject strengthWarning;      // 体魄不足警告
    public TMP_Text strengthWarningText;

    [Header("设置")]
    public int lowAmmoThreshold = 3;        // 低弹药阈值

    // 当前选择的连射数
    private int selectedBurstCount = 1;
    // 当前选择的武器
    private RangedWeapon currentRangedWeapon;

    private void Start()
    {
        // 自动获取引用
        if (player == null)
            player = FindObjectOfType<Player>();

        if (rangedController == null)
            rangedController = FindObjectOfType<RangedCombatController>();

        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();

        if (actionPointSystem == null)
            actionPointSystem = FindObjectOfType<ActionPointSystem>();

        if (targetSelector == null)
            targetSelector = FindObjectOfType<TargetSelector>();

        // 绑定按钮事件
        SetupButtons();

        // 订阅事件
        if (rangedController != null)
        {
            rangedController.OnReloadStart += OnReloadStart;
            rangedController.OnReloadComplete += OnReloadComplete;
            rangedController.OnSelfDamage += OnSelfDamage;
        }

        // 初始隐藏
        if (ammoPanel != null) ammoPanel.SetActive(false);
        if (burstPanel != null) burstPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (rangedController != null)
        {
            rangedController.OnReloadStart -= OnReloadStart;
            rangedController.OnReloadComplete -= OnReloadComplete;
            rangedController.OnSelfDamage -= OnSelfDamage;
        }
    }

    private void Update()
    {
        UpdateUI();
    }

    /// <summary>
    /// 设置按钮事件
    /// </summary>
    private void SetupButtons()
    {
        if (reloadButton != null)
        {
            reloadButton.onClick.AddListener(OnReloadClicked);
        }

        if (singleShotButton != null)
        {
            singleShotButton.onClick.AddListener(() => SelectFireMode(1));
        }

        if (burstButton != null)
        {
            burstButton.onClick.AddListener(OnBurstClicked);
        }

        if (burstSlider != null)
        {
            burstSlider.onValueChanged.AddListener(OnBurstSliderChanged);
        }
    }

    /// <summary>
    /// 更新 UI
    /// </summary>
    private void UpdateUI()
    {
        // 获取当前枪械
        currentRangedWeapon = GetCurrentRangedWeapon();

        bool hasRangedWeapon = currentRangedWeapon != null;
        bool isPlayerTurn = battleManager != null && battleManager.IsPlayerTurn;
        bool isReloading = rangedController != null && rangedController.isReloading;

        // 显示/隐藏弹药面板
        if (ammoPanel != null)
        {
            ammoPanel.SetActive(hasRangedWeapon);
        }

        if (!hasRangedWeapon) return;

        // 更新弹药显示
        UpdateAmmoDisplay();

        // 更新换弹按钮
        UpdateReloadButton(isPlayerTurn, isReloading);

        // 更新连射面板
        UpdateBurstPanel(isPlayerTurn);

        // 更新警告
        UpdateWarnings();

        // 更新换弹进度
        if (isReloading)
        {
            UpdateReloadProgress();
        }
    }

    /// <summary>
    /// 更新弹药显示
    /// </summary>
    private void UpdateAmmoDisplay()
    {
        if (currentRangedWeapon == null) return;

        if (ammoText != null)
        {
            ammoText.text = $"{currentRangedWeapon.CurrentAmmo} / {currentRangedWeapon.MaxAmmo}";
        }

        if (reserveAmmoText != null)
        {
            reserveAmmoText.text = $"备用: {currentRangedWeapon.ReserveAmmo}";
        }

        if (ammoBar != null)
        {
            ammoBar.fillAmount = (float)currentRangedWeapon.CurrentAmmo / currentRangedWeapon.MaxAmmo;

            // 根据弹药量改变颜色
            if (currentRangedWeapon.CurrentAmmo <= 0)
            {
                ammoBar.color = Color.red;
            }
            else if (currentRangedWeapon.CurrentAmmo <= lowAmmoThreshold)
            {
                ammoBar.color = Color.yellow;
            }
            else
            {
                ammoBar.color = Color.green;
            }
        }
    }

    /// <summary>
    /// 更新换弹按钮
    /// </summary>
    private void UpdateReloadButton(bool isPlayerTurn, bool isReloading)
    {
        if (reloadButton == null) return;

        bool canReload = isPlayerTurn && 
                         !isReloading && 
                         currentRangedWeapon.CurrentAmmo < currentRangedWeapon.MaxAmmo &&
                         currentRangedWeapon.ReserveAmmo > 0 &&
                         (actionPointSystem == null || actionPointSystem.CanDoMinorAction());

        reloadButton.interactable = canReload;

        if (reloadButtonText != null)
        {
            if (isReloading)
            {
                reloadButtonText.text = "换弹中...";
            }
            else if (currentRangedWeapon.ReserveAmmo <= 0)
            {
                reloadButtonText.text = "无弹药";
            }
            else
            {
                reloadButtonText.text = "换弹 (次要)";
            }
        }
    }

    /// <summary>
    /// 更新连射面板
    /// </summary>
    private void UpdateBurstPanel(bool isPlayerTurn)
    {
        if (burstPanel == null) return;

        bool showBurstPanel = currentRangedWeapon != null && currentRangedWeapon.MaxBurst > 1;
        burstPanel.SetActive(showBurstPanel);

        if (!showBurstPanel) return;

        // 更新滑块范围
        if (burstSlider != null)
        {
            burstSlider.minValue = 1;
            burstSlider.maxValue = Mathf.Min(currentRangedWeapon.MaxBurst, currentRangedWeapon.CurrentAmmo);
            burstSlider.value = Mathf.Min(selectedBurstCount, (int)burstSlider.maxValue);
            burstSlider.interactable = isPlayerTurn && currentRangedWeapon.CurrentAmmo > 1;
        }

        // 更新连射数显示
        if (burstCountText != null)
        {
            burstCountText.text = $"连射: {selectedBurstCount} 发";
        }

        // 更新按钮状态
        if (singleShotButton != null)
        {
            singleShotButton.interactable = isPlayerTurn;
        }

        if (burstButton != null)
        {
            burstButton.interactable = isPlayerTurn && currentRangedWeapon.CurrentAmmo >= selectedBurstCount;
        }
    }

    /// <summary>
    /// 更新警告显示
    /// </summary>
    private void UpdateWarnings()
    {
        if (currentRangedWeapon == null) return;

        // 无弹药警告
        if (noAmmoWarning != null)
        {
            noAmmoWarning.SetActive(currentRangedWeapon.CurrentAmmo <= 0);
        }

        // 低弹药警告
        if (lowAmmoWarning != null)
        {
            lowAmmoWarning.SetActive(
                currentRangedWeapon.CurrentAmmo > 0 && 
                currentRangedWeapon.CurrentAmmo <= lowAmmoThreshold
            );
        }

        // 体魄不足警告
        if (strengthWarning != null && player != null)
        {
            int deficit = currentRangedWeapon.RequiredStrength - player.combatData.strength;
            bool showWarning = deficit > 0;

            strengthWarning.SetActive(showWarning);

            if (showWarning && strengthWarningText != null)
            {
                int hitPenalty = -2 * deficit;
                int selfDamage = deficit * currentRangedWeapon.WeaponSize;
                strengthWarningText.text = $"体魄不足！命中{hitPenalty}，每次开火自伤{selfDamage}";
            }
        }
    }

    /// <summary>
    /// 更新换弹进度
    /// </summary>
    private void UpdateReloadProgress()
    {
        if (reloadProgressBar == null || rangedController == null) return;

        reloadProgressBar.fillAmount = rangedController.GetReloadProgress();
    }

    /// <summary>
    /// 获取当前枪械
    /// </summary>
    private RangedWeapon GetCurrentRangedWeapon()
    {
        if (player == null || player.currentWeapon == null) return null;

        return player.currentWeapon as RangedWeapon;
    }

    /// <summary>
    /// 换弹按钮点击
    /// </summary>
    private void OnReloadClicked()
    {
        if (rangedController != null && currentRangedWeapon != null)
        {
            rangedController.StartReload(currentRangedWeapon);
        }
    }

    /// <summary>
    /// 选择射击模式
    /// </summary>
    private void SelectFireMode(int burstCount)
    {
        selectedBurstCount = burstCount;

        if (burstSlider != null)
        {
            burstSlider.value = burstCount;
        }
    }

    /// <summary>
    /// 连射按钮点击
    /// </summary>
    private void OnBurstClicked()
    {
        if (targetSelector != null && currentRangedWeapon != null)
        {
            // 开始选择目标，使用连射模式
            targetSelector.StartTargetSelection(currentRangedWeapon);
            
            // 保存连射数（可以通过其他方式传递给攻击逻辑）
            PlayerPrefs.SetInt("BurstCount", selectedBurstCount);
        }
    }

    /// <summary>
    /// 连射滑块变化
    /// </summary>
    private void OnBurstSliderChanged(float value)
    {
        selectedBurstCount = Mathf.RoundToInt(value);

        if (burstCountText != null)
        {
            burstCountText.text = $"连射: {selectedBurstCount} 发";
        }
    }

    /// <summary>
    /// 换弹开始事件
    /// </summary>
    private void OnReloadStart(RangedWeapon weapon)
    {
        if (reloadProgressBar != null)
        {
            reloadProgressBar.gameObject.SetActive(true);
            reloadProgressBar.fillAmount = 0f;
        }
    }

    /// <summary>
    /// 换弹完成事件
    /// </summary>
    private void OnReloadComplete(RangedWeapon weapon)
    {
        if (reloadProgressBar != null)
        {
            reloadProgressBar.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 自伤事件
    /// </summary>
    private void OnSelfDamage(int damage)
    {
        Debug.Log($"后坐力自伤: {damage}");
        // 可以在这里添加屏幕震动或其他效果
    }

    /// <summary>
    /// 获取当前选择的连射数
    /// </summary>
    public int GetSelectedBurstCount()
    {
        return selectedBurstCount;
    }
}
