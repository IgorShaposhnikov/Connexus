using Connexus.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Connexus.AspNetCore;

internal sealed class ModuleLifecycleHostedService(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var modules = ModuleRegistrationExtensions.GetSortedModules();
        using var scope = serviceProvider.CreateScope();

        foreach (var module in modules)
        {
            await module.OnStartedAsync(scope.ServiceProvider);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var modules = ModuleRegistrationExtensions.GetSortedModules();
        using var scope = serviceProvider.CreateScope();

        foreach (var module in modules)
        {
            await module.OnStoppingAsync(scope.ServiceProvider);
        }
    }
}