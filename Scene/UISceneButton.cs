using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI场景切换按钮 - 用于菜单、提示页面等无玩家场景
/// 附加到Button对象上，点击后跳转到指定场景
/// </summary>
[RequireComponent(typeof(Button))]
public class UISceneButton : MonoBehaviour
{
    [Header("目标场景")]
    [Tooltip("要跳转到的场景名称")]
    public string targetSceneName;

    [Header("跳转方式")]
    public SceneLoadType loadType = SceneLoadType.Specific;
    
    public enum SceneLoadType
    {
        Specific,       // 跳转到指定场景
        Next,           // 跳转到流程中的下一个场景
        Previous,       // 跳转到流程中的上一个场景
        Restart         // 重新开始游戏
    }

    [Header("过渡效果")]
    public bool useFade = true;
    public float fadeDuration = 0.5f;

    [Header("音效（可选）")]
    public AudioClip clickSound;
    private AudioSource audioSource;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);

        // 获取或添加AudioSource
        if (clickSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }

    /// <summary>
    /// 按钮点击事件
    /// </summary>
    private void OnButtonClick()
    {
        // 播放音效
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        // 执行场景跳转
        switch (loadType)
        {
            case SceneLoadType.Specific:
                LoadSpecificScene();
                break;
            case SceneLoadType.Next:
                LoadNextScene();
                break;
            case SceneLoadType.Previous:
                LoadPreviousScene();
                break;
            case SceneLoadType.Restart:
                RestartGame();
                break;
        }
    }

    /// <summary>
    /// 加载指定场景
    /// </summary>
    private void LoadSpecificScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[UISceneButton] 目标场景名称为空！");
            return;
        }

        Debug.Log($"[UISceneButton] 加载场景: {targetSceneName}");

        if (useFade && SceneTransitionEffect.Instance != null)
        {
            SceneTransitionEffect.Instance.FadeOutAndLoadScene(targetSceneName, fadeDuration);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }
    }

    /// <summary>
    /// 加载下一个场景（使用SceneFlowManager）
    /// </summary>
    private void LoadNextScene()
    {
        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.GoToNextScene();
        }
        else
        {
            // 备用：直接加载指定场景
            LoadSpecificScene();
        }
    }

    /// <summary>
    /// 加载上一个场景
    /// </summary>
    private void LoadPreviousScene()
    {
        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.GoToPreviousScene();
        }
        else
        {
            Debug.LogWarning("[UISceneButton] SceneFlowManager不存在，无法返回上一个场景");
        }
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    private void RestartGame()
    {
        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.RestartGame();
        }
        else
        {
            // 备用：加载第一个场景
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// 代码调用：设置目标场景并加载
    /// </summary>
    public void LoadScene(string sceneName)
    {
        targetSceneName = sceneName;
        loadType = SceneLoadType.Specific;
        LoadSpecificScene();
    }
}
