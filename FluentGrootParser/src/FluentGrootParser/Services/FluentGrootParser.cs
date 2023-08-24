using FluentGrootParser.FileReader;
using FluentGrootParser.Models.FluentBehaviorTree;
using FluentGrootParser.Models.Groot2;
using FluentGrootParser.TreeConversion;

namespace FluentGrootParser.Services;

public class FluentGrootParser : IFluentGrootParser
{
    private readonly INodeConverter _nodeConverter;
    private readonly ITreeConverter _treeConverter;
    private readonly IXmlFileReader _xmlFileReader;
    private string _grootTreePath = null!;

    public FluentGrootParser(ITreeConverter treeConverter, INodeConverter nodeConverter, IXmlFileReader xmlFileReader)
    {
        _treeConverter = treeConverter;
        _nodeConverter = nodeConverter;
        _xmlFileReader = xmlFileReader;
    }

    public IBehaviourTreeNode ConvertToTree(string grootTreePath,
        Func<Node, Func<BehaviourTreeStatus>> mapActions,
        Func<Node, Func<bool>> mapConditions)
    {
        _grootTreePath = grootTreePath;
        var xmlFiles = _xmlFileReader.ReadXmlFiles(grootTreePath);
        var projFile = _xmlFileReader.ReadBtProjFile(grootTreePath);
        var nodes = _nodeConverter.GetNodes(projFile);
        var tree = _treeConverter.ConvertToTree(xmlFiles, nodes, mapActions, mapConditions);
        return tree;
    }

    public List<Node> GetAllTreeNodes()
    {
        if (_grootTreePath == null)
            throw new InvalidOperationException("Groot tree path is null. Create a tree first.");
        var projFile = _xmlFileReader.ReadBtProjFile(_grootTreePath);
        return _nodeConverter.GetNodes(projFile);
    }
}