using UnityEngine;

/// <summary>
/// 出生点脚本 - 玩家进入场景时的出生位置
/// 与Portal分离，避免玩家在传送门处出生
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Header("出生点设置")]
    public string spawnPointID = "default";     // 出生点ID，用于从不同入口进入时选择不同出生点
    public bool isDefaultSpawn = true;          // 是否为默认出生点
    public bool changeScale = false;            // 是否改变玩家缩放
    public float playerScale = 1f;              // 玩家缩放值

    [Header("出生方向")]
    public Vector2 spawnDirection = Vector2.down;   // 玩家出生后面向的方向

    [Header("调试")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.green;

    private static SpawnPoint currentSpawnPoint;

    private void Awake()
    {
        // 注册出生点
        if (isDefaultSpawn || currentSpawnPoint == null)
        {
            currentSpawnPoint = this;
        }
    }

    private void Start()
    {
        // 检查是否需要在此出生点生成玩家
        string targetSpawnID = SceneFlowManager.Instance?.nextSpawnPointID ?? "";
        
        if (isDefaultSpawn && string.IsNullOrEmpty(targetSpawnID))
        {
            SpawnPlayer();
        }
        else if (!string.IsNullOrEmpty(targetSpawnID) && spawnPointID == targetSpawnID)
        {
            SpawnPlayer();
        }
    }

    /// <summary>
    /// 在此出生点生成玩家
    /// </summary>
    public void SpawnPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning($"[SpawnPoint] 找不到Player标签的玩家对象");
            return;
        }

        // 设置位置
        player.transform.position = transform.position;

        // 设置缩放
        if (changeScale)
        {
            player.transform.localScale = new Vector3(playerScale, playerScale, 1f);
        }

        // 设置方向（如果玩家有动画组件）
        Animator animator = player.GetComponent<Animator>();
        if (animator != null && spawnDirection != Vector2.zero)
        {
            animator.SetFloat("MoveX", spawnDirection.x);
            animator.SetFloat("MoveY", spawnDirection.y);
        }

        Debug.Log($"[SpawnPoint] 玩家出生在: {transform.position} (ID: {spawnPointID})");
        
        // 清除目标出生点ID
        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.nextSpawnPointID = "";
        }
    }

    /// <summary>
    /// 获取当前场景的默认出生点
    /// </summary>
    public static SpawnPoint GetDefaultSpawnPoint()
    {
        return currentSpawnPoint;
    }

    /// <summary>
    /// 根据ID查找出生点
    /// </summary>
    public static SpawnPoint FindByID(string id)
    {
        SpawnPoint[] allSpawnPoints = FindObjectsOfType<SpawnPoint>();
        foreach (var sp in allSpawnPoints)
        {
            if (sp.spawnPointID == id)
            {
                return sp;
            }
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        
        // 绘制出生点位置
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // 绘制方向箭头
        if (spawnDirection != Vector2.zero)
        {
            Vector3 direction = new Vector3(spawnDirection.x, spawnDirection.y, 0).normalized;
            Gizmos.DrawRay(transform.position, direction * 0.8f);
        }
        
        // 默认出生点用实心圆
        if (isDefaultSpawn)
        {
            Gizmos.DrawSphere(transform.position, 0.2f);
        }
    }
}
