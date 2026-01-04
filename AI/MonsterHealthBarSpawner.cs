using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 怪物血条生成器
/// 挂在怪物身上，自动生成头顶血条
/// </summary>
public class MonsterHealthBarSpawner : MonoBehaviour
{
    [Header("血条预制体")]
    [Tooltip("如果为空，会自动创建一个简单血条")]
    public GameObject healthBarPrefab;

    [Header("位置设置")]
    public Vector3 offset = new Vector3(0, 1.2f, 0);

    [Header("血条设置")]
    public float barWidth = 1f;
    public float barHeight = 0.15f;

    [Header("颜色设置")]
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    private MonsterHealthBar healthBar;

    private void Start()
    {
        CreateHealthBar();
    }

    /// <summary>
    /// 创建血条
    /// </summary>
    private void CreateHealthBar()
    {
        GameObject healthBarObj;

        if (healthBarPrefab != null)
        {
            // 使用预制体
            healthBarObj = Instantiate(healthBarPrefab, transform.position + offset, Quaternion.identity);
        }
        else
        {
            // 自动创建
            healthBarObj = CreateDefaultHealthBar();
        }

        // 设置父物体（不跟随旋转）
        healthBarObj.transform.SetParent(transform);
        healthBarObj.transform.localPosition = offset;

        // 获取或添加 MonsterHealthBar 组件
        healthBar = healthBarObj.GetComponent<MonsterHealthBar>();
        if (healthBar == null)
        {
            healthBar = healthBarObj.AddComponent<MonsterHealthBar>();
        }

        // 设置引用
        healthBar.SetTarget(transform);
        healthBar.offset = offset;  // 传递偏移值
        healthBar.fullHealthColor = fullHealthColor;
        healthBar.midHealthColor = midHealthColor;
        healthBar.lowHealthColor = lowHealthColor;
    }

    /// <summary>
    /// 创建默认血条
    /// </summary>
    private GameObject CreateDefaultHealthBar()
    {
        // 创建根物体
        GameObject root = new GameObject("HealthBar");

        // 创建 Canvas
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;

        // 设置 Canvas 大小
        RectTransform canvasRect = root.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100, 25);  // 宽度100，高度25
        canvasRect.localScale = new Vector3(0.065f, 0.05f, 1f); // 缩放

        // 添加 CanvasScaler
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;

        // 创建背景
        GameObject background = new GameObject("Background");
        background.transform.SetParent(root.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = backgroundColor;
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // 创建填充条
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(root.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = fullHealthColor;
        
        // 必须先设置 sprite，Filled 类型才能正常工作
        fillImage.sprite = CreateWhiteSprite();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0; // 从左到右
        fillImage.fillAmount = 1f;
        
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0.3f);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;

        // 创建数字文本
        GameObject textObj = new GameObject("HPText");
        textObj.transform.SetParent(root.transform, false);
        TMP_Text hpText = textObj.AddComponent<TextMeshProUGUI>();
        hpText.text = "0/0";
        hpText.fontSize = 14;
        hpText.alignment = TextAlignmentOptions.Center;
        hpText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        // 添加 MonsterHealthBar 组件并设置引用
        MonsterHealthBar healthBarComponent = root.AddComponent<MonsterHealthBar>();
        healthBarComponent.fillImage = fillImage;
        healthBarComponent.hpText = hpText;

        return root;
    }

    /// <summary>
    /// 获取血条组件
    /// </summary>
    public MonsterHealthBar GetHealthBar()
    {
        return healthBar;
    }

    /// <summary>
    /// 当受到伤害时调用
    /// </summary>
    public void OnDamaged()
    {
        if (healthBar != null)
        {
            healthBar.OnDamaged();
        }
    }

    /// <summary>
    /// 创建一个白色的Sprite（用于Filled类型Image）
    /// </summary>
    private Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }
}
