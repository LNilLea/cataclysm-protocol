using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Character Creation - English Version
/// 
/// Attribute System (8 attributes):
/// - Intelligence: Check attribute
/// - Strength: Combat attribute, determines HP (MaxHP = Strength × 5)
/// - Agility: Combat attribute, determines AC and Initiative
/// - Technology: Check attribute
/// - Willpower: Resistance attribute
/// - Humanity: Social attribute, key for modifications
/// - Charisma: Social attribute
/// - Mobility: Combat attribute
/// </summary>
public class CharacterCreation : MonoBehaviour
{
    // ===== UI Elements =====
    public TMP_Text statPointsText;
    public TMP_Text selectedFeatText;
    public TMP_Text featDescriptionText;
    public TMP_Dropdown featDropdown;
    public Button confirmButton;

    // ===== Real-time Combat Stats Display =====
    public TMP_Text hpDisplayText;      // Shows calculated HP
    public TMP_Text acDisplayText;      // Shows calculated AC

    // ===== Attribute Sliders (8 total) =====
    public Slider intelligenceSlider;
    public Slider strengthSlider;
    public Slider agilitySlider;
    public Slider technologySlider;
    public Slider willpowerSlider;
    public Slider humanitySlider;
    public Slider charismaSlider;
    public Slider mobilitySlider;

    // ===== Attribute Text Labels =====
    public TMP_Text intelligenceText;
    public TMP_Text strengthText;
    public TMP_Text agilityText;
    public TMP_Text technologyText;
    public TMP_Text willpowerText;
    public TMP_Text humanityText;
    public TMP_Text charismaText;
    public TMP_Text mobilityText;

    // ===== Character Attributes (8 total) =====
    public int statPoints = 40;
    public int intelligence = 0;
    public int strength = 0;      // Determines HP (HP = Strength × 5)
    public int agility = 0;       // Determines AC and Initiative
    public int technology = 0;
    public int willpower = 0;
    public int humanity = 0;
    public int charisma = 0;
    public int mobility = 0;
    public string selectedFeat = "None";

    // ===== Feat Descriptions =====
    private string[] featDescriptions = new string[] {
        "Iron Fortress: Reduce physical damage taken by 2. Each time you take damage, reduce it by 2 (does not stack with other damage reduction effects).",
        "Heavy Strike: When using heavy weapons, increase damage dealt by 1d4, with a 10% chance to inflict 'Knockback' status, pushing the enemy back 1 tile.",
        "Unyielding Will: When HP drops below 30%, enter 'Battle Fury' state. All attack damage increases by 5%, but defense decreases by 3.",
        "Endurance Wall: When using a shield, AC increases by +1, and you can absorb 15% of enemy attack damage while defending.",
        "Quick Reflexes: Before the enemy's turn begins, you can spend 1 action point to perform a 'Dodge' action, reducing enemy hit chance by 5%.",
    };

    // ===== Available Feats =====
    private string[] availableFeats = new string[] {
        "Iron Fortress",
        "Heavy Strike",
        "Unyielding Will",
        "Endurance Wall",
        "Quick Reflexes",
    };

    void Start()
    {
        // Initialize UI text
        statPointsText.text = "Remaining Stat Points: " + statPoints;
        selectedFeatText.text = "Selected Feat: " + selectedFeat;
        featDescriptionText.text = "Feat Description: Select a feat to see details";

        // Initialize HP and AC display
        UpdateCombatStatsDisplay();

        // Setup feat dropdown
        featDropdown.ClearOptions();
        featDropdown.AddOptions(new System.Collections.Generic.List<string>(availableFeats));
        featDropdown.onValueChanged.AddListener(OnFeatSelected);

        // Add slider event listeners for all 8 attributes
        intelligenceSlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref intelligence, "Intelligence", intelligenceText, intelligenceSlider));
        strengthSlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref strength, "Strength", strengthText, strengthSlider));
        agilitySlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref agility, "Agility", agilityText, agilitySlider));
        technologySlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref technology, "Technology", technologyText, technologySlider));
        willpowerSlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref willpower, "Willpower", willpowerText, willpowerSlider));
        humanitySlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref humanity, "Humanity", humanityText, humanitySlider));
        charismaSlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref charisma, "Charisma", charismaText, charismaSlider));
        mobilitySlider.onValueChanged.AddListener((value) => OnStatSliderChanged(value, ref mobility, "Mobility", mobilityText, mobilitySlider));

        // Confirm button
        confirmButton.onClick.AddListener(OnConfirmClicked);

        // Disable confirm button initially
        UpdateConfirmButton();
    }

    // Update confirm button state
    void UpdateConfirmButton()
    {
        confirmButton.interactable = statPoints == 0;
    }

    // Feat selection handler
    void OnFeatSelected(int index)
    {
        selectedFeat = availableFeats[index];
        selectedFeatText.text = "Selected Feat: " + selectedFeat;
        featDescriptionText.text = featDescriptions[index];
    }

    // Update stat points display
    public void UpdateStatPoints()
    {
        statPointsText.text = "Remaining Stat Points: " + statPoints;
        UpdateConfirmButton();
    }

    /// <summary>
    /// Update HP and AC display in real-time
    /// HP = Strength × 5
    /// AC = 10 + (Agility - 3)  [3 is the average human baseline]
    /// </summary>
    void UpdateCombatStatsDisplay()
    {
        int calculatedHP = strength * 5;
        int agilityModifier = agility - 3;  // 3 is average baseline
        int calculatedAC = 10 + agilityModifier;

        if (hpDisplayText != null)
        {
            hpDisplayText.text = "HP: " + calculatedHP + " (STR × 5)";
        }

        if (acDisplayText != null)
        {
            // Show modifier with +/- sign
            string modifierStr = agilityModifier >= 0 ? "+" + agilityModifier : agilityModifier.ToString();
            acDisplayText.text = "AC: " + calculatedAC + " (10 " + modifierStr + ")";
        }
    }

    // Slider change handler
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

        // Update HP/AC display when any stat changes
        UpdateCombatStatsDisplay();
    }

    // Attribute level descriptions
    string GetAttributeDescription(int value)
    {
        switch (value)
        {
            case 0: return "Non-existent";
            case 1: return "Crippled";
            case 2: return "Feeble";
            case 3: return "Average";
            case 4: return "Above Average";
            case 5: return "Strong";
            case 6: return "Exceptional";
            case 7: return "Extraordinary";
            case 8: return "Unparalleled";
            default: return "";
        }
    }

    // Confirm button click handler
    void OnConfirmClicked()
    {
        // Save data to static CharacterData class
        CharacterData.SaveFromCreation(this);

        Debug.Log("Character creation complete!");
        Debug.Log($"Attributes: INT:{intelligence} STR:{strength} AGI:{agility} TECH:{technology} WILL:{willpower} HUM:{humanity} CHA:{charisma} MOB:{mobility}");
        Debug.Log($"Calculated HP: {strength * 5}, AC: {10 + agility}");
        Debug.Log("Selected Feat: " + selectedFeat);

        // Load Research Institute scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("ResearchInstitute");
    }
}
