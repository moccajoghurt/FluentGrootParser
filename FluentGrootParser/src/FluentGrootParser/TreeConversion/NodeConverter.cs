using System.Xml.Linq;
using FluentGrootParser.Models.Groot2;

namespace FluentGrootParser.TreeConversion;

public class NodeConverter : INodeConverter
{
    public List<Node> GetNodes(XDocument btProjFile)
    {
        var nodes = new List<Node>();
        nodes.AddRange(GetNodes(btProjFile, NodeType.Action));
        nodes.AddRange(GetNodes(btProjFile, NodeType.Condition));

        return nodes;
    }

    private static IEnumerable<Node> GetNodes(XDocument btProjFile, NodeType type)
    {
        var nodes = new List<Node>();
        var actionElements = btProjFile.Descendants(type.ToString());
        foreach (var actionElement in actionElements)
        {
            var actionParams = actionElement.Descendants("input_port")
                .Select(p => new
                {
                    Name = p.Attribute("name")?.Value,
                    DefaultValue = p.Attribute("default")?.Value ?? ""
                })
                .Where(attr => attr.Name != null)
                .ToDictionary(attr => attr.Name!, attr => attr.DefaultValue);

            var node = new Node
            {
                Type = type,
                Name = actionElement.Attribute("ID")?.Value ?? throw new InvalidOperationException(),
                Params = actionParams.Any() ? actionParams : null
            };
            nodes.Add(node);
        }

        return nodes;
    }
}