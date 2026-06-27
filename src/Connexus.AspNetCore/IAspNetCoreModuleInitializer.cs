using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Connexus.AspNetCore
{
    /// <summary>
    /// Extended contract for web-aware modules.
    /// Allows modules to hook into the ASP.NET Core Request Pipeline and Endpoint Routing.
    /// </summary>
    public interface IAspNetCoreModuleInitializer
    {
        /// <summary>
        /// Configures Middlewares (e.g., app.UseMiddleware<T>()).
        /// Called typically after app.UseAuthorization().
        /// </summary>
        void ConfigureMiddleware(IApplicationBuilder app);
        /// <summary>
        /// Registers specific endpoints (e.g., SignalR Hubs, HealthChecks, Minimal APIs).
        /// Called typically after app.MapControllers().
        /// </summary>
        void ConfigureEndpoints(IEndpointRouteBuilder endpoints);
        /// <summary>
        /// Allows the module to register global filters or customize MVC behavior.
        /// </summary>
        void ConfigureMvc(MvcOptions options) { }
        /// <summary>
        /// Registers module-specific authorization policies.
        /// </summary>
        /// <remarks>
        /// !IMPORTANT! NOTE FOR FUTURE ARCHITECTURAL REFACTORING:
        /// Currently, this method is tightly coupled with ASP.NET Core 'AuthorizationOptions'.
        /// To make this library truly platform-agnostic (Enterprise-ready for CLI, Workers, etc.),
        /// consider refactoring this into a three-tier permission system:
        /// 1. Define: Use a generic 'IPermissionDefinitionContext' in the Core project to declare strings like "Billing.View".
        /// 2. Store: Create an 'IPermissionStore' to manage these definitions and their relationships to roles/users.
        /// 3. Check: Implement an 'IPermissionChecker' service that can be used anywhere.
        /// In the Web layer, you would then create a 'PermissionRequirement : IAuthorizationRequirement' 
        /// that bridges these generic permissions to the ASP.NET Core Policy system automatically.
        /// </remarks>
        void ConfigureAuthorization(AuthorizationOptions options) { }
    }
}
