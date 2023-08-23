using FluentGrootParser.Models.FluentBehaviorTree;
using FluentGrootParser.Models.Groot2;
using FluentGrootParser.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FluentGrootParser.Test.Integration;

public class TreeConverterTest
{
    private readonly List<string> _actionOrder;
    private readonly ServiceCollection _serviceCollection = new();
    private readonly IServiceProvider _serviceProvider;

    public TreeConverterTest()
    {
        _serviceCollection.AddFluentGrootParser();
        _serviceProvider = _serviceCollection.BuildServiceProvider();
        _actionOrder = new List<string>();
    }

    [Fact]
    public void ConvertToTree_BuildsValidTree()
    {
        // Arrange
        var parser = _serviceProvider.GetService<IFluentGrootParser>();
        var path = Path.Combine(Directory.GetCurrentDirectory(), "../../../SampleFiles");
        var expectedOrder = new List<string>
        {
            "HasNoWaypoints",
            "WaypointReached",
            "Walk",
            "DistanceToWaypointBiggerThan",
            "HasNoWaypoints",
            "WaypointReached"
        };
        // Act
        var tree = parser?.ConvertToTree(path, MapActions, MapConditions);
        tree?.Tick();
        // Assert
        Assert.NotNull(tree);
        Assert.True(expectedOrder.SequenceEqual(_actionOrder));
    }

    private Func<BehaviourTreeStatus> MapActions(Node node)
    {
        return () =>
        {
            _actionOrder.Add(node.Name);
            return BehaviourTreeStatus.Success;
        };
    }

    private Func<bool> MapConditions(Node node)
    {
        return () =>
        {
            _actionOrder.Add(node.Name);
            return false;
        };
    }
}