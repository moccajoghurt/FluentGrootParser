using System.Xml.Linq;
using FluentGrootParser.Models.Groot2;

namespace FluentGrootParser.TreeConversion;

public interface INodeConverter
{
    public List<Node> GetNodes(XDocument btProjFile);
}