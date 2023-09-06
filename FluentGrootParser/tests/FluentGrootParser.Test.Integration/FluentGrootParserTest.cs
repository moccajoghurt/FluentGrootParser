using FluentGrootParser.Models.FluentBehaviorTree;
using FluentGrootParser.Models.Groot2;
using FluentGrootParser.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FluentGrootParser.Test.Integration;

public class FluentGrootParserTest
{
    private readonly List<string> _actionOrder;
    private readonly ServiceCollection _serviceCollection = new();
    private readonly IServiceProvider _serviceProvider;

    public FluentGrootParserTest()
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

    [Fact]
    public void GetAllNodes_ReturnsAllNodes()
    {
        // Arrange
        var expectedNodes = new List<string>
        {
            "DistanceToWaypointBiggerThan",
            "DoAdvanceWaypoint",
            "HasNoWaypoints",
            "LastJoinLongerAgoThanRandomized",
            "PressKeyKeepWalking",
            "StopWalking",
            "Walk",
            "WaypointEndpointReached",
            "WaypointReached"
        };
        var parser = _serviceProvider.GetService<IFluentGrootParser>();
        var path = Path.Combine(Directory.GetCurrentDirectory(), "../../../SampleFiles");
        parser?.ConvertToTree(path, MapActions, MapConditions);
        // Act
        var nodes = parser?.GetAllTreeNodes();
        // Assert
        var nodeNames = nodes?.Select(n => n.Name).ToList();
        Assert.NotNull(nodeNames);
        Assert.True(expectedNodes.TrueForAll(expectedNode => nodeNames.Contains(expectedNode)));
    }

    [Fact]
    public void SubtreeHasParameters_ChildNodesUseParameters()
    {
        // Arrange
        Func<bool> Conditions(Node node) =>
            () =>
            {
                if (node.Name == "DistanceToWaypointBiggerThan")
                {
                    // Assert
                    Assert.Equal("13", node.Params?["distance"]);
                }

                return false;
            };

        var parser = _serviceProvider.GetService<IFluentGrootParser>();
        var path = Path.Combine(Directory.GetCurrentDirectory(), "../../../SampleFiles");
        // Act
        var tree = parser?.ConvertToTree(path, MapActions, Conditions);
        tree?.Tick();

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