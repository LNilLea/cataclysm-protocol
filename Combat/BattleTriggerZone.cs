using UnityEngine;
using MyGame;
/// <summary>
/// 战斗触发器 - 用于从叙事场景进入战斗场景
/// 可以通过碰撞、交互或事件触发
/// </summary>
public class BattleTriggerZone : MonoBehaviour
{
    [Header("战斗场景设置")]
    public string battleSceneName;            // 战斗场景名称
    public string returnSceneName;            // 战斗后返回的场景（留空则继续下一个场景）
    public string returnSpawnPointID = "";    // 返回时的出生点ID

    [Header("触发方式")]
    public TriggerType triggerType = TriggerType.OnEnter;
    public KeyCode interactKey = KeyCode.E;   // 交互按键

    public enum TriggerType
    {
        OnEnter,        // 进入触发器自动进入战斗
        OnInteract,     // 按键交互进入战斗
        OnEvent         // 通过代码触发
    }

    [Header("提示设置")]
    public bool showPrompt = true;            // 显示交互提示
    public string promptText = "按 E 进入战斗";

    [Header("过渡效果")]
    public bool useFade = true;
    public float fadeDuration = 0.5f;

    [Header("战斗前保存")]
    public bool savePlayerDataBeforeBattle = true;  // 战斗前保存玩家数据

    [Header("调试")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.red;

    private bool isPlayerInZone = false;
    private bool isTriggered = false;

    private void Update()
    {
        // 交互触发
        if (triggerType == TriggerType.OnInteract && isPlayerInZone && !isTriggered)
        {
            if (Input.GetKeyDown(interactKey))
            {
                TriggerBattle();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInZone = true;

        if (triggerType == TriggerType.OnEnter && !isTriggered)
        {
            TriggerBattle();
        }
        else if (triggerType == TriggerType.OnInteract && showPrompt)
        {
            ShowPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInZone = false;

        if (showPrompt)
        {
            HidePrompt();
        }
    }

    // 3D碰撞支持
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInZone = true;

        if (triggerType == TriggerType.OnEnter && !isTriggered)
        {
            TriggerBattle();
        }
        else if (triggerType == TriggerType.OnInteract && showPrompt)
        {
            ShowPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInZone = false;

        if (showPrompt)
        {
            HidePrompt();
        }
    }

    /// <summary>
    /// 触发战斗
    /// </summary>
    public void TriggerBattle()
    {
        if (isTriggered) return;
        if (string.IsNullOrEmpty(battleSceneName))
        {
            Debug.LogError("[BattleTriggerZone] 战斗场景名称未设置！");
            return;
        }

        isTriggered = true;
        HidePrompt();

        Debug.Log($"[BattleTriggerZone] 触发战斗，进入场景: {battleSceneName}");

        // 保存玩家数据
        if (savePlayerDataBeforeBattle)
        {
            SavePlayerData();
        }

        // 保存返回信息
        if (SceneFlowManager.Instance != null)
        {
            // 可以在这里保存返回场景信息（如果需要）
        }

        // 加载战斗场景
        if (useFade && SceneTransitionEffect.Instance != null)
        {
            SceneTransitionEffect.Instance.FadeOutAndLoadScene(battleSceneName, fadeDuration);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(battleSceneName);
        }
    }

    /// <summary>
    /// 保存玩家数据
    /// </summary>
    private void SavePlayerData()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Player playerComponent = player.GetComponent<Player>();
            if (playerComponent != null)
            {
                CharacterData.SaveFromPlayer(playerComponent);
            }
        }

        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.SavePlayerData();
        }
    }

    /// <summary>
    /// 显示提示
    /// </summary>
    private void ShowPrompt()
    {
        if (InteractPromptUI.Instance != null)
        {
            InteractPromptUI.Instance.Show(promptText);
        }
    }

    /// <summary>
    /// 隐藏提示
    /// </summary>
    private void HidePrompt()
    {
        if (InteractPromptUI.Instance != null)
        {
            InteractPromptUI.Instance.Hide();
        }
    }

    /// <summary>
    /// 重置触发器（允许重新触发）
    /// </summary>
    public void ResetTrigger()
    {
        isTriggered = false;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;

        // 绘制触发区域
        Collider2D col2D = GetComponent<Collider2D>();
        if (col2D != null)
        {
            if (col2D is BoxCollider2D box)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
            }
            else if (col2D is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
        }

        Collider col3D = GetComponent<Collider>();
        if (col3D != null)
        {
            Gizmos.DrawWireCube(col3D.bounds.center, col3D.bounds.size);
        }

        // 显示战斗场景名称
        #if UNITY_EDITOR
        if (!string.IsNullOrEmpty(battleSceneName))
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up, $"Battle: {battleSceneName}");
        }
        #endif
    }
}
