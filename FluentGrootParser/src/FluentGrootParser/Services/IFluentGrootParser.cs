using FluentGrootParser.Models.FluentBehaviorTree;
using FluentGrootParser.Models.Groot2;

namespace FluentGrootParser.Services;

public interface IFluentGrootParser
{
    IBehaviourTreeNode ConvertToTree(string grootTreePath,
        Func<Node, Func<BehaviourTreeStatus>> mapActions,
        Func<Node, Func<bool>> mapConditions);

    List<Node> GetAllTreeNodes();
}