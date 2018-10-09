using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceGovernance.Registry.Agent
{
    /// <summary>
    /// Interface to client registry functions
    /// </summary>
    public interface IRegistryClient
    {
        /// <summary>
        /// Register this service in the registry
        /// </summary>        
        void RegisterService();

        /// <summary>
        /// Unregister this service in the registry
        /// </summary>
        void UnregisterService();
    }
}
