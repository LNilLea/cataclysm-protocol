using UnityEngine;

/// <summary>
/// 游戏初始化器 - 确保必要的管理器存在
/// 放在第一个场景中，或者每个场景都放一个（会自动检测避免重复）
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("管理器预制体")]
    [Tooltip("如果没有预制体，会自动创建空对象并添加组件")]
    public GameObject sceneFlowManagerPrefab;
    public GameObject sceneTransitionEffectPrefab;
    public GameObject gameProgressManagerPrefab;

    [Header("自动创建")]
    public bool autoCreateSceneFlowManager = true;
    public bool autoCreateTransitionEffect = true;
    public bool autoCreateProgressManager = true;

    private void Awake()
    {
        InitializeManagers();
    }

    /// <summary>
    /// 初始化所有必要的管理器
    /// </summary>
    private void InitializeManagers()
    {
        // SceneFlowManager
        if (autoCreateSceneFlowManager && SceneFlowManager.Instance == null)
        {
            CreateManager<SceneFlowManager>("SceneFlowManager", sceneFlowManagerPrefab);
        }

        // SceneTransitionEffect
        if (autoCreateTransitionEffect && SceneTransitionEffect.Instance == null)
        {
            CreateManager<SceneTransitionEffect>("SceneTransitionEffect", sceneTransitionEffectPrefab);
        }

        // GameProgressManager
        if (autoCreateProgressManager && GameProgressManager.Instance == null)
        {
            CreateManager<GameProgressManager>("GameProgressManager", gameProgressManagerPrefab);
        }
    }

    /// <summary>
    /// 创建管理器
    /// </summary>
    private void CreateManager<T>(string name, GameObject prefab) where T : MonoBehaviour
    {
        GameObject managerObj;

        if (prefab != null)
        {
            managerObj = Instantiate(prefab);
            managerObj.name = name;
        }
        else
        {
            managerObj = new GameObject(name);
            managerObj.AddComponent<T>();
        }

        DontDestroyOnLoad(managerObj);
        Debug.Log($"[GameInitializer] 创建了 {name}");
    }
}
