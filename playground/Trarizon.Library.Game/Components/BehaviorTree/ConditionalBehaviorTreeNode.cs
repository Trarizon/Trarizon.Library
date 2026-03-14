namespace Trarizon.Library.Game.Components.BehaviorTree;
public abstract class ConditionalBehaviorTreeNode : BehaviorTreeNode
{
    protected internal override sealed Status OnUpdate()
    {
        return JudgeCondition() ? Status.Success : Status.Failure;
    }

    protected abstract bool JudgeCondition();
}
