using ServiceGovernance.Registry.Agent;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up the agent in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the registry agent services to the collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="setupBuilder">Delegate to define the configuration.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// services
        /// or
        /// setupBuilder
        /// </exception>
        public static IServiceCollection AddServiceRegistryAgent(this IServiceCollection services, Action<AgentOptions> setupBuilder)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (setupBuilder == null)
                throw new ArgumentNullException(nameof(setupBuilder));

            var options = new AgentOptions();
            setupBuilder(options);

            return AddServiceRegistryAgent(services, options);
        }

        /// <summary>
        /// Adds the registry agent services to the collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">The agent options.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// services
        /// or
        /// options
        /// </exception>
        public static IServiceCollection AddServiceRegistryAgent(this IServiceCollection services, AgentOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.Validate();

            services.AddSingleton(options);
            services.AddSingleton<IRegistryClient, RegistryClient>();

            services.AddHttpClient(RegistryClient.HTTPCLIENT_NAME, client => {
                client.BaseAddress = options.Registry;
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", $"ServiceRegistryClient - {Assembly.GetExecutingAssembly().GetName().Version} - {options.ServiceIdentifier}");
            });

            return services;
        }
    }
}
