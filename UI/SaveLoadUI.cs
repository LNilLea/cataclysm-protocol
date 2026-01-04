using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 存档 UI - 存档/读档界面
/// </summary>
public class SaveLoadUI : MonoBehaviour
{
    [Header("面板")]
    public GameObject saveLoadPanel;
    public bool isSaveMode = true;          // true=存档模式, false=读档模式

    [Header("标题")]
    public TMP_Text titleText;

    [Header("存档槽位列表")]
    public Transform slotContainer;
    public GameObject slotPrefab;

    [Header("确认对话框")]
    public GameObject confirmDialog;
    public TMP_Text confirmText;
    public Button confirmYesButton;
    public Button confirmNoButton;

    [Header("按钮")]
    public Button closeButton;

    // 当前选中的槽位
    private int selectedSlot = -1;
    private List<GameObject> slotObjects = new List<GameObject>();

    private void Start()
    {
        // 绑定按钮事件
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        if (confirmYesButton != null)
        {
            confirmYesButton.onClick.AddListener(OnConfirmYes);
        }

        if (confirmNoButton != null)
        {
            confirmNoButton.onClick.AddListener(OnConfirmNo);
        }

        // 初始隐藏
        if (saveLoadPanel != null)
            saveLoadPanel.SetActive(false);

        if (confirmDialog != null)
            confirmDialog.SetActive(false);

        // 监听快捷键
        // F5 = 快速存档, F9 = 快速读档
    }

    private void Update()
    {
        // 快捷键
        if (Input.GetKeyDown(KeyCode.F5))
        {
            QuickSave();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            QuickLoad();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && saveLoadPanel != null && saveLoadPanel.activeSelf)
        {
            Close();
        }
    }

    /// <summary>
    /// 打开存档界面
    /// </summary>
    public void OpenSavePanel()
    {
        isSaveMode = true;
        Open();
    }

    /// <summary>
    /// 打开读档界面
    /// </summary>
    public void OpenLoadPanel()
    {
        isSaveMode = false;
        Open();
    }

    /// <summary>
    /// 打开界面
    /// </summary>
    private void Open()
    {
        if (saveLoadPanel != null)
        {
            saveLoadPanel.SetActive(true);
        }

        // 更新标题
        if (titleText != null)
        {
            titleText.text = isSaveMode ? "保存游戏" : "加载游戏";
        }

        // 刷新槽位列表
        RefreshSlotList();

        // 暂停游戏
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 关闭界面
    /// </summary>
    public void Close()
    {
        if (saveLoadPanel != null)
        {
            saveLoadPanel.SetActive(false);
        }

        if (confirmDialog != null)
        {
            confirmDialog.SetActive(false);
        }

        // 恢复游戏
        Time.timeScale = 1f;
    }

    /// <summary>
    /// 刷新存档槽位列表
    /// </summary>
    private void RefreshSlotList()
    {
        // 清除旧槽位
        foreach (var slot in slotObjects)
        {
            Destroy(slot);
        }
        slotObjects.Clear();

        if (SaveManager.Instance == null || slotContainer == null || slotPrefab == null)
            return;

        // 快速存档槽位
        CreateSlotUI(-1, "快速存档");

        // 自动存档槽位
        CreateSlotUI(0, "自动存档");

        // 普通存档槽位
        for (int i = 1; i <= SaveManager.Instance.maxSaveSlots; i++)
        {
            CreateSlotUI(i, $"存档 {i}");
        }
    }

    /// <summary>
    /// 创建存档槽位 UI
    /// </summary>
    private void CreateSlotUI(int slot, string defaultName)
    {
        GameObject slotObj = Instantiate(slotPrefab, slotContainer);
        slotObjects.Add(slotObj);

        // 获取存档信息
        SaveData saveInfo = SaveManager.Instance.GetSaveInfo(slot);

        // 设置槽位内容
        TMP_Text nameText = slotObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
        TMP_Text infoText = slotObj.transform.Find("InfoText")?.GetComponent<TMP_Text>();
        Button slotButton = slotObj.GetComponent<Button>();
        Button deleteButton = slotObj.transform.Find("DeleteButton")?.GetComponent<Button>();

        if (saveInfo != null)
        {
            // 有存档
            if (nameText != null)
            {
                nameText.text = saveInfo.saveName;
            }

            if (infoText != null)
            {
                string playTime = SaveManager.FormatPlayTime(saveInfo.playTimeSeconds);
                infoText.text = $"{saveInfo.saveTime}\n游戏时长: {playTime}";
            }

            if (deleteButton != null)
            {
                deleteButton.gameObject.SetActive(true);
                int capturedSlot = slot;
                deleteButton.onClick.AddListener(() => OnDeleteClicked(capturedSlot));
            }
        }
        else
        {
            // 空槽位
            if (nameText != null)
            {
                nameText.text = isSaveMode ? defaultName : "空";
            }

            if (infoText != null)
            {
                infoText.text = isSaveMode ? "点击保存" : "无存档";
            }

            if (deleteButton != null)
            {
                deleteButton.gameObject.SetActive(false);
            }
        }

        // 绑定点击事件
        if (slotButton != null)
        {
            int capturedSlot = slot;
            slotButton.onClick.AddListener(() => OnSlotClicked(capturedSlot));

            // 读档模式下，空槽位不可点击
            if (!isSaveMode && saveInfo == null)
            {
                slotButton.interactable = false;
            }
        }
    }

    /// <summary>
    /// 槽位点击
    /// </summary>
    private void OnSlotClicked(int slot)
    {
        selectedSlot = slot;

        if (isSaveMode)
        {
            // 存档模式
            if (SaveManager.Instance.SaveExists(slot))
            {
                // 已有存档，确认覆盖
                ShowConfirmDialog($"确定要覆盖存档 {slot} 吗？");
            }
            else
            {
                // 直接存档
                PerformSave(slot);
            }
        }
        else
        {
            // 读档模式
            ShowConfirmDialog($"确定要加载存档吗？\n当前未保存的进度将丢失。");
        }
    }

    /// <summary>
    /// 删除按钮点击
    /// </summary>
    private void OnDeleteClicked(int slot)
    {
        selectedSlot = slot;
        ShowConfirmDialog($"确定要删除存档 {slot} 吗？\n此操作无法撤销。");
    }

    /// <summary>
    /// 显示确认对话框
    /// </summary>
    private void ShowConfirmDialog(string message)
    {
        if (confirmDialog != null)
        {
            confirmDialog.SetActive(true);
        }

        if (confirmText != null)
        {
            confirmText.text = message;
        }
    }

    /// <summary>
    /// 确认对话框 - 是
    /// </summary>
    private void OnConfirmYes()
    {
        if (confirmDialog != null)
        {
            confirmDialog.SetActive(false);
        }

        if (isSaveMode)
        {
            PerformSave(selectedSlot);
        }
        else
        {
            PerformLoad(selectedSlot);
        }
    }

    /// <summary>
    /// 确认对话框 - 否
    /// </summary>
    private void OnConfirmNo()
    {
        if (confirmDialog != null)
        {
            confirmDialog.SetActive(false);
        }
    }

    /// <summary>
    /// 执行存档
    /// </summary>
    private void PerformSave(int slot)
    {
        if (SaveManager.Instance == null) return;

        bool success = SaveManager.Instance.SaveGame(slot);

        if (success)
        {
            Debug.Log($"存档成功: 槽位 {slot}");
            RefreshSlotList();
        }
        else
        {
            Debug.LogError("存档失败");
        }
    }

    /// <summary>
    /// 执行读档
    /// </summary>
    private void PerformLoad(int slot)
    {
        if (SaveManager.Instance == null) return;

        Close();

        bool success = SaveManager.Instance.LoadGame(slot);

        if (!success)
        {
            Debug.LogError("读档失败");
        }
    }

    /// <summary>
    /// 快速存档
    /// </summary>
    public void QuickSave()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.QuickSave();
            Debug.Log("快速存档完成");
        }
    }

    /// <summary>
    /// 快速读档
    /// </summary>
    public void QuickLoad()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.QuickLoad();
        }
    }
}
