namespace FluentGrootParser.Models.FluentBehaviorTree.Nodes;

public class ActionNode : IBehaviourTreeNode
{
    private readonly Func<BehaviourTreeStatus> _fn;
    public ActionNode(Func<BehaviourTreeStatus> fn)
    {
        _fn = fn;
    }

    public BehaviourTreeStatus Tick()
    {
        return _fn();
    }
}