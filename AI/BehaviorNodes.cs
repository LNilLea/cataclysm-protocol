using System.Collections.Generic;
using MyGame;

/// <summary>
/// 选择器节点 - 依次执行子节点，直到有一个成功
/// </summary>
public class SelectorNode : BehaviorNode
{
    private List<BehaviorNode> children;

    public SelectorNode(List<BehaviorNode> children)
    {
        this.children = children;
    }

    public SelectorNode(params BehaviorNode[] children)
    {
        this.children = new List<BehaviorNode>(children);
    }

    public override BehaviorResult Execute(BehaviorContext context)
    {
        foreach (var child in children)
        {
            BehaviorResult result = child.Execute(context);

            if (result == BehaviorResult.Success)
            {
                return BehaviorResult.Success;
            }

            if (result == BehaviorResult.Running)
            {
                return BehaviorResult.Running;
            }

            // Failure 则继续下一个节点
        }

        return BehaviorResult.Failure;
    }
}

/// <summary>
/// 序列器节点 - 依次执行子节点，全部成功才成功
/// </summary>
public class SequenceNode : BehaviorNode
{
    private List<BehaviorNode> children;

    public SequenceNode(List<BehaviorNode> children)
    {
        this.children = children;
    }

    public SequenceNode(params BehaviorNode[] children)
    {
        this.children = new List<BehaviorNode>(children);
    }

    public override BehaviorResult Execute(BehaviorContext context)
    {
        foreach (var child in children)
        {
            BehaviorResult result = child.Execute(context);

            if (result == BehaviorResult.Failure)
            {
                return BehaviorResult.Failure;
            }

            if (result == BehaviorResult.Running)
            {
                return BehaviorResult.Running;
            }

            // Success 则继续下一个节点
        }

        return BehaviorResult.Success;
    }
}

/// <summary>
/// 条件节点 - 检查条件
/// </summary>
public class ConditionNode : BehaviorNode
{
    private System.Func<BehaviorContext, bool> condition;

    public ConditionNode(System.Func<BehaviorContext, bool> condition)
    {
        this.condition = condition;
    }

    public override BehaviorResult Execute(BehaviorContext context)
    {
        return condition(context) ? BehaviorResult.Success : BehaviorResult.Failure;
    }
}

/// <summary>
/// 动作节点 - 执行动作
/// </summary>
public class ActionNode : BehaviorNode
{
    private System.Func<BehaviorContext, BehaviorResult> action;

    public ActionNode(System.Func<BehaviorContext, BehaviorResult> action)
    {
        this.action = action;
    }

    public override BehaviorResult Execute(BehaviorContext context)
    {
        return action(context);
    }
}
