using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.Interfaces;
using CobolMutantForge.Infrastructure.Plugins;
using Xunit;

namespace CobolMutantForge.Tests.Plugins;

public class TestAcceleratorPluginTests
{
    [Fact]
    public void Plugin_IsMarkedForV2()
    {
        var plugin = new TestAcceleratorPlugin();

        Assert.Equal("testaccelerator", plugin.Name);
        Assert.Equal("2.0.0", plugin.Version);
    }

    [Fact]
    public void Import_ReportsNotSupported()
    {
        var plugin = new TestAcceleratorPlugin();

        var exception = Assert.Throws<NotSupportedException>(() => plugin.Import("any-path"));

        Assert.Contains("not yet supported", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Import_AsImportPluginInterface_ReportsNotSupported()
    {
        IImportPlugin plugin = new TestAcceleratorPlugin();

        var exception = Assert.Throws<NotSupportedException>(() => plugin.Import("any-path"));

        Assert.Contains("not yet supported", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Export_ReportsNotSupported()
    {
        var plugin = new TestAcceleratorPlugin();
        var package = new MutantPackage("pkg-1");

        var exception = Assert.Throws<NotSupportedException>(() => plugin.Export(package, "out"));

        Assert.Contains("not yet supported", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Export_AsExportPluginInterface_ReportsNotSupported()
    {
        IExportPlugin plugin = new TestAcceleratorPlugin();
        var package = new MutantPackage("pkg-1");

        var exception = Assert.Throws<NotSupportedException>(() => plugin.Export(package, "out"));

        Assert.Contains("not yet supported", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
