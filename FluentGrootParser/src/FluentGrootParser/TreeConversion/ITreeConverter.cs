using System.Xml.Linq;
using FluentGrootParser.Models.FluentBehaviorTree;
using FluentGrootParser.Models.Groot2;

namespace FluentGrootParser.TreeConversion;

public interface ITreeConverter
{
    IBehaviourTreeNode ConvertToTree(List<XDocument> xmlDocuments,
        Dictionary<Node, Func<List<string>, BehaviourTreeStatus>> mappedActions,
        Dictionary<Node, Func<List<string>, bool>> mappedConditions);
}