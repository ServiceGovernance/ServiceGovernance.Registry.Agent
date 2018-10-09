using FluentAssertions;
using NUnit.Framework;
using System;

namespace ServiceGovernance.Registry.Agent.Tests
{
    [TestFixture]
    public class AgentOptionsTests
    {
        protected AgentOptions _options;

        [SetUp]
        public void Setup()
        {
            _options = new AgentOptions();
        }

        public class ValidateMethod : AgentOptionsTests
        {
            [Test]
            public void Should_Throw_Exception_If_No_Registry_Is_Defined()
            {
                _options.Registry = null;

                Action action = () => _options.Validate();
                action.Should().ThrowExactly<ConfigurationException>().Where(e => e.ConfigurationName == "Registry");
            }

            [Test]
            public void Should_Throw_Exception_If_No_ServiceIdentifier_Is_Defined()
            {
                _options.Registry = new Uri("http://test.com");
                _options.ServiceIdentifier = null;

                Action action = () => _options.Validate();
                action.Should().ThrowExactly<ConfigurationException>().Where(e => e.ConfigurationName == "ServiceIdentifier");
            }

            [Test]
            public void Should_Throw_Exception_If_ServiceIdentifier_Is_Empty()
            {
                _options.Registry = new Uri("http://test.com");
                _options.ServiceIdentifier = "";

                Action action = () => _options.Validate();
                action.Should().ThrowExactly<ConfigurationException>().Where(e => e.ConfigurationName == "ServiceIdentifier");
            }

            [Test]
            public void Should_Not_Throw_Exception_If_Required_Values_Filled()
            {
                _options.Registry = new Uri("http://test.com");
                _options.ServiceIdentifier = "myapi";

                Action action = () => _options.Validate();
                action.Should().NotThrow();
            }
        }
    }
}
