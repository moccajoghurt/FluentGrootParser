using FluentGrootParser.FileReader;
using FluentGrootParser.Services;
using FluentGrootParser.TreeConversion;
using Microsoft.Extensions.DependencyInjection;

namespace FluentGrootParser;

public static class Registry
{
    public static void AddFluentGrootParser(this IServiceCollection services)
    {
        services.AddSingleton<IXmlFileReader, XmlFileReader>();
        services.AddSingleton<INodeConverter, NodeConverter>();
        services.AddSingleton<ITreeConverter, TreeConverter>();
        services.AddSingleton<IFluentGrootParser, Services.FluentGrootParser>();
    }
}