namespace FluentGrootParser.Models.FluentBehaviorTree;

public interface IBehaviourTreeNode
{
    BehaviourTreeStatus Tick();
}