using UnityEngine;
using TMPro;

/// <summary>
/// 伤害数字管理器 - 负责生成和显示伤害飘字
/// </summary>
public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [Header("预制体设置")]
    public GameObject damagePopupPrefab;    // 伤害数字预制体

    [Header("生成设置")]
    public Vector3 spawnOffset = new Vector3(0, 1f, 0);  // 生成位置偏移
    public int sortingOrder = 100;
    public float popupScale = 0.01f;        // 飘字缩放

    [Header("字体设置（如果没有预制体）")]
    public TMP_FontAsset defaultFont;
    public float defaultFontSize = 36f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 显示伤害数字
    /// </summary>
    public void ShowDamage(Vector3 position, int damage, bool isCritical = false)
    {
        CreatePopup(position, damage, isCritical, false);
    }

    /// <summary>
    /// 显示治疗数字
    /// </summary>
    public void ShowHeal(Vector3 position, int amount)
    {
        CreatePopup(position, amount, false, true);
    }

    /// <summary>
    /// 显示未命中
    /// </summary>
    public void ShowMiss(Vector3 position)
    {
        GameObject popup = CreatePopupObject(position);
        if (popup != null)
        {
            DamagePopup dp = popup.GetComponent<DamagePopup>();
            if (dp != null)
            {
                dp.SetupMiss();
            }
        }
    }

    /// <summary>
    /// 创建伤害数字
    /// </summary>
    private void CreatePopup(Vector3 position, int value, bool isCritical, bool isHeal)
    {
        GameObject popup = CreatePopupObject(position);
        if (popup != null)
        {
            DamagePopup dp = popup.GetComponent<DamagePopup>();
            if (dp != null)
            {
                dp.Setup(value, isCritical, isHeal);
            }
        }
    }

    /// <summary>
    /// 创建飘字物体
    /// </summary>
    private GameObject CreatePopupObject(Vector3 position)
    {
        Vector3 spawnPos = position + spawnOffset;

        // 如果有预制体，使用预制体
        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
            popup.transform.localScale = Vector3.one * popupScale;  // 缩放到合适大小
            return popup;
        }

        // 否则动态创建
        return CreateDefaultPopup(spawnPos);
    }

    /// <summary>
    /// 动态创建默认伤害数字（不需要预制体）
    /// </summary>
    private GameObject CreateDefaultPopup(Vector3 position)
    {
        // 创建 Canvas
        GameObject canvasObj = new GameObject("DamagePopup");
        canvasObj.transform.position = position;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = sortingOrder;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(200, 50);
        canvasRect.localScale = Vector3.one * popupScale;

        // 创建文字
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(canvasObj.transform);
        textObj.transform.localPosition = Vector3.zero;

        TMP_Text tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = "0";
        tmpText.fontSize = defaultFontSize;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.fontStyle = FontStyles.Bold;

        if (defaultFont != null)
        {
            tmpText.font = defaultFont;
        }

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // 添加动画脚本
        canvasObj.AddComponent<DamagePopup>();

        return canvasObj;
    }

    // ========== 静态便捷方法 ==========

    /// <summary>
    /// 静态方法：显示伤害
    /// </summary>
    public static void Damage(Vector3 position, int damage, bool critical = false)
    {
        if (Instance != null)
        {
            Instance.ShowDamage(position, damage, critical);
        }
        else
        {
            Debug.LogWarning("[DamagePopupManager] Instance not found, creating temporary one");
            CreateTemporaryInstance().ShowDamage(position, damage, critical);
        }
    }

    /// <summary>
    /// 静态方法：显示治疗
    /// </summary>
    public static void Heal(Vector3 position, int amount)
    {
        if (Instance != null)
        {
            Instance.ShowHeal(position, amount);
        }
        else
        {
            CreateTemporaryInstance().ShowHeal(position, amount);
        }
    }

    /// <summary>
    /// 静态方法：显示未命中
    /// </summary>
    public static void Miss(Vector3 position)
    {
        if (Instance != null)
        {
            Instance.ShowMiss(position);
        }
        else
        {
            CreateTemporaryInstance().ShowMiss(position);
        }
    }

    /// <summary>
    /// 创建临时实例
    /// </summary>
    private static DamagePopupManager CreateTemporaryInstance()
    {
        GameObject obj = new GameObject("DamagePopupManager_Temp");
        return obj.AddComponent<DamagePopupManager>();
    }
}
