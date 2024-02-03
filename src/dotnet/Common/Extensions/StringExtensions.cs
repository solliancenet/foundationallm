using Asp.Versioning.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Extensions
{
    /// <summary>
    /// Extends <see cref="string"/> with helper methods.
    /// </summary>
    public static class StringExtensions
    {
        private static List<string> KnownFQDNs => [
            "onelake.dfs.fabric.microsoft.com",
            "blob.core.windows.net",
            "dfs.core.windows.net",
            "sharepoint.com"
        ];

        /// <summary>
        /// Translates a URL into its neutral form.
        /// </summary>
        /// <param name="url"></param>
        public static string FromKnownNeutralUrl(this string url)
        {
            var originalUrl = url.ToLower();

            if (originalUrl.StartsWith("https://")
                || url.StartsWith("http://"))
                // Not a FoundationaLLM neutral URL format, so leave unchanged.
                return url;
            else if (url.StartsWith("FLLM:"))
            {
                // The neutral form is permitted to have one of the following forms:
                // - FLLM:<name>#blob#core#windows#net (translates into https://<name>.blob.core.windows.net
                // - FLLM:<name>#dfs#core#windows#net (translates into https://<name>.dfs.core.windows.net
                // - FLLM:<name>#sharepoint#com (translates into https://<name>.sharepoint.com
                // For the first two, <name> might end with `.privatelink` (when using private endpoints).
                // For the third, <name> must be the SharePoint Online tenant name.

                originalUrl = url
                    .Replace("FLLM:", "https://")
                    .Replace("#", ".")
                    .ToLower();
            }
            else
            {
                // The neutral form is permitted to have one of the following forms:
                // - <name>#blob#core#windows#net (translates into https://<name>.blob.core.windows.net
                // - <name>#dfs#core#windows#net (translates into https://<name>.dfs.core.windows.net
                // - <name>#sharepoint#com (translates into https://<name>.sharepoint.com
                // For the first two, <name> might end with `.privatelink` (when using private endpoints).
                // For the third, <name> must be the SharePoint Online tenant name.

                originalUrl = $"https://{url.ToLower()}";
            }

            // We only accept URL translation for predefined URLs
            return IsKnownUrl(originalUrl)
                ? originalUrl
                : url;
        }

        private static bool IsKnownUrl(string url)
        {
            var urlToCheck = url.ToLower();

            if (urlToCheck.EndsWith('/'))
                urlToCheck = urlToCheck.Remove(urlToCheck.Length - 1);

            return
                KnownFQDNs.Any(kf => urlToCheck.EndsWith(kf));
        }
    }
}
