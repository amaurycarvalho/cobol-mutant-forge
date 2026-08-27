using CobolMutantForge.Application.Services;
using CobolMutantForge.Application.UseCases;
using CobolMutantForge.CLI.Extensions;
using CobolMutantForge.Domain.Interfaces;
using CobolMutantForge.Infrastructure.Configuration;
using CobolMutantForge.Infrastructure.Exporters;
using CobolMutantForge.Infrastructure.Mutators;
using CobolMutantForge.Infrastructure.Parsers;
using CobolMutantForge.Infrastructure.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CobolMutantForge.Tests.CLI;

public class ServiceCollectionTests
{
    private static ServiceProvider CreateProvider()
        => new ServiceCollection().AddCobolMutantForge().BuildServiceProvider();

    [Fact]
    public void AddCobolMutantForge_ReturnsSameCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddCobolMutantForge();

        Assert.Same(services, result);
    }

    [Fact]
    public void ResolvesCobolParserAsTypeCobolParserAdapter()
    {
        using var provider = CreateProvider();

        Assert.IsType<TypeCobolParserAdapter>(provider.GetRequiredService<ICobolParser>());
    }

    [Fact]
    public void ResolvesMutationEngineAsSingleton()
    {
        using var provider = CreateProvider();

        var direct = provider.GetRequiredService<MutationEngine>();
        var viaInterface = provider.GetRequiredService<IMutationEngine>();

        Assert.IsType<MutationEngine>(viaInterface);
        Assert.Same(direct, viaInterface);
    }

    [Fact]
    public void ResolvesApplicationServices()
    {
        using var provider = CreateProvider();

        Assert.IsType<JsonConfigSerializer>(provider.GetRequiredService<JsonConfigSerializer>());
        Assert.IsType<ValidationService>(provider.GetRequiredService<ValidationService>());
        Assert.IsType<GenerateMutationsUseCase>(provider.GetRequiredService<GenerateMutationsUseCase>());
        Assert.IsType<ExportMutantsUseCase>(provider.GetRequiredService<ExportMutantsUseCase>());
    }

    [Fact]
    public void ResolvesPluginsAsSingletons()
    {
        using var provider = CreateProvider();

        var zunit = provider.GetRequiredService<ZUnitPlugin>();
        var testAccelerator = provider.GetRequiredService<TestAcceleratorPlugin>();

        Assert.Same(zunit, provider.GetRequiredService<ZUnitPlugin>());
        Assert.Same(testAccelerator, provider.GetRequiredService<TestAcceleratorPlugin>());
    }

    [Fact]
    public void RegistersTwoPluginBases()
    {
        using var provider = CreateProvider();

        var plugins = provider.GetServices<PluginBase>().ToList();

        Assert.Equal(2, plugins.Count);
        Assert.Contains(plugins, plugin => plugin is ZUnitPlugin);
        Assert.Contains(plugins, plugin => plugin is TestAcceleratorPlugin);
    }

    [Fact]
    public void RegistersTwoImportPlugins()
    {
        using var provider = CreateProvider();

        var plugins = provider.GetServices<IImportPlugin>().ToList();

        Assert.Equal(2, plugins.Count);
        Assert.Contains(plugins, plugin => plugin is ZUnitPlugin);
        Assert.Contains(plugins, plugin => plugin is TestAcceleratorPlugin);
    }

    [Fact]
    public void RegistersExportPluginsIncludingFolderExporter()
    {
        using var provider = CreateProvider();

        var plugins = provider.GetServices<IExportPlugin>().ToList();

        Assert.Equal(2, plugins.Count);
        Assert.Contains(plugins, plugin => plugin is MutantPackageExporter);
        Assert.Contains(plugins, plugin => plugin is TestAcceleratorPlugin);
    }
}
