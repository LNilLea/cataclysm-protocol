using UnityEngine;

/// <summary>
/// 玩家边界限制 - 挂在玩家身上，限制移动范围
/// </summary>
public class PlayerBounds : MonoBehaviour
{
    [Header("启用边界")]
    public bool enableBounds = true;

    private void LateUpdate()
    {
        if (!enableBounds) return;
        if (SceneBounds.Instance == null) return;

        // 限制玩家位置在边界内
        transform.position = SceneBounds.Instance.ClampPosition(transform.position);
    }
}
