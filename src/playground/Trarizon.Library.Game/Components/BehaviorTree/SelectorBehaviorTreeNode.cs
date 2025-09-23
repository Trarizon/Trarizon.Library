
namespace Trarizon.Library.Game.Components.BehaviorTree;
public sealed class SelectorBehaviorTreeNode : CompositeBehaviorTreeNode
{
    public SelectorBehaviorTreeNode(params IEnumerable<BehaviorTreeNode> nodes) : base(nodes)
    {
    }

    protected internal override Status OnUpdate()
    {
        foreach (var node in Nodes) {
            if (node.OnUpdate() is Status.Success)
                return Status.Success;
        }
        return Status.Failure;
    }
}
