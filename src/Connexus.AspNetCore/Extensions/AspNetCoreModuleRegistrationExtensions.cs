using Connexus.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Connexus.AspNetCore.Extensions;

public static class AspNetCoreModuleRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers lifecycle hooks (Start/Stop) of all modules using an IHostedService.
        /// This is the preferred non-blocking way to handle startup and shutdown hooks in ASP.NET Core.
        /// </summary>
        public IServiceCollection AddModuleLifecycle()
        {
            services.AddHostedService<ModuleLifecycleHostedService>();
            return services;
        }
    }

    extension(IMvcBuilder mvcBuilder)
    {
        /// <summary>
        /// Dynamically discovers and adds MVC Application Parts (Controllers) from all modules.
        /// </summary>
        public IMvcBuilder AddModuleControllers()
        {
            var modules = ModuleRegistrationExtensions.GetSortedModules();

            foreach (var module in modules)
            {
                var metadata = module.GetMetadata();
                if (metadata.HasApiControllers)
                {
                    mvcBuilder.AddApplicationPart(metadata.Assembly);
                }
            }

            return mvcBuilder;
        }
    }

    extension(MvcOptions options)
    {
        /// <summary>
        /// Aggregates global MVC filters from all web-aware modules.
        /// </summary>
        public void AddModuleFilters()
        {
            foreach (var module in ModuleRegistrationExtensions.GetSortedModules().OfType<IAspNetCoreModuleInitializer>())
            {
                module.ConfigureMvc(options);
            }
        }
    }

    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// Injects module-specific Middlewares into the request pipeline.
        /// </summary>
        public IApplicationBuilder UseModuleMiddlewares()
        {
            var modules = ModuleRegistrationExtensions.GetSortedModules().OfType<IAspNetCoreModuleInitializer>();

            foreach (var module in modules)
            {
                module.ConfigureMiddleware(app);
            }

            return app;
        }

        /// <summary>
        /// A convenience wrapper for ASP.NET Core applications. 
        /// Internally calls the pure ServiceProvider extension from Core.
        /// </summary>
        /// <param name="app">The web application builder.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task SeedModuleDataAsync(CancellationToken cancellationToken = default)
        {
            // Just a bridge to the Core implementation
            await app.ApplicationServices.SeedModuleDataAsync(cancellationToken);
        }
    }

    extension(IEndpointRouteBuilder endpoints)
    {
        /// <summary>
        /// Maps module-defined endpoints into the system route table.
        /// </summary>
        public IEndpointRouteBuilder MapModuleEndpoints()
        {
            var modules = ModuleRegistrationExtensions.GetSortedModules().OfType<IAspNetCoreModuleInitializer>();

            foreach (var module in modules)
            {
                module.ConfigureEndpoints(endpoints);
            }

            return endpoints;
        }
    }

    extension(AuthorizationOptions options)
    {
        public void ConfigureAuthorization()
        {
            var webModules = ModuleRegistrationExtensions.GetSortedModules().OfType<IAspNetCoreModuleInitializer>();

            foreach (var module in webModules)
            {
                module.ConfigureAuthorization(options);
            }
        }
    }
}
