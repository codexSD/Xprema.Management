# Xprema Framework Module System

This framework provides a modular architecture for ASP.NET Core applications, allowing you to easily create and inject modules into your projects.

## Features

- Simple module registration
- Dependency injection support
- Middleware configuration
- Version tracking
- Modular architecture

## Getting Started

### 1. Create a Module

Create a new class that inherits from `ModuleBase`:

```csharp
public class MyModule : ModuleBase
{
    public override string Name => "My Module";
    public override string Description => "Description of my module";
    
    public override void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register your services here
        services.AddScoped<IMyService, MyService>();
    }
    
    public override void Configure(IApplicationBuilder app)
    {
        // Configure middleware here
        app.UseMiddleware<MyMiddleware>();
    }
}
```

### 2. Register the Module

In your `Program.cs` or `Startup.cs`:

```csharp
// Register the module
builder.Services.AddFrameworkModule<MyModule>(builder.Configuration);

// ... other configuration ...

// Configure the module middleware
app.UseFrameworkModules();
```

### 3. Using Multiple Modules

You can register multiple modules:

```csharp
builder.Services
    .AddFrameworkModule<Module1>(builder.Configuration)
    .AddFrameworkModule<Module2>(builder.Configuration)
    .AddFrameworkModule<Module3>(builder.Configuration);
```

## Best Practices

1. Keep modules focused on a single responsibility
2. Use dependency injection for services
3. Implement proper error handling in middleware
4. Version your modules appropriately
5. Document your module's configuration options

## Sample Module

Check out the `SampleModule` in the framework for a complete example of how to implement a module.

## License

This project is licensed under the MIT License. 