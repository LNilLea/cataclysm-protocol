using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyGame;

/// <summary>
/// 换弹系统 - 点击按钮或按R键换弹
/// </summary>
public class ReloadSystem : MonoBehaviour
{
    [Header("引用")]
    public Player player;
    public ActionPointSystem actionPointSystem;

    [Header("UI")]
    public Button reloadButton;             // 换弹按钮
    public TMP_Text reloadButtonText;       // 按钮文字
    public GameObject reloadPanel;          // 换弹面板（远程武器才显示）

    [Header("设置")]
    public KeyCode reloadKey = KeyCode.R;   // 换弹快捷键
    public bool requireMinorAction = true;  // 是否消耗次要动作

    [Header("音效（可选）")]
    public AudioClip reloadSound;
    private AudioSource audioSource;

    private void Start()
    {
        if (player == null)
            player = FindObjectOfType<Player>();

        if (actionPointSystem == null)
            actionPointSystem = FindObjectOfType<ActionPointSystem>();

        audioSource = GetComponent<AudioSource>();

        // 绑定按钮
        if (reloadButton != null)
        {
            reloadButton.onClick.AddListener(TryReload);
        }
    }

    private void Update()
    {
        // 快捷键换弹
        if (Input.GetKeyDown(reloadKey))
        {
            TryReload();
        }

        // 更新UI显示
        UpdateUI();
    }

    /// <summary>
    /// 尝试换弹
    /// </summary>
    public void TryReload()
    {
        if (player == null || player.currentWeapon == null) return;

        string weaponName = player.currentWeapon.Name;

        // 检查是否是远程武器
        if (!PlayerInventoryData.IsRangedWeapon(weaponName))
        {
            Debug.Log("[ReloadSystem] 近战武器不需要换弹");
            return;
        }

        // 检查是否需要消耗次要动作
        if (requireMinorAction && actionPointSystem != null)
        {
            if (!actionPointSystem.CanDoMinorAction())
            {
                Debug.Log("[ReloadSystem] 没有次要动作，无法换弹");
                return;
            }
        }

        // 获取弹药数据
        var ammo = PlayerInventoryData.GetAmmoData(weaponName);
        if (ammo == null)
        {
            Debug.Log("[ReloadSystem] 没有弹药数据");
            return;
        }

        // 获取最大弹匣容量
        int maxMag = PlayerInventoryData.GetMaxMagazine(weaponName);

        // 检查是否需要换弹
        if (ammo.currentAmmo >= maxMag)
        {
            Debug.Log("[ReloadSystem] 弹匣已满");
            return;
        }

        // 检查是否有备用弹药
        if (ammo.reserveAmmo <= 0)
        {
            Debug.Log("[ReloadSystem] 没有备用弹药");
            return;
        }

        // 执行换弹
        int needed = maxMag - ammo.currentAmmo;
        int toReload = Mathf.Min(needed, ammo.reserveAmmo);

        ammo.currentAmmo += toReload;
        ammo.reserveAmmo -= toReload;

        // 消耗次要动作
        if (requireMinorAction && actionPointSystem != null)
        {
            actionPointSystem.UseMinorAction();
        }

        // 播放音效
        if (reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        Debug.Log($"[ReloadSystem] 换弹完成: {weaponName} 弹匣:{ammo.currentAmmo}/{maxMag} 备弹:{ammo.reserveAmmo}");
    }

    /// <summary>
    /// 更新UI
    /// </summary>
    private void UpdateUI()
    {
        if (player == null || player.currentWeapon == null)
        {
            if (reloadPanel != null) reloadPanel.SetActive(false);
            return;
        }

        string weaponName = player.currentWeapon.Name;
        bool isRanged = PlayerInventoryData.IsRangedWeapon(weaponName);

        // 显示/隐藏换弹面板
        if (reloadPanel != null)
        {
            reloadPanel.SetActive(isRanged);
        }

        if (!isRanged) return;

        // 更新按钮状态
        if (reloadButton != null)
        {
            bool canReload = CanReload();
            reloadButton.interactable = canReload;
        }

        // 更新按钮文字
        if (reloadButtonText != null)
        {
            var ammo = PlayerInventoryData.GetAmmoData(weaponName);
            if (ammo != null)
            {
                int maxMag = PlayerInventoryData.GetMaxMagazine(weaponName);
                if (ammo.currentAmmo >= maxMag)
                {
                    reloadButtonText.text = "弹匣已满";
                }
                else if (ammo.reserveAmmo <= 0)
                {
                    reloadButtonText.text = "无备弹";
                }
                else
                {
                    reloadButtonText.text = $"换弹 [R]";
                }
            }
        }
    }

    /// <summary>
    /// 是否可以换弹
    /// </summary>
    private bool CanReload()
    {
        if (player == null || player.currentWeapon == null) return false;

        string weaponName = player.currentWeapon.Name;

        if (!PlayerInventoryData.IsRangedWeapon(weaponName)) return false;

        var ammo = PlayerInventoryData.GetAmmoData(weaponName);
        if (ammo == null) return false;

        int maxMag = PlayerInventoryData.GetMaxMagazine(weaponName);

        // 弹匣已满
        if (ammo.currentAmmo >= maxMag) return false;

        // 没有备弹
        if (ammo.reserveAmmo <= 0) return false;

        // 没有次要动作
        if (requireMinorAction && actionPointSystem != null)
        {
            if (!actionPointSystem.CanDoMinorAction()) return false;
        }

        return true;
    }
}
