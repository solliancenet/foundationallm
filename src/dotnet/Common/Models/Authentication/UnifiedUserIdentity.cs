using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Authentication
{
    /// <summary>
    /// Represents strongly-typed user identity information, regardless of
    /// the identity provider.
    /// </summary>
    public class UnifiedUserIdentity
    {
        /// <summary>
        /// The user's display name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// The username of the user used to authenticate.
        /// </summary>
        [JsonProperty("user_name")]
        public string Username { get; set; }
        /// <summary>
        /// The User Principal Name (UPN) of the user.
        /// </summary>
        [JsonProperty("upn")]
        public string UPN { get; set; }
    }
}
