namespace FluentGrootParser.Models.FluentBehaviorTree.Nodes;

/// <summary>
///     Runs childs nodes in parallel.
/// </summary>
public class ParallelNode : IParentBehaviourTreeNode
{
    private readonly List<IBehaviourTreeNode> _children = new();
    private readonly int _numRequiredToFail;
    private readonly int _numRequiredToSucceed;

    public ParallelNode(int numRequiredToFail, int numRequiredToSucceed)
    {
        _numRequiredToFail = numRequiredToFail;
        _numRequiredToSucceed = numRequiredToSucceed;
    }

    public BehaviourTreeStatus Tick()
    {
        var numChildrenSucceeded = 0;
        var numChildrenFailed = 0;

        foreach (var child in _children)
        {
            var childStatus = child.Tick();
            switch (childStatus)
            {
                case BehaviourTreeStatus.Success:
                    ++numChildrenSucceeded;
                    break;
                case BehaviourTreeStatus.Failure:
                    ++numChildrenFailed;
                    break;
            }
        }

        if (_numRequiredToSucceed > 0 && numChildrenSucceeded >= _numRequiredToSucceed)
            return BehaviourTreeStatus.Success;

        if (_numRequiredToFail > 0 && numChildrenFailed >= _numRequiredToFail) return BehaviourTreeStatus.Failure;

        return BehaviourTreeStatus.Running;
    }

    public void AddChild(IBehaviourTreeNode child)
    {
        _children.Add(child);
    }
}