using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("精灵图设置")]
    public Sprite[] downSprites;    // 向下走的帧（正面）
    public Sprite[] upSprites;      // 向上走的帧（背面）
    public Sprite[] sideSprites;    // 侧面走的帧

    [Header("动画设置")]
    public float frameRate = 0.15f; // 每帧持续时间

    private SpriteRenderer spriteRenderer;
    private int currentFrame = 0;
    private float frameTimer = 0f;
    private Vector2 lastDirection = Vector2.down;
    private Vector2 previousDirection = Vector2.down;  // 新增：记录上一帧的方向
    private bool isMoving = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        // 获取移动输入
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector2 moveDir = new Vector2(h, v);

        // 判断是否在移动
        isMoving = moveDir.magnitude > 0.1f;

        // 保存上一帧的方向
        previousDirection = lastDirection;

        // 更新朝向
        if (isMoving)
        {
            // 判断主要方向（优先水平方向）
            if (Mathf.Abs(h) > Mathf.Abs(v))
            {
                lastDirection = h > 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                lastDirection = v > 0 ? Vector2.up : Vector2.down;
            }
        }

        // ★ 修复：如果方向改变了，重置帧索引
        if (GetDirectionType(lastDirection) != GetDirectionType(previousDirection))
        {
            currentFrame = 0;
            frameTimer = 0f;
        }

        // 更新动画
        UpdateAnimation();
    }

    /// <summary>
    /// 获取方向类型（用于判断是否切换了动画组）
    /// </summary>
    int GetDirectionType(Vector2 dir)
    {
        if (dir == Vector2.up) return 0;
        if (dir == Vector2.down) return 1;
        return 2;  // 左或右都用侧面
    }

    void UpdateAnimation()
    {
        Sprite[] currentSprites = GetCurrentSprites();

        // ★ 修复：增强检查
        if (currentSprites == null || currentSprites.Length == 0)
        {
            Debug.LogWarning("PlayerAnimation: 当前方向没有设置精灵图！");
            return;
        }

        if (isMoving)
        {
            // 播放行走动画
            frameTimer += Time.deltaTime;
            if (frameTimer >= frameRate)
            {
                frameTimer = 0f;
                currentFrame = (currentFrame + 1) % currentSprites.Length;
            }
        }
        else
        {
            // 静止时显示第一帧
            currentFrame = 0;
            frameTimer = 0f;
        }

        // ★ 修复：确保索引不会越界
        if (currentFrame >= currentSprites.Length)
        {
            currentFrame = 0;
        }

        // 设置精灵
        spriteRenderer.sprite = currentSprites[currentFrame];

        // 处理左右镜像
        if (lastDirection == Vector2.left)
        {
            spriteRenderer.flipX = true;
        }
        else if (lastDirection == Vector2.right)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            // 上下方向不翻转
            spriteRenderer.flipX = false;
        }
    }

    Sprite[] GetCurrentSprites()
    {
        if (lastDirection == Vector2.up)
        {
            return upSprites;
        }
        else if (lastDirection == Vector2.down)
        {
            return downSprites;
        }
        else // 左或右
        {
            return sideSprites;
        }
    }
}
