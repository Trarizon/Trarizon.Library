namespace Trarizon.Library.Game.Components.BehaviorTree;
public abstract class BehaviorTreeNode
{
    protected internal abstract Status OnUpdate();

    public enum Status
    {
        Success,
        Failure,
        Running
    }
}
