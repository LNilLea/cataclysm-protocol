using UnityEngine;
using TMPro;

/// <summary>
/// 伤害数字 - 单个飘字的行为
/// </summary>
public class DamagePopup : MonoBehaviour
{
    [Header("动画设置")]
    public float floatSpeed = 1.5f;         // 上飘速度
    public float floatDuration = 1f;        // 持续时间
    public float fadeStartTime = 0.5f;      // 开始淡出的时间
    public float scaleStart = 0.5f;         // 初始缩放
    public float scaleMax = 1.2f;           // 最大缩放
    public float scaleDuration = 0.2f;      // 缩放动画时长

    [Header("随机偏移")]
    public float randomOffsetX = 0.3f;      // X轴随机偏移
    public float randomOffsetY = 0.2f;      // Y轴随机偏移

    private TMP_Text textMesh;
    private Color originalColor;
    private float timer = 0f;
    private Vector3 moveDirection;

    private void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TMP_Text>();
        }
    }

    private void Start()
    {
        if (textMesh != null)
        {
            originalColor = textMesh.color;
        }

        // 随机偏移方向
        float randX = Random.Range(-randomOffsetX, randomOffsetX);
        float randY = Random.Range(0, randomOffsetY);
        moveDirection = new Vector3(randX, 1f + randY, 0).normalized;

        // 初始缩放
        transform.localScale = Vector3.one * scaleStart;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 上飘
        transform.position += moveDirection * floatSpeed * Time.deltaTime;

        // 缩放动画
        if (timer < scaleDuration)
        {
            float scaleProgress = timer / scaleDuration;
            float scale = Mathf.Lerp(scaleStart, scaleMax, scaleProgress);
            transform.localScale = Vector3.one * scale;
        }
        else if (timer < scaleDuration * 2)
        {
            float scaleProgress = (timer - scaleDuration) / scaleDuration;
            float scale = Mathf.Lerp(scaleMax, 1f, scaleProgress);
            transform.localScale = Vector3.one * scale;
        }

        // 淡出
        if (timer > fadeStartTime && textMesh != null)
        {
            float fadeProgress = (timer - fadeStartTime) / (floatDuration - fadeStartTime);
            Color c = originalColor;
            c.a = Mathf.Lerp(1f, 0f, fadeProgress);
            textMesh.color = c;
        }

        // 销毁
        if (timer >= floatDuration)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 设置伤害数字
    /// </summary>
    public void Setup(int damage, bool isCritical = false, bool isHeal = false)
    {
        if (textMesh == null) return;

        if (isHeal)
        {
            textMesh.text = $"+{damage}";
            textMesh.color = new Color(0.2f, 0.9f, 0.2f); // 绿色
        }
        else if (isCritical)
        {
            textMesh.text = $"{damage}!";
            textMesh.color = new Color(1f, 0.8f, 0f);     // 金色
            transform.localScale = Vector3.one * scaleStart * 1.5f;
            scaleMax *= 1.3f;
        }
        else
        {
            textMesh.text = $"-{damage}";
            textMesh.color = new Color(1f, 0.3f, 0.3f);   // 红色
        }

        originalColor = textMesh.color;
    }

    /// <summary>
    /// 设置未命中
    /// </summary>
    public void SetupMiss()
    {
        if (textMesh == null) return;

        textMesh.text = "MISS";
        textMesh.color = new Color(0.7f, 0.7f, 0.7f);     // 灰色
        textMesh.fontSize *= 0.8f;
        originalColor = textMesh.color;
    }
}
