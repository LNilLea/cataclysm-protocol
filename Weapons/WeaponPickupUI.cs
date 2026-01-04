using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 武器获取提示 UI - 显示玩家获得的武器信息
/// 【重要】此脚本所在的 GameObject 应保持激活状态！用 CanvasGroup 控制显示/隐藏
/// </summary>
public class WeaponPickupUI : MonoBehaviour
{
    public static WeaponPickupUI Instance { get; private set; }

    [Header("UI 元素")]
    public GameObject pickupPanel;           // 提示面板（可选，不设置则用自身）
    public TMP_Text weaponNameText;          // 武器名称
    public TMP_Text weaponStatsText;         // 武器属性（伤害、命中等）
    public TMP_Text weaponEffectText;        // 武器特效（可选）
    public Image weaponIcon;                 // 武器图标（可选）

    [Header("显示设置")]
    public float displayDuration = 3f;       // 显示时长
    public float fadeInDuration = 0.3f;      // 淡入时长
    public float fadeOutDuration = 0.5f;     // 淡出时长

    [Header("动画设置")]
    public bool useSlideAnimation = true;    // 是否使用滑入动画
    public float slideDistance = 100f;       // 滑入距离

    private CanvasGroup canvasGroup;
    private RectTransform panelRect;
    private Vector2 originalPosition;
    private Coroutine currentCoroutine;
    private bool isInitialized = false;

    private void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Initialize();
    }

    /// <summary>
    /// 初始化组件（可以多次调用，只会执行一次）
    /// </summary>
    private void Initialize()
    {
        if (isInitialized) return;

        // 确定要操作的面板
        GameObject targetPanel = pickupPanel != null ? pickupPanel : gameObject;

        // 获取或添加 CanvasGroup
        canvasGroup = targetPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = targetPanel.AddComponent<CanvasGroup>();
        }

        // 获取 RectTransform
        panelRect = targetPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            originalPosition = panelRect.anchoredPosition;
        }

        isInitialized = true;
        Debug.Log("[WeaponPickupUI] 初始化完成");
    }

    private void Start()
    {
        // 启动时隐藏（使用透明度，不是 SetActive）
        HideImmediate();
    }

    /// <summary>
    /// 显示武器获取提示
    /// </summary>
    public void ShowWeaponPickup(Weapon weapon)
    {
        if (weapon == null) return;

        // 确保已初始化
        Initialize();

        // 确保物体是激活的
        gameObject.SetActive(true);
        if (pickupPanel != null)
        {
            pickupPanel.SetActive(true);
        }

        // 停止之前的协程
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        // 更新UI文本
        UpdateUI(weapon);

        Debug.Log($"[WeaponPickupUI] 显示武器获取提示: {weapon.Name}");

        // 开始显示动画
        currentCoroutine = StartCoroutine(ShowPickupCoroutine());
    }

    /// <summary>
    /// 通过 WeaponChoice 枚举显示武器获取提示
    /// </summary>
    public void ShowWeaponPickup(WeaponChoice choice)
    {
        Weapon weapon = WeaponFactory.GetWeapon(choice);
        if (weapon != null)
        {
            ShowWeaponPickup(weapon);
        }
    }

    /// <summary>
    /// 更新UI显示内容
    /// </summary>
    private void UpdateUI(Weapon weapon)
    {
        // 武器名称
        if (weaponNameText != null)
        {
            weaponNameText.text = $"获得武器：{weapon.Name}";
        }

        // 武器属性
        if (weaponStatsText != null)
        {
            string stats = $"伤害：{weapon.DamageRange.x}-{weapon.DamageRange.y}\n";
            stats += $"命中加值：+{weapon.HitBonus}\n";
            stats += $"攻击范围：{weapon.AttackRangeMin}-{weapon.AttackRangeMax} 格";
            
            // 如果有力量需求，也显示
            if (weapon.RequiredStrength > 0)
            {
                stats += $"\n需求力量：{weapon.RequiredStrength}";
            }
            
            weaponStatsText.text = stats;
        }

        // 武器特效
        if (weaponEffectText != null)
        {
            // 如果有体魄需求，显示提示
            if (weapon.RequiredStrength > 0)
            {
                weaponEffectText.gameObject.SetActive(true);
                weaponEffectText.text = "需要一定体魄才能使用";
            }
            else if (!string.IsNullOrEmpty(weapon.Effect))
            {
                weaponEffectText.gameObject.SetActive(true);
                weaponEffectText.text = $"特效：{weapon.Effect}";
            }
            else
            {
                weaponEffectText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 显示动画协程
    /// </summary>
    private IEnumerator ShowPickupCoroutine()
    {
        // 设置初始状态
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        // 滑入动画初始位置
        if (useSlideAnimation && panelRect != null)
        {
            panelRect.anchoredPosition = originalPosition + new Vector2(slideDistance, 0);
        }

        // === 淡入动画 ===
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;

            // 淡入
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }

            // 滑入
            if (useSlideAnimation && panelRect != null)
            {
                panelRect.anchoredPosition = Vector2.Lerp(
                    originalPosition + new Vector2(slideDistance, 0),
                    originalPosition,
                    EaseOutQuad(t)
                );
            }

            yield return null;
        }

        // 确保完全显示
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (panelRect != null) panelRect.anchoredPosition = originalPosition;

        // === 等待显示时间 ===
        yield return new WaitForSeconds(displayDuration);

        // === 淡出动画 ===
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }

            yield return null;
        }

        // 隐藏面板
        HideImmediate();
    }

    /// <summary>
    /// 立即隐藏（使用透明度）
    /// </summary>
    public void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// 缓动函数 - EaseOutQuad
    /// </summary>
    private float EaseOutQuad(float t)
    {
        return 1 - (1 - t) * (1 - t);
    }
}
