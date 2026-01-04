using UnityEngine;
using MyGame;

/// <summary>
/// 伤害数字触发器 - 挂在角色上，监听血量变化并显示伤害数字
/// 适用于 Player、Beaver、Mantis 等有 HP 的角色
/// </summary>
public class DamagePopupTrigger : MonoBehaviour
{
    [Header("显示设置")]
    public Vector3 popupOffset = new Vector3(0, 0.5f, 0);  // 伤害数字偏移
    public bool showHeal = true;                            // 是否显示治疗
    public bool showMiss = true;                            // 是否显示未命中

    // 上一帧的HP值（用于检测变化）
    private int lastHP = -1;
    private int maxHP = 100;

    // 组件引用
    private Player player;
    private Beaver beaver;
    private Mantis mantis;
    private MonsterAI monsterAI;

    private void Start()
    {
        // 尝试获取各种角色组件
        player = GetComponent<Player>();
        beaver = GetComponent<Beaver>();
        mantis = GetComponent<Mantis>();
        monsterAI = GetComponent<MonsterAI>();

        // 初始化HP
        lastHP = GetCurrentHP();
        maxHP = GetMaxHP();
    }

    private void Update()
    {
        int currentHP = GetCurrentHP();

        // HP发生变化
        if (lastHP >= 0 && currentHP != lastHP)
        {
            int diff = currentHP - lastHP;

            if (diff < 0)
            {
                // 受到伤害
                int damage = -diff;
                DamagePopupManager.Damage(transform.position + popupOffset, damage);
            }
            else if (diff > 0 && showHeal)
            {
                // 被治疗
                DamagePopupManager.Heal(transform.position + popupOffset, diff);
            }

            lastHP = currentHP;
        }
    }

    /// <summary>
    /// 获取当前HP
    /// </summary>
    private int GetCurrentHP()
    {
        if (player != null && player.combatData != null)
            return player.combatData.currentHP;

        if (beaver != null)
            return beaver.currentHP;

        if (mantis != null)
            return mantis.currentHP;

        if (monsterAI != null)
            return monsterAI.combatData.currentHP;

        return 0;
    }

    /// <summary>
    /// 获取最大HP
    /// </summary>
    private int GetMaxHP()
    {
        if (player != null && player.combatData != null)
            return player.combatData.maxHP;

        if (beaver != null)
            return beaver.maxHP;

        if (mantis != null)
            return mantis.maxHP;

        if (monsterAI != null)
            return monsterAI.combatData.maxHP;

        return 100;
    }

    /// <summary>
    /// 手动显示伤害（可从外部调用）
    /// </summary>
    public void ShowDamage(int damage, bool critical = false)
    {
        DamagePopupManager.Damage(transform.position + popupOffset, damage, critical);
    }

    /// <summary>
    /// 手动显示治疗
    /// </summary>
    public void ShowHeal(int amount)
    {
        DamagePopupManager.Heal(transform.position + popupOffset, amount);
    }

    /// <summary>
    /// 手动显示未命中
    /// </summary>
    public void ShowMiss()
    {
        if (showMiss)
        {
            DamagePopupManager.Miss(transform.position + popupOffset);
        }
    }
}
