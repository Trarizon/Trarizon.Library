
namespace Trarizon.Library.Game.Components.BehaviorTree;
public sealed class SelectorBehaviorTreeNode : CompositeBehaviorTreeNode
{
    private int _prevRun = 0;
    public SelectorBehaviorTreeNode(params BehaviorTreeNode[] nodes) : base(nodes)
    {
    }

    protected internal override Status OnUpdate()
    {
        for (int i = _prevRun; i < Nodes.Length; i++) {
            var node = Nodes[i];
            switch (node.OnUpdate()) {
                case Status.Success:
                    _prevRun = 0;
                    return Status.Success;
                case Status.Running:
                    _prevRun = i;
                    return Status.Running;
                case Status.Failure:
                    break;
            }
        }
        _prevRun = 0;
        return Status.Failure;
    }
}
