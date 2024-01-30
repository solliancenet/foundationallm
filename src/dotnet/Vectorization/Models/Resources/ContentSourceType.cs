using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Models.Resources
{
    /// <summary>
    /// Types of content sources from which documents can be retrieved.
    /// </summary>
    public enum ContentSourceType
    {
        /// <summary>
        /// Azure data lake storage account.
        /// </summary>
        AzureDataLake,

        /// <summary>
        /// SharePoint Online document library.
        /// </summary>
        SharePointOnline,

        /// <summary>
        /// Azure SQL Database.
        /// </summary>
        AzureSQLDatabase
    }
}
