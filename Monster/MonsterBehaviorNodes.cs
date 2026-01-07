using UnityEngine;
using MyGame;

/// <summary>
/// 移动到玩家节点 - 回合制版本，考虑移动力限制
/// </summary>
public class MoveToPlayerNode : BehaviorNode
{
    public override BehaviorResult Execute(BehaviorContext context)
    {
        // 更新距离
        context.UpdateDistanceToPlayer();

        // 已经在攻击范围内，不需要移动
        if (context.distanceToPlayer <= context.attackRange)
        {
            context.Log($"{context.mobTransform.name} 已在攻击范围内，无需移动");
            return BehaviorResult.Success;
        }

        // 没有移动力了
        if (context.remainingMovement <= 0)
        {
            context.Log($"{context.mobTransform.name} 移动力耗尽");
            return BehaviorResult.Failure;
        }

        // 计算需要移动的距离
        float distanceToMove = context.distanceToPlayer - context.attackRange;
        int gridsToMove = Mathf.CeilToInt(distanceToMove);

        // 限制为剩余移动力
        int actualMove = Mathf.Min(gridsToMove, context.remainingMovement);

        if (actualMove > 0)
        {
            // 计算移动方向
            Vector3 direction = (context.targetPlayer.transform.position - context.mobTransform.position).normalized;
            direction.y = 0;

            // 计算目标位置
            float moveDistance = actualMove * context.gridSize;
            Vector3 targetPosition = context.mobTransform.position + direction * moveDistance;

            // 执行移动
            context.mobTransform.position = targetPosition;

            // 转向玩家
            if (direction != Vector3.zero)
            {
                context.mobTransform.rotation = Quaternion.LookRotation(direction);
            }

            // 消耗移动力
            context.UseMovement(actualMove);

            context.Log($"{context.mobTransform.name} 移动了 {actualMove} 格，剩余移动力: {context.remainingMovement}");
        }

        // 更新距离
        context.UpdateDistanceToPlayer();

        // 检查是否到达攻击范围
        if (context.distanceToPlayer <= context.attackRange)
        {
            return BehaviorResult.Success;
        }

        return BehaviorResult.Failure; // 还没到攻击范围
    }
}

/// <summary>
/// 攻击玩家节点 - 回合制版本
/// </summary>
public class AttackNode : BehaviorNode
{
    public override BehaviorResult Execute(BehaviorContext context)
    {
        // 更新距离
        context.UpdateDistanceToPlayer();

        // 不在攻击范围内
        if (context.distanceToPlayer > context.attackRange)
        {
            context.Log($"{context.mobTransform.name} 不在攻击范围内（距离: {context.distanceToPlayer:F1}，范围: {context.attackRange}）");
            return BehaviorResult.Failure;
        }

        // 没有主要动作
        if (!context.hasMainAction)
        {
            context.Log($"{context.mobTransform.name} 没有主要动作，无法攻击");
            return BehaviorResult.Failure;
        }

        // 消耗主要动作
        context.UseMainAction();

        // 执行攻击
        string attackLog = PerformAttack(context);
        context.Log(attackLog);

        return BehaviorResult.Success;
    }

    /// <summary>
    /// 执行攻击
    /// </summary>
    private string PerformAttack(BehaviorContext context)
    {
        string log = $"{context.mobTransform.name} 攻击 {context.targetPlayer.Name}！\n";

        // 命中检定：d20 + hitBonus vs AC
        int hitRoll = Random.Range(1, 21) + context.combatData.hitBonus;
        int targetAC = context.targetPlayer.CurrentAC;

        log += $"命中检定: {hitRoll} vs AC {targetAC}\n";

        if (hitRoll >= targetAC)
        {
            // 计算伤害
            int damage = 0;
            for (int i = 0; i < context.combatData.damageDiceCount; i++)
            {
                damage += Random.Range(1, context.combatData.damageDiceSides + 1);
            }
            damage += context.combatData.damageBonus;

            if (damage < 1) damage = 1;

            context.targetPlayer.TakeDamage(damage);

            log += $"→ 命中！造成 {damage} 点伤害";
        }
        else
        {
            log += "→ 未命中";
        }

        return log;
    }
}

/// <summary>
/// 检查攻击范围节点
/// </summary>
public class CheckAttackRangeNode : BehaviorNode
{
    public override BehaviorResult Execute(BehaviorContext context)
    {
        context.UpdateDistanceToPlayer();
        return context.distanceToPlayer <= context.attackRange 
            ? BehaviorResult.Success 
            : BehaviorResult.Failure;
    }
}

/// <summary>
/// 检查有移动力节点
/// </summary>
public class CheckHasMovementNode : BehaviorNode
{
    public override BehaviorResult Execute(BehaviorContext context)
    {
        return context.remainingMovement > 0 
            ? BehaviorResult.Success 
            : BehaviorResult.Failure;
    }
}

/// <summary>
/// 检查有主要动作节点
/// </summary>
public class CheckHasMainActionNode : BehaviorNode
{
    public override BehaviorResult Execute(BehaviorContext context)
    {
        return context.hasMainAction 
            ? BehaviorResult.Success 
            : BehaviorResult.Failure;
    }
}
