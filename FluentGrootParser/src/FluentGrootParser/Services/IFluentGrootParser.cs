using FluentGrootParser.Models.FluentBehaviorTree;
using FluentGrootParser.Models.Groot2;

namespace FluentGrootParser.Services;

public interface IFluentGrootParser
{
    IBehaviourTreeNode ConvertToTree(string grootTreePath,
        Func<Node, Func<List<string>, BehaviourTreeStatus>> mapActions,
        Func<Node, Func<List<string>, bool>> mapConditions);
}