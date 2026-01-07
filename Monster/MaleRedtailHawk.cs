using UnityEngine;
using MyGame;

/// <summary>
/// 红尾鹰（雄）- 使用Grid系统
/// </summary>
public class MaleRedtailHawk : MonsterBase
{
    [Header("攻击设置 - 爪击")]
    public int clawHitBonus = 3;
    public int clawDiceCount = 2;
    public int clawDiceSides = 2;
    public int clawDamageBonus = 1;

    [Header("攻击设置 - 啄击")]
    public int peckHitBonus = 2;
    public int peckDiceCount = 1;
    public int peckDiceSides = 4;
    public int peckDamageBonus = 1;

    [Header("攻击权重")]
    [Range(0, 1)] public float clawWeight = 0.5f;

    protected override void Awake()
    {
        // 设置默认属性
        monsterName = "红尾鹰（雄）";
        maxHP = 12;
        AC = 14;
        initiative = 20;
        movementPoints = 4;
        attackRangeMin = 1;
        attackRangeMax = 1;

        base.Awake();
    }

    protected override string MoveTowardsPlayer(Player player)
    {
        // 使用基类移动，但改变日志描述
        string baseLog = base.MoveTowardsPlayer(player);
        return baseLog.Replace("移动了", "飞行了");
    }

    public override string PerformAction(Player player)
    {
        if (player == null) player = FindObjectOfType<Player>();
        targetPlayer = player;

        string log = "";
        int gridDistance = GetGridDistanceToPlayer();

        // 如果不在攻击范围内，先移动
        if (gridDistance > attackRangeMax)
        {
            log += MoveTowardsPlayer(player);
            gridDistance = GetGridDistanceToPlayer();
        }

        // 如果在攻击范围内，攻击
        if (gridDistance >= attackRangeMin && gridDistance <= attackRangeMax)
        {
            // 随机选择攻击方式
            if (Random.value < clawWeight)
            {
                log += DoAttackRoll(player, "爪击", clawHitBonus, clawDiceCount, clawDiceSides, clawDamageBonus);
            }
            else
            {
                log += DoAttackRoll(player, "啄击", peckHitBonus, peckDiceCount, peckDiceSides, peckDamageBonus);
            }
        }
        else
        {
            log += $"{monsterName} 距离太远（{gridDistance}格），无法攻击";
        }

        return log;
    }
}
