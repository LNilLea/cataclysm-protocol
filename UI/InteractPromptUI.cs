using UnityEngine;
using TMPro;

/// <summary>
/// 交互提示 UI - 显示 "按 E 交互"
/// 注意：挂载此脚本的物体要保持激活状态！
/// </summary>
public class InteractPromptUI : MonoBehaviour
{
    public static InteractPromptUI Instance { get; private set; }

    [Header("UI 元素")]
    public GameObject promptPanel;      // 提示面板（如果不设置，用自己）
    public TMP_Text promptText;         // 提示文字

    [Header("默认设置")]
    public string defaultPrompt = "按 E 交互";

    private void Awake()
    {
        // 设置单例
        Instance = this;
        Debug.Log("InteractPromptUI: Awake - Instance 已设置");
    }

    private void Start()
    {
        // 启动时自动隐藏
        Hide();
        Debug.Log("InteractPromptUI: Start - 已隐藏");
    }

    /// <summary>
    /// 显示提示
    /// </summary>
    public void Show(string text = "")
    {
        // 如果没设置 promptPanel，用自己
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
        }
        else
        {
            // 显示所有子物体
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        // 设置文字
        if (promptText != null)
        {
            promptText.text = string.IsNullOrEmpty(text) ? defaultPrompt : text;
            promptText.gameObject.SetActive(true);
        }
        
        Debug.Log("InteractPromptUI: Show - " + (string.IsNullOrEmpty(text) ? defaultPrompt : text));
    }

    /// <summary>
    /// 隐藏提示
    /// </summary>
    public void Hide()
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
        else
        {
            // 隐藏所有子物体
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }
        
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }
}
