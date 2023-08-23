using FluentGrootParser.Models.FluentBehaviorTree;
using FluentGrootParser.Models.FluentBehaviorTree.Nodes;

namespace FluentGrootParser.TreeBuilder;

public class BehaviourTreeBuilder
{
    private readonly Stack<IParentBehaviourTreeNode> _parentNodeStack = new();

    private IBehaviourTreeNode? _curNode;

    public BehaviourTreeBuilder Do(Func<BehaviourTreeStatus> fn)
    {
        if (_parentNodeStack.Count <= 0)
            throw new InvalidOperationException("Can't create an unnested ActionNode, it must be a leaf node.");

        var actionNode = new ActionNode(fn);
        _parentNodeStack.Peek().AddChild(actionNode);
        return this;
    }

    public BehaviourTreeBuilder Condition(Func<bool> fn)
    {
        return Do(() => fn() ? BehaviourTreeStatus.Success : BehaviourTreeStatus.Failure);
    }

    public BehaviourTreeBuilder Inverter()
    {
        var inverterNode = new InverterNode();

        if (_parentNodeStack.Count > 0) _parentNodeStack.Peek().AddChild(inverterNode);

        _parentNodeStack.Push(inverterNode);
        return this;
    }

    public BehaviourTreeBuilder Sequence()
    {
        var sequenceNode = new SequenceNode();

        if (_parentNodeStack.Count > 0) _parentNodeStack.Peek().AddChild(sequenceNode);

        _parentNodeStack.Push(sequenceNode);
        return this;
    }

    public BehaviourTreeBuilder Parallel(int numRequiredToFail, int numRequiredToSucceed)
    {
        var parallelNode = new ParallelNode(numRequiredToFail, numRequiredToSucceed);

        if (_parentNodeStack.Count > 0) _parentNodeStack.Peek().AddChild(parallelNode);

        _parentNodeStack.Push(parallelNode);
        return this;
    }

    public BehaviourTreeBuilder Selector()
    {
        var selectorNode = new SelectorNode();

        if (_parentNodeStack.Count > 0) _parentNodeStack.Peek().AddChild(selectorNode);

        _parentNodeStack.Push(selectorNode);
        return this;
    }

    public BehaviourTreeBuilder Splice(IBehaviourTreeNode subTree)
    {
        if (subTree == null) throw new ArgumentNullException(nameof(subTree));

        if (_parentNodeStack.Count <= 0)
            throw new InvalidOperationException("Can't splice an unnested sub-tree, there must be a parent-tree.");

        _parentNodeStack.Peek().AddChild(subTree);
        return this;
    }

    public IBehaviourTreeNode Build()
    {
        if (_curNode == null) throw new InvalidOperationException("Can't create a behaviour tree with zero nodes");
        return _curNode;
    }

    public BehaviourTreeBuilder End()
    {
        _curNode = _parentNodeStack.Pop();
        return this;
    }
}