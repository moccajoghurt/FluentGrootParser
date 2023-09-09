using System.Xml.Linq;
using FluentGrootParser.Models.FluentBehaviorTree;
using FluentGrootParser.Models.Groot2;
using FluentGrootParser.TreeBuilder;

namespace FluentGrootParser.TreeConversion;

public class TreeConverter : ITreeConverter
{
    private readonly BehaviourTreeBuilder _builder = new();
    private Dictionary<string, string> _currentSubTreeParameters = new();
    private Func<Node, Func<BehaviourTreeStatus>> _mappedActions = null!;
    private Func<Node, Func<bool>> _mappedConditions = null!;
    private List<Node> _nodes = null!;
    private XDocument _treeDocument = null!;

    public IBehaviourTreeNode ConvertToTree(List<XDocument> xmlDocuments,
        List<Node> nodes,
        Func<Node, Func<BehaviourTreeStatus>> mappedActions,
        Func<Node, Func<bool>> mappedConditions)
    {
        _nodes = nodes;
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
                if (!AddActionOrCondition(currentNode))
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
        // Use placeholder parameters
        var buf = _currentSubTreeParameters;
        _currentSubTreeParameters = new Dictionary<string, string>();
        foreach (var attr in node.Attributes())
        {
            // If the subtree has placeholder parameters, take the values from the parent tree
            if (attr.Value.StartsWith('{') && attr.Value.EndsWith('}'))
            {
                var parentKey = attr.Value.Trim('{', '}');
                var currentKey = attr.Name.ToString();
                _currentSubTreeParameters.Add(currentKey, buf[parentKey]);
            }
            else
            {
                _currentSubTreeParameters.Add(attr.Name.ToString(), attr.Value);
            }
        }

        var subTreeId = node.Attribute("ID")?.Value;
        var subTreeElement = _treeDocument.Root?.Elements("BehaviorTree")
            .FirstOrDefault(e => e.Attribute("ID")?.Value == subTreeId);
        if (subTreeElement != null)
            BuildBehaviourTree(subTreeElement);

        _currentSubTreeParameters = buf;
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

    private bool AddActionOrCondition(XElement node)
    {
        var myNode = _nodes.Find(n => n.Name == node.Name.ToString());
        if (myNode == null) return false;
        var newNode = new Node
        {
            Name = myNode.Name,
            Type = myNode.Type
        };
        if (myNode.Params != null)
        {
            // fill in the parameters
            newNode.Params = node.Attributes()
                .Where(attr => myNode.Params.ContainsKey(attr.Name.ToString()))
                .ToDictionary(attr => attr.Name.ToString(), attr =>
                {
                    // if the parameter is a reference to a subtree parameter, replace it with the value
                    if (attr.Value.StartsWith('{') && attr.Value.EndsWith('}'))
                        return _currentSubTreeParameters[attr.Value.Trim('{', '}')];
                    return attr.Value;
                });
            // add the default values for the parameters that are not specified or where the value is empty
            foreach (var param in myNode.Params)
                if (!newNode.Params.ContainsKey(param.Key) || string.IsNullOrEmpty(newNode.Params[param.Key]))
                    newNode.Params[param.Key] = param.Value;
        }


        if (newNode.Type == NodeType.Action)
            _builder.Do(_mappedActions(newNode));
        else
            _builder.Condition(_mappedConditions(newNode));

        return true;
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