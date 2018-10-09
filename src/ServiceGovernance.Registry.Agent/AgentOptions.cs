using System;

namespace ServiceGovernance.Registry.Agent
{
    /// <summary>
    /// Options for the registry agent
    /// </summary>
    public class AgentOptions
    {
        /// <summary>
        /// Gets or sets the uri of the registry
        /// </summary>
        public Uri Registry { get; set; }

        /// <summary>
        /// Gets or sets a unique service identifier
        /// </summary>
        public string ServiceIdentifier { get; set; }

        /// <summary>
        /// Gets or sets a display name of the service
        /// </summary>
        public string ServiceDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the urls the service is available on
        /// </summary>
        public Uri[] ServiceUrls { get; set; }

        /// <summary>
        /// Validate the option's values
        /// </summary>
        public void Validate()
        {
            if (Registry == null)
                throw new ConfigurationException("The registry uri is not defined!", nameof(Registry));

            if (string.IsNullOrWhiteSpace(ServiceIdentifier))
                throw new ConfigurationException("ServiceIdentifier is not defined!", nameof(ServiceIdentifier));
        }
    }
}
