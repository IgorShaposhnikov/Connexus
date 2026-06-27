# 📁 Connexus — Modular Monolith Core for ASP.NET Core

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)

**Connexus** is a lightweight, structured module orchestrator designed to build decoupled, maintainable Modular Monoliths or plugin-based systems in .NET 10 and ASP.NET Core.

At a certain scale, monolithic applications risk becoming tightly coupled, making business boundaries difficult to enforce. While migrating directly to a microservices architecture can solve this, it introduces significant operational overhead, network latency, and deployment complexity. 

**Connexus** offers a pragmatic middle ground: a **Modular Monolith** core framework. It allows you to write isolated feature modules containing their own domain rules, controllers, and services, while letting the host dynamically discover, order, and run them as a single, cohesive unit. The core ensures that dependencies are resolved correctly and that initialization tasks are executed in a reliable, topologically sorted sequence.

> [!NOTE]  
> The name refers to *connexus* — Latin for connection, link, or union. It acts as the bridging link that binds separate modules into a single, unified runtime.

> [!IMPORTANT]  
> **Database Isolation Notice:**  
> Currently, Connexus focuses on service container orchestration, middleware pipeline mapping, and runtime lifecycle hooks. It **does not** solve database schema isolation, database-per-module separation, or distributed migration orchestration. These challenges are identified as key target areas for future architectural phases.

---

## ✨ Features

The system orchestrates module behaviors across the following key areas:

*   🔄 **Topological Dependency Sorting:** Automatically scans assemblies and resolves registration order based on priorities and explicit dependencies. If Module A depends on Module B, the core guarantees that Module B's DI configurations and startup tasks are processed first.
*   🛠 **Platform-Agnostic Core:** Base contracts (`IModuleInitializer` and `ModuleMetadata`) are isolated in a core library with no dependencies on the ASP.NET Core web stack, making them reusable in background workers, queue consumers, and CLI applications.
*   🌐 **Deep Web Integration:** An ASP.NET Core extension layer dynamically registers module controllers with the MVC framework, aggregates global filters, maps minimal endpoints, and injects custom middleware in a defined sequence.
*   ⚡ **Non-Blocking Lifecycles:** Out-of-the-box support for asynchronous module startup and shutdown hooks integrated directly into the `IHostApplicationLifetime` pipeline, eliminating sync-over-async blocking on startup.
*   🌱 **Ordered Data Seeding:** A dedicated seeding step executes lookup data or configuration updates after the dependency injection container has been fully built.

---

## 🛠 Tech Stack & Architecture

This library is organized into two primary packages to maintain a strict separation of concerns.

*   **`Connexus` (Core Library):** A standard C# library targeting only standard dependency injection and configuration abstractions (`Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Configuration.Abstractions`).
*   **`Connexus.AspNetCore` (Web Hosting Layer):** Built on top of the `Microsoft.AspNetCore.App` framework reference. It bridges the generic modules directly to the HTTP pipeline, Endpoint Routing, MVC, and Authorization systems.

### 📐 Structural Flow

```
[ Module Assemblies ] (Feature A, Feature B)
         │
         ▼
┌──────────────────────────────────────────────┐
│       Connexus Core                          │
│  - Topological Discovery & Sort              │
│  - DI Registrations (AddServices)            │
│  - Async Data Seeding (SeedModuleDataAsync)  │
└──────────────────────┬───────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────┐
│       Connexus.AspNetCore (Web)              │
│  - Controllers & Filters Registration        │
│  - Pipeline Middleware Injection             │
│  - Async Lifecycles (IHostedService)         │
│  - Endpoint Mapping                          │
└──────────────────────────────────────────────┘
```

---

## 🚀 Quick Start

Here is a quick overview of how to define a module and bootstrap it in your host application.

### 1. Define a Module
Implement the `IModuleInitializer` in your module project:

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Connexus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TextFlow.Modules.Billing;

public class BillingModuleInitializer : IModuleInitializer
{
    private static readonly Guid ModuleId = Guid.Parse("a5e2f5bf-478a-4d7a-8f5b-172159850e01");

    public ModuleMetadata GetMetadata()
    {
        return new ModuleMetadata(
            id: ModuleId,
            name: "BillingModule",
            assembly: typeof(BillingModuleInitializer).Assembly,
            version: "1.0.0",
            priority: 10,
            dependsOn: null, // Add dependent module GUIDs here to guarantee execution order
            hasApiControllers: true
        );
    }

    public IServiceCollection AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register module-specific services into the shared DI container
        services.AddScoped<IBillingService, BillingService>();
        return services;
    }

    public async Task SeedDataAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        // Execute seeding setup, or system parameters lookup
        await Task.CompletedTask;
    }
}
```

### 2. Configure your Program.cs
Bootstrap your application using standard extensions inside your web host:

```csharp
using Connexus.AspNetCore.Extensions;
using Connexus.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers and discover module-defined endpoints/parts
builder.Services.AddControllers(options =>
{
    options.AddModuleFilters();
})
.AddModuleControllers();

// Register services for all discovered modules using topological sorting
builder.Services.AddModuleServices(builder.Configuration);

// Register the background service to handle module startup & shutdown hooks
builder.Services.AddModuleLifecycle();

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map custom middleware from modules
app.UseModuleMiddlewares();

// Map custom endpoints (Minimal APIs, SignalR, Hubs) from modules
app.MapControllers();
app.MapModuleEndpoints();

// Optional: Execute seeding steps on startup
await app.SeedModuleDataAsync();

app.Run();
```

## 🤝 Contributing

Contributions are welcome! If you find a bug, have optimization suggestions, or would like to propose a feature:
1. Open an Issue outlining the bug or feature suggestion.
2. Submit a Pull Request targeting the primary development branch.

Please ensure your changes are modular, decoupled, and maintain compatibility across non-web environments where possible.
