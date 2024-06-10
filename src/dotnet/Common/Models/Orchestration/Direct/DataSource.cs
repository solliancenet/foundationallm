using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Orchestration.Direct
{
    public class DataSource
    {
        /// <summary>
        /// The type of the data source
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// The parameters of the datasource
        /// </summary>
        [JsonPropertyName("parameters")]
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
