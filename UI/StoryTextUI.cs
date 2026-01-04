using UnityEngine;
using TMPro;

/// <summary>
/// 剧情文本 UI - 显示剧情对话/描述
/// </summary>
public class StoryTextUI : MonoBehaviour
{
    [Header("UI 元素")]
    public GameObject panel;                  // 整个面板
    public TMP_Text titleText;                // 标题（物品名）
    public TMP_Text contentText;              // 内容（剧情文本）
    public GameObject continuePrompt;         // "按任意键继续" 提示

    [Header("设置")]
    public float typingSpeed = 0.03f;         // 打字机效果速度（0 = 立即显示）
    public bool useTypingEffect = true;       // 是否使用打字机效果

    private bool isDisplaying = false;
    private bool isTyping = false;
    private string fullText = "";
    private Coroutine typingCoroutine;

    private void Start()
    {
        // 初始隐藏
        if (panel != null)
            panel.SetActive(false);
    }

    private void Update()
    {
        if (!isDisplaying) return;

        // 按任意键
        if (Input.anyKeyDown)
        {
            if (isTyping)
            {
                // 正在打字，跳过打字效果
                SkipTyping();
            }
            else
            {
                // 关闭面板
                HideText();
            }
        }
    }

    /// <summary>
    /// 显示剧情文本
    /// </summary>
    public void ShowText(string content, string title = "")
    {
        if (panel == null)
        {
            Debug.LogError("StoryTextUI: 没有设置 Panel！");
            return;
        }

        fullText = content;

        // 设置标题
        if (titleText != null)
        {
            titleText.text = string.IsNullOrEmpty(title) ? "" : title;
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
        }

        // 显示面板
        panel.SetActive(true);
        isDisplaying = true;

        // 隐藏继续提示
        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        // 打字机效果或立即显示
        if (useTypingEffect && typingSpeed > 0)
        {
            typingCoroutine = StartCoroutine(TypeText(content));
        }
        else
        {
            contentText.text = content;
            OnTypingComplete();
        }
    }

    /// <summary>
    /// 打字机效果
    /// </summary>
    private System.Collections.IEnumerator TypeText(string text)
    {
        isTyping = true;
        contentText.text = "";

        foreach (char c in text)
        {
            contentText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        OnTypingComplete();
    }

    /// <summary>
    /// 跳过打字效果
    /// </summary>
    private void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        contentText.text = fullText;
        OnTypingComplete();
    }

    /// <summary>
    /// 打字完成
    /// </summary>
    private void OnTypingComplete()
    {
        isTyping = false;

        // 显示继续提示
        if (continuePrompt != null)
            continuePrompt.SetActive(true);
    }

    /// <summary>
    /// 隐藏文本
    /// </summary>
    public void HideText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        isDisplaying = false;
        isTyping = false;

        if (panel != null)
            panel.SetActive(false);
    }

    /// <summary>
    /// 是否正在显示
    /// </summary>
    public bool IsDisplaying()
    {
        return isDisplaying;
    }
}
