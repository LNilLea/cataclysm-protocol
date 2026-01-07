using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyGame;

/// <summary>
/// 玩家状态UI - 显示在屏幕上的玩家血条和状态
/// </summary>
public class PlayerStatusUI : MonoBehaviour
{
    [Header("引用")]
    public Player player;

    [Header("血条")]
    public Image healthBarFill;
    public TMP_Text healthText;

    [Header("动作点图标")]
    public Image moveActionIcon;
    public Image mainActionIcon;
    public Image minorAction1Icon;
    public Image minorAction2Icon;

    [Header("弹药")]
    public GameObject ammoPanel;
    public TMP_Text ammoText;

    [Header("当前武器")]
    public TMP_Text currentWeaponText;

    [Header("颜色")]
    public Color healthFullColor = new Color(0.2f, 0.8f, 0.2f);
    public Color healthLowColor = new Color(0.8f, 0.2f, 0.2f);
    public Color actionAvailable = new Color(0.3f, 0.7f, 1f);
    public Color actionUsed = new Color(0.3f, 0.3f, 0.3f);

    private ActionPointSystem actionPointSystem;

    private void Start()
    {
        if (player == null)
            player = FindObjectOfType<Player>();

        actionPointSystem = FindObjectOfType<ActionPointSystem>();
    }

    private void Update()
    {
        UpdateHealthBar();
        UpdateActionPoints();
        UpdateAmmo();
        UpdateWeapon();
    }

    void UpdateHealthBar()
    {
        if (player == null || player.combatData == null) return;

        int current = player.combatData.currentHP;
        int max = player.combatData.maxHP;
        float ratio = max > 0 ? (float)current / max : 0f;

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = ratio;
            healthBarFill.color = Color.Lerp(healthLowColor, healthFullColor, ratio);
        }

        if (healthText != null)
        {
            healthText.text = $"{current}/{max}";
        }
    }

    void UpdateActionPoints()
    {
        if (actionPointSystem == null) return;

        if (moveActionIcon != null)
            moveActionIcon.color = actionPointSystem.CanMove() ? actionAvailable : actionUsed;

        if (mainActionIcon != null)
            mainActionIcon.color = actionPointSystem.CanDoMainAction() ? actionAvailable : actionUsed;

        if (minorAction1Icon != null)
            minorAction1Icon.color = actionPointSystem.currentMinorActions >= 1 ? actionAvailable : actionUsed;

        if (minorAction2Icon != null)
            minorAction2Icon.color = actionPointSystem.currentMinorActions >= 2 ? actionAvailable : actionUsed;
    }

    void UpdateAmmo()
    {
        if (player == null || player.currentWeapon == null) return;

        // 通过武器名称判断是否远程
        bool isRanged = PlayerInventoryData.IsRangedWeapon(player.currentWeapon.Name);

        if (ammoPanel != null)
            ammoPanel.SetActive(isRanged);

        if (isRanged && ammoText != null)
        {
            // 通过武器名称获取弹药
            var ammo = PlayerInventoryData.GetAmmoData(player.currentWeapon.Name);
            if (ammo != null)
            {
                ammoText.text = $"{ammo.currentAmmo}/{ammo.reserveAmmo}";
            }
        }
    }

    void UpdateWeapon()
    {
        if (currentWeaponText == null) return;

        if (player != null && player.currentWeapon != null)
        {
            currentWeaponText.text = player.currentWeapon.Name;
        }
        else
        {
            currentWeaponText.text = "无武器";
        }
    }
}
