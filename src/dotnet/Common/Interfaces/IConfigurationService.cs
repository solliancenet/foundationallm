using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Common interface for configuration management.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Retrieves a configuration value by key.
        /// </summary>
        /// <param name="key">The key used to retrieve the configuration value.</param>
        /// <returns>The configuration value cast to the provided type.</returns>
        T GetValue<T>(string key);
    }

}
