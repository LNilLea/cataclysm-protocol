using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 场景过渡效果 - 淡入淡出
/// </summary>
public class SceneTransitionEffect : MonoBehaviour
{
    public static SceneTransitionEffect Instance { get; private set; }

    [Header("淡入淡出面板")]
    public Image fadeImage;                     // 全屏黑色 Image
    public float defaultFadeDuration = 0.5f;    // 默认淡入淡出时长

    [Header("淡入淡出颜色")]
    public Color fadeColor = Color.black;       // 【新增】可配置的淡入淡出颜色，默认黑色

    [Header("自动淡入")]
    public bool fadeInOnStart = true;           // 场景开始时自动淡入

    private void Awake()
    {
        // 单例（跨场景保留）
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 确保有 fadeImage
        if (fadeImage == null)
        {
            CreateFadeImage();
        }

        // 【新增】确保颜色正确
        EnsureFadeColor();

        // 场景开始时淡入
        if (fadeInOnStart)
        {
            FadeIn(defaultFadeDuration);
        }
    }

    /// <summary>
    /// 【新增】确保淡入淡出颜色正确
    /// </summary>
    private void EnsureFadeColor()
    {
        if (fadeImage != null)
        {
            Color c = fadeColor;
            c.a = fadeImage.color.a;  // 保留当前透明度
            fadeImage.color = c;
        }
    }

    /// <summary>
    /// 自动创建淡入淡出 Image
    /// </summary>
    private void CreateFadeImage()
    {
        // 创建 Canvas
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;  // 最顶层

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // 创建 Image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);  // 【修改】使用配置的颜色
        fadeImage.raycastTarget = false;

        // 全屏
        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// 淡入（从遮罩到透明）
    /// </summary>
    public void FadeIn(float duration = -1)
    {
        if (duration < 0) duration = defaultFadeDuration;

        // 【新增】确保开始时颜色正确且完全不透明
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        }

        StartCoroutine(FadeCoroutine(1f, 0f, duration, null));
    }

    /// <summary>
    /// 淡出（从透明到遮罩）
    /// </summary>
    public void FadeOut(float duration = -1, System.Action onComplete = null)
    {
        if (duration < 0) duration = defaultFadeDuration;

        // 【新增】确保开始时颜色正确且完全透明
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        }

        StartCoroutine(FadeCoroutine(0f, 1f, duration, onComplete));
    }

    /// <summary>
    /// 淡出后加载场景
    /// </summary>
    public void FadeOutAndLoadScene(string sceneName, float duration = -1)
    {
        if (duration < 0) duration = defaultFadeDuration;

        FadeOut(duration, () =>
        {
            // 尝试用名称加载
            int sceneIndex;
            if (int.TryParse(sceneName, out sceneIndex))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
        });
    }

    /// <summary>
    /// 淡入淡出协程
    /// </summary>
    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration, System.Action onComplete)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;

        // 【修改】使用配置的颜色
        Color color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, startAlpha);
        fadeImage.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;  // 不受 Time.timeScale 影响
            float t = elapsed / duration;

            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = color;

            yield return null;
        }

        // 确保最终值
        color.a = endAlpha;
        fadeImage.color = color;

        // 回调
        onComplete?.Invoke();
    }

    /// <summary>
    /// 场景加载后自动淡入
    /// </summary>
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 新场景加载后淡入
        if (fadeInOnStart && fadeImage != null)
        {
            // 【新增】确保颜色正确
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
            FadeIn(defaultFadeDuration);
        }
    }

    /// <summary>
    /// 【新增】运行时设置淡入淡出颜色
    /// </summary>
    public void SetFadeColor(Color newColor)
    {
        fadeColor = newColor;
    }
}
