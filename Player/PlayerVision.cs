using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 玩家视野系统 - 前方扇形视野，范围受环境影响
/// </summary>
public class PlayerVision : MonoBehaviour
{
    public static PlayerVision Instance { get; private set; }

    [Header("视野设置")]
    public float baseVisionRange = 6f;          // 基础视野范围（格数）
    public float visionAngle = 120f;            // 视野角度（前方扇形）
    public float currentVisionRange;            // 当前实际视野范围

    [Header("环境视野倍率")]
    public float brightMultiplier = 1.5f;       // 明亮区域倍率
    public float normalMultiplier = 1.0f;       // 普通区域倍率
    public float darkMultiplier = 0.5f;         // 黑暗区域倍率

    [Header("检测设置")]
    public float detectionInterval = 0.2f;      // 检测间隔（秒）
    public LayerMask obstacleLayer;             // 障碍物层（用于视线遮挡）
    public LayerMask monsterLayer;              // 怪物层

    [Header("调试")]
    public bool showVisionGizmo = true;
    public Color visionColor = new Color(1, 1, 0, 0.3f);

    // 当前视野内的怪物
    private List<GameObject> monstersInSight = new List<GameObject>();

    // 当前所在的视野区域类型
    private VisionZoneType currentZoneType = VisionZoneType.Normal;

    // 事件
    public event System.Action<GameObject> OnMonsterSpotted;    // 发现怪物
    public event System.Action<GameObject> OnMonsterLost;       // 丢失怪物视野

    private float detectionTimer = 0f;

    private void Awake()
    {
        Instance = this;
        currentVisionRange = baseVisionRange;
    }

    private void Update()
    {
        detectionTimer += Time.deltaTime;
        if (detectionTimer >= detectionInterval)
        {
            detectionTimer = 0f;
            DetectMonstersInVision();
        }
    }

    /// <summary>
    /// 检测视野内的怪物
    /// </summary>
    private void DetectMonstersInVision()
    {
        // 获取范围内所有怪物
        Collider[] colliders = Physics.OverlapSphere(transform.position, currentVisionRange, monsterLayer);

        List<GameObject> currentVisible = new List<GameObject>();

        foreach (var col in colliders)
        {
            GameObject monster = col.gameObject;

            // 检查是否在视野角度内
            if (IsInVisionAngle(monster.transform.position))
            {
                // 检查是否有障碍物遮挡
                if (!IsBlockedByObstacle(monster.transform.position))
                {
                    currentVisible.Add(monster);

                    // 新发现的怪物
                    if (!monstersInSight.Contains(monster))
                    {
                        OnMonsterSpotted?.Invoke(monster);
                        Debug.Log($"发现怪物: {monster.name}");
                    }
                }
            }
        }

        // 检查丢失视野的怪物
        foreach (var monster in monstersInSight)
        {
            if (!currentVisible.Contains(monster))
            {
                OnMonsterLost?.Invoke(monster);
                Debug.Log($"丢失怪物视野: {monster.name}");
            }
        }

        monstersInSight = currentVisible;
    }

    /// <summary>
    /// 检查目标是否在视野角度内（前方扇形）
    /// </summary>
    public bool IsInVisionAngle(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        directionToTarget.y = 0; // 忽略高度差

        Vector3 forward = transform.forward;
        forward.y = 0;

        float angle = Vector3.Angle(forward, directionToTarget);

        return angle <= visionAngle / 2f;
    }

    /// <summary>
    /// 检查视线是否被障碍物遮挡
    /// </summary>
    public bool IsBlockedByObstacle(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        float distance = direction.magnitude;

        // 从眼睛高度发射射线
        Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
        Vector3 targetEyePosition = targetPosition + Vector3.up * 1f;

        if (Physics.Raycast(eyePosition, (targetEyePosition - eyePosition).normalized, distance, obstacleLayer))
        {
            return true; // 被遮挡
        }

        return false;
    }

    /// <summary>
    /// 检查目标是否在视野内
    /// </summary>
    public bool IsInSight(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);

        // 检查距离
        if (distance > currentVisionRange) return false;

        // 检查角度
        if (!IsInVisionAngle(targetPosition)) return false;

        // 检查遮挡
        if (IsBlockedByObstacle(targetPosition)) return false;

        return true;
    }

    /// <summary>
    /// 检查目标是否在玩家背后
    /// </summary>
    public bool IsBehindPlayer(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        directionToTarget.y = 0;

        Vector3 forward = transform.forward;
        forward.y = 0;

        float angle = Vector3.Angle(forward, directionToTarget);

        // 背后定义为视野角度之外
        return angle > visionAngle / 2f;
    }

    /// <summary>
    /// 设置当前视野区域类型
    /// </summary>
    public void SetVisionZone(VisionZoneType zoneType)
    {
        currentZoneType = zoneType;
        UpdateVisionRange();
    }

    /// <summary>
    /// 更新视野范围
    /// </summary>
    private void UpdateVisionRange()
    {
        switch (currentZoneType)
        {
            case VisionZoneType.Bright:
                currentVisionRange = baseVisionRange * brightMultiplier;
                break;
            case VisionZoneType.Normal:
                currentVisionRange = baseVisionRange * normalMultiplier;
                break;
            case VisionZoneType.Dark:
                currentVisionRange = baseVisionRange * darkMultiplier;
                break;
        }

        Debug.Log($"视野范围更新: {currentVisionRange} (区域: {currentZoneType})");
    }

    /// <summary>
    /// 获取当前视野内的怪物列表
    /// </summary>
    public List<GameObject> GetMonstersInSight()
    {
        return new List<GameObject>(monstersInSight);
    }

    /// <summary>
    /// 检查指定怪物是否在视野内
    /// </summary>
    public bool IsMonsterInSight(GameObject monster)
    {
        return monstersInSight.Contains(monster);
    }

#if UNITY_EDITOR
    /// <summary>
    /// 绘制视野范围（编辑器调试用）
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showVisionGizmo) return;

        Gizmos.color = visionColor;

        // 绘制视野扇形
        Vector3 forward = transform.forward;
        float halfAngle = visionAngle / 2f;

        // 左边界
        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        // 右边界
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;

        float range = Application.isPlaying ? currentVisionRange : baseVisionRange;

        Gizmos.DrawLine(transform.position, transform.position + leftDir * range);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * range);

        // 绘制弧线（简化为多条线）
        int segments = 20;
        float angleStep = visionAngle / segments;
        Vector3 prevPoint = transform.position + leftDir * range;

        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + angleStep * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = transform.position + dir * range;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
#endif
}

/// <summary>
/// 视野区域类型
/// </summary>
public enum VisionZoneType
{
    Bright,     // 明亮
    Normal,     // 普通
    Dark        // 黑暗
}
