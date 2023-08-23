namespace FluentGrootParser.Models.FluentBehaviorTree.Nodes;

public class ActionNode : IBehaviourTreeNode
{
    private readonly Func<List<string>, BehaviourTreeStatus> _fn;
    public ActionNode(Func<List<string>, BehaviourTreeStatus> fn)
    {
        _fn = fn;
    }

    public BehaviourTreeStatus Tick(List<string> parameters)
    {
        return _fn(parameters);
    }
}