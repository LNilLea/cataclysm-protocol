public class Feat_IronBody : FeatBase
{
    public Feat_IronBody()
    {
        featName = "Ìú¹ÇÈçÉ½";
    }

    public override void OnPlayerTakeDamage(PlayerCombatData player, ref int damage)
    {
        damage -= 2;
        if (damage < 0) damage = 0;
    }
}

