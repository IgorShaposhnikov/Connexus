using System.Reflection;

namespace Connexus;

/// <summary>
/// Metadata required for module orchestration, including loading order and dependencies.
/// </summary>
/// <param name="DependsOn">List of Module IDs that must be initialized before this module.</param>
public readonly struct ModuleMetadata(Guid id, string name, Assembly assembly, string version, int priority, Guid[]? dependsOn = null, bool hasApiControllers = false)
{
    public Guid Id { get; init; } = id;
    public string Name { get; init; } = name;
    public string Version { get; init; } = version;
    public Assembly Assembly { get; init; } = assembly;
    public bool HasApiControllers { get; init; } = hasApiControllers;
    /// <summary>
    /// Lower values indicate higher priority (loaded earlier).
    /// </summary>
    public int Priority { get; init; } = priority;
    public Guid[] DependsOn { get; } = dependsOn ?? [];
}
