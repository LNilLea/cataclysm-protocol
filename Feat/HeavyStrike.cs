using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feat_HeavyStrike : FeatBase
{
    public Feat_HeavyStrike()
    {
        featName = "ÖØ»÷";
    }

    public override void OnPlayerDealDamage(PlayerCombatData player, ref int damage)
    {
        int extra = Random.Range(1, 5);  // 1d4
        damage += extra;
    }
}
