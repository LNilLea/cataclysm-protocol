using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 头顶血条 - 挂在敌人/NPC身上，显示在角色头顶
/// 【修复版 v2】在 Awake 和 CreateHealthBar 中添加完整的空引用检查
/// </summary>
public class HealthBarWorld : MonoBehaviour
{
    [Header("血条设置")]
    public Vector3 offset = new Vector3(0, 1.5f, 0);  // 血条相对角色的偏移
    public Vector2 barSize = new Vector2(1f, 0.15f);  // 血条大小

    [Header("颜色")]
    public Color healthColor = new Color(0.2f, 0.8f, 0.2f);      // 血量颜色（绿）
    public Color damageColor = new Color(0.8f, 0.2f, 0.2f);      // 受伤颜色（红）
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f);  // 背景颜色

    [Header("显示设置")]
    public bool alwaysShow = false;           // 是否一直显示
    public bool hideWhenFull = true;          // 满血时隐藏
    public float showDuration = 3f;           // 受伤后显示时长

    // 内部组件
    private Canvas canvas;
    private Image backgroundImage;
    private Image healthFillImage;
    private Transform barTransform;

    // 血量数据
    private int maxHP = 100;
    private int currentHP = 100;
    private float lastDamageTime = -999f;

    // 跟随的摄像机
    private Camera mainCamera;

    // 标记是否已初始化
    private bool isInitialized = false;
    private bool isDestroyed = false;

    private void Awake()
    {
        // 【修复】检查自身是否有效
        if (this == null || gameObject == null)
        {
            isDestroyed = true;
            return;
        }

        try
        {
            mainCamera = Camera.main;
            CreateHealthBar();
            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[HealthBarWorld] Awake 异常: {e.Message}");
            isDestroyed = true;
        }
    }

    private void Start()
    {
        if (isDestroyed || !isInitialized) return;
        
        // 尝试从组件获取血量数据
        TryGetHealthData();
    }

    private void LateUpdate()
    {
        // 【修复】多重安全检查
        if (isDestroyed || !isInitialized) return;
        if (this == null || gameObject == null) 
        {
            isDestroyed = true;
            return;
        }

        try
        {
            // 让血条面向摄像机
            if (mainCamera != null && barTransform != null && transform != null)
            {
                barTransform.position = transform.position + offset;
                barTransform.rotation = mainCamera.transform.rotation;
            }

            // 控制显示/隐藏
            UpdateVisibility();
        }
        catch
        {
            isDestroyed = true;
        }
    }

    private void OnDestroy()
    {
        isDestroyed = true;
        isInitialized = false;
    }

    /// <summary>
    /// 创建血条UI
    /// </summary>
    private void CreateHealthBar()
    {
        // 【修复】检查 transform 是否有效
        if (this == null || gameObject == null)
        {
            isDestroyed = true;
            return;
        }

        Transform myTransform = null;
        try
        {
            myTransform = transform;
        }
        catch
        {
            isDestroyed = true;
            return;
        }

        if (myTransform == null)
        {
            isDestroyed = true;
            return;
        }

        // 创建 Canvas（World Space）
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        if (canvasObj == null)
        {
            isDestroyed = true;
            return;
        }

        canvasObj.transform.SetParent(myTransform);
        barTransform = canvasObj.transform;

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = barSize;
        canvasRect.localScale = Vector3.one * 0.01f;  // 缩小到合适大小

        // 创建背景
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform);

        backgroundImage = bgObj.AddComponent<Image>();
        backgroundImage.color = backgroundColor;

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // 创建血量填充
        GameObject fillObj = new GameObject("HealthFill");
        fillObj.transform.SetParent(canvasObj.transform);

        healthFillImage = fillObj.AddComponent<Image>();
        healthFillImage.color = healthColor;

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0, 0.5f);  // 左对齐
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // 【修复】安全设置初始位置
        if (barTransform != null)
        {
            barTransform.localPosition = offset;
        }
    }

    /// <summary>
    /// 尝试从组件获取血量数据
    /// </summary>
    private void TryGetHealthData()
    {
        if (isDestroyed || !isInitialized) return;
        if (this == null || gameObject == null) return;

        try
        {
            // 尝试从 Beaver 获取
            var beaver = GetComponent<Beaver>();
            if (beaver != null)
            {
                maxHP = beaver.maxHP;
                currentHP = beaver.currentHP;
                return;
            }

            // 尝试从 Mantis 获取
            var mantis = GetComponent<Mantis>();
            if (mantis != null)
            {
                maxHP = mantis.maxHP;
                currentHP = mantis.currentHP;
                return;
            }

            // 尝试从 MonsterAI 获取
            var monsterAI = GetComponent<MonsterAI>();
            if (monsterAI != null && monsterAI.combatData != null)
            {
                maxHP = monsterAI.combatData.maxHP;
                currentHP = monsterAI.combatData.currentHP;
                return;
            }

            // 尝试通用方式：反射获取 currentHP 和 maxHP
            var monoBehaviour = GetComponent<MonoBehaviour>();
            if (monoBehaviour != null)
            {
                var type = monoBehaviour.GetType();
                if (type != null)
                {
                    var maxHPField = type.GetField("maxHP");
                    var currentHPField = type.GetField("currentHP");

                    if (maxHPField != null && currentHPField != null)
                    {
                        var component = GetComponent(type);
                        if (component != null)
                        {
                            maxHP = (int)maxHPField.GetValue(component);
                            currentHP = (int)currentHPField.GetValue(component);
                        }
                    }
                }
            }
        }
        catch
        {
            // 忽略异常
        }
    }

    /// <summary>
    /// 控制血条显示/隐藏
    /// </summary>
    private void UpdateVisibility()
    {
        if (canvas == null || isDestroyed || !isInitialized) return;

        // 刷新血量数据
        TryGetHealthData();
        UpdateHealthBar();

        bool shouldShow = alwaysShow;

        // 满血时隐藏
        if (hideWhenFull && currentHP >= maxHP)
        {
            shouldShow = false;
        }
        // 受伤后显示一段时间
        else if (Time.time - lastDamageTime < showDuration)
        {
            shouldShow = true;
        }
        // 不满血时显示
        else if (currentHP < maxHP)
        {
            shouldShow = true;
        }

        try
        {
            canvas.gameObject.SetActive(shouldShow);
        }
        catch
        {
            isDestroyed = true;
        }
    }

    /// <summary>
    /// 更新血条显示
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthFillImage == null || isDestroyed || !isInitialized) return;

        float ratio = maxHP > 0 ? (float)currentHP / maxHP : 0f;

        try
        {
            // 更新填充
            RectTransform fillRect = healthFillImage.GetComponent<RectTransform>();
            if (fillRect != null)
            {
                fillRect.anchorMax = new Vector2(ratio, 1f);
            }

            // 根据血量比例改变颜色
            if (ratio > 0.5f)
            {
                healthFillImage.color = healthColor;
            }
            else if (ratio > 0.25f)
            {
                healthFillImage.color = Color.Lerp(damageColor, healthColor, (ratio - 0.25f) / 0.25f);
            }
            else
            {
                healthFillImage.color = damageColor;
            }
        }
        catch
        {
            isDestroyed = true;
        }
    }

    /// <summary>
    /// 外部调用：设置血量
    /// </summary>
    public void SetHealth(int current, int max)
    {
        if (isDestroyed || !isInitialized) return;

        if (current < currentHP)
        {
            lastDamageTime = Time.time;  // 记录受伤时间
        }

        currentHP = current;
        maxHP = max;
        UpdateHealthBar();
    }

    /// <summary>
    /// 外部调用：受到伤害时调用（触发显示）
    /// </summary>
    public void OnTakeDamage(int damage)
    {
        if (isDestroyed || !isInitialized) return;

        lastDamageTime = Time.time;
        TryGetHealthData();
        UpdateHealthBar();
    }

    /// <summary>
    /// 外部调用：强制显示
    /// </summary>
    public void Show()
    {
        lastDamageTime = Time.time;
    }

    /// <summary>
    /// 外部调用：设置是否一直显示
    /// </summary>
    public void SetAlwaysShow(bool show)
    {
        alwaysShow = show;
    }
}
