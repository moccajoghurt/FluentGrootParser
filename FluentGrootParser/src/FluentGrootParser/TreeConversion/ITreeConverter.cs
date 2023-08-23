using System.Xml.Linq;
using FluentGrootParser.Models.FluentBehaviorTree;
using FluentGrootParser.Models.Groot2;

namespace FluentGrootParser.TreeConversion;

public interface ITreeConverter
{
    IBehaviourTreeNode ConvertToTree(List<XDocument> xmlDocuments,
        List<Node> nodes,
        Func<Node, Func<BehaviourTreeStatus>> mappedActions,
        Func<Node, Func<bool>> mappedConditions);
}