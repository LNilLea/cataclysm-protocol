using UnityEngine;
using MyGame;

/// <summary>
/// 螳螂敌人 - 使用Grid系统
/// </summary>
public class Mantis : MonsterBase
{
    [Header("攻击设置 - 螳螂刀")]
    public int bladeHitBonus = 8;
    public int bladeDiceCount = 2;
    public int bladeDiceSides = 6;
    public int bladeDamageBonus = 2;

    [Header("攻击设置 - 擒抱")]
    public int grappleHitBonus = 3;

    [Header("攻击设置 - 咬击(擒抱中)")]
    public int biteHitBonus = 6;
    public int biteDiceCount = 4;
    public int biteDiceSides = 6;
    public int biteDamageBonus = 2;

    [Header("擒抱状态")]
    public bool isGrapplingPlayer = false;
    public int biteCountWhileGrappling = 0;
    public float firstBiteReleaseChance = 0.2f;
    public float secondBiteReleaseChance = 0.5f;
    public float thirdPlusReleaseChance = 1.0f;

    protected override void Awake()
    {
        // 设置默认属性
        monsterName = "螳螂";
        maxHP = 30;
        AC = 11;
        initiative = 22;
        movementPoints = 4;
        attackRangeMin = 1;
        attackRangeMax = 2;

        base.Awake();
    }

    protected override void OnDeath()
    {
        // 死亡时释放擒抱
        if (isGrapplingPlayer && targetPlayer != null)
        {
            targetPlayer.combatData.isGrappledByMantis = false;
        }
        base.OnDeath();
    }

    public override string PerformAction(Player player)
    {
        if (player == null) player = FindObjectOfType<Player>();
        targetPlayer = player;

        string log = "";

        // 如果正在擒抱玩家，使用咬击
        if (isGrapplingPlayer)
        {
            log += UseBite(player);
            return log;
        }

        int gridDistance = GetGridDistanceToPlayer();

        // 如果不在攻击范围内，先移动
        if (gridDistance > attackRangeMax)
        {
            log += MoveTowardsPlayer(player);
            gridDistance = GetGridDistanceToPlayer();
        }

        // 如果在攻击范围内
        if (gridDistance >= attackRangeMin && gridDistance <= attackRangeMax)
        {
            // 50%几率尝试擒抱，50%几率使用螳螂刀
            if (Random.value < 0.5f)
            {
                log += UseGrapple(player);
            }
            else
            {
                log += DoAttackRoll(player, "螳螂刀", bladeHitBonus, bladeDiceCount, bladeDiceSides, bladeDamageBonus);
            }
        }
        else
        {
            log += $"{monsterName} 距离太远（{gridDistance}格），无法攻击";
        }

        return log;
    }

    /// <summary>
    /// 擒抱攻击
    /// </summary>
    private string UseGrapple(Player player)
    {
        var pData = player.combatData;
        int d20 = Random.Range(1, 21);
        int hitValue = d20 + grappleHitBonus;

        string log = $"{monsterName} 尝试 [擒抱]！命中: d20({d20})+{grappleHitBonus}={hitValue} vs AC{pData.CurrentAC}";

        if (hitValue >= pData.CurrentAC)
        {
            isGrapplingPlayer = true;
            biteCountWhileGrappling = 0;
            pData.isGrappledByMantis = true;
            log += "\n→ 擒抱成功！玩家敏捷AC失效";
        }
        else
        {
            log += "\n→ 擒抱失败";
        }
        return log;
    }

    /// <summary>
    /// 咬击攻击（擒抱中使用）
    /// </summary>
    private string UseBite(Player player)
    {
        var pData = player.combatData;
        if (!isGrapplingPlayer)
        {
            return $"{monsterName} 无法使用咬击（未处于擒抱状态）";
        }

        int d20 = Random.Range(1, 21);
        int hitValue = d20 + biteHitBonus;

        string log = $"{monsterName} 在擒抱中使用 [咬击]！命中: d20({d20})+{biteHitBonus}={hitValue} vs AC{pData.CurrentAC}";

        if (hitValue >= pData.CurrentAC)
        {
            int diceRoll = Roll(biteDiceCount, biteDiceSides);
            int damage = Mathf.Max(1, diceRoll + biteDamageBonus);
            log += $"\n→ 咬击命中！{biteDiceCount}d{biteDiceSides}({diceRoll})+{biteDamageBonus} = {damage} 点伤害";
            player.TakeDamage(damage);
        }
        else
        {
            log += "\n→ 咬击未命中";
        }

        // 计算松手概率
        biteCountWhileGrappling++;
        float releaseChance = biteCountWhileGrappling <= 1 ? firstBiteReleaseChance :
                             biteCountWhileGrappling == 2 ? secondBiteReleaseChance :
                             thirdPlusReleaseChance;

        float roll = Random.value;
        log += $"\n擒抱计数: {biteCountWhileGrappling}, 释放概率: {releaseChance:P0}, 骰值: {roll:F2}";

        if (roll <= releaseChance)
        {
            isGrapplingPlayer = false;
            pData.isGrappledByMantis = false;
            biteCountWhileGrappling = 0;
            log += "\n→ 螳螂松开玩家，擒抱结束，敏捷AC恢复";
        }
        else
        {
            log += "\n→ 螳螂继续抱住玩家";
        }

        return log;
    }
}
