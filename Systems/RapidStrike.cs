using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feat_RapidStrike : FeatBase
{
    public Feat_RapidStrike()
    {
        featName = "迅捷打击";
    }

    public override void OnPlayerDealDamage(PlayerCombatData player, ref int damage)
    {
        // 进行额外攻击
        string log = $"{player} 触发了 [迅捷打击]！";

        // 判定是否有额外攻击（通常根据武器决定）
        int d20 = Random.Range(1, 21);
        int hitBonus = player.agilityAC *2;  // 假设用敏捷AC作为额外命中

        int hitValue = d20 + hitBonus;

        log += $"\n额外攻击：d20({d20}) + {hitBonus} = {hitValue}";

        if (hitValue >= player.CurrentAC)
        {
            int extraDamage = Random.Range(1, 6);  // 简单示例：额外伤害 1~5
            damage += extraDamage;
            log += $"\n→ 额外攻击命中！造成 {extraDamage} 点伤害！";
        }
        else
        {
            log += "\n→ 额外攻击未命中！";
        }

        Debug.Log(log);
    }
}
