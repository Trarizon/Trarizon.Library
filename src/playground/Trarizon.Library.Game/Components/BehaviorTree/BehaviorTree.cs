using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trarizon.Library.Game.Components.BehaviorTree;
internal sealed class BehaviorTree
{
    public static BehaviorTreeNode Selector(params IEnumerable<BehaviorTreeNode> nodes)
    {
        return new SelectorBehaviorTreeNode(nodes);
    }

    public static BehaviorTreeNode Sequence(params IEnumerable<BehaviorTreeNode> nodes)
    {
        return new SequenceBehaviorTreeNode(nodes);
    }
}
