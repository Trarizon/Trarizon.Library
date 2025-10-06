namespace Trarizon.Library.Game.Components.BehaviorTree;
public abstract class CompositeBehaviorTreeNode(BehaviorTreeNode[] nodes) : BehaviorTreeNode
{
    protected BehaviorTreeNode[] Nodes { get; } = nodes;
}
