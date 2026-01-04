using UnityEngine;

namespace MyGame
{
    public class CombatSystem : MonoBehaviour
    {
        public PlayerCombatData combatData;
        public Weapon currentWeapon;

        public void InitializeCombat(Player player)
        {
            combatData = player.combatData;  // 获取战斗数据
            currentWeapon = player.currentWeapon;  // 获取玩家当前武器
        }

        public string UseWeaponOnTarget(ICombatTarget target, Player player, out string log)
        {
            log = "";

            if (currentWeapon == null)
            {
                log = "玩家没有装备武器，无法攻击。";
                return log;
            }

            // 【修复】从 combatData 获取体魄属性
            int atkAttribute = combatData.strength;

            // 命中判定
            int d20 = Random.Range(1, 21);
            int hitValue = d20 + (atkAttribute - 3) + currentWeapon.HitBonus;

            log += $"玩家使用武器 [{currentWeapon.Name}] 攻击 {target.Name}！\n";
            log += $"命中检定：d20({d20}) + (体魄-3={atkAttribute - 3}) + 武器命中({currentWeapon.HitBonus}) = {hitValue}\n";
            log += $"目标AC：{target.CurrentAC}\n";

            if (hitValue >= target.CurrentAC)
            {
                // 伤害计算
                int diceRoll = Random.Range(currentWeapon.DamageRange.x, currentWeapon.DamageRange.y + 1);
                int damage = diceRoll + (atkAttribute - 3) + currentWeapon.AdditionalBonus;

                combatData.DealDamage(ref damage);

                if (damage < 0) damage = 0;

                target.TakeDamage(damage);
                log += $"★ 命中！造成伤害：{diceRoll} + (体魄-3={atkAttribute - 3}) + 武器额外({currentWeapon.AdditionalBonus}) = {damage}\n";
            }
            else
            {
                log += "★ 攻击未命中。\n";
            }

            return log;
        }

        internal void GetCombatData(Player player)
        {
            throw new System.NotImplementedException();
        }
    }
}
