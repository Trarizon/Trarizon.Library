using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trarizon.Library.Game.Components.BehaviorTree;
public abstract class CompositeBehaviorTreeNode(IEnumerable<BehaviorTreeNode> nodes) : BehaviorTreeNode
{
    protected IEnumerable<BehaviorTreeNode> Nodes { get; } = nodes;
}
