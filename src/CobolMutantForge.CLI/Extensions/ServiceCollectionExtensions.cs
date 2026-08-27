using CobolMutantForge.Application.Configuration;
using CobolMutantForge.Application.Services;
using CobolMutantForge.Application.UseCases;
using CobolMutantForge.Domain.Interfaces;
using CobolMutantForge.Infrastructure.Configuration;
using CobolMutantForge.Infrastructure.Exporters;
using CobolMutantForge.Infrastructure.Mutators;
using CobolMutantForge.Infrastructure.Parsers;
using CobolMutantForge.Infrastructure.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace CobolMutantForge.CLI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCobolMutantForge(this IServiceCollection services)
    {
        services.AddSingleton<ICobolParser, TypeCobolParserAdapter>();

        services.AddSingleton<MutationEngine>();
        services.AddSingleton<IMutationEngine>(sp => sp.GetRequiredService<MutationEngine>());

        services.AddSingleton<JsonConfigSerializer>();
        services.AddSingleton<ValidationService>();
        services.AddSingleton<GenerateMutationsUseCase>();
        services.AddSingleton<ExportMutantsUseCase>();

        services.AddSingleton<ZUnitPlugin>();
        services.AddSingleton<TestAcceleratorPlugin>();
        services.AddSingleton<PluginBase>(sp => sp.GetRequiredService<ZUnitPlugin>());
        services.AddSingleton<PluginBase>(sp => sp.GetRequiredService<TestAcceleratorPlugin>());
        services.AddSingleton<IImportPlugin>(sp => sp.GetRequiredService<ZUnitPlugin>());
        services.AddSingleton<IImportPlugin>(sp => sp.GetRequiredService<TestAcceleratorPlugin>());
        services.AddSingleton<IExportPlugin>(sp => sp.GetRequiredService<TestAcceleratorPlugin>());
        services.AddSingleton<IExportPlugin>(sp => new MutantPackageExporter(ExportFormat.Folder));

        return services;
    }
}
