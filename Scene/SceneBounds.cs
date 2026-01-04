using UnityEngine;

/// <summary>
/// 场景边界 - 定义场景的可移动范围
/// </summary>
public class SceneBounds : MonoBehaviour
{
    public static SceneBounds Instance { get; private set; }

    [Header("边界坐标")]
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;

    [Header("调试显示")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.red;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 限制位置在边界内
    /// </summary>
    public Vector3 ClampPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        return position;
    }

    /// <summary>
    /// 限制位置（带边距，用于摄像头）
    /// </summary>
    public Vector3 ClampPosition(Vector3 position, float marginX, float marginY)
    {
        position.x = Mathf.Clamp(position.x, minX + marginX, maxX - marginX);
        position.y = Mathf.Clamp(position.y, minY + marginY, maxY - marginY);
        return position;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0);
        Gizmos.DrawWireCube(center, size);
    }
}
