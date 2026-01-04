using UnityEngine;
using TMPro;

/// <summary>
/// 剧情/对话 UI 控制器
/// 控制文字显示的出现和隐藏
/// </summary>
public class StoryUIController : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject storyPanel;       // "文字显示" 这个物体
    public TMP_Text titleText;          // 标题文本
    public TMP_Text contentText;        // 内容文本
    public TMP_Text continuePrompt;     // "按任意键继续" 提示

    [Header("设置")]
    public KeyCode continueKey = KeyCode.Space;  // 继续按键
    public float typeSpeed = 0.05f;              // 打字机效果速度

    // 状态
    private bool isShowing = false;
    private bool isTyping = false;
    private string fullText = "";

    void Start()
    {
        // 游戏开始时隐藏
        Hide();
    }

    void Update()
    {
        // 如果正在显示，按键继续
        if (isShowing && Input.GetKeyDown(continueKey))
        {
            if (isTyping)
            {
                // 跳过打字效果，直接显示全部
                StopAllCoroutines();
                if (contentText != null)
                    contentText.text = fullText;
                isTyping = false;
            }
            else
            {
                // 隐藏对话框
                Hide();
            }
        }
    }

    /// <summary>
    /// 显示剧情文字
    /// </summary>
    public void Show(string title, string content)
    {
        if (storyPanel != null)
            storyPanel.SetActive(true);

        if (titleText != null)
            titleText.text = title;

        if (contentText != null)
        {
            fullText = content;
            StartCoroutine(TypeText(content));
        }

        if (continuePrompt != null)
            continuePrompt.gameObject.SetActive(false);

        isShowing = true;
    }

    /// <summary>
    /// 简单显示（只有内容）
    /// </summary>
    public void Show(string content)
    {
        Show("", content);
    }

    /// <summary>
    /// 隐藏剧情UI
    /// </summary>
    public void Hide()
    {
        if (storyPanel != null)
            storyPanel.SetActive(false);

        isShowing = false;
        isTyping = false;
        StopAllCoroutines();
    }

    /// <summary>
    /// 打字机效果
    /// </summary>
    System.Collections.IEnumerator TypeText(string text)
    {
        isTyping = true;
        contentText.text = "";

        foreach (char c in text)
        {
            contentText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;

        // 显示继续提示
        if (continuePrompt != null)
            continuePrompt.gameObject.SetActive(true);
    }

    /// <summary>
    /// 检查是否正在显示
    /// </summary>
    public bool IsShowing()
    {
        return isShowing;
    }
}
