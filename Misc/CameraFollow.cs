using UnityEngine;

/// <summary>
/// 摄像头跟随 - 跟随玩家并限制在场景边界内
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;                    // 跟随的目标（玩家）
    public bool autoFindPlayer = true;          // 自动查找玩家

    [Header("跟随设置")]
    public float smoothSpeed = 5f;              // 平滑速度
    public Vector3 offset = new Vector3(0, 0, -10f);  // 摄像头偏移

    [Header("边界限制")]
    public bool useBounds = true;               // 是否使用边界限制

    private Camera cam;
    private float camHalfHeight;
    private float camHalfWidth;

    private void Start()
    {
        cam = GetComponent<Camera>();

        // 计算摄像头视野的一半尺寸
        if (cam != null && cam.orthographic)
        {
            camHalfHeight = cam.orthographicSize;
            camHalfWidth = camHalfHeight * cam.aspect;
        }

        // 自动查找玩家
        if (autoFindPlayer && target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("[CameraFollow] 自动找到玩家");
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 目标位置
        Vector3 desiredPosition = target.position + offset;

        // 边界限制
        if (useBounds && SceneBounds.Instance != null)
        {
            desiredPosition = SceneBounds.Instance.ClampPosition(
                desiredPosition, 
                camHalfWidth, 
                camHalfHeight
            );
        }

        // 平滑移动
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position, 
            desiredPosition, 
            smoothSpeed * Time.deltaTime
        );

        // 保持 Z 轴不变
        smoothedPosition.z = offset.z;

        transform.position = smoothedPosition;
    }

    /// <summary>
    /// 设置跟随目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// 立即移动到目标位置（无平滑）
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        if (useBounds && SceneBounds.Instance != null)
        {
            desiredPosition = SceneBounds.Instance.ClampPosition(
                desiredPosition, 
                camHalfWidth, 
                camHalfHeight
            );
        }

        desiredPosition.z = offset.z;
        transform.position = desiredPosition;
    }
}
