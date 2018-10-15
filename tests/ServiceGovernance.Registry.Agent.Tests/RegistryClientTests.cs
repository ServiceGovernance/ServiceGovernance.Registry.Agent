using FluentAssertions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using ServiceGovernance.Registry.Agent.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Testing.HttpClient;

namespace ServiceGovernance.Registry.Agent.Tests
{
    [TestFixture]
    public class RegistryClientTests
    {
        protected RegistryClient _registryClient;
        protected AgentOptions _options;
        protected Mock<IHttpClientFactory> _httpClientFactory;
        protected HttpClientTestingFactory _httpClientTestingFactory;
        protected Mock<IServer> _server;
        protected ServerAddressesFeature _serverAddressesFeature;

        [SetUp]
        public void Setup()
        {
            _options = new AgentOptions();
            _httpClientFactory = new Mock<IHttpClientFactory>();
            _server = new Mock<IServer>();
            var features = new FeatureCollection();
            _serverAddressesFeature = new ServerAddressesFeature();
            features.Set<IServerAddressesFeature>(_serverAddressesFeature);
            _server.SetupGet(s => s.Features).Returns(features);

            _registryClient = new RegistryClient(_options, _httpClientFactory.Object, _server.Object, new Mock<ILogger<RegistryClient>>().Object);

            _httpClientTestingFactory = new HttpClientTestingFactory();
            _httpClientTestingFactory.HttpClient.BaseAddress = new Uri("http://registry.com");
            _httpClientFactory.Setup(f => f.CreateClient(RegistryClient.HTTPCLIENT_NAME)).Returns(_httpClientTestingFactory.HttpClient);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClientTestingFactory.EnsureNoOutstandingRequests();
        }

        public class RegisterMethod : RegistryClientTests
        {
            [Test]
            public async Task Calls_Endpoint_With_Given_Information()
            {
                _options.ServiceDisplayName = "My Api";
                _options.ServiceIdentifier = "api1";
                _options.ServiceUrls = new[] { new Uri("http://test.com") };
                var action = Task.Run(() => _registryClient.RegisterService());

                var request = _httpClientTestingFactory.Expect(HttpMethod.Post, "http://registry.com/register");
                var registration = JsonConvert.DeserializeObject<ServiceRegistration>(request.Request.Content.ReadAsStringAsync().Result);
                registration.ServiceDisplayName.Should().Be(_options.ServiceDisplayName);
                registration.ServiceIdentifier.Should().Be(_options.ServiceIdentifier);
                registration.Endpoints.Should().Contain(new Uri("http://test.com"));

                request.Respond(HttpStatusCode.OK, "registerToken");

                await action;
            }

            [Test]
            public async Task Resolves_Service_Uri_And_Replace_Wildcard()
            {
                _options.ServiceIdentifier = "api1";
                var action = Task.Run(() => _registryClient.RegisterService());

                _serverAddressesFeature.Addresses.Add("http://*:5000");

                var request = _httpClientTestingFactory.Expect(HttpMethod.Post, "http://registry.com/register");
                var registration = JsonConvert.DeserializeObject<ServiceRegistration>(request.Request.Content.ReadAsStringAsync().Result);
                registration.Endpoints.Should().Contain(new Uri($"http://{Environment.MachineName}:5000"));

                request.Respond(HttpStatusCode.OK, "registerToken");

                await action;
            }

            [Test]
            public async Task Resolves_Service_Uri_From_Server()
            {
                _options.ServiceIdentifier = "api1";
                var action = Task.Run(() => _registryClient.RegisterService());

                _serverAddressesFeature.Addresses.Add("http://test.com");

                var request = _httpClientTestingFactory.Expect(HttpMethod.Post, "http://registry.com/register");
                var registration = JsonConvert.DeserializeObject<ServiceRegistration>(request.Request.Content.ReadAsStringAsync().Result);
                registration.Endpoints.Should().Contain(new Uri($"http://test.com"));

                request.Respond(HttpStatusCode.OK, "registerToken");

                await action;
            }
        }

        public class UnregisterMethod : RegistryClientTests
        {
            [Test]
            public async Task Calls_Endpoint_With_Token_From_Register_Call()
            {
                _registryClient.SetRegisterToken("superRegisterToken");

                var unregisterAction = Task.Run(() => _registryClient.UnregisterService());
                _httpClientTestingFactory.Expect(HttpMethod.Delete, "http://registry.com/register/superRegisterToken").Respond(HttpStatusCode.OK);
                await unregisterAction;
            }

            [Test]
            public void Calls_No_Endpoint_When_No_Token_Is_Set()
            {
                _registryClient.SetRegisterToken("");

                _registryClient.UnregisterService();
                _httpClientTestingFactory.EnsureNoOutstandingRequests();
            }
        }
    }
}
