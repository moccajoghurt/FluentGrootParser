namespace FluentGrootParser.Models.FluentBehaviorTree;

public interface IParentBehaviourTreeNode : IBehaviourTreeNode
{
    void AddChild(IBehaviourTreeNode child);
}