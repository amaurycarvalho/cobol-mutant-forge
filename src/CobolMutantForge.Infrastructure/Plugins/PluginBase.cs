namespace CobolMutantForge.Infrastructure.Plugins;

/// <summary>
/// Base abstraction for import/export plugins. Concrete plugins derive from this
/// class and realize <see cref="CobolMutantForge.Domain.Interfaces.IImportPlugin"/>
/// and/or <see cref="CobolMutantForge.Domain.Interfaces.IExportPlugin"/>, keeping the
/// CLI's plugin enumeration uniform.
/// </summary>
public abstract class PluginBase
{
    public abstract string Name { get; }

    public virtual string Version { get; } = "1.0.0";
}
