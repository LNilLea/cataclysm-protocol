using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 战争迷雾系统 - 探索模式视野限制
/// 玩家视野外的区域会被迷雾遮挡
/// </summary>
public class FogOfWar : MonoBehaviour
{
    public static FogOfWar Instance { get; private set; }

    [Header("引用")]
    public Transform player;
    public PlayerVision playerVision;

    [Header("迷雾设置")]
    public int fogResolution = 128;             // 迷雾纹理分辨率
    public float worldSize = 50f;               // 世界大小（覆盖范围）
    public float fogHeight = 10f;               // 迷雾平面高度

    [Header("视野设置")]
    public float visionRadius = 10f;            // 视野半径
    public float visionAngle = 120f;            // 视野角度
    public float edgeSoftness = 2f;             // 边缘柔和度

    [Header("迷雾颜色")]
    public Color unexploredColor = new Color(0, 0, 0, 1f);      // 未探索区域（完全黑）
    public Color exploredColor = new Color(0, 0, 0, 0.5f);      // 已探索但不可见区域（半透明）
    public Color visibleColor = new Color(0, 0, 0, 0f);         // 可见区域（透明）

    [Header("更新设置")]
    public float updateInterval = 0.1f;         // 更新间隔
    public bool rememberExplored = true;        // 是否记住已探索区域

    [Header("迷雾对象")]
    public GameObject fogPlane;                 // 迷雾平面
    public Material fogMaterial;                // 迷雾材质

    // 迷雾纹理
    private Texture2D fogTexture;
    private Color[] fogPixels;
    private Color[] exploredPixels;             // 记录已探索区域

    // 更新计时器
    private float updateTimer = 0f;

    // 是否启用
    private bool isEnabled = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 获取引用
        if (player == null)
        {
            var playerObj = FindObjectOfType<MyGame.Player>();
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (playerVision == null)
            playerVision = FindObjectOfType<PlayerVision>();

        // 初始化迷雾
        InitializeFog();
    }

    private void Update()
    {
        if (!isEnabled || player == null) return;

        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateFog();
        }
    }

    /// <summary>
    /// 初始化迷雾
    /// </summary>
    private void InitializeFog()
    {
        // 创建迷雾纹理
        fogTexture = new Texture2D(fogResolution, fogResolution, TextureFormat.RGBA32, false);
        fogTexture.filterMode = FilterMode.Bilinear;
        fogTexture.wrapMode = TextureWrapMode.Clamp;

        // 初始化像素数组
        fogPixels = new Color[fogResolution * fogResolution];
        exploredPixels = new Color[fogResolution * fogResolution];

        // 填充为未探索状态
        for (int i = 0; i < fogPixels.Length; i++)
        {
            fogPixels[i] = unexploredColor;
            exploredPixels[i] = unexploredColor;
        }

        fogTexture.SetPixels(fogPixels);
        fogTexture.Apply();

        // 创建迷雾平面
        CreateFogPlane();
    }

    /// <summary>
    /// 创建迷雾平面
    /// </summary>
    private void CreateFogPlane()
    {
        if (fogPlane == null)
        {
            fogPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fogPlane.name = "FogOfWarPlane";
            fogPlane.transform.SetParent(transform);

            // 移除碰撞体
            Destroy(fogPlane.GetComponent<Collider>());
        }

        // 设置位置和大小
        fogPlane.transform.position = new Vector3(worldSize / 2f, fogHeight, worldSize / 2f);
        fogPlane.transform.rotation = Quaternion.Euler(90, 0, 0);
        fogPlane.transform.localScale = new Vector3(worldSize, worldSize, 1);

        // 创建材质
        if (fogMaterial == null)
        {
            fogMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        fogMaterial.mainTexture = fogTexture;
        fogPlane.GetComponent<Renderer>().material = fogMaterial;

        // 设置渲染层级（确保在其他物体上方）
        fogPlane.GetComponent<Renderer>().sortingOrder = 100;
    }

    /// <summary>
    /// 更新迷雾
    /// </summary>
    private void UpdateFog()
    {
        if (player == null) return;

        // 获取玩家在纹理上的位置
        Vector2 playerTexPos = WorldToTexturePosition(player.position);

        // 获取视野参数
        float radius = visionRadius;
        float angle = visionAngle;

        if (playerVision != null)
        {
            radius = playerVision.currentVisionRange;
            angle = playerVision.visionAngle;
        }

        // 将视野半径转换为纹理像素
        float texRadius = (radius / worldSize) * fogResolution;
        float texSoftness = (edgeSoftness / worldSize) * fogResolution;

        // 获取玩家朝向
        Vector2 forward = new Vector2(player.forward.x, player.forward.z).normalized;

        // 更新迷雾像素
        for (int y = 0; y < fogResolution; y++)
        {
            for (int x = 0; x < fogResolution; x++)
            {
                int index = y * fogResolution + x;

                Vector2 pixelPos = new Vector2(x, y);
                Vector2 toPixel = pixelPos - playerTexPos;
                float distance = toPixel.magnitude;

                // 检查是否在视野范围内
                bool inRange = distance <= texRadius;

                // 检查是否在视野角度内
                bool inAngle = true;
                if (inRange && distance > 0.1f)
                {
                    Vector2 toPixelNorm = toPixel.normalized;
                    float dot = Vector2.Dot(forward, toPixelNorm);
                    float pixelAngle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
                    inAngle = pixelAngle <= angle / 2f;
                }

                // 检查视线遮挡
                bool blocked = false;
                if (inRange && inAngle)
                {
                    blocked = IsLineBlocked(playerTexPos, pixelPos);
                }

                // 计算可见度
                if (inRange && inAngle && !blocked)
                {
                    // 可见区域
                    float visibility = 1f;

                    // 边缘柔和
                    if (distance > texRadius - texSoftness)
                    {
                        visibility = 1f - (distance - (texRadius - texSoftness)) / texSoftness;
                    }

                    // 角度边缘柔和
                    if (distance > 0.1f)
                    {
                        Vector2 toPixelNorm = toPixel.normalized;
                        float dot = Vector2.Dot(forward, toPixelNorm);
                        float pixelAngle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
                        float angleRatio = pixelAngle / (angle / 2f);

                        if (angleRatio > 0.8f)
                        {
                            visibility *= 1f - (angleRatio - 0.8f) / 0.2f;
                        }
                    }

                    visibility = Mathf.Clamp01(visibility);

                    fogPixels[index] = Color.Lerp(exploredColor, visibleColor, visibility);

                    // 记住已探索区域
                    if (rememberExplored)
                    {
                        exploredPixels[index] = exploredColor;
                    }
                }
                else
                {
                    // 不可见区域
                    if (rememberExplored)
                    {
                        fogPixels[index] = exploredPixels[index];
                    }
                    else
                    {
                        fogPixels[index] = unexploredColor;
                    }
                }
            }
        }

        // 应用更新
        fogTexture.SetPixels(fogPixels);
        fogTexture.Apply();
    }

    /// <summary>
    /// 检查视线是否被遮挡（简化版，使用纹理采样）
    /// </summary>
    private bool IsLineBlocked(Vector2 from, Vector2 to)
    {
        // 简化实现：使用射线检测
        Vector3 worldFrom = TextureToWorldPosition(from);
        Vector3 worldTo = TextureToWorldPosition(to);

        Vector3 direction = worldTo - worldFrom;
        float distance = direction.magnitude;

        if (distance < 0.1f) return false;

        // 从玩家眼睛高度发射射线
        worldFrom.y = 1.5f;
        worldTo.y = 0.5f;

        RaycastHit hit;
        if (Physics.Raycast(worldFrom, direction.normalized, out hit, distance))
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Obstacle"))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 世界坐标转纹理坐标
    /// </summary>
    private Vector2 WorldToTexturePosition(Vector3 worldPos)
    {
        float x = (worldPos.x / worldSize) * fogResolution;
        float y = (worldPos.z / worldSize) * fogResolution;
        return new Vector2(x, y);
    }

    /// <summary>
    /// 纹理坐标转世界坐标
    /// </summary>
    private Vector3 TextureToWorldPosition(Vector2 texPos)
    {
        float x = (texPos.x / fogResolution) * worldSize;
        float z = (texPos.y / fogResolution) * worldSize;
        return new Vector3(x, 0, z);
    }

    // ===== 公共方法 =====

    /// <summary>
    /// 启用/禁用迷雾
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
        if (fogPlane != null)
        {
            fogPlane.SetActive(enabled);
        }
    }

    /// <summary>
    /// 显示整个地图（调试用）
    /// </summary>
    public void RevealAll()
    {
        for (int i = 0; i < fogPixels.Length; i++)
        {
            fogPixels[i] = visibleColor;
            exploredPixels[i] = exploredColor;
        }

        fogTexture.SetPixels(fogPixels);
        fogTexture.Apply();
    }

    /// <summary>
    /// 隐藏整个地图
    /// </summary>
    public void HideAll()
    {
        for (int i = 0; i < fogPixels.Length; i++)
        {
            fogPixels[i] = unexploredColor;
            exploredPixels[i] = unexploredColor;
        }

        fogTexture.SetPixels(fogPixels);
        fogTexture.Apply();
    }

    /// <summary>
    /// 揭示指定区域
    /// </summary>
    public void RevealArea(Vector3 worldPosition, float radius)
    {
        Vector2 texPos = WorldToTexturePosition(worldPosition);
        float texRadius = (radius / worldSize) * fogResolution;

        for (int y = 0; y < fogResolution; y++)
        {
            for (int x = 0; x < fogResolution; x++)
            {
                Vector2 pixelPos = new Vector2(x, y);
                float distance = Vector2.Distance(pixelPos, texPos);

                if (distance <= texRadius)
                {
                    int index = y * fogResolution + x;
                    exploredPixels[index] = exploredColor;
                }
            }
        }
    }

    /// <summary>
    /// 检查位置是否可见
    /// </summary>
    public bool IsPositionVisible(Vector3 worldPosition)
    {
        Vector2 texPos = WorldToTexturePosition(worldPosition);
        int x = Mathf.Clamp(Mathf.RoundToInt(texPos.x), 0, fogResolution - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(texPos.y), 0, fogResolution - 1);
        int index = y * fogResolution + x;

        return fogPixels[index].a < 0.5f;
    }

    /// <summary>
    /// 检查位置是否已探索
    /// </summary>
    public bool IsPositionExplored(Vector3 worldPosition)
    {
        Vector2 texPos = WorldToTexturePosition(worldPosition);
        int x = Mathf.Clamp(Mathf.RoundToInt(texPos.x), 0, fogResolution - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(texPos.y), 0, fogResolution - 1);
        int index = y * fogResolution + x;

        return exploredPixels[index].a < 1f;
    }

    /// <summary>
    /// 获取已探索数据（用于存档）
    /// </summary>
    public byte[] GetExploredData()
    {
        byte[] data = new byte[fogResolution * fogResolution];
        for (int i = 0; i < exploredPixels.Length; i++)
        {
            data[i] = exploredPixels[i].a < 1f ? (byte)1 : (byte)0;
        }
        return data;
    }

    /// <summary>
    /// 加载已探索数据（用于读档）
    /// </summary>
    public void LoadExploredData(byte[] data)
    {
        if (data == null || data.Length != fogResolution * fogResolution) return;

        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] == 1)
            {
                exploredPixels[i] = exploredColor;
            }
            else
            {
                exploredPixels[i] = unexploredColor;
            }
        }
    }
}
