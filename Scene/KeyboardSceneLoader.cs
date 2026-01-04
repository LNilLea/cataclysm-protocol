using UnityEngine;

/// <summary>
/// 按键场景切换 - 用于按特定按键跳转场景
/// 可用于开始画面、提示页面等
/// </summary>
public class KeyboardSceneLoader : MonoBehaviour
{
    [Header("按键设置")]
    public KeyCode triggerKey = KeyCode.Space;      // 触发按键
    public bool anyKey = false;                      // 任意键触发

    [Header("目标场景")]
    public string targetSceneName;                   // 目标场景名称
    public bool useNextInFlow = false;              // 使用流程中的下一个场景

    [Header("延迟设置")]
    public float initialDelay = 0.5f;               // 初始延迟（防止误触）
    public bool isReady = false;

    [Header("过渡效果")]
    public bool useFade = true;
    public float fadeDuration = 0.5f;

    [Header("提示文字（可选）")]
    public GameObject pressKeyPrompt;               // "按任意键继续"提示
    public float promptDelay = 1f;                  // 提示显示延迟

    private bool hasTriggered = false;

    private void Start()
    {
        // 隐藏提示
        if (pressKeyPrompt != null)
        {
            pressKeyPrompt.SetActive(false);
        }

        // 延迟后允许触发
        Invoke(nameof(SetReady), initialDelay);

        // 延迟显示提示
        if (pressKeyPrompt != null)
        {
            Invoke(nameof(ShowPrompt), promptDelay);
        }
    }

    private void SetReady()
    {
        isReady = true;
    }

    private void ShowPrompt()
    {
        if (pressKeyPrompt != null)
        {
            pressKeyPrompt.SetActive(true);
        }
    }

    private void Update()
    {
        if (!isReady || hasTriggered) return;

        bool shouldTrigger = false;

        if (anyKey)
        {
            shouldTrigger = Input.anyKeyDown;
        }
        else
        {
            shouldTrigger = Input.GetKeyDown(triggerKey);
        }

        if (shouldTrigger)
        {
            TriggerSceneLoad();
        }
    }

    /// <summary>
    /// 触发场景加载
    /// </summary>
    private void TriggerSceneLoad()
    {
        hasTriggered = true;

        string sceneToLoad = targetSceneName;

        // 如果使用流程中的下一个场景
        if (useNextInFlow && SceneFlowManager.Instance != null)
        {
            string nextScene = SceneFlowManager.Instance.GetNextSceneName();
            if (!string.IsNullOrEmpty(nextScene))
            {
                sceneToLoad = nextScene;
            }
        }

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("[KeyboardSceneLoader] 目标场景名称为空！");
            hasTriggered = false;
            return;
        }

        Debug.Log($"[KeyboardSceneLoader] 按键触发，加载场景: {sceneToLoad}");

        // 隐藏提示
        if (pressKeyPrompt != null)
        {
            pressKeyPrompt.SetActive(false);
        }

        // 加载场景
        if (useFade && SceneTransitionEffect.Instance != null)
        {
            SceneTransitionEffect.Instance.FadeOutAndLoadScene(sceneToLoad, fadeDuration);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
    }

    /// <summary>
    /// 手动触发（可从其他脚本调用）
    /// </summary>
    public void ManualTrigger()
    {
        if (!hasTriggered)
        {
            TriggerSceneLoad();
        }
    }
}
