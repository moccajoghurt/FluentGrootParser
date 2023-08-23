using System.Xml.Linq;

namespace FluentGrootParser.FileReader;

public class XmlFileReader : IXmlFileReader
{
    public List<XDocument> ReadXmlFiles(string rootFolder)
    {
        var xmlContents = new List<XDocument>();
        var xmlFiles = Directory.GetFiles(rootFolder, "*.xml", SearchOption.AllDirectories);
        foreach (var file in xmlFiles)
        {
            try
            {
                var doc = XDocument.Load(file);
                xmlContents.Add(doc);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {file}: {ex.Message}");
            }
        }
        return xmlContents;
    }

    public XDocument ReadBtProjFile(string rootFolder)
    {
        var xmlFiles = Directory.GetFiles(rootFolder, "*.btproj", SearchOption.AllDirectories);
        return XDocument.Load(xmlFiles[0]);
    }
}