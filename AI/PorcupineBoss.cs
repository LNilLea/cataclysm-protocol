using UnityEngine;
using MyGame;

/// <summary>
/// 豪猪Boss - 使用Grid系统
/// </summary>
public class PorcupineBoss : MonsterBase
{
    public enum Form
    {
        Offensive,  // 进攻形态
        Defensive   // 防御形态
    }

    [Header("当前形态")]
    public Form currentForm = Form.Offensive;

    [Header("攻击范围（格数）")]
    public int meleeRangeMax = 3;        // 近战范围
    public int rangedRangeMax = 8;       // 远程范围

    [Header("进攻形态 - 咬击")]
    public int biteHitBonus = 2;
    public int biteDiceCount = 2;
    public int biteDiceSides = 6;
    public int biteDamageBonus = 2;

    [Header("进攻形态 - 爪击")]
    public int clawHitBonus = 2;
    public int clawDiceCount = 2;
    public int clawDiceSides = 6;
    public int clawDamageBonus = 2;

    [Header("进攻形态 - 刺击连射")]
    public int pierceHitBonus = 4;
    public int pierceShotCount = 3;

    [Header("防御形态 - 飞弹刺")]
    public int shrapnelHitBonus = 5;
    public int shrapnelDiceCount = 4;
    public int shrapnelDiceSides = 6;

    [Header("防御形态 - 刺尾拍")]
    public int tailHitBonus = 2;
    public int tailDiceCount = 3;
    public int tailDiceSides = 6;

    private int actionCount = 0;

    protected override void Awake()
    {
        // 设置默认属性
        monsterName = "豪猪（Boss）";
        maxHP = 80;
        AC = 15;
        initiative = 20;
        movementPoints = 4;
        attackRangeMin = 1;
        attackRangeMax = rangedRangeMax;  // 默认远程

        base.Awake();
    }

    // 形态切换
    public void SwitchToOffensive()
    {
        currentForm = Form.Offensive;
        attackRangeMax = rangedRangeMax;
        Debug.Log("[PorcupineBoss] 豪猪切换到进攻形态");
    }

    public void SwitchToDefensive()
    {
        currentForm = Form.Defensive;
        attackRangeMax = meleeRangeMax;
        Debug.Log("[PorcupineBoss] 豪猪切换到防御形态");
    }

    public void ToggleForm()
    {
        if (currentForm == Form.Offensive) SwitchToDefensive();
        else SwitchToOffensive();
    }

    public override string PerformAction(Player player)
    {
        if (player == null) player = FindObjectOfType<Player>();
        targetPlayer = player;

        string log = "";
        int gridDistance = GetGridDistanceToPlayer();

        // 近战范围内：使用咬击或爪击
        if (gridDistance <= meleeRangeMax)
        {
            if (Random.value < 0.5f)
            {
                log += DoAttackRoll(player, "咬击", biteHitBonus, biteDiceCount, biteDiceSides, biteDamageBonus);
            }
            else
            {
                log += DoAttackRoll(player, "爪击", clawHitBonus, clawDiceCount, clawDiceSides, clawDamageBonus);
            }
            return log;
        }

        // 进攻形态：远程刺击
        if (currentForm == Form.Offensive)
        {
            // 如果不在远程攻击范围内，先移动
            if (gridDistance > rangedRangeMax)
            {
                attackRangeMax = rangedRangeMax;
                log += MoveTowardsPlayer(player);
                gridDistance = GetGridDistanceToPlayer();
            }

            if (gridDistance <= rangedRangeMax)
            {
                log += UsePierce(player, pierceShotCount);
            }
            else
            {
                log += $"{monsterName} 距离太远（{gridDistance}格），无法攻击";
            }
        }
        // 防御形态：飞弹刺或刺尾拍
        else
        {
            // 如果不在攻击范围内，先移动
            if (gridDistance > meleeRangeMax)
            {
                attackRangeMax = meleeRangeMax;
                log += MoveTowardsPlayer(player);
                gridDistance = GetGridDistanceToPlayer();
            }

            actionCount++;
            if (actionCount >= 2)
            {
                actionCount = 0;
                ToggleForm();
                log += $"\n{monsterName} 切换到进攻形态！\n";
            }

            if (gridDistance <= meleeRangeMax)
            {
                if (Random.value < 0.5f)
                {
                    log += DoAttackRoll(player, "飞弹刺", shrapnelHitBonus, shrapnelDiceCount, shrapnelDiceSides, 0);
                }
                else
                {
                    log += DoAttackRoll(player, "刺尾拍", tailHitBonus, tailDiceCount, tailDiceSides, 0);
                }
            }
            else
            {
                log += $"{monsterName} 距离太远（{gridDistance}格），无法攻击";
            }
        }

        return log;
    }

    /// <summary>
    /// 刺击连射
    /// </summary>
    private string UsePierce(Player player, int shots)
    {
        var pData = player.combatData;
        string log = $"{monsterName} 发动 [刺击连射]（{shots}发）！";

        int totalDamage = 0;

        for (int i = 0; i < shots; i++)
        {
            int d20 = Random.Range(1, 21);
            int hit = d20 + pierceHitBonus;

            log += $"\n第{i + 1}发: d20({d20})+{pierceHitBonus}={hit} vs AC{pData.CurrentAC}";

            if (hit >= pData.CurrentAC)
            {
                int dmg = Mathf.Max(1, Roll(2, 6));
                log += $" → 命中！2d6={dmg}";
                totalDamage += dmg;
            }
            else
            {
                log += " → 未命中";
            }
        }

        if (totalDamage > 0)
        {
            log += $"\n★ 总伤害: {totalDamage}";
            player.TakeDamage(totalDamage);
        }
        else
        {
            log += "\n★ 所有刺击均未命中";
        }

        return log;
    }
}
