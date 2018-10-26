using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServiceGovernance.Registry.Agent.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

namespace ServiceGovernance.Registry.Agent
{
    /// <summary>
    /// The registry client implementation
    /// </summary>
    public class RegistryClient : IRegistryClient
    {
        private readonly AgentOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private string _registerToken;

        internal const string HTTPCLIENT_NAME = "ServiceRegistryHttpClient";
        private const string REGISTER_URL = "/v1/register";
        private readonly ILogger<RegistryClient> _logger;
        private readonly IServer _server;

        public RegistryClient(AgentOptions options, IHttpClientFactory httpClientFactory, IServer server, ILogger<RegistryClient> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        /// <summary>
        /// Register this service in the registry
        /// </summary>        
        public void RegisterService()
        {
            var registration = new ServiceRegistration
            {
                ServiceIdentifier = _options.ServiceIdentifier,
                ServiceDisplayName = _options.ServiceDisplayName,
                Endpoints = _options.ServiceUrls ?? GetServiceUrls(),
                MachineIpAddress = GetIpAddress()
            };

            _logger.LogDebug($"Try registering the service as '{registration.ServiceIdentifier}' ({registration.ServiceDisplayName}) in registry with service url(s) '{GetServiceUrlsAsString(registration)}'.");

            var content = new StringContent(JsonConvert.SerializeObject(registration), Encoding.UTF8);
            var client = _httpClientFactory.CreateClient(HTTPCLIENT_NAME);

            try
            {
                var response = client.PostAsync(REGISTER_URL, content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                SetRegisterToken(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                _logger.LogInformation($"Service registration in registry as '{registration.ServiceIdentifier}' ({registration.ServiceDisplayName}) was successfull. Registered service url(s): {GetServiceUrlsAsString(registration)}");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Service registration in registry failed: {ex.Message}");
                throw;
            }
        }

        private string GetIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var address = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            if (address != null)
                return address.ToString();

            return string.Empty;
        }

        private string GetServiceUrlsAsString(ServiceRegistration registration)
        {
            return String.Join(", ", registration.Endpoints.Select(u => u.ToString()));
        }

        private Uri[] GetServiceUrls()
        {
            var addressesFeature = _server.Features.Get<IServerAddressesFeature>();

            var currentHostName = Dns.GetHostName();

            return addressesFeature.Addresses.Select(a => new Uri(ReplaceLocalHost(a, currentHostName))).ToArray();
        }

        private string ReplaceLocalHost(string uri, string address)
        {
            return uri.Replace("localhost", address).Replace("*", address);
        }

        /// <summary>
        /// Unregister this service in the registry
        /// </summary>
        public void UnregisterService()
        {
            if (!string.IsNullOrWhiteSpace(_registerToken))
            {
                _logger.LogDebug("Try unregistering the service in registry");
                var client = _httpClientFactory.CreateClient(HTTPCLIENT_NAME);
                try
                {
                    var response = client.DeleteAsync(REGISTER_URL + $"/{_registerToken}").GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();
                    _logger.LogInformation($"Service deregistration was successfull.");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical($"Service deregistration failed: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Sets the token, retrieved from the register method.
        /// </summary>
        /// <param name="registerToken"></param>
        internal void SetRegisterToken(string registerToken)
        {
            _registerToken = registerToken;
        }
    }
}
