using UnityEngine;
using MyGame;
/// <summary>
/// 战斗摄像机控制 - 修复版
/// 修复：拖拽后暂停跟随，直到按空格或点击玩家才恢复
/// </summary>
public class BattleCameraController : MonoBehaviour
{
    [Header("缩放设置")]
    public float zoomSpeed = 2f;            // 缩放速度
    public float minZoom = 3f;              // 最小视野（最近）
    public float maxZoom = 15f;             // 最大视野（最远）
    public float smoothSpeed = 10f;         // 平滑速度

    [Header("拖拽移动")]
    public bool enableDrag = true;          // 是否启用拖拽
    public float dragSpeed = 1f;            // 拖拽速度
    public int dragButton = 2;              // 拖拽按钮（0=左键, 1=右键, 2=中键）

    [Header("边界限制")]
    public bool useBounds = false;          // 是否限制移动范围
    public Vector2 minBounds = new Vector2(-20, -20);
    public Vector2 maxBounds = new Vector2(20, 20);

    [Header("跟随玩家")]
    public bool followPlayer = true;        // 是否跟随玩家
    public Transform playerTransform;
    public float followSmoothness = 5f;
    public KeyCode returnToPlayerKey = KeyCode.Space;  // 按此键回到玩家

    private Camera cam;
    private float targetZoom;
    private Vector3 dragOrigin;
    private bool isDragging = false;
    private bool isPaused = false;          // 拖拽后暂停跟随

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        if (cam != null)
        {
            targetZoom = cam.orthographicSize;
        }

        // 自动查找玩家
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                // 尝试用类型查找
                Player p = FindObjectOfType<Player>();
                if (p != null) playerTransform = p.transform;
            }
        }
    }

    private void Update()
    {
        HandleZoom();
        HandleDrag();
        HandleReturnToPlayer();
        HandleFollow();
        ApplyBounds();
    }

    /// <summary>
    /// 处理滚轮缩放
    /// </summary>
    void HandleZoom()
    {
        if (cam == null) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (scroll != 0)
        {
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // 平滑缩放
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * smoothSpeed);
    }

    /// <summary>
    /// 处理拖拽移动
    /// </summary>
    void HandleDrag()
    {
        if (!enableDrag || cam == null) return;

        // 开始拖拽
        if (Input.GetMouseButtonDown(dragButton))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        // 拖拽中
        if (Input.GetMouseButton(dragButton) && isDragging)
        {
            Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 difference = dragOrigin - currentPos;
            
            transform.position += new Vector3(difference.x, difference.y, 0);
            
            // 更新拖拽起点，避免累积
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        // 结束拖拽 - 暂停跟随
        if (Input.GetMouseButtonUp(dragButton) && isDragging)
        {
            isDragging = false;
            isPaused = true;  // 拖拽结束后暂停跟随
        }
    }

    /// <summary>
    /// 处理返回玩家
    /// </summary>
    void HandleReturnToPlayer()
    {
        // 按空格键回到玩家
        if (Input.GetKeyDown(returnToPlayerKey))
        {
            isPaused = false;
            FocusOnPlayer();
        }
    }

    /// <summary>
    /// 跟随玩家
    /// </summary>
    void HandleFollow()
    {
        // 如果暂停或正在拖拽，不跟随
        if (!followPlayer || playerTransform == null || isDragging || isPaused) return;

        Vector3 targetPos = new Vector3(
            playerTransform.position.x,
            playerTransform.position.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSmoothness);
    }

    /// <summary>
    /// 应用边界限制
    /// </summary>
    void ApplyBounds()
    {
        if (!useBounds) return;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        transform.position = pos;
    }

    /// <summary>
    /// 聚焦到指定位置
    /// </summary>
    public void FocusOn(Vector3 position)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        isPaused = true;  // 聚焦后也暂停跟随
    }

    /// <summary>
    /// 聚焦到玩家
    /// </summary>
    public void FocusOnPlayer()
    {
        if (playerTransform != null)
        {
            transform.position = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y,
                transform.position.z
            );
            isPaused = false;  // 回到玩家后恢复跟随
        }
    }

    /// <summary>
    /// 暂停/恢复跟随
    /// </summary>
    public void SetFollowPaused(bool paused)
    {
        isPaused = paused;
    }

    /// <summary>
    /// 设置缩放
    /// </summary>
    public void SetZoom(float zoom)
    {
        targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    /// <summary>
    /// 重置摄像机
    /// </summary>
    public void ResetCamera()
    {
        targetZoom = (minZoom + maxZoom) / 2f;
        isPaused = false;
        FocusOnPlayer();
    }
}
