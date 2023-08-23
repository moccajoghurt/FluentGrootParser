using System.Xml.Linq;

namespace FluentGrootParser.FileReader;

public interface IXmlFileReader
{
    public List<XDocument> ReadXmlFiles(string rootFolder);
    public XDocument ReadBtProjFile(string rootFolder);
}