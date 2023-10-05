using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Settings
{
    /// <summary>
    /// JSON serializer settings for the API classes and their libraries.
    /// </summary>
    public static class CommonJsonSerializerSettings
    {
        /// <summary>
        /// Configures the Newtonsoft JSON serializer settings.
        /// </summary>
        /// <returns></returns>
        public static JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
    }
}
