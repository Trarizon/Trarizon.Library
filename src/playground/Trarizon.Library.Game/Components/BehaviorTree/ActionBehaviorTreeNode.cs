namespace Trarizon.Library.Game.Components.BehaviorTree;
public abstract class ActionBehaviorTreeNode : BehaviorTreeNode
{
    private readonly Func<Status> _func;

    protected internal override Status OnUpdate()
    {
        return _func();
    }
}
