using System.Collections;
using UnityEngine;
using MyGame;

/// <summary>
/// 可交互物品 - 支持重复交互
/// </summary>
public class InteractableItem : MonoBehaviour
{
    public enum ItemType
    {
        Weapon,
        StoryText,
        WeaponAndText
    }

    /// <summary>
    /// 重复交互模式
    /// </summary>
    public enum RepeatMode
    {
        NoRepeat,           // 不允许重复交互（交互一次后失效）
        AlwaysRepeat,       // 始终允许重复交互（每次都执行完整逻辑）
        ShowInfoOnly        // 重复时只显示信息（武器只给一次，但可以重复查看）
    }

    [Header("物品设置")]
    public ItemType itemType;
    public string itemName;

    [Header("剧情文本")]
    [TextArea(3, 10)]
    public string storyText;

    [Header("武器")]
    public WeaponChoice weaponChoice;  // 使用下拉菜单选择武器

    [Header("交互设置")]
    public float interactDistance = 3f;
    public RepeatMode repeatMode = RepeatMode.ShowInfoOnly;  // 默认：可重复查看但只给一次武器
    public bool destroyAfterInteract = false;
    public bool isRequired = false;

    [Header("提示文本")]
    public string interactPrompt = "按 E 交互";
    public string repeatInteractPrompt = "按 E 再次查看";  // 重复交互时的提示

    [Header("UI 引用")]
    public GameObject interactPromptUI;
    public StoryTextUI storyTextUI;

    // 状态追踪
    [HideInInspector] public bool hasBeenInteracted = false;     // 是否交互过
    [HideInInspector] public bool hasGivenWeapon = false;        // 是否已给过武器

    private bool isPlayerNearby = false;
    private Transform playerTransform;

    private void Start()
    {
        // 方法1：用 Tag 查找
        GameObject playerByTag = GameObject.FindWithTag("Player");
        
        // 方法2：用 FindObjectOfType 查找 MyGame.Player
        Player playerComponent = FindObjectOfType<Player>();

        if (playerByTag != null)
        {
            playerTransform = playerByTag.transform;
        }
        else if (playerComponent != null)
        {
            playerTransform = playerComponent.transform;
        }
        else
        {
            Debug.LogError("InteractableItem [" + itemName + "]: 找不到玩家！");
        }

        // 查找 UI
        if (storyTextUI == null)
            storyTextUI = FindObjectOfType<StoryTextUI>();
    }

    private void Update()
    {
        if (playerTransform == null) return;
        
        // 检查是否可以交互
        if (!CanInteract()) return;

        // 计算距离
        float distance = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(playerTransform.position.x, playerTransform.position.y)
        );

        // 距离检测
        if (distance <= interactDistance)
        {
            if (!isPlayerNearby)
            {
                isPlayerNearby = true;
                ShowInteractPrompt();
            }

            // 按 E 交互
            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }
        else
        {
            if (isPlayerNearby)
            {
                isPlayerNearby = false;
                HideInteractPrompt();
            }
        }
    }

    /// <summary>
    /// 检查当前是否可以交互
    /// </summary>
    private bool CanInteract()
    {
        switch (repeatMode)
        {
            case RepeatMode.NoRepeat:
                return !hasBeenInteracted;  // 只能交互一次
            
            case RepeatMode.AlwaysRepeat:
            case RepeatMode.ShowInfoOnly:
                return true;  // 始终可以交互
            
            default:
                return true;
        }
    }

    /// <summary>
    /// 执行交互
    /// </summary>
    private void Interact()
    {
        Debug.Log("=== 交互: " + itemName + " ===");

        bool isFirstInteract = !hasBeenInteracted;
        hasBeenInteracted = true;

        switch (itemType)
        {
            case ItemType.Weapon:
                HandleWeaponInteract(isFirstInteract);
                break;

            case ItemType.StoryText:
                ShowStoryText();
                break;

            case ItemType.WeaponAndText:
                HandleWeaponInteract(isFirstInteract);
                ShowStoryText();
                break;
        }

        HideInteractPrompt();

        if (destroyAfterInteract)
        {
            Destroy(gameObject, 0.5f);
        }
    }

    /// <summary>
    /// 处理武器交互
    /// </summary>
    private void HandleWeaponInteract(bool isFirstInteract)
    {
        if (weaponChoice == WeaponChoice.None)
        {
            Debug.LogWarning(itemName + ": 没有设置武器！");
            return;
        }

        Weapon weapon = WeaponFactory.GetWeapon(weaponChoice);
        if (weapon == null)
        {
            Debug.LogWarning(itemName + ": 无法创建武器 - " + weaponChoice);
            return;
        }

        // 根据模式决定是否给武器
        bool shouldGiveWeapon = false;

        switch (repeatMode)
        {
            case RepeatMode.NoRepeat:
                // 只交互一次，肯定给武器
                shouldGiveWeapon = true;
                break;

            case RepeatMode.AlwaysRepeat:
                // 每次都给武器（慎用，可能导致重复获得）
                shouldGiveWeapon = true;
                break;

            case RepeatMode.ShowInfoOnly:
                // 只有第一次给武器
                shouldGiveWeapon = !hasGivenWeapon;
                break;
        }

        if (shouldGiveWeapon)
        {
            GiveWeaponToPlayer(weapon);
            hasGivenWeapon = true;
        }
        else
        {
            // 已经给过了，只显示武器信息
            ShowWeaponInfo(weapon);
        }
    }

    /// <summary>
    /// 给玩家武器
    /// </summary>
    private void GiveWeaponToPlayer(Weapon weapon)
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.AddWeaponToInventory(weapon);
            Debug.Log("玩家获得武器: " + weapon.Name);

            // 【新增】同步到静态数据
            PlayerInventoryData.AddWeapon(weaponChoice);

            // 显示武器获取UI提示
            ShowWeaponPickupNotification(weapon);
        }
        else
        {
            Debug.LogError("找不到 Player！");
        }
    }

    /// <summary>
    /// 只显示武器信息（不给武器）
    /// </summary>
    private void ShowWeaponInfo(Weapon weapon)
    {
        Debug.Log("查看武器信息: " + weapon.Name);

        // 同样显示UI，但可以修改提示文字
        if (WeaponPickupUI.Instance != null)
        {
            WeaponPickupUI.Instance.ShowWeaponPickup(weapon);
        }
    }

    /// <summary>
    /// 显示武器获取UI提示
    /// </summary>
    private void ShowWeaponPickupNotification(Weapon weapon)
    {
        if (WeaponPickupUI.Instance != null)
        {
            WeaponPickupUI.Instance.ShowWeaponPickup(weapon);
        }
        else
        {
            WeaponPickupUI pickupUI = FindObjectOfType<WeaponPickupUI>();
            if (pickupUI != null)
            {
                pickupUI.ShowWeaponPickup(weapon);
            }
            else
            {
                Debug.LogWarning("[InteractableItem] 找不到 WeaponPickupUI！");
            }
        }
    }

    private void ShowStoryText()
    {
        if (string.IsNullOrEmpty(storyText))
        {
            Debug.LogWarning(itemName + ": 没有设置剧情文本！");
            return;
        }

        if (storyTextUI != null)
        {
            storyTextUI.ShowText(storyText, itemName);
        }
        else
        {
            StoryTextUI ui = FindObjectOfType<StoryTextUI>();
            if (ui != null)
            {
                ui.ShowText(storyText, itemName);
            }
            else
            {
                Debug.LogError("找不到 StoryTextUI！");
            }
        }
    }

    private void ShowInteractPrompt()
    {
        // 根据是否交互过显示不同提示
        string prompt = hasBeenInteracted ? repeatInteractPrompt : interactPrompt;

        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(true);
        }
        else if (InteractPromptUI.Instance != null)
        {
            InteractPromptUI.Instance.Show(prompt);
        }
    }

    private void HideInteractPrompt()
    {
        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(false);
        }
        else if (InteractPromptUI.Instance != null)
        {
            InteractPromptUI.Instance.Hide();
        }
    }

    /// <summary>
    /// 重置交互状态（可用于存档读取等）
    /// </summary>
    public void ResetInteraction()
    {
        hasBeenInteracted = false;
        hasGivenWeapon = false;
    }

    // Scene 视图显示交互范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}
