namespace FluentGrootParser.Models.Groot2;

public class Node
{
    public NodeType Type { get; set; }
    public string Name { get; set; } = null!;
    public string ParentName { get; set; } = null!;
    public Dictionary<string, string>? Params { get; set; } = null!;
}