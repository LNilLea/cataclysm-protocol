using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feat_Lightfooted : FeatBase
{
    public Feat_Lightfooted()
    {
        featName = "轻盈步伐";
    }

    // 提升5%的移动速度
    public override void OnBattleStart(PlayerCombatData player)
    {
        // 每 5% 提升 1 格
        player.movementSquares += 1;
        Debug.Log($"{player} 使用了轻盈步伐，增加 1 格移动距离！");
    }
}
