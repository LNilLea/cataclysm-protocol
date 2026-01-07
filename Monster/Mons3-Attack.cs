using UnityEngine;
using MyGame;
[System.Serializable]
public class MonsterAttack
{
    public string attackName;
    public int hitBonus;
    public int damageDiceCount;
    public int damageDiceSides;
    public int bonusDamage;
    public string extraEffectNote;

    // 新增：每个怪物的移动能力（行动力）
    public int movementPoints;  // 每回合可以移动的格子数

    // 构造函数：初始化怪物攻击
    public MonsterAttack(string name, int hit, int diceCount, int diceSides, int bonus, int movePoints, string note = "")
    {
        attackName = name;
        hitBonus = hit;
        damageDiceCount = diceCount;
        damageDiceSides = diceSides;
        bonusDamage = bonus;
        movementPoints = movePoints;  // 设置怪物的行动力
        extraEffectNote = note;
    }

    // 投掷骰子计算伤害
    public int Roll(int count, int sides)
    {
        int sum = 0;
        for (int i = 0; i < count; i++)
            sum += Random.Range(1, sides + 1);
        return sum;
    }

    // 攻击方法（如咬击、爪击等）
    public string PerformAttack(Player player)
    {
        var pData = player.combatData;
        int d20 = Random.Range(1, 21);
        int hitValue = d20 + hitBonus;

        string log = $"{attackName} 命中判定: d20({d20}) + {hitBonus} = {hitValue} vs 玩家AC {pData.CurrentAC}";

        if (hitValue >= pData.CurrentAC)
        {
            int dice = Roll(damageDiceCount, damageDiceSides);
            int damage = dice + bonusDamage;
            log += $"\n→ 命中！造成 {damage} 点伤害";
            player.TakeDamage(damage);
        }
        else
        {
            log += "\n→ 未命中";
        }

        return log;
    }
}
