namespace FluentGrootParser.Models.FluentBehaviorTree.Nodes;

public class InverterNode : IParentBehaviourTreeNode
{
    private IBehaviourTreeNode? _childNode;

    public BehaviourTreeStatus Tick()
    {
        if (_childNode == null) throw new InvalidOperationException("InverterNode must have a child node!");

        var result = _childNode.Tick();
        if (result == BehaviourTreeStatus.Failure)
            return BehaviourTreeStatus.Success;
        if (result == BehaviourTreeStatus.Success)
            return BehaviourTreeStatus.Failure;
        return result;
    }

    public void AddChild(IBehaviourTreeNode child)
    {
        if (_childNode != null)
            throw new InvalidOperationException("Can't add more than a single child to InverterNode!");

        _childNode = child;
    }
}