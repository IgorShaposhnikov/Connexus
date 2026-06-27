using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Connexus.Extensions;

public static class ModuleRegistrationExtensions
{
    private static List<IModuleInitializer>? _sortedModules;

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers services from all modules following the dependency order.
        /// </summary>
        public IServiceCollection AddModuleServices(IConfiguration configuration, HashSet<string>? excludeModules = null)
        {
            var modules = GetSortedModules();

            foreach (var module in modules)
            {
                var metadata = module.GetMetadata();

                if (excludeModules != null && (excludeModules.Contains(metadata.Id.ToString()) || excludeModules.Contains(metadata.Name)))
                {
                    continue;
                }

                module.AddServices(services, configuration);
            }

            return services;
        }
    }

    /// <summary>
    /// Pure DI extension to initialize data seeding. 
    /// Can be used in any .NET application (Console, Worker, Web).
    /// </summary>
    /// <param name="serviceProvider">The root service provider.</param>
    /// <param name="ct">Cancellation token.</param>
    public static async Task SeedModuleDataAsync(this IServiceProvider serviceProvider, CancellationToken ct = default)
    {
        // We create a scope here to ensure Scoped services (like DB Contexts) 
        // can be resolved during the seeding process.
        using var scope = serviceProvider.CreateScope();
        var modules = GetSortedModules();

        foreach (var module in modules)
        {
            await module.SeedDataAsync(scope.ServiceProvider, ct);
        }
    }

    /// <summary>
    /// Resolves the load order of modules based on Priorities and Dependencies using Topological Sort.
    /// Ensures that if Module A depends on Module B, B is always loaded first.
    /// </summary>
    public static IEnumerable<IModuleInitializer> GetSortedModules()
    {
        if (_sortedModules != null)
            return _sortedModules;

        var initializerInterfaceType = typeof(IModuleInitializer);
        var instances = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => initializerInterfaceType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .Select(type => (IModuleInitializer)Activator.CreateInstance(type)!)
            .OrderBy(m => m.GetMetadata().Priority)
            .ThenBy(m => m.GetMetadata().Name)
            .ToList();

        _sortedModules = [];
        var visited = new HashSet<Guid>();
        var acting = new HashSet<Guid>();

        void Visit(IModuleInitializer module)
        {
            var id = module.GetMetadata().Id;
            if (visited.Contains(id))
                return;

            if (acting.Contains(id))
                throw new Exception($"Circular dependency detected in module: {module.GetMetadata().Name}");

            acting.Add(id);

            foreach (var depId in module.GetMetadata().DependsOn)
            {
                var depModule = instances.FirstOrDefault(m => m.GetMetadata().Id == depId);
                if (depModule != null)
                {
                    Visit(depModule);
                }
                else
                {
                    throw new Exception($"Module {module.GetMetadata().Name} depends on missing module with ID: {depId}");
                }
            }

            acting.Remove(id);
            visited.Add(id);
            _sortedModules?.Add(module);
        }

        foreach (var module in instances)
        {
            Visit(module);
        }

        return _sortedModules;
    }
}
