using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trarizon.Library.Game.Components.BehaviorTree;
public sealed class SequenceBehaviorTreeNode : CompositeBehaviorTreeNode
{
    private int _prevRun;

    public SequenceBehaviorTreeNode(params BehaviorTreeNode[] nodes) : base(nodes)
    {
        _prevRun = 0;
    }

    protected internal override Status OnUpdate()
    {
        for (int i = _prevRun; i < Nodes.Length; i++) {
            var node = Nodes[i];
            switch (node.OnUpdate()) {
                case Status.Failure:
                    _prevRun = 0;
                    return Status.Failure;
                case Status.Running:
                    _prevRun = i;
                    return Status.Running;
                case Status.Success:
                    break;
            }
        }
        _prevRun = 0;
        return Status.Success;
    }
}
