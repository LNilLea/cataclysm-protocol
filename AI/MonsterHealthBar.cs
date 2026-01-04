using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 怪物头顶血条
/// 世界空间Canvas，跟随怪物移动，始终面向摄像机
/// </summary>
public class MonsterHealthBar : MonoBehaviour
{
    [Header("UI 组件")]
    [Tooltip("血条填充图片（需要设置为 Filled 类型）")]
    public Image fillImage;

    [Tooltip("血量数字显示")]
    public TMP_Text hpText;

    [Tooltip("怪物名字显示（可选）")]
    public TMP_Text nameText;

    [Header("颜色设置")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Tooltip("低血量阈值（百分比）")]
    [Range(0, 1)]
    public float lowHealthThreshold = 0.5f;   // 50% 以下变红

    [Tooltip("中等血量阈值（百分比）")]
    [Range(0, 1)]
    public float midHealthThreshold = 0.75f;  // 75% 以下变黄

    [Header("位置设置")]
    [Tooltip("血条在怪物头顶的偏移")]
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    [Tooltip("是否始终面向摄像机")]
    public bool faceCamera = true;

    [Header("显示设置")]
    [Tooltip("是否只在受伤时显示")]
    public bool showOnlyWhenDamaged = false;

    [Tooltip("受伤后显示的时间")]
    public float showDuration = 3f;

    // 内部引用
    private Transform target;           // 跟随的怪物
    private Camera mainCamera;
    private Canvas canvas;

    // MonsterBase 或 MonsterAI
    private MonsterBase monsterBase;
    private MonsterAI monsterAI;

    // 显示计时器
    private float showTimer = 0f;
    private bool isVisible = true;

    private void Awake()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInChildren<Canvas>();
    }

    private void Start()
    {
        // 如果没有手动设置目标，尝试从父物体获取
        if (target == null)
        {
            target = transform.parent;
        }

        // 获取怪物组件
        if (target != null)
        {
            monsterBase = target.GetComponent<MonsterBase>();
            monsterAI = target.GetComponent<MonsterAI>();
        }

        // 初始化显示
        if (showOnlyWhenDamaged)
        {
            SetVisible(false);
        }

        UpdateHealthBar();
    }

    private void LateUpdate()
    {
        // 跟随目标（如果不是子物体才需要）
        if (target != null && transform.parent != target)
        {
            transform.position = target.position + offset;
        }
        // 如果是子物体，用localPosition
        else if (target != null)
        {
            transform.localPosition = offset;
        }

        // 面向摄像机
        if (faceCamera && mainCamera != null)
        {
            // 2D 游戏通常不需要旋转，但如果需要可以启用
            // transform.forward = mainCamera.transform.forward;
        }

        // 更新血条
        UpdateHealthBar();

        // 处理显示计时器
        if (showOnlyWhenDamaged && showTimer > 0)
        {
            showTimer -= Time.deltaTime;
            if (showTimer <= 0)
            {
                SetVisible(false);
            }
        }
    }

    /// <summary>
    /// 更新血条显示
    /// </summary>
    public void UpdateHealthBar()
    {
        int currentHP = 0;
        int maxHP = 1;
        string monsterName = "";

        // 从 MonsterBase 获取数据
        if (monsterBase != null)
        {
            currentHP = monsterBase.currentHP;
            maxHP = monsterBase.maxHP;
            monsterName = monsterBase.monsterName;
        }
        // 从 MonsterAI 获取数据
        else if (monsterAI != null)
        {
            currentHP = monsterAI.combatData.currentHP;
            maxHP = monsterAI.combatData.maxHP;
            monsterName = monsterAI.Name;
        }
        else
        {
            // 调试：没有找到怪物组件
            Debug.LogWarning($"[MonsterHealthBar] 没有找到怪物组件! target={target?.name}");
            return;
        }

        // 计算血量百分比
        float healthPercent = maxHP > 0 ? (float)currentHP / maxHP : 0f;
        healthPercent = Mathf.Clamp01(healthPercent);

        // 调试日志
        Debug.Log($"[MonsterHealthBar] {monsterName}: {currentHP}/{maxHP} = {healthPercent:P0}");

        // 更新填充条
        if (fillImage != null)
        {
            fillImage.fillAmount = healthPercent;
            fillImage.color = GetHealthColor(healthPercent);
        }
        else
        {
            Debug.LogWarning("[MonsterHealthBar] fillImage 为空!");
        }

        // 更新数字
        if (hpText != null)
        {
            hpText.text = $"{currentHP}/{maxHP}";
        }

        // 更新名字
        if (nameText != null)
        {
            nameText.text = monsterName;
        }
    }

    /// <summary>
    /// 根据血量百分比获取颜色（细腻渐变）
    /// 100% 绿色 → 50% 黄色 → 0% 红色
    /// </summary>
    private Color GetHealthColor(float percent)
    {
        // 直接使用 Lerp 实现平滑渐变
        // percent: 1.0 → 0.0
        
        if (percent > 0.5f)
        {
            // 100% ~ 50%：绿色 → 黄色
            // percent: 1.0 → 0.5，需要转换为 t: 0 → 1
            float t = 1f - (percent - 0.5f) * 2f;  // 0 → 1
            return Color.Lerp(fullHealthColor, midHealthColor, t);
        }
        else
        {
            // 50% ~ 0%：黄色 → 红色
            // percent: 0.5 → 0，需要转换为 t: 0 → 1
            float t = 1f - percent * 2f;  // 0 → 1
            return Color.Lerp(midHealthColor, lowHealthColor, t);
        }
    }

    /// <summary>
    /// 设置跟随目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (target != null)
        {
            monsterBase = target.GetComponent<MonsterBase>();
            monsterAI = target.GetComponent<MonsterAI>();
        }

        UpdateHealthBar();
    }

    /// <summary>
    /// 设置可见性
    /// </summary>
    public void SetVisible(bool visible)
    {
        isVisible = visible;

        if (canvas != null)
        {
            canvas.enabled = visible;
        }
        else
        {
            gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// 当受到伤害时调用（用于显示血条）
    /// </summary>
    public void OnDamaged()
    {
        if (showOnlyWhenDamaged)
        {
            SetVisible(true);
            showTimer = showDuration;
        }

        UpdateHealthBar();
    }

    /// <summary>
    /// 检查怪物是否死亡
    /// </summary>
    public bool IsTargetDead()
    {
        if (monsterBase != null)
        {
            return !monsterBase.IsAlive();
        }
        if (monsterAI != null)
        {
            return !monsterAI.IsAlive();
        }
        return true;
    }
}
