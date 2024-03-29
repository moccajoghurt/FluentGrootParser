﻿using System.Xml.Linq;
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

        if (buf.TryGetValue("ID", out var parentName))
            _currentSubTreeParameters["ID"] = parentName + " " + _currentSubTreeParameters["ID"];

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
        var hasParent = _currentSubTreeParameters.TryGetValue("ID", out var parentName);
        var newNode = new Node
        {
            Name = myNode.Name,
            Type = myNode.Type,
            ParentName = hasParent ? parentName! : ""
        };
        if (myNode.Params != null)
        {
            // Fill in the parameters and add default values if not specified or empty
            newNode.Params = myNode.Params.ToDictionary(
                param => param.Key,
                param =>
                {
                    var attrValue = node.Attributes().FirstOrDefault(attr => attr.Name.ToString() == param.Key)?.Value;
                    return string.IsNullOrEmpty(attrValue) ? param.Value : attrValue;
                }
            );

            // Insert placeholder parameters
            foreach (var param in newNode.Params.Where(p => p.Value.StartsWith('{') && p.Value.EndsWith('}')).ToList())
            {
                var key = param.Value.Trim('{', '}');
                newNode.Params[param.Key] = _currentSubTreeParameters[key];
            }
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