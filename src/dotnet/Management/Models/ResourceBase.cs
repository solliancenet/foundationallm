using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Management.Models
{
    public class ResourceBase
    {
        public required string Type { get; set; }
        public required string Name { get; set; }
    }
}
