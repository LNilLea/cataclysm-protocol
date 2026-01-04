using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MyGame;

/// <summary>
/// 武器选择UI - 上拉式武器选择栏
/// </summary>
public class WeaponSelectorUI : MonoBehaviour
{
    [Header("引用")]
    public Player player;
    public WeaponManager weaponManager;

    [Header("武器栏面板")]
    public GameObject weaponPanel;
    public Transform weaponListContainer;
    public GameObject weaponSlotPrefab;

    [Header("当前武器显示")]
    public TMP_Text currentWeaponName;
    public TMP_Text attackRangeText;
    public TMP_Text damageText;

    [Header("弹药显示")]
    public GameObject ammoDisplay;
    public TMP_Text currentAmmoText;
    public TMP_Text reserveAmmoText;
    public Image ammoBarFill;

    [Header("攻击范围预览")]
    public RangeVisualizer2D rangeVisualizer;
    public Color meleeRangeColor = new Color(1f, 0.5f, 0.5f, 0.3f);
    public Color rangedRangeColor = new Color(0.5f, 0.5f, 1f, 0.3f);

    [Header("交互设置")]
    public KeyCode toggleKey = KeyCode.Tab;
    public KeyCode[] quickSelectKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5 };

    [Header("动画")]
    public float slideSpeed = 10f;
    public float panelHiddenY = -200f;
    public float panelShownY = 0f;

    private bool isPanelOpen = false;
    private RectTransform panelRect;
    private List<GameObject> weaponSlots = new List<GameObject>();
    private int selectedIndex = 0;

    private void Start()
    {
        if (player == null)
            player = FindObjectOfType<Player>();

        if (weaponManager == null && player != null)
            weaponManager = player.GetComponent<WeaponManager>();

        if (rangeVisualizer == null)
            rangeVisualizer = FindObjectOfType<RangeVisualizer2D>();

        if (weaponPanel != null)
            panelRect = weaponPanel.GetComponent<RectTransform>();

        RefreshWeaponList();

        if (panelRect != null)
        {
            Vector2 pos = panelRect.anchoredPosition;
            pos.y = panelHiddenY;
            panelRect.anchoredPosition = pos;
        }

        UpdateCurrentWeaponDisplay();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePanel();
        }

        HandleQuickSelect();
        HandleScrollWheel();
        AnimatePanel();
        UpdateAmmoDisplay();
    }

    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        if (isPanelOpen) RefreshWeaponList();
    }

    private void AnimatePanel()
    {
        if (panelRect == null) return;

        float targetY = isPanelOpen ? panelShownY : panelHiddenY;
        Vector2 pos = panelRect.anchoredPosition;
        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * slideSpeed);
        panelRect.anchoredPosition = pos;
    }

    private void HandleQuickSelect()
    {
        for (int i = 0; i < quickSelectKeys.Length; i++)
        {
            if (Input.GetKeyDown(quickSelectKeys[i]))
            {
                SelectWeaponByIndex(i);
                break;
            }
        }
    }

    private void HandleScrollWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            int weaponCount = PlayerInventoryData.OwnedWeapons.Count;
            if (weaponCount == 0) return;

            if (scroll > 0)
                selectedIndex = (selectedIndex - 1 + weaponCount) % weaponCount;
            else
                selectedIndex = (selectedIndex + 1) % weaponCount;

            SelectWeaponByIndex(selectedIndex);
        }
    }

    public void RefreshWeaponList()
    {
        foreach (var slot in weaponSlots)
            Destroy(slot);
        weaponSlots.Clear();

        if (weaponListContainer == null || weaponSlotPrefab == null) return;

        var weapons = PlayerInventoryData.OwnedWeapons;

        for (int i = 0; i < weapons.Count; i++)
        {
            CreateWeaponSlot(weapons[i], i);
        }

        UpdateSelectionHighlight();
    }

    private void CreateWeaponSlot(WeaponChoice weaponChoice, int index)
    {
        GameObject slot = Instantiate(weaponSlotPrefab, weaponListContainer);
        weaponSlots.Add(slot);

        Weapon weapon = WeaponFactory.GetWeapon(weaponChoice);

        TMP_Text nameText = slot.transform.Find("WeaponName")?.GetComponent<TMP_Text>();
        if (nameText != null)
            nameText.text = weapon.Name;

        TMP_Text hotkeyText = slot.transform.Find("HotkeyText")?.GetComponent<TMP_Text>();
        if (hotkeyText != null && index < quickSelectKeys.Length)
            hotkeyText.text = $"[{index + 1}]";

        TMP_Text rangeText = slot.transform.Find("RangeText")?.GetComponent<TMP_Text>();
        if (rangeText != null)
            rangeText.text = $"范围: {weapon.AttackRangeMin}-{weapon.AttackRangeMax}";

        TMP_Text dmgText = slot.transform.Find("DamageText")?.GetComponent<TMP_Text>();
        if (dmgText != null)
            dmgText.text = $"伤害: {weapon.DamageRange.x}-{weapon.DamageRange.y}";

        TMP_Text ammoText = slot.transform.Find("AmmoText")?.GetComponent<TMP_Text>();
        if (ammoText != null)
        {
            if (PlayerInventoryData.IsRangedWeapon(weaponChoice))
            {
                var ammo = PlayerInventoryData.GetAmmoData(weaponChoice);
                if (ammo != null)
                    ammoText.text = $"弹药: {ammo.currentAmmo}/{ammo.reserveAmmo}";
                ammoText.gameObject.SetActive(true);
            }
            else
            {
                ammoText.gameObject.SetActive(false);
            }
        }

        Button btn = slot.GetComponent<Button>();
        if (btn != null)
        {
            int idx = index;
            btn.onClick.AddListener(() => SelectWeaponByIndex(idx));
        }
    }

    public void SelectWeaponByIndex(int index)
    {
        var weapons = PlayerInventoryData.OwnedWeapons;
        if (index < 0 || index >= weapons.Count) return;

        selectedIndex = index;
        WeaponChoice choice = weapons[index];

        // 切换武器（传递索引）
        if (weaponManager != null)
            weaponManager.SwitchToWeapon(index);

        UpdateCurrentWeaponDisplay();
        UpdateSelectionHighlight();
        UpdateRangePreview();

        Debug.Log($"[WeaponSelectorUI] 选择武器: {choice}");
    }

    private void UpdateSelectionHighlight()
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            Image bg = weaponSlots[i].GetComponent<Image>();
            if (bg != null)
            {
                bg.color = (i == selectedIndex) 
                    ? new Color(0.3f, 0.6f, 1f, 0.8f)
                    : new Color(0.2f, 0.2f, 0.2f, 0.8f);
            }
        }
    }

    private void UpdateCurrentWeaponDisplay()
    {
        if (player == null || player.currentWeapon == null) return;

        Weapon weapon = player.currentWeapon;

        if (currentWeaponName != null)
            currentWeaponName.text = weapon.Name;

        if (attackRangeText != null)
            attackRangeText.text = $"范围: {weapon.AttackRangeMin}-{weapon.AttackRangeMax} 格";

        if (damageText != null)
            damageText.text = $"伤害: {weapon.DamageRange.x}-{weapon.DamageRange.y}";

        UpdateAmmoDisplay();
    }

    private void UpdateAmmoDisplay()
    {
        if (player == null || player.currentWeapon == null) return;

        bool isRanged = PlayerInventoryData.IsRangedWeapon(player.currentWeapon.Name);

        if (ammoDisplay != null)
            ammoDisplay.SetActive(isRanged);

        if (!isRanged) return;

        var ammo = PlayerInventoryData.GetAmmoData(player.currentWeapon.Name);
        if (ammo == null) return;

        if (currentAmmoText != null)
            currentAmmoText.text = ammo.currentAmmo.ToString();

        if (reserveAmmoText != null)
            reserveAmmoText.text = ammo.reserveAmmo.ToString();

        if (ammoBarFill != null)
        {
            int maxMag = PlayerInventoryData.GetMaxMagazine(player.currentWeapon.Name);
            ammoBarFill.fillAmount = maxMag > 0 ? (float)ammo.currentAmmo / maxMag : 0f;
        }
    }

    private void UpdateRangePreview()
    {
        if (rangeVisualizer == null || player == null || player.currentWeapon == null) return;

        Weapon weapon = player.currentWeapon;
        bool isRanged = PlayerInventoryData.IsRangedWeapon(weapon.Name);

        rangeVisualizer.SetRangeColor(isRanged ? rangedRangeColor : meleeRangeColor);
        rangeVisualizer.ShowRange(player.transform.position, weapon.AttackRangeMin, weapon.AttackRangeMax);
    }

    public void HideRangePreview()
    {
        if (rangeVisualizer != null)
            rangeVisualizer.HideRange();
    }
}
