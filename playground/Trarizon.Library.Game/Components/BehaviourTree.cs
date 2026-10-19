using System.Net;

namespace Trarizon.Library.Game.Components.BehaviourTree;

public static class BehaviourTree
{

}

public enum BTNodeStatus { Success, Failure, Running, }

public abstract class BTNode
{
    public abstract BTNodeStatus Process();
}

public abstract class BTNode<TContext>
{
    public abstract BTNodeStatus Process(TContext context);
}

