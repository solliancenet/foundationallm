using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations
{
    /// <summary>
    /// Blob storage configuration settings.
    /// </summary>
    public class BlobStorageConfiguration
    {
        [JsonProperty("connection_string_secret")]
        public string ConnectionStringSecretName { get;set; }

        [JsonProperty("container")]
        public string ContainerName { get;set; }

        [JsonProperty("files")]
        public List<string> Files { get;set; }
    }
}
