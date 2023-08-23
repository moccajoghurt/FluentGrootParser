namespace FluentGrootParser.Models.FluentBehaviorTree.Nodes;

public class SelectorNode : IParentBehaviourTreeNode
{
    private readonly List<IBehaviourTreeNode> _children = new();

    public BehaviourTreeStatus Tick()
    {
        foreach (var child in _children)
        {
            var childStatus = child.Tick();
            if (childStatus != BehaviourTreeStatus.Failure) return childStatus;
        }

        return BehaviourTreeStatus.Failure;
    }

    public void AddChild(IBehaviourTreeNode child)
    {
        _children.Add(child);
    }
}