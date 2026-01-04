using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feat_QuickReflexes : FeatBase
{
    public Feat_QuickReflexes()
    {
        featName = "快速反应";
    }

    // 每5%减少敌人命中率相当于增加1AC
    public override void OnTurnStart(PlayerCombatData player)
    {
        // 增加1 AC
        player.agilityAC += 1;
        Debug.Log($"{player} 使用了快速反应，AC +1！");
    }
}

