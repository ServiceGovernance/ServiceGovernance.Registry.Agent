using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using ServiceGovernance.Registry.Agent;
using System;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Pipeline extension methods for adding registry agent
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds registry agent to the pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseServiceRegistryAgent(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            var lifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();
            var client = app.ApplicationServices.GetRequiredService<IRegistryClient>();            

            // register service when it's running
            lifetime.ApplicationStarted.Register(() => client.RegisterService());

            // remove registration when service is shutting down
            lifetime.ApplicationStopping.Register(() => client.UnregisterService());

            return app;
        }
    }
}
