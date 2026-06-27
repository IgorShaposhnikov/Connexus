using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Connexus;

/// <summary>
/// The base contract for all modules in the system.
/// Handles Dependency Injection and data initialization.
/// </summary>
public interface IModuleInitializer
{
    ModuleMetadata GetMetadata();
    /// <summary>
    /// Registers module-specific services into the DI container.
    /// </summary>
    IServiceCollection AddServices(IServiceCollection services, IConfiguration configuration);
    /// <summary>
    /// Populates the database with initial data (Lookup data, default settings, etc.).
    /// Called after all services are registered and the DB is ready.
    /// </summary>
    Task SeedDataAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) => Task.CompletedTask;
    /// <summary>
    /// Triggered when the application host has fully started.
    /// Use this for background tasks or cache warming.
    /// </summary>
    Task OnStartedAsync(IServiceProvider sp) => Task.CompletedTask;

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// Use this for resource cleanup.
    /// </summary>
    Task OnStoppingAsync(IServiceProvider sp) => Task.CompletedTask;
}
