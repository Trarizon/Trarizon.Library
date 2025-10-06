namespace Trarizon.Library.Game.Components.BehaviorTree.Decorators;
internal class NotBehaviorTreeNodeDecorator(BehaviorTreeNode node) : BehaviorTreeNode
{
    protected internal override Status OnUpdate() => node.OnUpdate() switch
    {
        Status.Success => Status.Failure,
        Status.Failure => Status.Success,
        _ => Status.Running,
    };
}
