using UnityEngine;

/// <summary>
/// 视野区域 - 放在场景中，定义不同区域的视野范围
/// 玩家进入该区域时，视野范围会改变
/// </summary>
[RequireComponent(typeof(Collider))]
public class VisionZone : MonoBehaviour
{
    [Header("区域设置")]
    public VisionZoneType zoneType = VisionZoneType.Normal;
    
    [Header("自定义视野范围（可选）")]
    public bool useCustomRange = false;
    public float customVisionRange = 6f;

    [Header("视觉效果（可选）")]
    public Color zoneColor = new Color(1, 1, 1, 0.1f);
    public bool showZoneGizmo = true;

    private void Start()
    {
        // 确保 Collider 是 Trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 检查是否是玩家
        if (other.CompareTag("Player"))
        {
            OnPlayerEnterZone();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 检查是否是玩家
        if (other.CompareTag("Player"))
        {
            OnPlayerExitZone();
        }
    }

    /// <summary>
    /// 玩家进入区域
    /// </summary>
    private void OnPlayerEnterZone()
    {
        if (PlayerVision.Instance == null) return;

        PlayerVision.Instance.SetVisionZone(zoneType);

        // 如果使用自定义范围
        if (useCustomRange)
        {
            PlayerVision.Instance.currentVisionRange = customVisionRange;
        }

        Debug.Log($"玩家进入 {zoneType} 区域，视野范围: {PlayerVision.Instance.currentVisionRange}");
    }

    /// <summary>
    /// 玩家离开区域
    /// </summary>
    private void OnPlayerExitZone()
    {
        // 恢复默认视野
        if (PlayerVision.Instance != null)
        {
            PlayerVision.Instance.SetVisionZone(VisionZoneType.Normal);
        }

        Debug.Log("玩家离开视野区域，恢复默认视野");
    }

    /// <summary>
    /// 获取区域颜色（根据类型）
    /// </summary>
    private Color GetZoneDisplayColor()
    {
        switch (zoneType)
        {
            case VisionZoneType.Bright:
                return new Color(1, 1, 0.5f, 0.2f); // 亮黄色
            case VisionZoneType.Normal:
                return new Color(0.5f, 0.5f, 0.5f, 0.2f); // 灰色
            case VisionZoneType.Dark:
                return new Color(0.2f, 0.2f, 0.3f, 0.3f); // 深蓝色
            default:
                return zoneColor;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showZoneGizmo) return;

        Gizmos.color = GetZoneDisplayColor();

        // 绘制区域
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = oldMatrix;
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
            Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
        }

        // 绘制标签
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"{zoneType} Zone");
    }
#endif
}
