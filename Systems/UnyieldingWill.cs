using UnityEngine;

public class Feat_UnyieldingWill : FeatBase
{
    public Feat_UnyieldingWill()
    {
        featName = "不屈意志";
    }

    // 回合开始时检查是否满足条件（HP ≤ 0）
    public override void OnTurnStart(PlayerCombatData player)
    {
        if (player.currentHP <= 0 && !player.hasUsedUnyieldingWillThisTurn)
        {
            // 给玩家一次额外行动机会（反击）
            player.hasUsedUnyieldingWillThisTurn = true;
            Debug.Log($"{player} 触发了不屈意志，获得一次额外的行动机会！");
        }
    }

    // 战斗结束后检查玩家是否消灭所有敌人
    public override void OnTurnEnd(PlayerCombatData player)
    {
        // 如果玩家的HP仍然是0，并且他在战斗结束时消灭了所有敌人
        if (player.currentHP == 0 && player.isEnemyAllDead)
        {
            player.currentHP = 1;
            Debug.Log($"{player} 被不屈意志恢复到 1HP！");
        }

        // 重置玩家“是否已经使用不屈意志”状态
        player.hasUsedUnyieldingWillThisTurn = false;
    }
}
