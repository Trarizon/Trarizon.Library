using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trarizon.Library.Game.Components.BehaviorTree;
public sealed class SequenceBehaviorTreeNode : CompositeBehaviorTreeNode
{
    public SequenceBehaviorTreeNode(params IEnumerable<BehaviorTreeNode> nodes) : base(nodes)
    {
    }

    protected internal override Status OnUpdate()
    {
        foreach (var node in Nodes) {
            if (node.OnUpdate() is Status.Failure)
                return Status.Failure;
        }
        return Status.Success;
    }
}
