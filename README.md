# ServiceGovernance.Registry.Agent

[![Build status](https://ci.appveyor.com/api/projects/status/int05fhd174ds1wx?svg=true)](https://ci.appveyor.com/project/twenzel/servicegovernance-registry-agent)
[![NuGet Version](http://img.shields.io/nuget/v/ServiceGovernance.Registry.Agent.svg?style=flat)](https://www.nuget.org/packages/ServiceGovernance.Registry.Agent/)
[![License](https://img.shields.io/badge/license-Apache-blue.svg)](LICENSE)

Provides an Agent (client) for the [ServiceRegistry](https://github.com/ServiceGovernance/ServiceGovernance.Registry). This agent registers the ASP.NET Core Service (your API web service) in the ServiceRegistry on application startup and unregisters it on application shutdown.

## Usage

Install the NuGet package `ServiceGovernance.Registry.Agent`.

```CSharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddServiceRegistryAgent(options => {
            options.Registry = new Uri("https://myserviceregistry.mycompany.com");
            options.ServiceIdentifier = "Api1";
            options.ServiceDisplayName = "My Api";
            });
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    ...
    app.UseMvc();
    ...
    app.UseServiceRegistryAgent();
}
```

## Configuration

It's also possible to provide these options via the configuration:

```CSharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddServiceRegistryAgent(options => Configuration.Bind("ServiceRegistry", options));
}
```

```json
{
    "ServiceRegistry": {
        "Registry": "https://myserviceregistry.mycompany.com",
        "ServiceIdentifier": "Api1",
        "ServiceDisplayName": "My Api"
    }
}
```

### Service urls

The url, the service is available on and which will be registered in the ServiceRegistry, will be detected automatically. You can manually provide the service urls via the `ServiceUrls` property (or configuration key).