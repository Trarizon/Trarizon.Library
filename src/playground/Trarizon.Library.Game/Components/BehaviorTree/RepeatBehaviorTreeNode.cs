using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trarizon.Library.Game.Components.BehaviorTree;
public sealed class RepeatBehaviorTreeNode(BehaviorTreeNode node,int count) : BehaviorTreeNode
{
    private int _current;

    protected internal override Status OnUpdate()
    {

    }
}
