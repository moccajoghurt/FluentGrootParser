using System.Xml.Linq;
using FluentGrootParser.Models.FluentBehaviorTree;
using FluentGrootParser.Models.Groot2;
using FluentGrootParser.TreeBuilder;

namespace FluentGrootParser.TreeConversion;

public class TreeConverter : ITreeConverter
{
    private readonly BehaviourTreeBuilder _builder = new();
    private Dictionary<Node, Func<BehaviourTreeStatus>> _mappedActions = new();
    private Dictionary<Node, Func<bool>> _mappedConditions = new();
    private XDocument _treeDocument = new();

    public IBehaviourTreeNode ConvertToTree(List<XDocument> xmlDocuments,
        Dictionary<Node, Func<BehaviourTreeStatus>> mappedActions,
        Dictionary<Node, Func<bool>> mappedConditions)
    {
        _mappedActions = mappedActions;
        _mappedConditions = mappedConditions;
        _treeDocument = GetCombinedDoc(xmlDocuments);
        var rootNode = _treeDocument.Root?.Elements("BehaviorTree")
            .FirstOrDefault(bt => bt.Attribute("ID")?.Value == "TreeRoot");
        if (rootNode == null) throw new InvalidOperationException("TreeRoot not found");
        BuildBehaviourTree(rootNode);
        var tree = _builder.Build();
        return tree;
    }

    private void BuildBehaviourTree(XElement currentNode)
    {
        switch (currentNode.Name.ToString())
        {
            case "BehaviorTree":
            case "Sequence":
            case "Inverter":
            case "Fallback":
                ProcessCompositeNode(currentNode);
                break;

            case "SubTree":
                ProcessSubTree(currentNode);
                break;

            case "Parallel":
                ProcessParallelNode(currentNode);
                break;

            default:
                if (!AddActionOrCondition(currentNode.Name.ToString()))
                    throw new InvalidOperationException($"Node {currentNode.Name} not found");
                break;
        }
    }

    private void ProcessCompositeNode(XElement node)
    {
        switch (node.Name.ToString())
        {
            case "Sequence":
                _builder.Sequence();
                break;
            case "Inverter":
                _builder.Inverter();
                break;
            case "Fallback":
                _builder.Selector();
                break;
        }

        foreach (var child in node.Elements())
            BuildBehaviourTree(child);

        if (node.Name.ToString() != "BehaviorTree") _builder.End();
    }

    private void ProcessSubTree(XElement node)
    {
        var subTreeId = node.Attribute("ID")?.Value;
        var subTreeElement = _treeDocument.Root?.Elements("BehaviorTree")
            .FirstOrDefault(e => e.Attribute("ID")?.Value == subTreeId);
        if (subTreeElement != null)
            BuildBehaviourTree(subTreeElement);
    }

    private void ProcessParallelNode(XElement node)
    {
        var (numRequiredToFail, numRequiredToSucceed) = ParseParallelAttributes(node);
        _builder.Parallel(numRequiredToFail, numRequiredToSucceed);

        foreach (var child in node.Elements())
            BuildBehaviourTree(child);

        _builder.End();
    }

    private (int, int) ParseParallelAttributes(XElement node)
    {
        var numRequiredToFail =
            int.Parse(node.Attribute("failure_count")?.Value ?? throw new InvalidOperationException());
        var numRequiredToSucceed =
            int.Parse(node.Attribute("success_count")?.Value ?? throw new InvalidOperationException());

        return (numRequiredToFail, numRequiredToSucceed);
    }

    private bool AddActionOrCondition(string name)
    {
        var action = MapAction(name);
        if (action != null)
        {
            _builder.Do(action);
            return true;
        }

        var condition = MapCondition(name);
        if (condition == null) return false;
        _builder.Condition(condition);
        return true;
    }

    private Func<BehaviourTreeStatus>? MapAction(string nodeName)
    {
        return _mappedActions.All(n => n.Key.Name != nodeName)
            ? null
            : _mappedActions.First(n => n.Key.Name == nodeName).Value;
    }

    private Func<bool>? MapCondition(string nodeName)
    {
        return _mappedConditions.All(n => n.Key.Name != nodeName)
            ? null
            : _mappedConditions.First(n => n.Key.Name == nodeName).Value;
    }

    private static XDocument GetCombinedDoc(IEnumerable<XDocument> xmlDocuments)
    {
        return new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement("root",
                new XAttribute("BTCPP_format", "4"),
                xmlDocuments.SelectMany(doc => doc.Descendants("BehaviorTree"))
            )
        );
    }
}