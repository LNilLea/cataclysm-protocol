using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterCreation : MonoBehaviour
{
    // UI 元素
    public TMP_Text statPointsText;
    public TMP_Text selectedFeatText;
    public TMP_Text featDescriptionText;
    public TMP_Dropdown featDropdown;
    public Slider strengthSlider;
    public Slider agilitySlider;
    public Slider intelligenceSlider;
    public Slider vitalitySlider;
    public Slider willpowerSlider;
    public Slider charismaSlider;
    public Slider mobilitySlider;
    public Button confirmButton;

    // 对应文本
    public TMP_Text strengthText;
    public TMP_Text agilityText;
    public TMP_Text intelligenceText;
    public TMP_Text vitalityText;
    public TMP_Text willpowerText;
    public TMP_Text charismaText;
    public TMP_Text mobilityText;

    // 角色属性和专长
    public int statPoints = 40;
    public int strength = 0;
    public int agility = 0;
    public int intelligence = 0;
    public int vitality = 0;
    public int willpower = 0;
    public int charisma = 0;
    public int mobility = 0;
    public string selectedFeat = "None";

    // 专长描述
    private string[] featDescriptions = new string[] {
        "铁骨如山: 在受到物理攻击时，减少2点伤害。每次受到伤害时，减少2点（此效果不与其他减伤效果叠加）。",
        "重击: 使用重型武器时，攻击造成的伤害增加1d4，并且有10%的几率使敌人受到'击退'状态，推开敌人1格。",
        "不屈意志: 当生命值降至30%以下时，玩家进入'战斗狂怒'状态，所有攻击的伤害增加5%，但防御值降低3。",
        "耐力之墙: 当玩家使用盾牌时，AC增加+1，并且玩家可以在防御时吸收敌人攻击的15%伤害。",
        "快速反应: 在敌人回合开始前，玩家可以消耗1点行动力进行一次'闪避'动作，减少敌人命中率5%。",
    };

    // 可选择的专长
    private string[] availableFeats = new string[] {
        "铁骨如山",
        "重击",
        "不屈意志",
        "耐力之墙",
        "快速反应",
    };

    void Start()
    {
        // 设置UI元素初始值
        statPointsText.text = "剩余属性点: " + statPoints;
        selectedFeatText.text = "选择的专长: " + selectedFeat;
        featDescriptionText.text = "专长描述: 请悬停在专长上查看详细信息";

        // 设置专长下拉菜单
        featDropdown.ClearOptions();
        featDropdown.AddOptions(new System.Collections.Generic.List<string>(availableFeats));
        featDropdown.onValueChanged.AddListener(OnFeatSelected);

        // 为每个滑块添加事件监听
        strengthSlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref strength, "体魄", strengthText, strengthSlider));
        agilitySlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref agility, "反应", agilityText, agilitySlider));
        intelligenceSlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref intelligence, "智力", intelligenceText, intelligenceSlider));
        vitalitySlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref vitality, "体质", vitalityText, vitalitySlider));
        willpowerSlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref willpower, "意志", willpowerText, willpowerSlider));
        charismaSlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref charisma, "魅力", charismaText, charismaSlider));
        mobilitySlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref mobility, "移动力", mobilityText, mobilitySlider));

        // 确认按钮的点击事件
        confirmButton.onClick.AddListener(OnConfirmClicked);

        // 初始时禁用确认按钮
        UpdateConfirmButton();
    }

    // 更新确认按钮的可用状态
    void UpdateConfirmButton()
    {
        confirmButton.interactable = statPoints == 0;
    }

    // 当玩家选择专长时更新显示
    void OnFeatSelected(int index)
    {
        selectedFeat = availableFeats[index];
        selectedFeatText.text = "选择的专长: " + selectedFeat;
        featDescriptionText.text = featDescriptions[index];
    }

    // 更新属性点
    public void UpdateStatPoints()
    {
        statPointsText.text = "剩余属性点: " + statPoints;
        UpdateConfirmButton();
    }

    // 属性滑块的变化逻辑
    public void OnStatSliderChanged(float value, ref int stat, string statName, TMP_Text statText, Slider slider)
    {
        int newValue = Mathf.RoundToInt(value);
        int diff = newValue - stat;

        if (statPoints - diff < 0)
        {
            slider.SetValueWithoutNotify(stat);
            return;
        }

        stat = newValue;
        statPoints -= diff;
        UpdateStatPoints();
        statText.text = statName + ": " + newValue + " (" + GetAttributeDescription(newValue) + ")";
    }

    // 根据属性值返回不同的描述
    string GetAttributeDescription(int value)
    {
        switch (value)
        {
            case 0: return "完全不存在该能力";
            case 1: return "残疾";
            case 2: return "体弱";
            case 3: return "普通";
            case 4: return "稍微优于普通";
            case 5: return "强壮";
            case 6: return "极强";
            case 7: return "超凡";
            case 8: return "无与伦比";
            default: return "";
        }
    }

    // 确认按钮点击事件
    void OnConfirmClicked()
    {
        // ========== 新增：保存数据到静态类 ==========
        CharacterData.SaveFromCreation(this);

        Debug.Log("角色创建完成！");
        Debug.Log("属性：体魄：" + strength + ", 反应：" + agility + ", 智力：" + intelligence + ", 体质：" + vitality + ", 意志：" + willpower + ", 魅力：" + charisma + ", 移动力：" + mobility);
        Debug.Log("选择的专长：" + selectedFeat);

        // 跳转到研究所场景
        UnityEngine.SceneManagement.SceneManager.LoadScene("ResearchInstitute");
    }
}
