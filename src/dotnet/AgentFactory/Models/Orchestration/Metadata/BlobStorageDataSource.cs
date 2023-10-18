using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata
{
    /// <summary>
    /// Blob storage data source metadata model.
    /// </summary>
    public class BlobStorageDataSource : MetadataBase
    {
        /// <summary>
        /// Blob storage configuration settings.
        /// </summary>
        [JsonProperty("configuration")]
        public BlobStorageConfiguration Configuration { get; set; }
    }
}
