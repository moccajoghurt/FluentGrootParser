namespace FluentGrootParser.Models.FluentBehaviorTree.Nodes;

public class SequenceNode : IParentBehaviourTreeNode
{
    private readonly List<IBehaviourTreeNode> _children = new();

    public BehaviourTreeStatus Tick(List<string> parameters)
    {
        foreach (var child in _children)
        {
            var childStatus = child.Tick(parameters);
            if (childStatus != BehaviourTreeStatus.Success) return childStatus;
        }

        return BehaviourTreeStatus.Success;
    }

    public void AddChild(IBehaviourTreeNode child)
    {
        _children.Add(child);
    }
}