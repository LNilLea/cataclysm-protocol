using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feat_StaminaWall : FeatBase
{
    public Feat_StaminaWall()
    {
        featName = "耐力之墙";
    }

    // 吸收15%伤害，相当于减少3点伤害
    public override void OnPlayerTakeDamage(PlayerCombatData player, ref int damage)
    {
        // 减少3点伤害（等价于15%）
        damage = Mathf.Max(0, damage - 3);
        Debug.Log($"{player} 使用耐力之墙，伤害减少了 3 点！");
    }
}
