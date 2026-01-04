using UnityEngine;
using MyGame;
/// <summary>
/// 战斗触发器 - 触碰后进入战斗场景
/// 放在探索场景中，玩家碰到后进入战斗
/// </summary>
public class BattleTrigger : MonoBehaviour
{
    [Header("战斗设置")]
    public string battleSceneName = "BattleScene";  // 战斗场景名
    public bool destroyAfterTrigger = true;          // 触发后销毁（一次性战斗）

    [Header("触发方式")]
    public TriggerType triggerType = TriggerType.OnEnter;
    public KeyCode interactKey = KeyCode.E;
    public string interactPrompt = "按 E 战斗";

    [Header("视觉效果")]
    public bool showWarningIcon = true;              // 显示警告图标
    public GameObject warningIconPrefab;             // 警告图标预制体

    [Header("过渡效果")]
    public bool useFadeTransition = true;
    public float fadeDuration = 0.5f;

    private bool playerInRange = false;
    private bool hasTriggered = false;
    private GameObject warningIcon;

    public enum TriggerType
    {
        OnEnter,        // 进入触发器立即战斗
        OnInteract      // 需要按键交互
    }

    private void Start()
    {
        // 创建警告图标
        if (showWarningIcon && warningIconPrefab != null)
        {
            warningIcon = Instantiate(warningIconPrefab, transform.position + Vector3.up, Quaternion.identity, transform);
        }
    }

    private void Update()
    {
        if (hasTriggered) return;

        if (triggerType == TriggerType.OnInteract && playerInRange)
        {
            if (Input.GetKeyDown(interactKey))
            {
                TriggerBattle();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (triggerType == TriggerType.OnEnter)
            {
                TriggerBattle();
            }
            else
            {
                ShowPrompt();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HidePrompt();
        }
    }

    /// <summary>
    /// 触发战斗
    /// </summary>
    private void TriggerBattle()
    {
        hasTriggered = true;
        HidePrompt();

        // 获取当前场景名和玩家位置
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Player player = FindObjectOfType<Player>();
        Vector2 playerPos = player != null ? (Vector2)player.transform.position : Vector2.zero;

        Debug.Log($"[BattleTrigger] 触发战斗！进入: {battleSceneName}");

        // 使用过渡效果
        if (useFadeTransition && SceneTransitionEffect.Instance != null)
        {
            // 先保存数据
            BattleSceneEntry.EnterBattleScene(battleSceneName, currentScene, playerPos);
        }
        else
        {
            BattleSceneEntry.EnterBattleScene(battleSceneName, currentScene, playerPos);
        }

        // 销毁触发器（如果设置）
        if (destroyAfterTrigger)
        {
            // 标记为已触发（可以保存到存档）
            // SaveManager.MarkBattleCompleted(this.gameObject.name);
        }
    }

    private void ShowPrompt()
    {
        if (InteractPromptUI.Instance != null)
        {
            InteractPromptUI.Instance.Show(interactPrompt);
        }
    }

    private void HidePrompt()
    {
        if (InteractPromptUI.Instance != null)
        {
            InteractPromptUI.Instance.Hide();
        }
    }

    private void OnDrawGizmos()
    {
        // 战斗触发器用红色显示
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f);
        
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
        }
        else
        {
            CircleCollider2D circle = GetComponent<CircleCollider2D>();
            if (circle != null)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
            else
            {
                Gizmos.DrawSphere(transform.position, 1f);
            }
        }
    }
}
