using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feat_QuickCounter : FeatBase
{
    public Feat_QuickCounter()
    {
        featName = "急速反击";
    }

    public override void OnPlayerDealDamage(PlayerCombatData player, ref int damage)
    {
        // 假设在敌人攻击未命中的情况下玩家可以反击
        string log = $"{player} 触发了 [急速反击]！";

        // 这里可以通过判断敌人的命中状态来触发反击（假设命中检定失败）
        int d20 = Random.Range(1, 21);
        int hitBonus = player.agilityAC; // 假设使用敏捷 AC

        int hitValue = d20 + hitBonus;

        log += $"\n反击：d20({d20}) + {hitBonus} = {hitValue}";

        if (hitValue >= player.CurrentAC)
        {
            int extraDamage = Random.Range(1, 6);  // 额外伤害 1~5
            damage += extraDamage;
            log += $"\n→ 反击命中！造成 {extraDamage} 点伤害！";
        }
        else
        {
            log += "\n→ 反击未命中！";
        }

        Debug.Log(log);
    }
}
