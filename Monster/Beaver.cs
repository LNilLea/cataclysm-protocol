using UnityEngine;
using MyGame;

/// <summary>
/// 河狸敌人 - 使用Grid系统
/// </summary>
public class Beaver : MonsterBase
{
    [Header("攻击设置 - 咬击")]
    public int attackHitBonus = 2;
    public int damageDiceCount = 1;
    public int damageDiceSides = 6;
    public int damageBonus = 2;

    protected override void Awake()
    {
        // 设置默认属性
        monsterName = "河狸";
        maxHP = 30;
        AC = 9;
        initiative = 10;
        movementPoints = 3;
        attackRangeMin = 1;
        attackRangeMax = 1;

        base.Awake();
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
            log += DoAttackRoll(player, "咬击", attackHitBonus, damageDiceCount, damageDiceSides, damageBonus);
        }
        else
        {
            log += $"{monsterName} 距离太远（{gridDistance}格），无法攻击";
        }

        return log;
    }
}
