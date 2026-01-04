using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyGame;
/// <summary>
/// 玩家血条UI
/// 显示在屏幕固定位置，颜色填充条 + 数字
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    [Header("UI 组件")]
    [Tooltip("血条填充图片（需要设置为 Filled 类型）")]
    public Image fillImage;

    [Tooltip("血量数字显示")]
    public TMP_Text hpText;

    [Header("颜色设置")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Tooltip("低血量阈值（百分比）")]
    [Range(0, 1)]
    public float lowHealthThreshold = 0.3f;

    [Tooltip("中等血量阈值（百分比）")]
    [Range(0, 1)]
    public float midHealthThreshold = 0.6f;

    [Header("引用")]
    public Player player;

    private void Start()
    {
        if (player == null)
            player = FindObjectOfType<Player>();

        UpdateHealthBar();
    }

    private void Update()
    {
        UpdateHealthBar();
    }

    /// <summary>
    /// 更新血条显示
    /// </summary>
    public void UpdateHealthBar()
    {
        if (player == null || player.combatData == null) return;

        int currentHP = player.combatData.currentHP;
        int maxHP = player.combatData.maxHP;

        // 计算血量百分比
        float healthPercent = maxHP > 0 ? (float)currentHP / maxHP : 0f;
        healthPercent = Mathf.Clamp01(healthPercent);

        // 更新填充条
        if (fillImage != null)
        {
            fillImage.fillAmount = healthPercent;
            fillImage.color = GetHealthColor(healthPercent);
        }

        // 更新数字
        if (hpText != null)
        {
            hpText.text = $"{currentHP}/{maxHP}";
        }
    }

    /// <summary>
    /// 根据血量百分比获取颜色
    /// </summary>
    private Color GetHealthColor(float percent)
    {
        if (percent <= lowHealthThreshold)
        {
            return lowHealthColor;
        }
        else if (percent <= midHealthThreshold)
        {
            return midHealthColor;
        }
        else
        {
            return fullHealthColor;
        }
    }
}
