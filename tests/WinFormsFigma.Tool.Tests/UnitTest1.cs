namespace WinFormsFigma.Tool.Tests;

using WinFormsFigma.Tool;

public class UnitTest1
{
    [Fact]
    public void BuildOutput_WithNoArgs_ReturnsDefaultGreeting()
    {
        var result = ToolRunner.BuildOutput([]);

        Assert.Equal("Hello, World! WinFormsFigma tool is ready.", result);
    }

    [Fact]
    public void BuildOutput_WithHelpArg_ReturnsUsage()
    {
        var result = ToolRunner.BuildOutput(["--help"]);

        Assert.Contains("Usage: wffigma", result);
    }
}
